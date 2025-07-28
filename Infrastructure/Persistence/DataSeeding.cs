using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.ProductModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System.Text.Json;

namespace Persistence
{
    public class DataSeeding(
        StoreDbContext _dbContext,
        UserManager<ApplicationUser> _userManager,
        RoleManager<IdentityRole> _roleManager,
        StoreIdentityDbContext _identityDbContext) : IDataSeeding
    {
        public async Task DataSeedAsync()
        {
            try
            {
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _dbContext.Database.MigrateAsync();
                }

                if (!_dbContext.ProductBrands.Any())
                {
                    var brandPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Persistence", "Data", "DataSeed", "brands.json");
                    using var brandStream = File.OpenRead(brandPath);
                    var brands = await JsonSerializer.DeserializeAsync<List<ProductBrand>>(brandStream);

                    if (brands is not null && brands.Any())
                    {
                        await _dbContext.ProductBrands.AddRangeAsync(brands);
                    }
                }

                if (!_dbContext.ProductTypes.Any())
                {
                    var typePath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Persistence", "Data", "DataSeed", "types.json");
                    using var typeStream = File.OpenRead(typePath);
                    var types = await JsonSerializer.DeserializeAsync<List<ProductType>>(typeStream);

                    if (types is not null && types.Any())
                    {
                        await _dbContext.ProductTypes.AddRangeAsync(types);
                    }
                }

                if (!_dbContext.Products.Any())
                {
                    var productPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Persistence", "Data", "DataSeed", "products.json");
                    using var productStream = File.OpenRead(productPath);
                    var products = await JsonSerializer.DeserializeAsync<List<Product>>(productStream);

                    if (products is not null && products.Any())
                    {
                        await _dbContext.Products.AddRangeAsync(products);
                    }
                }

                if (!_dbContext.Set<DeliveryMethod>().Any())
                {
                    var deliveryPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Persistence", "Data", "DataSeed", "delivery.json");
                    using var deliveryStream = File.OpenRead(deliveryPath);
                    var deliveryMethods = await JsonSerializer.DeserializeAsync<List<DeliveryMethod>>(deliveryStream);

                    if (deliveryMethods is not null && deliveryMethods.Any())
                    {
                        await _dbContext.Set<DeliveryMethod>().AddRangeAsync(deliveryMethods);
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataSeeding Error - DataSeedAsync]: {ex.Message}");
                throw;
            }
        }

        public async Task IdentityDataSeedAsync()
        {
            try
            {
                if (!_roleManager.Roles.Any())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                }

                if (!_userManager.Users.Any())
                {
                    var user1 = new ApplicationUser
                    {
                        Email = "Mohamed@gmail.com",
                        DisplayName = "Mohamed Kayyali",
                        PhoneNumber = "01101244495",
                        UserName = "MohamedKayyali"
                    };

                    var user2 = new ApplicationUser
                    {
                        Email = "Nadine@gmail.com",
                        DisplayName = "Nadine Kayyali",
                        PhoneNumber = "01101244475",
                        UserName = "NadineKayyali"
                    };

                    var result1 = await _userManager.CreateAsync(user1, "Kayyali1!");
                    var result2 = await _userManager.CreateAsync(user2, "Kayyali1!");

                    if (result1.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user1, "SuperAdmin");
                        await _userManager.AddToRoleAsync(user1, "Admin");
                    }

                    if (result2.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user2, "Admin");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataSeeding Error - IdentityDataSeedAsync]: {ex.Message}");
                throw;
            }
        }
    }
}
