using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class GroceryListItemsService : IGroceryListItemsService
    {
        private readonly IGroceryListItemsRepository _groceriesRepository;
        private readonly IProductRepository _productRepository;

        public GroceryListItemsService(IGroceryListItemsRepository groceriesRepository, IProductRepository productRepository)
        {
            _groceriesRepository = groceriesRepository;
            _productRepository = productRepository;
        }

        public List<GroceryListItem> GetAll()
        {
            List<GroceryListItem> groceryListItems = _groceriesRepository.GetAll();
            FillService(groceryListItems);
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            List<GroceryListItem> groceryListItems = _groceriesRepository.GetAll().Where(g => g.GroceryListId == groceryListId).ToList();
            FillService(groceryListItems);
            return groceryListItems;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            return _groceriesRepository.Add(item);
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            //throw new NotImplementedException();
            return _groceriesRepository.Delete(item);
        }

        public GroceryListItem? Get(int id)
        {
            return _groceriesRepository.Get(id);
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            return _groceriesRepository.Update(item); 
        }

        private void FillService(List<GroceryListItem> groceryListItems)
        {
            foreach (GroceryListItem g in groceryListItems)
            {
                g.Product = _productRepository.Get(g.ProductId) ?? new(0, "", 0);
            }
        }

        public List<BestSellingProducts> GetBestSellingProducts(int topX = 5)
        {
            Dictionary<Product, int> productCount = [];
            GetAll().ForEach(g =>
            {
                if (productCount.ContainsKey(g.Product)) productCount[g.Product] += g.Amount;
                else productCount[g.Product] = 1;
            });
            var products = (from entry in productCount orderby entry.Value descending select entry).Take(topX);
            List<BestSellingProducts> bestProducts = [];
            int ranking = 0;
            foreach (var p in products)
            {
                ranking++;
                bestProducts.Add(new(p.Key.Id, p.Key.Name, p.Key.Stock, p.Value, ranking));
            }
            return bestProducts;
        }
    }
}
