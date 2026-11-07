using System.Globalization;

namespace DeFluitendeFietser.Models
{
    public class Product
    {
        public string Category { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string DisplayName => Name + "  -  " + Price.ToString("C2", CultureInfo.GetCultureInfo("nl-NL")) + " / stuk";
    }
}
