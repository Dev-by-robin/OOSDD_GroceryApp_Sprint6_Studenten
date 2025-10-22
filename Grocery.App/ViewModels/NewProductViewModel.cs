using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly GlobalViewModel _global;

        [ObservableProperty]
        private Client client;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int stock;

        [ObservableProperty]
        private DateTime shelfLifeDateTime = DateTime.Today;

        [ObservableProperty]
        private DateTime minimumDate = DateTime.Today;

        [ObservableProperty]
        private decimal price;

        public NewProductViewModel(IProductService productService, GlobalViewModel global)
        {
            _productService = productService;
            _global = global;
            client = global.Client;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Fout", "Voer een productnaam in.", "OK");
                return;
            }

            if (Stock < 0)
            {
                await Shell.Current.DisplayAlert("Fout", "Voorraad kan niet negatief zijn.", "OK");
                return;
            }

            if (Price <= 0)
            {
                await Shell.Current.DisplayAlert("Fout", "Voer een geldige prijs in.", "OK");
                return;
            }

            // Nieuw product aanmaken

            //ID wordt automatisch gegenereerd
            var product = new Product(0, Name, Stock, DateOnly.FromDateTime(ShelfLifeDateTime), Price);

            _productService.Add(product);

            await Shell.Current.DisplayAlert("Succes", "Product is toegevoegd.", "OK");
            await Shell.Current.GoToAsync(".."); // Terug naar vorige pagina
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            var confirm = await Shell.Current.DisplayAlert("Annuleren", "Weet je zeker dat je wilt annuleren?", "Ja", "Nee");
            if (confirm)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
