using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace DeFluitendeFietser
{
    public partial class BestelpaginaWindow : Window
    {
        private DispatcherTimer timer;
        private int resterendeSeconden = 60;
        private bool bezigMetSelectie = false;

        public BestelpaginaWindow()
        {
            InitializeComponent();

            LaadProducten();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void LaadProducten()
        {
            string bestand = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "prijzen_fietsen_verzekeringen_services.csv"
);

            if (!File.Exists(bestand))
            {
                MessageBox.Show(
                    "Het prijzenbestand is niet gevonden.\n" +
                    "Controleer of het CSV-bestand in de uitvoermap staat.",
                    "Bestand niet gevonden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string[] regels = File.ReadAllLines(bestand);

            foreach (string regel in regels)
            {
                if (string.IsNullOrWhiteSpace(regel))
                {
                    continue;
                }

                string[] gegevens = regel.Split(',');

                if (gegevens.Length < 3)
                {
                    continue;
                }

                string categorie = gegevens[0].Trim();
                string naam = gegevens[1].Trim();
                string prijsTekst = gegevens[2].Trim();

                decimal prijs;

                if (!decimal.TryParse(
                    prijsTekst,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out prijs))
                {
                    continue;
                }

                Product product = new Product();

                product.Categorie = categorie;
                product.Naam = naam;
                product.PrijsPerDag = prijs;

                if (categorie.Equals(
                    "Fietsen",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbFietsen.Items.Add(product);
                }
                else if (categorie.Equals(
                    "Verzekeringen",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbVerzekeringen.Items.Add(product);
                }
                else if (categorie.Equals(
                    "Services",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbServices.Items.Add(product);
                }
            }
        }

        private void ComboBox_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (bezigMetSelectie)
            {
                return;
            }

            ResetTimer();

            ComboBox gekozenComboBox = sender as ComboBox;

            if (gekozenComboBox == null ||
                gekozenComboBox.SelectedItem == null)
            {
                return;
            }

            bezigMetSelectie = true;

            if (gekozenComboBox != cmbFietsen)
            {
                cmbFietsen.SelectedItem = null;
            }

            if (gekozenComboBox != cmbVerzekeringen)
            {
                cmbVerzekeringen.SelectedItem = null;
            }

            if (gekozenComboBox != cmbServices)
            {
                cmbServices.SelectedItem = null;
            }

            bezigMetSelectie = false;
        }

     
        private void BestelArtikel_Click(
            object sender, RoutedEventArgs e)
        {
            ResetTimer();

            Product gekozenProduct = null;

            if (cmbFietsen.SelectedItem != null)
            {
                gekozenProduct =
                    cmbFietsen.SelectedItem as Product;
            }
            else if (cmbVerzekeringen.SelectedItem != null)
            {
                gekozenProduct =
                    cmbVerzekeringen.SelectedItem as Product;
            }
            else if (cmbServices.SelectedItem != null)
            {
                gekozenProduct =
                    cmbServices.SelectedItem as Product;
            }

            if (gekozenProduct == null)
            {
                MessageBox.Show(
                    "Selecteer eerst een fiets, verzekering of service.",
                    "Geen artikel geselecteerd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            int dagen;

            if (!int.TryParse(txtDagen.Text, out dagen) ||
                dagen < 1)
            {
                MessageBox.Show(
                    "Vul een geldig aantal dagen in. Minimaal 1 dag.",
                    "Ongeldig aantal dagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Orderregel nieuweRegel = new Orderregel();

            nieuweRegel.Categorie = gekozenProduct.Categorie;
            nieuweRegel.Naam = gekozenProduct.Naam;
            nieuweRegel.PrijsPerDag = gekozenProduct.PrijsPerDag;
            nieuweRegel.Dagen = dagen;
            nieuweRegel.Totaal = gekozenProduct.PrijsPerDag * dagen;

            // Toevoegen aan de ListBox.
            lstBestelling.Items.Add(nieuweRegel);

            WerkTotaalBij();

            bezigMetSelectie = true;

            cmbFietsen.SelectedItem = null;
            cmbVerzekeringen.SelectedItem = null;
            cmbServices.SelectedItem = null;

            bezigMetSelectie = false;

            txtDagen.Text = "1";
        }

        private void WerkTotaalBij()
        {
            decimal totaal = 0;

            foreach (Orderregel regel in lstBestelling.Items)
            {
                totaal += regel.Totaal;
            }

            txtTotaal.Text = totaal.ToString(
                "C2",
                CultureInfo.GetCultureInfo("nl-NL"));
        }

        // Artikel verwijderen via de verwijderknop.
        private void VerwijderArtikel_Click(
            object sender, RoutedEventArgs e)
        {
            ResetTimer();

            Button knop = sender as Button;

            if (knop != null && knop.Tag is Orderregel)
            {
                Orderregel regel = (Orderregel)knop.Tag;
                VerwijderRegel(regel);
            }
        }

   
        private void lstBestelling_MouseDoubleClick(
            object sender, MouseButtonEventArgs e)
        {
            ResetTimer();

            Orderregel regel =
                lstBestelling.SelectedItem as Orderregel;

            if (regel != null)
            {
                VerwijderRegel(regel);
            }
        }

        private void VerwijderRegel(Orderregel regel)
        {
            MessageBoxResult antwoord = MessageBox.Show(
                "Weet je zeker dat je '" + regel.Naam +
                "' wilt verwijderen?",
                "Artikel verwijderen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (antwoord == MessageBoxResult.Yes)
            {
                lstBestelling.Items.Remove(regel);
                WerkTotaalBij();
            }
        }

        private void VolgendeKlant_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();

            if (lstBestelling.Items.Count == 0)
            {
                MessageBox.Show(
                    "Er zijn nog geen artikelen besteld.",
                    "Geen bestelling",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult antwoord = MessageBox.Show(
                "Is de bestelling betaald?",
                "Betaling controleren",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (antwoord == MessageBoxResult.Yes)
            {
                lstBestelling.Items.Clear();

                bezigMetSelectie = true;

                cmbFietsen.SelectedItem = null;
                cmbVerzekeringen.SelectedItem = null;
                cmbServices.SelectedItem = null;

                bezigMetSelectie = false;

                txtDagen.Text = "1";

                WerkTotaalBij();
            }
            else 
            {
                MessageBox.Show("Bestelling nog niet betaald");
                return;
            
            }

        }

       
        private void ResetTimer()
        {
            resterendeSeconden = 60;

            pbTimer.Value = 60;

            txtTimer.Text =
                "Applicatie sluit over 60 seconden";
        }

        
        private void Timer_Tick(object sender, EventArgs e)
        {
            resterendeSeconden--;

            pbTimer.Value = resterendeSeconden;

            txtTimer.Text =
                "Applicatie sluit over " +
                resterendeSeconden + " seconden";

            if (resterendeSeconden <= 0)
            {
                timer.Stop();

                MessageBox.Show(
                    "De applicatie wordt gesloten wegens inactiviteit.",
                    "Inactiviteit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Application.Current.Shutdown();
            }
        }
    }

  
    public class Product
    {
        public string Categorie { get; set; }
        public string Naam { get; set; }
        public decimal PrijsPerDag { get; set; }

        public string Weergave
        {
            get
            {
                return Naam + " - " +
                    PrijsPerDag.ToString(
                        "C2",
                        CultureInfo.GetCultureInfo("nl-NL")) +
                    " / dag";
            }
        }
    }

    
    public class Orderregel
    {
        public string Categorie { get; set; }
        public string Naam { get; set; }
        public decimal PrijsPerDag { get; set; }
        public int Dagen { get; set; }
        public decimal Totaal { get; set; }

        public string PrijsTekst
        {
            get
            {
                return PrijsPerDag.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("nl-NL"));
            }
        }

        public string TotaalTekst
        {
            get
            {
                return Totaal.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("nl-NL"));
            }
        }
    }
}