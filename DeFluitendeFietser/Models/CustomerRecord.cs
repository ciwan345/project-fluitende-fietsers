using System;
using System.Globalization;

namespace DeFluitendeFietser.Models
{
    public class CustomerRecord
    {
        public int Number { get; set; }
        public DateTime CompletedAt { get; set; }
        public decimal Total { get; set; }
        public string ProductsText { get; set; } = "";
        public string CustomerName => "Klant " + Number;
        public string TimeText => CompletedAt.ToString("HH:mm");
        public string TotalText => Total.ToString("C2", CultureInfo.GetCultureInfo("nl-NL"));
    }
}
