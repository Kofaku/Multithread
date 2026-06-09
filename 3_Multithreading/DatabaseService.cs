using Microsoft.Data.Sqlite;
using System.Data;

namespace _3_Multithreading
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string databasePath = "phones.db")
        {
            _connectionString = $"Data Source={databasePath}";
        }

        public async Task CreateTablesAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            DROP TABLE IF EXISTS Phone;
            DROP TABLE IF EXISTS Manufacturer;

            CREATE TABLE Manufacturer (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Country TEXT NOT NULL
            );

            CREATE TABLE Phone (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Model TEXT NOT NULL,
                Price REAL NOT NULL,
                ManufacturerId INTEGER NOT NULL,
                FOREIGN KEY (ManufacturerId) REFERENCES Manufacturer(Id) ON DELETE CASCADE
            );
        ";
            await command.ExecuteNonQueryAsync();
        }

        public async Task FillDataAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            for (int i = 1; i <= 30; i++)
            {
                var insertManufacturer = connection.CreateCommand();
                insertManufacturer.CommandText = @"
                INSERT INTO Manufacturer (Name, Country)
                VALUES (@name, @country);
                SELECT last_insert_rowid();
            ";
                insertManufacturer.Parameters.AddWithValue("@name", $"Manufacturer_{i}");
                insertManufacturer.Parameters.AddWithValue("@country", i % 2 == 0 ? "China" : "USA");

                var manufacturerId = Convert.ToInt32(await insertManufacturer.ExecuteScalarAsync());

                var insertPhone = connection.CreateCommand();
                insertPhone.CommandText = @"
                INSERT INTO Phone (Model, Price, ManufacturerId)
                VALUES (@model, @price, @manId);
            ";
                insertPhone.Parameters.AddWithValue("@model", $"Phone_{i}");
                insertPhone.Parameters.AddWithValue("@price", 200 + i * 10);
                insertPhone.Parameters.AddWithValue("@manId", manufacturerId);

                await insertPhone.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> AddManufacturerAsync(string name, string country)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO Manufacturer (Name, Country)
            VALUES (@name, @country);
            SELECT last_insert_rowid();
        ";
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@country", country);

            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task AddPhoneAsync(string model, decimal price, int manufacturerId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO Phone (Model, Price, ManufacturerId)
            VALUES (@model, @price, @manId);
        ";
            command.Parameters.AddWithValue("@model", model);
            command.Parameters.AddWithValue("@price", price);
            command.Parameters.AddWithValue("@manId", manufacturerId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<Phone>> GetPhonesByManufacturerAsync(int manufacturerId)
        {
            var phones = new List<Phone>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, Model, Price, ManufacturerId
            FROM Phone
            WHERE ManufacturerId = @manId;
        ";
            command.Parameters.AddWithValue("@manId", manufacturerId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                phones.Add(new Phone
                {
                    Id = reader.GetInt32(0),
                    Model = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    ManufacturerId = reader.GetInt32(3)
                });
            }

            return phones;
        }
        public async Task<bool> ManufacturerExistsAsync(int manufacturerId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Manufacturer WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", manufacturerId);
            long count = (long)await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }
}
