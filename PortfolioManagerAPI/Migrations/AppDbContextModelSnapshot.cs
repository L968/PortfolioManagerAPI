﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PortfolioManagerAPI.Infrastructure;

#nullable disable

namespace PortfolioManagerAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("PortfolioManagerAPI.Entities.InvestmentProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<decimal>("Price")
                        .HasPrecision(65, 2)
                        .HasColumnType("decimal(65,2)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("InvestmentProducts");
                });

            modelBuilder.Entity("PortfolioManagerAPI.Entities.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("InvestmentProductId")
                        .HasColumnType("int");

                    b.Property<string>("InvestmentProductName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<decimal>("Price")
                        .HasPrecision(65, 2)
                        .HasColumnType("decimal(65,2)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("PortfolioManagerAPI.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PortfolioManagerAPI.Entities.UserProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("AveragePrice")
                        .HasPrecision(65, 2)
                        .HasColumnType("decimal(65,2)");

                    b.Property<int>("InvestmentProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InvestmentProductId");

                    b.ToTable("UserProducts");
                });

            modelBuilder.Entity("PortfolioManagerAPI.Entities.UserProduct", b =>
                {
                    b.HasOne("PortfolioManagerAPI.Entities.InvestmentProduct", "InvestmentProduct")
                        .WithMany()
                        .HasForeignKey("InvestmentProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InvestmentProduct");
                });
#pragma warning restore 612, 618
        }
    }
}
