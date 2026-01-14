using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CostMasterAI.Models;
using CostMasterAI.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace CostMasterAI.ViewModels
{
    public partial class RecipesViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;
        private readonly AIService _aiService;

        // List semua resep buat sidebar kiri
        public ObservableCollection<Recipe> Recipes { get; } = new();

        // List bahan baku buat dipilih di ComboBox
        public ObservableCollection<Ingredient> AvailableIngredients { get; } = new();

        // Resep yang lagi diedit sekarang
        [ObservableProperty]
        private Recipe? _selectedRecipe;

        // Inputan buat nambah bahan ke resep
        [ObservableProperty]
        private Ingredient? _selectedIngredientToAdd;

        [ObservableProperty]
        private string _usageQtyInput = "0";

        // Inputan buat bikin resep baru
        [ObservableProperty]
        private string _newRecipeName = "";

        [ObservableProperty]
        private bool _isAiLoading;

        public RecipesViewModel(AppDbContext dbContext, AIService aiService)
        {
            _dbContext = dbContext;
            _aiService = aiService;
            LoadDataAsync();
        }

        public async void LoadDataAsync()
        {
            // Load Bahan Baku buat dropdown
            var ingredients = await _dbContext.Ingredients.ToListAsync();
            AvailableIngredients.Clear();
            foreach (var i in ingredients) AvailableIngredients.Add(i);

            // Load Resep lengkap sama Item-itemnya (Include)
            var recipes = await _dbContext.Recipes
                .Include(r => r.Items)
                .ThenInclude(i => i.Ingredient)
                .ToListAsync();

            Recipes.Clear();
            foreach (var r in recipes) Recipes.Add(r);
        }

        [RelayCommand]
        private async Task CreateRecipeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewRecipeName)) return;

            var newRecipe = new Recipe { Name = NewRecipeName, YieldQty = 1 };
            _dbContext.Recipes.Add(newRecipe);
            await _dbContext.SaveChangesAsync();

            Recipes.Add(newRecipe);
            SelectedRecipe = newRecipe; // Langsung pilih resep baru
            NewRecipeName = "";
        }

        [RelayCommand]
        private async Task AddItemToRecipeAsync()
        {
            if (SelectedRecipe == null || SelectedIngredientToAdd == null) return;

            if (double.TryParse(UsageQtyInput, out var qty) && qty > 0)
            {
                var newItem = new RecipeItem
                {
                    RecipeId = SelectedRecipe.Id,
                    IngredientId = SelectedIngredientToAdd.Id,
                    UsageQty = qty
                };

                _dbContext.RecipeItems.Add(newItem);
                await _dbContext.SaveChangesAsync();

                // Kita reload resep biar angkanya update (cara gampang)
                await ReloadSelectedRecipe();

                // Reset input
                UsageQtyInput = "0";
                SelectedIngredientToAdd = null;
            }
        }

        [RelayCommand]
        private async Task RemoveItemFromRecipeAsync(RecipeItem? item)
        {
            if (item == null) return;
            _dbContext.RecipeItems.Remove(item);
            await _dbContext.SaveChangesAsync();
            await ReloadSelectedRecipe();
        }

        [RelayCommand]
        private async Task GenerateDescriptionAsync()
        {
            if (SelectedRecipe == null) return;

            IsAiLoading = true;

            // 1. Kumpulin nama bahan jadi satu kalimat
            var sb = new StringBuilder();
            foreach (var item in SelectedRecipe.Items)
            {
                sb.Append(item.Ingredient.Name + ", ");
            }
            var bahanList = sb.ToString().TrimEnd(',', ' ');

            // 2. Panggil AI
            var result = await _aiService.GenerateMarketingCopyAsync(SelectedRecipe.Name, bahanList);

            // 3. Update UI & Database
            SelectedRecipe.Description = result;

            // Trik biar UI sadar ada perubahan (Force Notification)
            OnPropertyChanged(nameof(SelectedRecipe));

            // Simpan ke DB
            _dbContext.Recipes.Update(SelectedRecipe);
            await _dbContext.SaveChangesAsync();

            IsAiLoading = false;
        }

        // Helper buat refresh data resep yang lagi dipilih
        private async Task ReloadSelectedRecipe()
        {
            if (SelectedRecipe == null) return;
            var id = SelectedRecipe.Id;

            // Tarik ulang dari DB biar Cost-nya kehitung ulang
            var updatedRecipe = await _dbContext.Recipes
                .Include(r => r.Items)
                .ThenInclude(i => i.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            // Ganti object lama dengan yang baru biar UI sadar ada perubahan
            var index = Recipes.IndexOf(SelectedRecipe);
            if (index != -1 && updatedRecipe != null)
            {
                Recipes[index] = updatedRecipe;
                SelectedRecipe = updatedRecipe;
            }
        }
    }
}