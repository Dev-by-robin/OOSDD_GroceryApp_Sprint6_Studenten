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
            LoadProducts();
            client = global.Client;
        }

        private void LoadProducts()
        {
            Products.Clear();
            foreach (Product p in _productService.GetAll()) Products.Add(p);
        }

        // Deze methode wordt aangeroepen wanneer de pagina verschijnt
        public void OnAppearing()
        {
            LoadProducts();
        }

        [RelayCommand]
        public async Task ShowNewProduct()
        {
            if (Client.Role == Role.Admin)
            {
                // Navigeren naar de nieuwe productpagina
                await Shell.Current.GoToAsync(nameof(NewProductView));
            }
            else
            {
                await Shell.Current.DisplayAlert("Toegang geweigerd", "Je hebt geen toestemming om een nieuw product toe te voegen.", "OK");
            }
        }
    }
}
