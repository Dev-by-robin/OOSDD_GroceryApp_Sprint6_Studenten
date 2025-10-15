using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    public partial class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        public ObservableCollection<Product> Products { get; set; }
        [ObservableProperty]
        Client client;

        public ProductViewModel(IProductService productService, GlobalViewModel global)
        {
            _productService = productService;
            Products = [];
            foreach (Product p in _productService.GetAll()) Products.Add(p);
            client = global.Client;
        }

        [RelayCommand]
        public async Task ShowNewProduct()
        {
            // Navigeren naar de nieuwe productpagina
            await Shell.Current.GoToAsync(nameof(NewProductView));
        }
    }
}
