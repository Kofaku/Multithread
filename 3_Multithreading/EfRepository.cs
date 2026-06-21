using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3_Multithreading
{
    public class EfRepository
    {
        private readonly AppDbContext _context;

        public EfRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Manufacturer>> GetAllManufacturersAsync()
        {
            return await _context.Manufacturers.ToListAsync();
        }

        public async Task<Manufacturer> GetManufacturerByIdAsync(int id)
        {
            return await _context.Manufacturers.FindAsync(id);
        }

        public async Task<int> AddManufacturerAsync(Manufacturer manufacturer)
        {
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();
            return manufacturer.Id;
        }

        public async Task UpdateManufacturerAsync(Manufacturer manufacturer)
        {
            _context.Entry(manufacturer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteManufacturerAsync(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer != null)
            {
                _context.Manufacturers.Remove(manufacturer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ManufacturerExistsAsync(int id)
        {
            return await _context.Manufacturers.AnyAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Phone>> GetAllPhonesAsync()
        {
            return await _context.Phones.Include(p => p.Manufacturer).ToListAsync();
        }

        public async Task<Phone> GetPhoneByIdAsync(int id)
        {
            return await _context.Phones.Include(p => p.Manufacturer)
                                         .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> AddPhoneAsync(Phone phone)
        {
            _context.Phones.Add(phone);
            await _context.SaveChangesAsync();
            return phone.Id;
        }

        public async Task UpdatePhoneAsync(Phone phone)
        {
            _context.Entry(phone).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePhoneAsync(int id)
        {
            var phone = await _context.Phones.FindAsync(id);
            if (phone != null)
            {
                _context.Phones.Remove(phone);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Phone>> GetPhonesByManufacturerIdAsync(int manufacturerId)
        {
            return await _context.Phones
                                 .Where(p => p.ManufacturerId == manufacturerId)
                                 .Include(p => p.Manufacturer)
                                 .ToListAsync();
        }

        public async Task AddProductWithNewManufacturerAsync(string manufacturerName, string country,
                                                             string model, decimal price)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var manufacturer = new Manufacturer { Name = manufacturerName, Country = country };
                _context.Manufacturers.Add(manufacturer);
                await _context.SaveChangesAsync();

                var phone = new Phone
                {
                    Model = model,
                    Price = price,
                    ManufacturerId = manufacturer.Id
                };
                _context.Phones.Add(phone);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}