using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAll()
        {
            return _productRepository.GetAll();
        }

        public Product Add(Product item)
        {
            // Product toevoegen via repository
            return _productRepository.Add(item);
        }

        public Product? Delete(Product item)
        {
            // Product verwijderen via repository
            return _productRepository.Delete(item);
        }

        public Product? Get(int id)
        {
            // Product ophalen via repository
            return _productRepository.Get(id);
        }

        public Product? Update(Product item)
        {
            return _productRepository.Update(item);
        }
    }
}
