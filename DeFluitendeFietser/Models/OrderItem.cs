using System.Globalization;

namespace DeFluitendeFietser.Models
{
    public class OrderItem
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => UnitPrice * Quantity;
        public string Detail => Category + " • " + Quantity + " × " + UnitPrice.ToString("C2", CultureInfo.GetCultureInfo("nl-NL"));
        public string TotalText => Total.ToString("C2", CultureInfo.GetCultureInfo("nl-NL"));
        public string DisplayLine => Name + "  ·  " + Quantity + " × " + UnitPrice.ToString("C2", CultureInfo.GetCultureInfo("nl-NL")) + "  =  " + TotalText;
        public string PaymentLine => Name + "  |  " + Quantity + " stuks  |  " + TotalText;
    }
}
