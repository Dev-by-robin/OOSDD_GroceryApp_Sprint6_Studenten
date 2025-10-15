using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = [];
        public ProductRepository()
        {

            // Product tabel maken
            CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                        [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                        [Name] TEXT NOT NULL UNIQUE,
                        [Stock] INTEGER NOT NULL,
                        [ShelfLife] DATE NOT NULL,
                        [Price] REAL NOT NULL);");
            List<string> insertQueries = [@"INSERT OR IGNORE INTO Product (Name, Stock, ShelfLife, Price) VALUES ('Melk', 300, '2025-09-25', 0.95);",
                                        @"INSERT OR IGNORE INTO Product (Name, Stock, ShelfLife, Price) VALUES ('Kaas', 100, '2025-09-30', 7.98);",
                                        @"INSERT OR IGNORE INTO Product (Name, Stock, ShelfLife, Price) VALUES ('Brood', 400, '2025-09-12', 2.19);",
                                        @"INSERT OR IGNORE INTO Product (Name, Stock, ShelfLife, Price) VALUES ('Cornflakes', 0, '2025-12-31', 1.48);"];
            InsertMultipleWithTransaction(insertQueries);
            Console.WriteLine("ProductRepository initialized.");
        }
        public List<Product> GetAll()
        {
            // Alle producten ophalen
            products.Clear();
            string selectQuery = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product";
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = selectQuery;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        int stock = reader.GetInt32(2);
                        DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                        decimal price = reader.GetDecimal(4);
                        products.Add(new Product(id, name, stock, shelfLife, price));
                    }
                }
            }
            CloseConnection();
            Console.WriteLine($"Loaded {products.Count} products from database.");
            return products;
        }

        public Product? Get(int id)
        {
            // Product ophalen op Id
            Product? product = null;
            string selectQuery = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product WHERE Id = @Id";
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = selectQuery;
                command.Parameters.AddWithValue("@Id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        int stock = reader.GetInt32(2);
                        DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                        decimal price = reader.GetDecimal(4);
                        product = new Product(productId, name, stock, shelfLife, price);
                    }
                }
            }
            CloseConnection();
            return product;
        }

        public Product Add(Product item)
        {
            // Product toevoegen
            string insertQuery = "INSERT INTO Product (Name, Stock, ShelfLife, Price) VALUES (@Name, @Stock, @ShelfLife, @Price);";
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = insertQuery;
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Stock", item.Stock);
                command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Price", item.Price);
                command.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }

        public Product? Delete(Product item)
        {
            // Product verwijderen
            string deleteQuery = "DELETE FROM Product WHERE Id = @Id;";
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = deleteQuery;
                command.Parameters.AddWithValue("@Id", item.Id);
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    CloseConnection();
                    return null; // No rows deleted, item not found
                }
            }
            CloseConnection();
            return item;
        }

        public Product? Update(Product item)
        {
            // Product bijwerken
            string updateQuery = "UPDATE Product SET Name = @Name, Stock = @Stock, ShelfLife = @ShelfLife, Price = @Price WHERE Id = @Id;";
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = updateQuery;
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Stock", item.Stock);
                command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@Id", item.Id);
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    CloseConnection();
                    return null; // Geen rijen bijgewerkt
                }
            }
            CloseConnection();
            return item;
        }
    }
}
