using Microsoft.EntityFrameworkCore;
using NLayerShop.Data.Context;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Data.Seed;

/// <summary>
/// Applies pending migrations and inserts demo data for local development.
/// Idempotent: data is only inserted when the database has no categories and no products
/// (soft-deleted rows included), so restarts never duplicate the seed.
/// </summary>
public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(AppDbContext context, CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        var hasData = await context.Categories.IgnoreQueryFilters().AnyAsync(ct)
                      || await context.Products.IgnoreQueryFilters().AnyAsync(ct);
        if (hasData)
            return;

        var electronics = new Category { Name = "Electronics" };
        var books = new Category { Name = "Books" };
        var home = new Category { Name = "Home & Kitchen" };
        var sports = new Category { Name = "Sports & Outdoors" };

        var products = new List<Product>
        {
            // Electronics
            Product("Wireless Mouse", "Ergonomic 2.4 GHz wireless mouse with silent clicks.", 24.99m, 150, electronics),
            Product("Mechanical Keyboard", "Tenkeyless mechanical keyboard with brown switches.", 89.90m, 40, electronics),
            Product("27\" 4K Monitor", "IPS panel, 60 Hz, USB-C with 65 W power delivery.", 349.00m, 12, electronics),
            Product("Noise Cancelling Headphones", "Over-ear Bluetooth headphones, 30 h battery life.", 199.50m, 0, electronics),
            Product("USB-C Hub 7-in-1", "HDMI, 3x USB-A, SD/microSD and 100 W pass-through.", 39.95m, 85, electronics),
            Product("Portable SSD 1TB", "USB 3.2 Gen 2 external SSD, up to 1050 MB/s.", 109.00m, 0, electronics),

            // Books
            Product("Clean Code", "A handbook of agile software craftsmanship.", 37.50m, 60, books),
            Product("Domain-Driven Design", "Tackling complexity in the heart of software.", 54.99m, 18, books),
            Product("The Pragmatic Programmer", "20th anniversary edition.", 42.00m, 0, books),
            Product("Designing Data-Intensive Applications", "Reliable, scalable and maintainable systems.", 49.90m, 25, books),
            Product("Refactoring", "Improving the design of existing code, 2nd edition.", 46.75m, 7, books),

            // Home & Kitchen
            Product("Espresso Machine", "15-bar pump espresso machine with milk frother.", 249.00m, 9, home),
            Product("Chef's Knife 8\"", "High-carbon stainless steel chef's knife.", 59.90m, 33, home),
            Product("Cast Iron Skillet", "Pre-seasoned 12-inch cast iron skillet.", 34.99m, 0, home),
            Product("Electric Kettle", "1.7 L stainless steel kettle with auto shut-off.", 29.99m, 120, home),
            Product("Air Fryer", "5.5 L digital air fryer with 8 presets.", 119.00m, 14, home),

            // Sports & Outdoors
            Product("Yoga Mat", "6 mm non-slip TPE yoga mat with carry strap.", 22.50m, 200, sports),
            Product("Adjustable Dumbbells", "Pair of 2-24 kg quick-adjust dumbbells.", 299.99m, 4, sports),
            Product("Running Shoes", "Lightweight neutral running shoes.", 129.00m, 0, sports),
            Product("Camping Tent 2P", "Waterproof two-person dome tent.", 159.00m, 11, sports),
            Product("Insulated Water Bottle", "750 ml double-wall stainless steel bottle.", 19.95m, 95, sports),
        };

        context.Categories.AddRange(electronics, books, home, sports);
        context.Products.AddRange(products);
        await context.SaveChangesAsync(ct);
    }

    private static Product Product(string name, string description, decimal price, int stock, Category category)
        => new()
        {
            Name = name,
            Description = description,
            Price = price,
            StockCount = stock,
            Category = category
        };
}
