using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace RekenmachineWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string som = "";
        private string invoer = "";
        private bool laatsteWasOperator = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();

            switch (content)
            {
                case "C": WisAlles(); return;
                case "«": Undo(); return;
                case "+/-": Negatief(); return;
                case ",": VoegKommaToe(); return;
                case "π": VoegPiToe(); return;
                case "=": Bereken(); return;

                case "+":
                case "-":
                case "*":
                case "/":
                    VoegOperatorToe(content);
                    return;

                default:
                    VoegCijferToe(content);
                    return;
            }
        }

        private void WisAlles()
        {
            som = "";
            invoer = "";
            laatsteWasOperator = false;
            SomTekst.Text = "";
            AntwoordTekst.Text = "";
        }

        private void Undo()
        {
            if (som.Length > 0)
            {
                som = som.Substring(0, som.Length - 1);
                SomTekst.Text = som;
            }
        }

        private void VoegCijferToe(string c)
        {
            if (!char.IsDigit(c[0])) return;

            som += c;
            invoer += c;
            laatsteWasOperator = false;

            SomTekst.Text = som;
        }

        private void VoegKommaToe()
        {
            if (!invoer.Contains(","))
            {
                if (invoer == "")
                    invoer = "0,";
                else
                    invoer += ",";

                som += ",";
                SomTekst.Text = som;
            }
        }

        private void VoegPiToe()
        {
            string pi = Math.PI.ToString("0.############", CultureInfo.InvariantCulture);
            som += pi;
            invoer = pi;
            laatsteWasOperator = false;

            SomTekst.Text = som;
        }

        private void Negatief()
        {
            if (invoer == "")
            {
                som += "-";
                invoer = "-";
            }
            else
            {
                if (invoer.StartsWith("-"))
                {
                    invoer = invoer.Substring(1);
                    som = som.Substring(0, som.Length - 1);
                }
                else
                {
                    invoer = "-" + invoer;
                    som += "-";
                }
            }

            SomTekst.Text = som;
        }

        private void VoegOperatorToe(string op)
        {
            if (som == "") return;
            if (laatsteWasOperator) return;

            som += " " + op + " ";
            invoer = "";
            laatsteWasOperator = true;

            SomTekst.Text = som;
        }

        private void Bereken()
        {
            try
            {
                string raw = som.Replace(",", ".");
                double resultaat = Convert.ToDouble(new System.Data.DataTable().Compute(raw, ""));

                SomTekst.Text = $"{som} = {resultaat}";
                AntwoordTekst.Text = $"{resultaat}";

                som = resultaat.ToString(CultureInfo.InvariantCulture);
                invoer = som;
                laatsteWasOperator = false;
            }
            catch
            {
                SomTekst.Text = "Foutieve invoer";
                AntwoordTekst.Text = "";
            }
        }
    }
}





