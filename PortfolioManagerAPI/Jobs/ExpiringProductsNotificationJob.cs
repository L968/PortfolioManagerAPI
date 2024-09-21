using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Services;

namespace PortfolioManagerAPI.Jobs;

public class ExpiringProductsNotificationJob(
    AppDbContext context,
    IEmailService emailService,
    ILogger<ExpiringProductsNotificationJob> logger
    )
{
    private readonly AppDbContext _context = context;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<ExpiringProductsNotificationJob> _logger = logger;

    public void SendEmailNotification()
    {
        var products = _context.InvestmentProducts
            .Where(p => p.ExpirationDate < DateTime.UtcNow.AddDays(7))
            .ToList();

        if (products.Count <= 0)
        {
            _logger.LogInformation("No products expiring within the next 7 days.");
            return;
        }

        _emailService.SendExpiringProductsEmail(products);
        _logger.LogInformation("Notifying about {Count} expiring products.", products.Count);
    }
}
