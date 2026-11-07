using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DeFluitendeFietser.Models;

namespace DeFluitendeFietser.Views
{
    public partial class CustomerWindow : Window
    {
        public CustomerWindow(List<CustomerRecord> customers)
        {
            InitializeComponent();

            CustomerList.ItemsSource = customers;

            CustomerCountText.Text = customers.Count.ToString();
            decimal total = customers.Sum(customer => customer.Total);
            RevenueText.Text = total.ToString("C2",
                System.Globalization.CultureInfo.GetCultureInfo("nl-NL"));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
