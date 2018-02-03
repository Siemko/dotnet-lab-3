using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Lab3
{
    public partial class MainWindow : Window
    {
        private bool _isThreadActive;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Platforma .NET Lab - 3";
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            ClearViewValues();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog =
                new OpenFileDialog {DefaultExt = ".txt", Filter = "TXT (*.txt)|*.txt|CSV (*.csv)|*csv"};
            if (openFileDialog.ShowDialog() != true) return;
            var filename = openFileDialog.SafeFileName;
            StatusBar.Text = "Nazwa otwartego pliku: " + filename;
            Stream myStream;
            var fileContent = string.Empty;
            using (myStream = openFileDialog.OpenFile())
            {
                var file = Path.GetFileName(openFileDialog.FileName);
                var path = Path.GetDirectoryName(openFileDialog.FileName);
                var reader = new StreamReader(myStream);
                while (!reader.EndOfStream) fileContent = reader.ReadLine();

                if (fileContent == null) return;
                var list = fileContent.Split(',').ToList();
                ListBox.ItemsSource = list;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(From.Text) || string.IsNullOrWhiteSpace(To.Text) ||
                string.IsNullOrWhiteSpace(Amount.Text))
                return;
            _isThreadActive = true;
            var from = int.Parse(From.Text);
            var to = int.Parse(To.Text);
            var amount = int.Parse(Amount.Text);
            StartButton.Visibility = Visibility.Collapsed;
            StopButton.Visibility = Visibility.Visible;
            var numbers = new List<int>();
            var rnd = new Random();
            for (var i = 0; i < amount; i++)
            {
                var value = rnd.Next(from, to);
                numbers.Add(value);
            }

            var progress = new Progress<int>(value => ProgressBar.Value = value);
            await Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    if (!_isThreadActive) continue;
                    ((IProgress<int>) progress).Report(i);
                    Thread.Sleep(25);
                }
            });
            var csv = string.Join(",", numbers.Select(x => x.ToString()).ToArray());
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Numbers.txt");
            if (File.Exists(path)) File.Delete(path);

            if (!File.Exists(path)) File.WriteAllText(path, csv);

            ListBox.ItemsSource = numbers;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _isThreadActive = false;
            ClearViewValues();
        }

        private void ClearViewValues()
        {
            To.Text = string.Empty;
            From.Text = string.Empty;
            Amount.Text = string.Empty;
            ListBox.ItemsSource = null;
            ProgressBar.Value = 0;
            StatusBar.Text = "Wykonał: Dominik Guzy";
            StartButton.Visibility = Visibility.Visible;
            StopButton.Visibility = Visibility.Collapsed;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz zamknąć program?", "Wyjście", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
            else
                e.Cancel = true;
        }
    }
}