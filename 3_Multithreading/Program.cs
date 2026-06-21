using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Multithreading
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using var context = new AppDbContext();
            await context.Database.EnsureCreatedAsync();
            await DataInitializer.SeedAsync(context);

            var repo = new EfRepository(context);

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("1. Производители (CRUD)");
                Console.WriteLine("2. Телефоны (CRUD)");
                Console.WriteLine("3. Добавить продукт с новым производителем (бизнес-операция)");
                Console.WriteLine("4. Выход");
                Console.Write("Выберите действие: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ManufacturerMenu(repo);
                        break;
                    case "2":
                        await PhoneMenu(repo);
                        break;
                    case "3":
                        await AddProductWithNewManufacturer(repo);
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неверный ввод. Нажмите Enter.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static async Task ManufacturerMenu(EfRepository repo)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("1. Показать всех");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Обновить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("5. Назад");
                Console.Write("Выберите: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ShowAllManufacturers(repo);
                        break;
                    case "2":
                        await AddManufacturer(repo);
                        break;
                    case "3":
                        await UpdateManufacturer(repo);
                        break;
                    case "4":
                        await DeleteManufacturer(repo);
                        break;
                    case "5":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Неверный ввод");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static async Task ShowAllManufacturers(EfRepository repo)
        {
            var list = await repo.GetAllManufacturersAsync();
            Console.Clear();
            Console.WriteLine("Список производителей:");
            foreach (var m in list)
                Console.WriteLine($"Id: {m.Id}, Name: {m.Name}, Country: {m.Country}");
            Console.WriteLine("\nНажмите Enter...");
            Console.ReadLine();
        }

        static async Task AddManufacturer(EfRepository repo)
        {
            Console.Clear();
            Console.Write("Название: ");
            string name = Console.ReadLine();
            Console.Write("Страна: ");
            string country = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(country))
            {
                Console.WriteLine("Поля не могут быть пустыми");
                Console.ReadLine();
                return;
            }
            var manufacturer = new Manufacturer { Name = name, Country = country };
            int id = await repo.AddManufacturerAsync(manufacturer);
            Console.WriteLine($"Производитель добавлен с Id = {id}");
            Console.ReadLine();
        }

        static async Task UpdateManufacturer(EfRepository repo)
        {
            Console.Clear();
            Console.Write("Введите Id производителя для обновления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некорректный Id");
                Console.ReadLine();
                return;
            }
            var manufacturer = await repo.GetManufacturerByIdAsync(id);
            if (manufacturer == null)
            {
                Console.WriteLine("Производитель не найден");
                Console.ReadLine();
                return;
            }
            Console.Write($"Новое название (было: {manufacturer.Name}): ");
            string name = Console.ReadLine();
            Console.Write($"Новая страна (было: {manufacturer.Country}): ");
            string country = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name))
                manufacturer.Name = name;
            if (!string.IsNullOrWhiteSpace(country))
                manufacturer.Country = country;
            await repo.UpdateManufacturerAsync(manufacturer);
            Console.WriteLine("Обновлено");
            Console.ReadLine();
        }

        static async Task DeleteManufacturer(EfRepository repo)
        {
            Console.Clear();
            Console.Write("Введите Id производителя для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некорректный Id");
                Console.ReadLine();
                return;
            }
            var manufacturer = await repo.GetManufacturerByIdAsync(id);
            if (manufacturer == null)
            {
                Console.WriteLine("Производитель не найден");
                Console.ReadLine();
                return;
            }
            Console.Write($"Удалить производителя \"{manufacturer.Name}\"? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                await repo.DeleteManufacturerAsync(id);
                Console.WriteLine("Удалено");
            }
            Console.ReadLine();
        }

        static async Task PhoneMenu(EfRepository repo)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("1. Показать все");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Обновить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("5. Найти по производителю");
                Console.WriteLine("6. Назад");
                Console.Write("Выберите: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ShowAllPhones(repo);
                        break;
                    case "2":
                        await AddPhone(repo);
                        break;
                    case "3":
                        await UpdatePhone(repo);
                        break;
                    case "4":
                        await DeletePhone(repo);
                        break;
                    case "5":
                        await ShowPhonesByManufacturer(repo);
                        break;
                    case "6":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Неверный ввод");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static async Task ShowAllPhones(EfRepository repo)
        {
            var list = await repo.GetAllPhonesAsync();
            Console.Clear();
            Console.WriteLine("Список телефонов:");
            foreach (var p in list)
                Console.WriteLine($"Id: {p.Id}, Model: {p.Model}, Price: {p.Price:C}, Manufacturer: {p.Manufacturer?.Name ?? "не указан"}");
            Console.WriteLine("\nНажмите Enter...");
            Console.ReadLine();
        }

        static async Task AddPhone(EfRepository repo)
        {
            Console.Clear();
            var manufacturers = await repo.GetAllManufacturersAsync();
            if (!manufacturers.Any())
            {
                Console.WriteLine("Сначала добавьте производителя.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Выберите производителя:");
            foreach (var m in manufacturers)
                Console.WriteLine($"Id: {m.Id}, Name: {m.Name}");
            Console.Write("Id производителя: ");
            if (!int.TryParse(Console.ReadLine(), out int manId))
            {
                Console.WriteLine("Некорректный Id");
                Console.ReadLine();
                return;
            }
            if (!await repo.ManufacturerExistsAsync(manId))
            {
                Console.WriteLine("Производитель не найден");
                Console.ReadLine();
                return;
            }
            Console.Write("Модель: ");
            string model = Console.ReadLine();
            Console.Write("Цена: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                Console.WriteLine("Некорректная цена");
                Console.ReadLine();
                return;
            }
            var phone = new Phone { Model = model, Price = price, ManufacturerId = manId };
            int id = await repo.AddPhoneAsync(phone);
            Console.WriteLine($"Телефон добавлен с Id = {id}");
            Console.ReadLine();
        }

        static async Task UpdatePhone(EfRepository repo)
        {
            Console.Clear();
            Console.Write("Введите Id телефона: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некорректный Id");
                Console.ReadLine();
                return;
            }
            var phone = await repo.GetPhoneByIdAsync(id);
            if (phone == null)
            {
                Console.WriteLine("Телефон не найден");
                Console.ReadLine();
                return;
            }
            Console.Write($"Новая модель (было: {phone.Model}): ");
            string model = Console.ReadLine();
            Console.Write($"Новая цена (было: {phone.Price}): ");
            string priceStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(model))
                phone.Model = model;
            if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, out decimal price))
                phone.Price = price;
            await repo.UpdatePhoneAsync(phone);
            Console.WriteLine("Обновлено");
            Console.ReadLine();
        }

        static async Task DeletePhone(EfRepository repo)
        {
            Console.Clear();
            Console.Write("Введите Id телефона для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некорректный Id");
                Console.ReadLine();
                return;
            }
            var phone = await repo.GetPhoneByIdAsync(id);
            if (phone == null)
            {
                Console.WriteLine("Телефон не найден");
                Console.ReadLine();
                return;
            }
            Console.Write($"Удалить телефон \"{phone.Model}\"? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                await repo.DeletePhoneAsync(id);
                Console.WriteLine("Удалено");
            }
            Console.ReadLine();
        }

        static async Task ShowPhonesByManufacturer(EfRepository repo)
        {
            Console.Clear();
            var manufacturers = await repo.GetAllManufacturersAsync();
            if (!manufacturers.Any())
            {
                Console.WriteLine("Нет производителей.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Выберите производителя:");
            foreach (var m in manufacturers)
                Console.WriteLine($"Id: {m.Id}, Name: {m.Name}");
            Console.Write("Id: ");
            if (!int.TryParse(Console.ReadLine(), out int manId))
            {
                Console.WriteLine("Некорректный Id");
                Console.ReadLine();
                return;
            }
            var phones = await repo.GetPhonesByManufacturerIdAsync(manId);
            Console.Clear();
            if (!phones.Any())
                Console.WriteLine("У этого производителя нет телефонов.");
            else
            {
                Console.WriteLine($"Телефоны производителя (Id={manId}):");
                foreach (var p in phones)
                    Console.WriteLine($"Id: {p.Id}, Model: {p.Model}, Price: {p.Price:C}");
            }
            Console.WriteLine("\nНажмите Enter...");
            Console.ReadLine();
        }

        static async Task AddProductWithNewManufacturer(EfRepository repo)
        {
            Console.Clear();
            Console.Write("Название производителя: ");
            string manName = Console.ReadLine();
            Console.Write("Страна: ");
            string country = Console.ReadLine();
            Console.Write("Модель телефона: ");
            string model = Console.ReadLine();
            Console.Write("Цена: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                Console.WriteLine("Некорректная цена");
                Console.ReadLine();
                return;
            }
            try
            {
                await repo.AddProductWithNewManufacturerAsync(manName, country, model, price);
                Console.WriteLine("Продукт успешно добавлен!");
            }
            catch
            {
                Console.WriteLine("Ошибка при добавлении. Транзакция отменена.");
            }
            Console.ReadLine();
        }
    }
}