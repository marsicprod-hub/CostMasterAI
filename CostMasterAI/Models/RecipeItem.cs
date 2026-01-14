using System.ComponentModel.DataAnnotations;

namespace CostMasterAI.Models
{
    public class RecipeItem
    {
        [Key]
        public int Id { get; set; }

        // Link ke Resep
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        // Link ke Bahan Baku
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        // Berapa banyak bahan yang dipake? Misal: 200 (Gram)
        public double UsageQty { get; set; }

        // Hitung harga pemakaian (Harga/Unit x Jumlah Pakai)
        public decimal CalculatedCost => Ingredient != null
            ? Ingredient.PricePerUnit * (decimal)UsageQty
            : 0;
    }
}