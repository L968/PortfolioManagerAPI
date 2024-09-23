using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MySql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using PortfolioManagerAPI.Behaviours;
using PortfolioManagerAPI.Handlers;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Services;
using PortfolioManagerAPI.Jobs;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddFluentValidationAutoValidation();
ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var connectionString = builder.Configuration.GetConnectionString("Database");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseMySql(
            connectionString,
            serverVersion,
            mysqlOptions => mysqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
        )
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(token =>
{
    token.RequireHttpsMetadata = false;
    token.SaveToken = true;
    token.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddHangfire(options =>
{
    options.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
           .UseColouredConsoleLogProvider()
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions()));
});

builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(o => { });

app.UseHttpsRedirection();

app.UseHangfireDashboard();

app.MapHangfireDashboard();

RecurringJob.AddOrUpdate<ExpiringProductsNotificationJob>("Notify Admins of Expiring Products", job => job.SendEmailNotification(), Cron.Daily);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
