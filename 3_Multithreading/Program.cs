using System.Text;

namespace _3_Multithreading
{
    class Program
    {
        static async Task Main()
        {

            
                Console.OutputEncoding = Encoding.UTF8;

                var db = new DatabaseService("phones.db");

                await db.CreateTablesAsync();
                await db.FillDataAsync();

                int appleId = await db.AddManufacturerAsync("Apple", "USA");
                await db.AddPhoneAsync("iPhone 15", 999.99m, appleId);
                await db.AddPhoneAsync("iPhone 14", 799.99m, appleId);

                bool exit = false;
                while (!exit)
                {
                    Console.Clear();
                    Console.WriteLine("1. Добавить нового производителя");
                    Console.WriteLine("2. Добавить новый телефон (к существующему производителю)");
                    Console.WriteLine("3. Найти все телефоны по ID производителя");
                    Console.WriteLine("4. Выйти");
                    Console.Write("Выберите действие: ");

                    string choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                            await AddManufacturer(db);
                            break;
                        case "2":
                            await AddPhone(db);
                            break;
                        case "3":
                            await FindPhones(db);
                            break;
                        case "4":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Неверный ввод");
                            Console.ReadLine();
                            break;
                    }
                }

                
            /*Console.OutputEncoding = Encoding.UTF8;

            List<Person> people = GeneratePeople(20);
            TaskFileProcessor taskProcessor = new TaskFileProcessor();

            taskProcessor.WriteTwoFilesWithTasks(people);

            taskProcessor.ReadAndMergeWithTasks();

            await taskProcessor.ReadAndOutputAsync();

            Assignment5Runner.Run();*/
        }


            static async Task AddManufacturer(DatabaseService db)
            {
                Console.Write("Введите название производителя: ");
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Название не может быть пустым");
                    name = Console.ReadLine();
                    return;
                }

                Console.Write("Введите страну: ");
                string country = Console.ReadLine();

                int id = await db.AddManufacturerAsync(name, country);
                Console.WriteLine($"Производитель добавлен с ID = {id}");
                Console.ReadLine();
            }

            static async Task AddPhone(DatabaseService db)
            {
                Console.Write("Введите ID производителя (цифра): ");
                if (!int.TryParse(Console.ReadLine(), out int manId))
                {
                    Console.WriteLine("Некорректный ID");
                    Console.ReadLine();
                    return;
                }

                bool exists = await db.ManufacturerExistsAsync(manId);
                if (!exists)
                {
                    Console.WriteLine($"Производитель с ID {manId} не найден");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Введите модель телефона: ");
                string model = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(model))
                {
                    Console.WriteLine("Модель не может быть пустой");
                    model = Console.ReadLine();
                    return;
                }

                Console.Write("Введите цену (например, 59.99): ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    Console.WriteLine("Некорректная цена");
                    return;
                }

                await db.AddPhoneAsync(model, price, manId);
                Console.WriteLine("Телефон добавлен");
                Console.ReadLine();
            }

            static async Task FindPhones(DatabaseService db)
            {
                Console.Write("Введите ID производителя: ");
                if (!int.TryParse(Console.ReadLine(), out int manId))
                {
                    Console.WriteLine("Некорректный ID");
                    Console.ReadLine();
                    return;
                }

                var phones = await db.GetPhonesByManufacturerAsync(manId);
                if (phones.Count == 0)
                {
                    Console.WriteLine($"У производителя с ID {manId} нет телефонов или такого ID не существует.");
                }
                else
                {
                    Console.WriteLine($"Найдено телефонов: {phones.Count}");
                    foreach (var phone in phones)
                    {
                        Console.WriteLine($"{phone.Model} - {phone.Price:C}");
                    }
                }
                Console.ReadLine();
            }
        }
        /*static List<Person> GeneratePeople(int count)
        {
            var list = new List<Person>();
            for (int i = 1; i <= count; i++)
                list.Add(new Person { Id = i, Name = $"Person_{i}", Age = 20 + i % 30 });
            return list;
        }*/
    }
