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
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using System.Security.Policy;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Assassins_Creed_Remastered_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Global
        static readonly HttpClient client = new HttpClient();
        string path = string.Empty;
        Dictionary<string, string> Sources = new Dictionary<string, string>();

        //Methods
        // Read all sources and put them into a Dictionary
        private async void ReadSources(string url)
        {
            Description.Text = "Grabbing Sources";
            HttpWebRequest SourceText = (HttpWebRequest)HttpWebRequest.Create(url);
            SourceText.UserAgent = "Mozilla/5.0";
            var response = SourceText.GetResponse();
            var content = response.GetResponseStream();
            using (var reader = new StreamReader(content))
            {
                string fileContent = reader.ReadToEnd();
                string[] lines = fileContent.Split(new char[] { '\n' });
                foreach (string line in lines)
                {
                    try
                    {
                        if (line != "")
                        {
                            string[] splitLine = line.Split(';');
                            Sources.Add(splitLine[0], splitLine[1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        continue;
                    }
                }
            }
            foreach(string key in Sources.Keys)
            {
                Console.WriteLine("Item: " + key + ", Link: " + Sources[key]);
            };
            await Task.Delay(10);
            GC.Collect();
        }

        // Download all of the Sources in the Dictionary
        private void DownloadFiles(string url, string Destination, int position)
        {
            Description.Text = "Downloading mods";
            Progress.Value = position;
            TextProgress.Text = $"{position}/5 mods";
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += WebClientDownloadProgressChanged;
                client.DownloadFileAsync(new Uri(url), Destination);
            }
        }

        // This is used to show progress on the ProgressBar
        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            TextProgress.Text = Progress.Value + "%";
        }

        // Events
        // This is needed for Window moving
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Exit the installer when Exit is clicked
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(10);
            Environment.Exit(0);
        }

        // Download and install mods when Install is clicked
        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "Executable Files|AssassinsCreed_Dx9.exe";
                FileDialog.Title = "Select an Assassins Creed Executable";
                var result = FileDialog.ShowDialog();
                if (result == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                    Console.WriteLine(path);
                };
                using (StreamWriter sw = new StreamWriter(path + @"\Path.txt"))
                {
                    sw.WriteLine(path + @"\");
                };
                ReadSources("https://raw.githubusercontent.com/shazzaam7/Assassins-Creed-Remastered-Installer/master/Sources.txt");
                for (int i = 0; i < Sources.Keys.Count; i++)
                {
                    KeyValuePair<string, string> keyValue = Sources.ElementAt(i);
                    DownloadFiles(keyValue.Value, keyValue.Key, i);
                }
                Progress.Value = 0;
                TextProgress.Text = "";
                await Task.Delay(10);
                MessageBox.Show("Installation Complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // Uninstall everything that was used by AC Remastered
        private async void Uninstall_Click(object sender, RoutedEventArgs e)
        {

            await Task.Delay(10);
        }
    }
}
