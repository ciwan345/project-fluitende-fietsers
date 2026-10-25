using System.Windows;
using System.Windows.Controls;

namespace DeFluitendeFietser
{
    public partial class MainWindow : Window
    {
        private Window? huidigVenster;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Bestelpagina_Openen(object sender, RoutedEventArgs e)
        {
            ToonVenster(new BestelpaginaWindow());
        }

        private void Betaalsysteem_Openen(object sender, RoutedEventArgs e)
        {
            ToonVenster(new BetaalsysteemWindow());
        }

        private void Rekenmachine_Openen(object sender, RoutedEventArgs e)
        {
            if (huidigVenster != null)
            {
                huidigVenster.Content = null;
                huidigVenster.Close();
                huidigVenster = null;
            }

            VensterInhoud.Content = new RekenmachineWindow();
            VensterViewbox.Visibility = Visibility.Visible;
            WelkomTekst.Visibility = Visibility.Collapsed;
        }

        private void Klok_Openen(object sender, RoutedEventArgs e)
        {
            ToonVenster(new KlokWindow());
        }

        private void ToonVenster(Window venster)
        {
            // Maak de inhoud los van het oorspronkelijke Window en toon die rechts.
            // De originele XAML-layouts van de vier projecten blijven behouden.
            if (huidigVenster != null)
            {
                huidigVenster.Content = null;
                huidigVenster.Close();
            }

            huidigVenster = venster;
            object? inhoud = venster.Content;
            venster.Content = null;

            if (inhoud is UIElement element)
            {
                VensterInhoud.Content = element;
                VensterViewbox.Visibility = Visibility.Visible;
                WelkomTekst.Visibility = Visibility.Collapsed;
            }
        }
    }
}
