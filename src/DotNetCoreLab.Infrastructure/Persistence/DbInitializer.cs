using System.Security.Cryptography;
using System.Text;
using DotNetCoreLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("🌱 [SEED DATA] Varsayılan Admin ve User hesapları oluşturuluyor...");

                var defaultUsers = new List<User>
                {
                    new User
                    {
                        Name = "Sistem Yöneticisi (Admin)",
                        Email = "admin@test.com",
                        PasswordHash = HashPassword("Admin123!"),
                        Role = "Admin"
                    },
                    new User
                    {
                        Name = "Standart Müşteri (User)",
                        Email = "user@test.com",
                        PasswordHash = HashPassword("User123!"),
                        Role = "User"
                    }
                };

                await context.Users.AddRangeAsync(defaultUsers);
                await context.SaveChangesAsync();
            }

            if (!await context.Products.AnyAsync())
            {
                logger.LogInformation("🌱 [SEED DATA] Başlangıç ürünleri (Products) veritabanına ekleniyor...");

                var defaultProducts = new List<Product>
                {
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Apple MacBook Pro 16 M3 Max",
                        Price = 115000m,
                        Stock = 10
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Logitech MX Master 3S Kablosuz Mouse",
                        Price = 4250m,
                        Stock = 45
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Keychron Q1 Pro Mekanik Klavye",
                        Price = 7800m,
                        Stock = 20
                    }
                };

                await context.Products.AddRangeAsync(defaultProducts);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Veritabanı Seed Data işlemi sırasında bir hata oluştu.");
            throw;
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}