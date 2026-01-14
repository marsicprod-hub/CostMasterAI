using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CostMasterAI.Models;
using CostMasterAI.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CostMasterAI.ViewModels
{
    public partial class IngredientsViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        // ObservableCollection buat nampung data tabel
        public ObservableCollection<Ingredient> Ingredients { get; } = new();

        // --- INI PERUBAHANNYA ---
        // Kita pake Field (huruf kecil ada underscore)
        // [ObservableProperty] otomatis bikinin Property publiknya (NewName, NewPrice, dll)

        [ObservableProperty]
        private string _newName = string.Empty;

        [ObservableProperty]
        private string _newPrice = string.Empty;

        [ObservableProperty]
        private string _newQty = string.Empty;

        [ObservableProperty]
        private string _newUnit = "Gram";

        public IngredientsViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            LoadDataAsync();
        }

        public async void LoadDataAsync()
        {
            var list = await _dbContext.Ingredients.ToListAsync();
            Ingredients.Clear();
            foreach (var item in list) Ingredients.Add(item);
        }

        [RelayCommand]
        private async Task AddIngredientAsync()
        {
            // Validasi: Kalau kosong jangan diproses
            if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewPrice)) return;

            // Parsing angka
            if (decimal.TryParse(NewPrice, out var price) && double.TryParse(NewQty, out var qty))
            {
                var newItem = new Ingredient
                {
                    Name = NewName,
                    PricePerPackage = price,
                    QuantityPerPackage = qty,
                    Unit = NewUnit,
                    Category = "General"
                };

                // Masuk Database
                _dbContext.Ingredients.Add(newItem);
                await _dbContext.SaveChangesAsync();

                // Masuk UI
                Ingredients.Add(newItem);

                // Reset Form (Panggil property publiknya yang dibikin otomatis sama ObservableProperty)
                NewName = string.Empty;
                NewPrice = string.Empty;
                NewQty = string.Empty;
            }
        }

        [RelayCommand]
        private async Task DeleteIngredientAsync(Ingredient? item)
        {
            if (item == null) return;

            _dbContext.Ingredients.Remove(item);
            await _dbContext.SaveChangesAsync();
            Ingredients.Remove(item);
        }
    }
}