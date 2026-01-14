using System;
using System.ComponentModel.DataAnnotations;

namespace CostMasterAI.Models
{
    // Ini cetakan untuk satu bahan baku
    public class Ingredient
    {
        // Key: ID unik buat tiap bahan (auto-generate sama database)
        [Key]
        public int Id { get; set; }

        // Nama bahan, misal: "Tepung Terigu Segitiga"
        [Required]
        public string Name { get; set; } = string.Empty;

        // Harga beli per kemasan, misal: 12000
        public decimal PricePerPackage { get; set; }

        // Jumlah isi per kemasan, misal: 1000 (untuk 1kg)
        public double QuantityPerPackage { get; set; }

        // Satuan, misal: "Gram", "ML", "Pcs"
        public string Unit { get; set; } = "Gram";

        // Fitur AI nanti: Kategori otomatis (misal: "Dairy", "Spice")
        public string Category { get; set; } = "General";

        // Helper buat hitung harga per satuan terkecil (Price / Quantity)
        // Ini properti pintar, gak disimpen di DB, tapi dihitung on-the-fly
        public decimal PricePerUnit => QuantityPerPackage > 0
            ? PricePerPackage / (decimal)QuantityPerPackage
            : 0;
    }
}