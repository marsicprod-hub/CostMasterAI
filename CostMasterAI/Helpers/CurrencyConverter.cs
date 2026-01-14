using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace CostMasterAI.Helpers
{
    // Tugas: Ubah angka decimal jadi format Rupiah
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal d)
            {
                return d.ToString("C0", new CultureInfo("id-ID")); // Format Rp Indonesia
            }
            if (value is double db)
            {
                return db.ToString("C0", new CultureInfo("id-ID"));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}