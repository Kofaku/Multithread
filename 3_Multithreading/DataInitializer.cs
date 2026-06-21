using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _3_Multithreading
{
    public static class DataInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Manufacturers.AnyAsync())
                return;

            var manufacturers = new List<Manufacturer>();
            for (int i = 1; i <= 30; i++)
            {
                manufacturers.Add(new Manufacturer
                {
                    Name = $"Manufacturer_{i}",
                    Country = i % 2 == 0 ? "China" : "USA"
                });
            }
            await context.Manufacturers.AddRangeAsync(manufacturers);
            await context.SaveChangesAsync();

            var phones = new List<Phone>();
            for (int i = 0; i < manufacturers.Count; i++)
            {
                phones.Add(new Phone
                {
                    Model = $"Phone_{i + 1}",
                    Price = 200 + (i + 1) * 10,
                    ManufacturerId = manufacturers[i].Id
                });
            }
            await context.Phones.AddRangeAsync(phones);
            await context.SaveChangesAsync();
        }
    }
}