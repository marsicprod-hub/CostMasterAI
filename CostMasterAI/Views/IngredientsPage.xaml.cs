using CostMasterAI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection; // Wajib ada

namespace CostMasterAI.Views
{
    public sealed partial class IngredientsPage : Page
    {
        public IngredientsViewModel ViewModel { get; }

        public IngredientsPage()
        {
            this.InitializeComponent();
            // Ambil ViewModel dari Service
            ViewModel = App.Current.Services.GetService<IngredientsViewModel>();
            this.DataContext = ViewModel;

            // Trik buat akses elemen parent di DataGrid (buat tombol delete)
            this.Name = "ParentPage";
        }
    }
}