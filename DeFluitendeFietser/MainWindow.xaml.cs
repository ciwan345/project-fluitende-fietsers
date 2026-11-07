using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DeFluitendeFietser.Models;
using DeFluitendeFietser.Helpers;
using DeFluitendeFietser.Views;

namespace DeFluitendeFietser
{
    public partial class MainWindow : Window
    {
        private readonly List<OrderItem> order = new();
        private readonly List<CustomerRecord> completedCustomers = new();
        private int nextCustomerNumber = 1;
        private readonly DispatcherTimer countdownTimer = new();
        private TimeSpan countdownRemaining = TimeSpan.Zero;
        private OrderItem? selectedOrderItem;
        private decimal total;
        private decimal PaymentDue => Math.Round(total * 20m, MidpointRounding.AwayFromZero) / 20m;
        private bool changingSelection;

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
            LoadCustomerHistory();
            UpdateOrder();

            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;

            UpdateCountdownDisplay();
        }

        private void LoadProducts()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prijzen_fietsen_verzekeringen_services.csv");
            if (!File.Exists(path))
            {
                MessageBox.Show("Het productbestand ontbreekt:\n" + path, "Bestand niet gevonden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (string line in File.ReadAllLines(path).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(',');
                if (parts.Length < 3 || !decimal.TryParse(parts[2].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)) continue;
                var product = new Product { Category = parts[0].Trim(), Name = parts[1].Trim(), Price = price };
                if (product.Category == "Fietsen") BikeCombo.Items.Add(product);
                else if (product.Category == "Verzekeringen") InsuranceCombo.Items.Add(product);
                else if (product.Category == "Services") ServiceCombo.Items.Add(product);
            }
        }

        private string CustomerHistoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "klanten_historie.csv");

        private void LoadCustomerHistory()
        {
            if (!File.Exists(CustomerHistoryPath)) return;
            foreach (string line in File.ReadAllLines(CustomerHistoryPath).Skip(1))
            {
                string[] parts = line.Split(';');
                if (parts.Length < 4) continue;
                if (!int.TryParse(parts[0], out int number)) continue;
                if (!DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime completedAt)) continue;
                if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)) continue;
                completedCustomers.Add(new CustomerRecord
                {
                    Number = number,
                    CompletedAt = completedAt,
                    Total = amount,
                    ProductsText = parts[3].Replace("|", ", ")
                });
            }
            nextCustomerNumber = completedCustomers.Count == 0 ? 1 : completedCustomers.Max(c => c.Number) + 1;
        }

        private void SaveCustomerRecord(CustomerRecord record)
        {
            bool newFile = !File.Exists(CustomerHistoryPath);
            using var writer = new StreamWriter(CustomerHistoryPath, append: true);
            if (newFile) writer.WriteLine("Number;CompletedAt;Total;Products");
            string safeProducts = record.ProductsText.Replace(";", ",").Replace("|", ",");
            writer.WriteLine($"{record.Number};{record.CompletedAt.ToString("O", CultureInfo.InvariantCulture)};{record.Total.ToString(CultureInfo.InvariantCulture)};{safeProducts.Replace(", ", "|")}");
        }

        private void Customers_Click(object sender, RoutedEventArgs e)
        {
            CustomerWindow window = new CustomerWindow(completedCustomers);
            window.Owner = this;
            window.ShowDialog();
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            string page = (string)((Button)sender).Tag;
            OrderPage.Visibility = page == "Order" ? Visibility.Visible : Visibility.Collapsed;
            PaymentPage.Visibility = page == "Payment" ? Visibility.Visible : Visibility.Collapsed;
            CalculatorPage.Visibility = page == "Calculator" ? Visibility.Visible : Visibility.Collapsed;
            ClockPage.Visibility = page == "Clock" ? Visibility.Visible : Visibility.Collapsed;
            PageTitle.Text = page switch { "Order" => "Bestelling", "Payment" => "Betalen", "Calculator" => "Rekenmachine", _ => "Klok" };
            PageSubtitle.Text = page switch { "Order" => "Voeg producten toe aan de kassabon", "Payment" => "Bereken betaling en wisselgeld", "Calculator" => "Reken met decimalen, percentages en haakjes", _ => "Stel een tijd in en start de afteltimer" };
            if (page == "Payment") RefreshPayment();
        }

        private void ProductCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingSelection) return;

            var currentCombo = (ComboBox)sender;
            // Bewaar de keuze in elke ComboBox onafhankelijk.
        }

        private List<Product> GetSelectedProducts()
        {
            return new[] { BikeCombo, InsuranceCombo, ServiceCombo }
                .Select(combo => combo.SelectedItem as Product)
                .Where(product => product != null)
                .Cast<Product>()
                .ToList();
        }

        private Product? GetProductForCategory(string category)
        {
            ComboBox combo = category == "Fietsen" ? BikeCombo :
                             category == "Verzekeringen" ? InsuranceCombo : ServiceCombo;
            return combo.SelectedItem as Product;
        }

        private void ClearProductSelections()
        {
            changingSelection = true;
            BikeCombo.SelectedItem = null;
            InsuranceCombo.SelectedItem = null;
            ServiceCombo.SelectedItem = null;
            changingSelection = false;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            List<Product> selectedProducts = GetSelectedProducts();
            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Selecteer minstens één product.", "Geen product", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 1 || quantity > 999)
            {
                MessageBox.Show("Vul een geldig aantal in (1 t/m 999).", "Ongeldig aantal", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityBox.Focus();
                return;
            }

            // Voeg alle gekozen categorieën toe. Bijvoorbeeld: fiets + verzekering + helm.
            foreach (Product selected in selectedProducts)
            {
                OrderItem? existing = order.FirstOrDefault(x => x.Name == selected.Name && x.Category == selected.Category);
                if (existing == null)
                    order.Add(new OrderItem { Name = selected.Name, Category = selected.Category, Quantity = quantity, UnitPrice = selected.Price });
                else
                    existing.Quantity += quantity;
            }

            ClearProductSelections();
            QuantityBox.Text = "1";
            UpdateOrder();
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (order.Count == 0)
            {
                MessageBox.Show("Er staan nog geen producten in de bestelling.", "Lege bestelling", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // De hoofdpagina toont één compacte bestelling. Kies hier welke productregel je wilt wijzigen.
            var chooseDialog = new Window { Title = "Product in bestelling kiezen", Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, Width = 390, Height = 330, ResizeMode = ResizeMode.NoResize, Background = new SolidColorBrush(Color.FromRgb(243, 245, 247)), FontFamily = new FontFamily("Segoe UI") };
            var choosePanel = new StackPanel { Margin = new Thickness(18) };
            choosePanel.Children.Add(new TextBlock { Text = "Welk product wil je wijzigen?", FontSize = 18, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(32,54,75)), Margin = new Thickness(0,0,0,12) });
            var linePicker = new ListBox { ItemsSource = order.ToList(), DisplayMemberPath = "DisplayLine", Height = 185, SelectedIndex = 0 };
            choosePanel.Children.Add(linePicker);
            var chooseButtons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,12,0,0) };
            var chooseCancel = new Button { Content = "Annuleren", MinWidth = 90, Margin = new Thickness(0,0,8,0) };
            chooseCancel.Click += (_, _) => chooseDialog.DialogResult = false;
            var chooseOk = new Button { Content = "Verder", MinWidth = 90, Background = new SolidColorBrush(Color.FromRgb(33,133,107)), Foreground = Brushes.White };
            chooseOk.Click += (_, _) => { if (linePicker.SelectedItem != null) chooseDialog.DialogResult = true; };
            chooseButtons.Children.Add(chooseCancel); chooseButtons.Children.Add(chooseOk); choosePanel.Children.Add(chooseButtons); chooseDialog.Content = choosePanel;
            if (chooseDialog.ShowDialog() != true || linePicker.SelectedItem is not OrderItem item) return;

            // Bewerken gebeurt in een apart dialoogvenster. De ComboBoxen op het
            // bestelscherm worden hierdoor niet aangepast of leeggemaakt.
            var products = BikeCombo.Items.Cast<Product>()
                .Concat(InsuranceCombo.Items.Cast<Product>())
                .Concat(ServiceCombo.Items.Cast<Product>())
                .ToList();

            var dialog = new Window
            {
                Title = "Bestelregel wijzigen",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 390,
                Background = new SolidColorBrush(Color.FromRgb(243, 245, 247)),
                FontFamily = new FontFamily("Segoe UI")
            };

            var panel = new StackPanel { Margin = new Thickness(22), Width = 350 };
            panel.Children.Add(new TextBlock { Text = "Product aanpassen", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(32, 54, 75)), Margin = new Thickness(0, 0, 0, 16) });
            panel.Children.Add(new TextBlock { Text = "Product", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            var productBox = new ComboBox { ItemsSource = products, DisplayMemberPath = "DisplayName", IsTextSearchEnabled = true, Height = 42, Padding = new Thickness(8) };
            productBox.SelectedItem = products.FirstOrDefault(p => p.Name == item.Name && p.Category == item.Category);
            panel.Children.Add(productBox);
            panel.Children.Add(new TextBlock { Text = "Aantal", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 6) });
            var quantityBox = new TextBox { Text = item.Quantity.ToString(), Height = 42 };
            panel.Children.Add(quantityBox);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            var cancelButton = new Button { Content = "Annuleren", Background = new SolidColorBrush(Color.FromRgb(233, 238, 242)), Foreground = new SolidColorBrush(Color.FromRgb(32, 54, 75)), Margin = new Thickness(0, 0, 8, 0), MinWidth = 95 };
            cancelButton.Click += (_, _) => dialog.DialogResult = false;
            var saveButton = new Button { Content = "Wijzigingen opslaan", Background = new SolidColorBrush(Color.FromRgb(33, 133, 107)), Foreground = Brushes.White, MinWidth = 145 };
            saveButton.Click += (_, _) =>
            {
                if (productBox.SelectedItem is not Product chosen)
                {
                    MessageBox.Show(dialog, "Selecteer een product.", "Product ontbreekt", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(quantityBox.Text, out int newQuantity) || newQuantity < 1 || newQuantity > 999)
                {
                    MessageBox.Show(dialog, "Vul een aantal in tussen 1 en 999.", "Ongeldig aantal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    quantityBox.Focus();
                    return;
                }
                dialog.Tag = new EditResult { Product = chosen, Quantity = newQuantity };
                dialog.DialogResult = true;
            };
            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);
            panel.Children.Add(buttons);
            dialog.Content = panel;

            if (dialog.ShowDialog() == true && dialog.Tag is EditResult result)
            {
                OrderItem? duplicate = order.FirstOrDefault(x => x != item && x.Name == result.Product.Name && x.Category == result.Product.Category);
                if (duplicate != null)
                {
                    duplicate.Quantity += result.Quantity;
                    order.Remove(item);
                }
                else
                {
                    item.Name = result.Product.Name;
                    item.Category = result.Product.Category;
                    item.UnitPrice = result.Product.Price;
                    item.Quantity = result.Quantity;
                }
                selectedOrderItem = null;
                UpdateOrder();
                MessageBox.Show("De bestelregel is bijgewerkt.", "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private sealed class EditResult
        {
            public Product Product { get; set; } = new Product();
            public int Quantity { get; set; }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is OrderItem item)
            {
                order.Remove(item);
                selectedOrderItem = null;
                UpdateOrder();
            }
        }

        private void ClearOrder_Click(object sender, RoutedEventArgs e)
        {
            if (order.Count == 0) return;
            if (MessageBox.Show("Weet je zeker dat je de hele bestelling wilt verwijderen?", "Bestelling leegmaken", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                order.Clear();
                UpdateOrder();
            }
        }

        private void UpdateOrder()
        {
            total = order.Sum(item => item.Total);
            OrderInfo.Text = order.Count == 0 ? "1 bestelling · leeg" : "1 bestelling";
            OrderSummaryDetails.Text = order.Count == 0 ? "Nog geen producten toegevoegd" : string.Join("  •  ", order.Select(item => item.Name + " (aantal: " + item.Quantity + ")"));
            OrderSummaryTotal.Text = Money(total);
            OrderTotal.Text = Money(total);
            PaymentTotal.Text = Money(PaymentDue);
            PaymentOrderList.ItemsSource = null;
            PaymentOrderList.ItemsSource = order.ToList();
            RefreshPayment();
        }

        private void GoPayment_Click(object sender, RoutedEventArgs e)
        {
            if (order.Count == 0)
            {
                MessageBox.Show("Voeg eerst een product toe aan de bestelling.", "Lege bestelling", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ReceivedBox.Text = "0,00";
            Nav_Click(new Button { Tag = "Payment" }, e);
        }

        private void BackToOrder_Click(object sender, RoutedEventArgs e) => Nav_Click(new Button { Tag = "Order" }, e);

        private void QuickCash_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;
            ReceivedBox.Text = tag == "Exact" ? PaymentDue.ToString("0.00", Nl()) : decimal.Parse(tag, CultureInfo.InvariantCulture).ToString("0.00", Nl());
        }

        private void AddCash_Click(object sender, RoutedEventArgs e)
        {
            decimal extra = decimal.Parse((string)((Button)sender).Tag, CultureInfo.InvariantCulture);
            ReceivedBox.Text = (ParseMoney(ReceivedBox.Text) + extra).ToString("0.00", Nl());
        }

        private void ClearCash_Click(object sender, RoutedEventArgs e) { ReceivedBox.Text = "0,00"; ReceivedBox.Focus(); }
        private void ReceivedBox_TextChanged(object sender, TextChangedEventArgs e) { if (IsInitialized) RefreshPayment(); }

        private void RefreshPayment()
        {
            if (ReceivedBox == null || PaymentStatus == null) return;
            decimal received = ParseMoney(ReceivedBox.Text);
            decimal difference = received - PaymentDue;
            if (difference < 0)
            {
                PaymentStatus.Text = "Nog te ontvangen";
                ChangeText.Text = Money(Math.Abs(difference));
                ChangeText.Foreground = Brushes.DarkOrange;
                ChangeBreakdown.Text = "Er ontbreekt nog " + Money(Math.Abs(difference)) + ".";
            }
            else
            {
                PaymentStatus.Text = difference == 0 ? "Gepast betaald" : "Wisselgeld terug";
                ChangeText.Text = Money(difference);
                ChangeText.Foreground = Brushes.SeaGreen;
                ChangeBreakdown.Text = difference == 0 ? "De klant betaalt gepast. Geen wisselgeld nodig." : BuildChangeBreakdown(difference);
            }
        }

        private string BuildChangeBreakdown(decimal amount)
        {
            decimal[] values = { 200m, 100m, 50m, 20m, 10m, 5m, 2m, 1m, .50m, .20m, .10m, .05m };
            string[] names = { "Biljet € 200", "Biljet € 100", "Biljet € 50", "Biljet € 20", "Biljet € 10", "Biljet € 5", "Munt € 2", "Munt € 1", "Munt € 0,50", "Munt € 0,20", "Munt € 0,10", "Munt € 0,05" };
            int cents = (int)Math.Round(amount * 100m);
            var lines = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                int value = (int)(values[i] * 100m);
                int count = cents / value;
                if (count > 0) { lines.Add(count + " × " + names[i]); cents -= count * value; }
            }
            return lines.Count == 0 ? "Geen wisselgeld." : string.Join(Environment.NewLine, lines);
        }

        private void FinishPayment_Click(object sender, RoutedEventArgs e)
        {
            decimal received = ParseMoney(ReceivedBox.Text);
            if (order.Count == 0) { MessageBox.Show("Er staat geen bestelling klaar."); return; }
            if (received < PaymentDue)
            {
                MessageBox.Show("Het ontvangen bedrag is niet voldoende. Nog nodig: " + Money(PaymentDue - received), "Betaling niet afgerond", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            decimal change = received - PaymentDue;
            var customerRecord = new CustomerRecord
            {
                Number = nextCustomerNumber++,
                CompletedAt = DateTime.Now,
                Total = PaymentDue,
                ProductsText = string.Join(", ", order.Select(item => item.Name + " (aantal: " + item.Quantity + ")"))
            };
            completedCustomers.Add(customerRecord);
            SaveCustomerRecord(customerRecord);
            MessageBox.Show("Betaling afgerond!\nTotaal: " + Money(PaymentDue) + "\nOntvangen: " + Money(received) + "\nWisselgeld: " + Money(change), "Kassabon", MessageBoxButton.OK, MessageBoxImage.Information);
            order.Clear();
            ReceivedBox.Text = "0,00";
            UpdateOrder();
            Nav_Click(new Button { Tag = "Order" }, e);
        }

        // Calculator: decimal arithmetic avoids floating-point rounding surprises.
        private void CalcButton_Click(object sender, RoutedEventArgs e)
        {
            string key = ((Button)sender).Content.ToString() ?? "";
            string current = CalcDisplay.Text == "0" || CalcDisplay.Text == "Fout" ? "" : CalcDisplay.Text;
            if (key == "C") { CalcDisplay.Text = "0"; return; }
            if (key == "CE")
            {
                int operatorAt = current.LastIndexOfAny(new[] { '+', '-', '*', '/', '(', ')' });
                int keep = operatorAt + 1;
                if (operatorAt >= 0 && current[operatorAt] == '-' && (operatorAt == 0 || "+-*/(".Contains(current[operatorAt - 1]))) keep = operatorAt;
                CalcDisplay.Text = keep > 0 ? current[..keep] : "0";
                return;
            }
            if (key == "⌫") { CalcDisplay.Text = current.Length <= 1 ? "0" : current[..^1]; return; }
            if (key == "±") { CalcDisplay.Text = ToggleLastSign(current); return; }
            if (key == "=") { Calculate(); return; }
            string symbol = key switch { "×" => "*", "÷" => "/", _ => key };
            if (symbol == ",")
            {
                string last = current.Split(new[] { '+', '-', '*', '/', '(', ')' }).Last();
                if (last.Contains(',') || last.Contains('.')) return;
                if (current.Length == 0 || "+-*/(".Contains(current[^1])) symbol = "0,";
            }
            CalcDisplay.Text = current + symbol;
        }

        private static string ToggleLastSign(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return "-";
            if (expression.EndsWith(")")) return "-(" + expression + ")";
            int start = expression.Length;
            while (start > 0 && (char.IsDigit(expression[start - 1]) || expression[start - 1] == ',' || expression[start - 1] == '.')) start--;
            if (start == expression.Length) return expression + "-";
            bool hasMinus = start > 0 && expression[start - 1] == '-' &&
                (start - 1 == 0 || "+-*/(".Contains(expression[start - 2]));
            return hasMinus ? expression.Remove(start - 1, 1) : expression.Insert(start, "-");
        }

        private void Calculate()
        {
            try
            {
                decimal result = new CalculatorParser(CalcDisplay.Text).Parse();
                CalcDisplay.Text = result.ToString("0.############################", Nl());
            }
            catch (DivideByZeroException) { MessageBox.Show("Delen door nul kan niet.", "Rekenfout", MessageBoxButton.OK, MessageBoxImage.Warning); }
            catch { MessageBox.Show("Controleer de berekening. Gebruik bijvoorbeeld 12,5 × 2 of (10 + 5) / 3.", "Ongeldige berekening", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void CopyResult_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText(CalcDisplay.Text); MessageBox.Show("Resultaat gekopieerd.", "Kopieer", MessageBoxButton.OK, MessageBoxImage.Information); }
            catch { MessageBox.Show("Kopiëren is niet gelukt."); }
        }

        private void CalcDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Calculate(); e.Handled = true; }
            else if (e.Key == Key.Escape) { CalcDisplay.Text = "0"; e.Handled = true; }
        }

        private void CountdownInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized || countdownTimer.IsEnabled) return;

            if (TimeSpan.TryParse(CountdownInput.Text, out TimeSpan tijd) && tijd > TimeSpan.Zero)
            {
                countdownRemaining = tijd;
                CountdownStatus.Text = "Tijd ingesteld";
                UpdateCountdownDisplay();
            }
            else
            {
                CountdownStatus.Text = "Gebruik uu:mm:ss";
            }
        }

        private void CountdownStart_Click(object sender, RoutedEventArgs e)
        {
            if (countdownTimer.IsEnabled)
            {
                countdownTimer.Stop();
                CountdownButton.Content = "Verder";
                CountdownStatus.Text = "Gepauzeerd";
                return;
            }

            if (countdownRemaining <= TimeSpan.Zero)
            {
                if (!TimeSpan.TryParse(CountdownInput.Text, out countdownRemaining) || countdownRemaining <= TimeSpan.Zero)
                {
                    MessageBox.Show("Vul een geldige tijd in, bijvoorbeeld 00:05:00.");
                    return;
                }
            }

            countdownTimer.Start();
            CountdownButton.Content = "Pauze";
            CountdownStatus.Text = "Timer loopt";
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            countdownRemaining -= TimeSpan.FromSeconds(1);

            if (countdownRemaining <= TimeSpan.Zero)
            {
                countdownRemaining = TimeSpan.Zero;
                countdownTimer.Stop();
                CountdownButton.Content = "Start";
                CountdownStatus.Text = "Tijd is om";
                System.Media.SystemSounds.Exclamation.Play();
                MessageBox.Show("De tijd is voorbij.");
            }

            UpdateCountdownDisplay();
        }

        private void CountdownReset_Click(object sender, RoutedEventArgs e)
        {
            countdownTimer.Stop();
            countdownRemaining = TimeSpan.Zero;
            CountdownInput.Text = "00:00:00";
            CountdownButton.Content = "Start";
            CountdownStatus.Text = "Stel een tijd in";
            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            CountdownText.Text = $"{(int)countdownRemaining.TotalHours:00}:{countdownRemaining.Minutes:00}:{countdownRemaining.Seconds:00}";
        }

        private static CultureInfo Nl() => CultureInfo.GetCultureInfo("nl-NL");
        private static decimal ParseMoney(string text)
        {
            string value = (text ?? "").Trim().Replace("€", "").Replace(" ", "");
            if (value.Contains(',') && value.Contains('.')) value = value.Replace(".", "").Replace(',', '.');
            else value = value.Replace(',', '.');
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) && amount >= 0 ? amount : 0m;
        }
        private static string Money(decimal amount) => amount.ToString("C2", Nl());
    }


}
