using System.Text;
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
    public partial class KlokWindow : Window
    {
        private readonly DispatcherTimer timer =
            new DispatcherTimer();

        private int hours;
        private int minutes;
        private int seconds;
        private bool isRunning;

        public KlokWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            UpdateClock();
        }

        private void StartButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (isRunning)
                return;

            if (!int.TryParse(HoursBox.Text, out hours) ||
                !int.TryParse(MinutesBox.Text, out minutes) ||
                !int.TryParse(SecondsBox.Text, out seconds) ||
                hours < 0 || hours > 23 ||
                minutes < 0 || minutes > 59 ||
                seconds < 0 || seconds > 59)
            {
                MessageBox.Show("Vul een geldige tijd in.");
                return;
            }

            isRunning = true;

            HoursBox.IsEnabled = false;
            MinutesBox.IsEnabled = false;
            SecondsBox.IsEnabled = false;

            UpdateClock();
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            seconds++;

            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
            }

            if (minutes >= 60)
            {
                minutes = 0;
                hours++;
            }

            if (hours >= 24)
                hours = 0;

            UpdateClock();
        }

        private void UpdateClock()
        {
            ClockText.Text =
                $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        private void ResetButton_Click(
            object sender, RoutedEventArgs e)
        {
            timer.Stop();
            isRunning = false;

            hours = 0;
            minutes = 0;
            seconds = 0;

            HoursBox.Text = "0";
            MinutesBox.Text = "0";
            SecondsBox.Text = "0";

            HoursBox.IsEnabled = true;
            MinutesBox.IsEnabled = true;
            SecondsBox.IsEnabled = true;

            UpdateClock();
        }
    }
}