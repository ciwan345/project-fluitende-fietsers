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

namespace DeFluitendeFietser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>



    public partial class RekenmachineWindow : UserControl
    {
        private string som = "";
        private string invoer = "";
        private bool laatsteWasOperator = false;

        public RekenmachineWindow()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            string knop = (sender as Button).Content.ToString();

            switch (knop)
            {
                case "C":
                    WisAlles();
                    break;

                case "«":
                    Undo();
                    break;

                case "+/-":
                    Negatief();
                    break;

                case ",":
                    VoegKommaToe();
                    break;

                case "π":
                    VoegPiToe();
                    break;

                case "=":
                    Bereken();
                    break;

                case "+":
                case "-":
                case "*":
                case "/":
                    VoegOperatorToe(knop);
                    break;

                default:
                    VoegCijferToe(knop);
                    break;
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
            if (som.Length == 0)
                return;

            if (invoer.Length > 0)
            {
                som = som.Substring(0, som.Length - 1);
                invoer = invoer.Substring(0, invoer.Length - 1);
            }
            else if (laatsteWasOperator)
            {
                som = som.TrimEnd();

                if (som.Length > 0)
                    som = som.Substring(0, som.Length - 1);

                som = som.TrimEnd();
                invoer = som.Substring(som.LastIndexOf(' ') + 1);
                laatsteWasOperator = false;
            }
            else
            {
                som = som.Substring(0, som.Length - 1);
            }

            SomTekst.Text = som;
        }

        private void VoegCijferToe(string cijfer)
        {
            if (cijfer.Length == 0 || !char.IsDigit(cijfer[0]))
                return;

            som += cijfer;
            invoer += cijfer;
            laatsteWasOperator = false;

            SomTekst.Text = som;
        }

        private void VoegKommaToe()
        {
            if (invoer.Contains(","))
                return;

            if (invoer == "" || invoer == "-")
            {
                som += "0,";
                invoer += "0,";
            }
            else
            {
                som += ",";
                invoer += ",";
            }

            SomTekst.Text = som;
        }

        private void VoegPiToe()
        {
            string pi = Math.PI.ToString(
                "0.############",
                CultureInfo.InvariantCulture
            );

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
            else if (invoer == "-")
            {
                som = som.Substring(0, som.Length - 1);
                invoer = "";
            }
            else if (invoer.StartsWith("-"))
            {
                som = som.Substring(0, som.Length - invoer.Length);
                invoer = invoer.Substring(1);
                som += invoer;
            }
            else
            {
                som = som.Substring(0, som.Length - invoer.Length);
                invoer = "-" + invoer;
                som += invoer;
            }

            SomTekst.Text = som;
        }

        private void VoegOperatorToe(string op)
        {
            if (op == "-" && (som == "" || laatsteWasOperator))
            {
                som += "-";
                invoer = "-";
                laatsteWasOperator = false;

                SomTekst.Text = som;
                return;
            }

            if (som == "" || laatsteWasOperator || invoer == "-")
                return;

            som += " " + op + " ";
            invoer = "";
            laatsteWasOperator = true;

            SomTekst.Text = som;
        }

        private List<string> onderdelen = new List<string>();
        private int positie;

        private double LeesGetal()
        {
            if (positie >= onderdelen.Count)
                throw new Exception();

            string getal = onderdelen[positie];
            positie++;

            return double.Parse(
                getal,
                CultureInfo.InvariantCulture
            );
        }

        private double LeesTerm()
        {
            double resultaat = LeesFactor();

            while (positie < onderdelen.Count)
            {
                string op = onderdelen[positie];

                if (op != "*" && op != "/")
                    break;

                positie++;

                double getal = LeesFactor();

                if (op == "*")
                    resultaat *= getal;
                else
                    resultaat /= getal;
            }

            return resultaat;
        }

        private double LeesFactor()
        {
            if (positie < onderdelen.Count && onderdelen[positie] == "-")
            {
                positie++;
                return -LeesFactor();
            }

            if (positie < onderdelen.Count && onderdelen[positie] == "+")
            {
                positie++;
                return LeesFactor();
            }

            return LeesGetal();
        }

        private double LeesSom()
        {
            double resultaat = LeesTerm();

            while (positie < onderdelen.Count)
            {
                string op = onderdelen[positie];

                if (op != "+" && op != "-")
                    break;

                positie++;

                double getal = LeesTerm();

                if (op == "+")
                    resultaat += getal;
                else
                    resultaat -= getal;
            }

            return resultaat;
        }

        private void Bereken()
        {
            try
            {
                string tekst = som.Replace(",", ".");

                onderdelen = new List<string>(
                    tekst.Split(
                        new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                );

                positie = 0;

                double resultaat = LeesSom();

                if (positie != onderdelen.Count ||
                    double.IsInfinity(resultaat) ||
                    double.IsNaN(resultaat))
                {
                    throw new Exception();
                }

                string antwoord = resultaat.ToString(
                    "G",
                    CultureInfo.GetCultureInfo("nl-NL")
                );

                SomTekst.Text = som + " = " + antwoord;
                AntwoordTekst.Text = antwoord;

                som = resultaat.ToString(
                    CultureInfo.InvariantCulture
                );

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





