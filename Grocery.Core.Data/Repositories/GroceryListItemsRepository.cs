using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = [];
        private readonly IProductRepository _productRepository;

        public GroceryListItemsRepository(IProductRepository productRepository)
        {
            _productRepository = productRepository;

            // Foreign keys uit zodat tabel aangemaakt kan worden zonder dat andere tabellen al bestaan
            OpenConnection();
            using (SqliteCommand command = new("PRAGMA foreign_keys = OFF;", Connection))
            {
                command.ExecuteNonQuery();
            }
            CloseConnection();
            // Table maken voor GroceryListItems (zonder foreign key naar Product)
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL,
                            FOREIGN KEY(GroceryListId) REFERENCES GroceryList(Id),
                            UNIQUE(GroceryListId, ProductId))");

            // Haal producten op uit ProductRepository
            List<Product> allProducts = _productRepository.GetAll();

            var melk = allProducts.FirstOrDefault(p => p.Name == "Melk");
            var kaas = allProducts.FirstOrDefault(p => p.Name == "Kaas");
            var brood = allProducts.FirstOrDefault(p => p.Name == "Brood");

            if (melk != null && kaas != null && brood != null)
            {
                List<string> insertQueries = [
                    $"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, {melk.Id}, 3)",
                    $"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, {kaas.Id}, 1)",
                    $"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, {brood.Id}, 4)",
                    $"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, {melk.Id}, 2)",
                    $"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, {kaas.Id}, 5)"];

                InsertMultipleWithTransaction(insertQueries);
            } 
            else
            {
                Console.WriteLine("(Melk, Kaas of Brood) ontbreekt in de Product tabel.");
            }
            //GetAll();
        }

        public List<GroceryListItem> GetAll()
        {
            // Alle items ophalen
            groceryListItems.Clear();
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    //Console.WriteLine($"Geladen: Id={id}, ListId={groceryListId}, ProductId={productId}, Amount={amount}");
                    groceryListItems.Add(new(id, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            //Console.WriteLine($"Totaal geladen: {groceryListItems.Count} items");
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            // Alle items ophalen die op een specifieke boodschappenlijst staan
            List<GroceryListItem> itemsOnList = [];

            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE GroceryListId = @GroceryListId";

            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", id);
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int itemId = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    itemsOnList.Add(new(itemId, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            return itemsOnList;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            // Nieuw item toevoegen
            string insertQuery = $"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(@GroceryListId, @ProductId, @Amount) Returning RowId;";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);
                // Debugging
                try
                {
                    item.Id = Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                CloseConnection();
                return item;
            }
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            // Bestaand item verwijderen
            string deleteQuery = "DELETE FROM GroceryListItem WHERE Id = @Id";
            OpenConnection();
            using (SqliteCommand command = new(deleteQuery, Connection))
            {
                command.Parameters.AddWithValue("Id", item.Id);
                command.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            // Bestaand item ophalen op Id
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = @Id";
            GroceryListItem? listItem = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("Id", id);
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int itemId = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    listItem = new(itemId, groceryListId, productId, amount);
                }
            }
            CloseConnection();
            return listItem;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            // Bestaand item updaten
            int recordsAffected;

            string updateQuery = $"UPDATE GroceryListItem SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = @Id";

            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);
                command.Parameters.AddWithValue("Id", item.Id);

                recordsAffected = command.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }
    }
}