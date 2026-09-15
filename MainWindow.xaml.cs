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


namespace de_rekenmachine_project
{
    public partial class MainWindow : Window
    {
        Calculator calc = new Calculator();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void clicknummer(object sender, RoutedEventArgs e)
        {
            Button knop = (Button)sender;
            string nummer = knop.Content.ToString();

            if (!char.IsDigit(nummer[0]))
            {
                return;
            }

            calc.AddNumber(nummer);

            resultaat.Text = calc.Input;
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            Button knop = (Button)sender;
            string operatorTekst = knop.Content.ToString();

            calc.AddOperator(operatorTekst);

            resultaat.Text = calc.Input;
        }

        private void ClickKomma(object sender, RoutedEventArgs e)
        {
            calc.AddComma();

            resultaat.Text = calc.Input;
        }

        private void Clickongedaan(object sender, RoutedEventArgs e)
        {
            calc.Undo();

            resultaat.Text = calc.Input;
        }

        private void Clickweg(object sender, RoutedEventArgs e)
        {
            calc.Clear();

            resultaat.Text = "";
            grootresultaat.Text = "";
        }

        private void ClickPi(object sender, RoutedEventArgs e)
        {
            calc.AddPi();

            resultaat.Text = calc.Input;
        }

        private void Clickgelijk(object sender, RoutedEventArgs e)
        {
            string som = calc.Input;
            string antwoord = calc.Calculate();

            resultaat.Text = som + " = " + antwoord;
            grootresultaat.Text = antwoord;

            calc.Clear();
        }

        private void ClickNegate(object sender, RoutedEventArgs e)
        {
            calc.ToggleNegate();

            resultaat.Text = calc.Input;
        }


        public class Calculator
        {
            public string Input { get; private set; } = "";


            public void AddNumber(string nummer)
            {
                Input = Input + nummer;
            }


            public void AddOperator(string operatorTekst)
            {
                if (Input == "" && operatorTekst == "-")
                {
                    Input = "-";
                    return;
                }

                if (Input.EndsWith("+"))
                {
                    return;
                }

                if (Input.EndsWith("-"))
                {
                    return;
                }

                if (Input.EndsWith("*"))
                {
                    return;
                }

                if (Input.EndsWith("/"))
                {
                    return;
                }

                Input = Input + operatorTekst;
            }


            public void AddComma()
            {
                if (Input == "")
                {
                    Input = "0,";
                    return;
                }

                string laatsteDeel = Input;

                int plus = laatsteDeel.LastIndexOf("+");
                int min = laatsteDeel.LastIndexOf("-");
                int keer = laatsteDeel.LastIndexOf("*");
                int delen = laatsteDeel.LastIndexOf("/");

                int laatsteOperator = plus;

                if (min > laatsteOperator)
                {
                    laatsteOperator = min;
                }

                if (keer > laatsteOperator)
                {
                    laatsteOperator = keer;
                }

                if (delen > laatsteOperator)
                {
                    laatsteOperator = delen;
                }

                if (laatsteOperator >= 0)
                {
                    laatsteDeel = Input.Substring(laatsteOperator + 1);
                }

                if (laatsteDeel.Contains(","))
                {
                    return;
                }

                Input = Input + ",";
            }


            public void Undo()
            {
                if (Input.Length == 0)
                {
                    return;
                }

                Input = Input.Remove(Input.Length - 1);
            }


            public void Clear()
            {
                Input = "";
            }


            public void AddPi()
            {
                Input = Input + Math.PI;
            }


            public void ToggleNegate()
            {
                if (Input == "")
                {
                    Input = "-";
                    return;
                }

                int laatsteOperator = -1;

                int plus = Input.LastIndexOf("+");
                int min = Input.LastIndexOf("-");
                int keer = Input.LastIndexOf("*");
                int delen = Input.LastIndexOf("/");

                if (plus > laatsteOperator)
                {
                    laatsteOperator = plus;
                }

                if (min > laatsteOperator)
                {
                    laatsteOperator = min;
                }

                if (keer > laatsteOperator)
                {
                    laatsteOperator = keer;
                }

                if (delen > laatsteOperator)
                {
                    laatsteOperator = delen;
                }

                string laatsteGetal = Input.Substring(laatsteOperator + 1);

                if (laatsteGetal.StartsWith("-"))
                {
                    laatsteGetal = laatsteGetal.Substring(1);
                }
                else
                {
                    laatsteGetal = "-" + laatsteGetal;
                }

                Input = Input.Substring(0, laatsteOperator + 1) + laatsteGetal;
            }


            public string Calculate()
            {
                try
                {
                    string som = Input.Replace(",", ".");

                    System.Data.DataTable tabel = new System.Data.DataTable();

                    object antwoord = tabel.Compute(som, "");

                    return antwoord.ToString();
                }
                catch
                {
                    return "ERROR";
                }
            }
        }
    }
}



