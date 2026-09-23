using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2Betaalsysteem
{
    public partial class MainWindow : Window
    {
        Kassa kassa = new Kassa();

    
        TextBox[] aantalVakjes;

        public MainWindow()
        {
            InitializeComponent();

            aantalVakjes = new TextBox[]
            {
                txt0, txt1, txt2, txt3, txt4, txt5,
                txt6, txt7, txt8, txt9, txt10, txt11
            };

            WerkSchermBij();
        }

        private void PlusKlik(object sender, RoutedEventArgs e)
        {
            Button knop = (Button)sender;
            int index = int.Parse(knop.Tag.ToString());

            kassa.Verhoog(index);

            WerkSchermBij();
        }

        private void MinKlik(object sender, RoutedEventArgs e)
        {
            Button knop = (Button)sender;
            int index = int.Parse(knop.Tag.ToString());

            kassa.Verlaag(index);

            WerkSchermBij();
        }

        private void TotaalPrijsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char teken = e.Text[0];

            if (char.IsDigit(teken))
            {
                return;
            }

            if (teken == ',' && !TotaalPrijsTextBox.Text.Contains(","))
            {
                return;
            }

            // Alles anders (letters, tekens, tweede komma) wordt geblokkeerd
            e.Handled = true;
        }

        private void TotaalPrijsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TotaalPrijsTextBox.Text == "")
            {
                TotaalPrijsTextBox.Text = "0";
            }
        }

        private void TotaalPrijsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            WerkSchermBij();
        }

        // Werkt alle tekst op het scherm bij: het aantal per rij, het totaal
        // gegeven bedrag, het retour (of nog te betalen) bedrag en de
        // uitsplitsing van het retour in biljetten/munten.
        private void WerkSchermBij()
        {
            if (aantalVakjes == null)
            {
                return;
            }

            // Toon in elk vakje hoeveel de klant van dat biljet/munt heeft gegeven
            for (int i = 0; i < aantalVakjes.Length; i++)
            {
                aantalVakjes[i].Text = kassa.GetAantal(i).ToString();
            }

            decimal totaalGegeven = kassa.BerekenTotaalGegeven();
            TotalGivenRun.Text = "€ " + totaalGegeven.ToString("F2").Replace(".", ",");

            string prijsTekst = TotaalPrijsTextBox.Text.Replace(",", ".");
            decimal teBetalen;
            decimal.TryParse(prijsTekst, out teBetalen);

            decimal verschil = totaalGegeven - teBetalen;

            RetourListBox.Items.Clear();

            if (verschil >= 0)
            {
                decimal verschilAfgerond = kassa.RondAfOp5Cent(verschil);

                RetourLabelRun.Text = "Totaal retour: ";
                TotalReturnRun.Text = "€ " + verschilAfgerond.ToString("F2").Replace(".", ",");

                List<string> retourLijst = kassa.BerekenRetour(verschilAfgerond);

                for (int i = 0; i < retourLijst.Count; i++)
                {
                    RetourListBox.Items.Add(retourLijst[i]);
                }
            }
            else
            {
                RetourLabelRun.Text = "Nog te betalen: ";
                TotalReturnRun.Text = "€ " + Math.Abs(verschil).ToString("F2").Replace(".", ",");
            }
        }


        // Deze klasse bevat alle rekenlogica van de kassa: welke biljetten en
        // munten er zijn, hoeveel de klant er van elk gegeven heeft, en hoe
        // het wisselgeld berekend wordt. Zo blijft MainWindow zelf alleen
        // verantwoordelijk voor het bijwerken van het scherm.
        public class Kassa
        {
            // De waarde van elk biljet/munt, van hoog naar laag.
            // Index 0 t/m 5  = biljetten (200, 100, 50, 20, 10, 5)
            // Index 6 t/m 11 = munten    (2, 1, 0.50, 0.20, 0.10, 0.05)
            // Geen 1- en 2-centmunten: in Nederland wordt contant geld
            // afgerond op 5 cent, dus die munten zijn niet nodig.
            private decimal[] waardes = new decimal[]
            {
                200m, 100m, 50m, 20m, 10m, 5m,
                2m, 1m, 0.50m, 0.20m, 0.10m, 0.05m
            };

            // Hoeveel de klant van elk biljet/munt gegeven heeft
            private int[] aantallen = new int[12];

            public int GetAantal(int index)
            {
                return aantallen[index];
            }

            public void Verhoog(int index)
            {
                aantallen[index] = aantallen[index] + 1;
            }

            public void Verlaag(int index)
            {
                if (aantallen[index] > 0)
                {
                    aantallen[index] = aantallen[index] - 1;
                }
            }

            // Telt op wat de klant in totaal gegeven heeft
            public decimal BerekenTotaalGegeven()
            {
                decimal totaal = 0;

                for (int i = 0; i < waardes.Length; i++)
                {
                    totaal = totaal + waardes[i] * aantallen[i];
                }

                return totaal;
            }

            // Rondt een bedrag af naar het dichtstbijzijnde veelvoud van 5 cent
            // (de gebruikelijke afronding bij contant betalen in Nederland).
            public decimal RondAfOp5Cent(decimal bedrag)
            {
                int centen = (int)Math.Round(bedrag * 100);
                int rest = centen % 5;

                if (rest >= 3)
                {
                    centen = centen + (5 - rest);
                }
                else
                {
                    centen = centen - rest;
                }

                return centen / 100m;
            }

            // Berekent met welke biljetten en munten het bedrag teruggegeven
            // wordt, met zo min mogelijk stuks (steeds het grootste biljet of
            // munt passen dat nog in het resterende bedrag past).
            public List<string> BerekenRetour(decimal bedrag)
            {
                List<string> lijst = new List<string>();

                int resterend = (int)Math.Round(bedrag * 100);

                for (int i = 0; i < waardes.Length; i++)
                {
                    int waardeInCenten = (int)Math.Round(waardes[i] * 100);
                    int aantal = resterend / waardeInCenten;

                    if (aantal > 0)
                    {
                        resterend = resterend - aantal * waardeInCenten;

                        string waardeTekst = "€ " + waardes[i].ToString("F2").Replace(".", ",");
                        lijst.Add(waardeTekst + "   x " + aantal);
                    }
                }

                return lijst;
            }
        }
    }
}