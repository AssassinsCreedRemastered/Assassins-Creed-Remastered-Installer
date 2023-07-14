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
using System.Security.Policy;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using SharpCompress.Common;
using SharpCompress.Archives;
using System.Configuration;
using System.Xml.Linq;

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

        // Global
        static readonly HttpClient client = new HttpClient();
        string path = string.Empty;
        Dictionary<string, string> Sources = new Dictionary<string, string>();
        public static long totalSize { get; set; }

        // Functions
        // Read all sources and put them into a Dictionary
        private async Task ReadSources(string url)
        {
            try
            {
                Description.Text = "Grabbing Sources";
                Sources.Clear();
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
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // Download all of the Sources in the Dictionary
        private async Task DownloadFiles(string url, string Destination, int position)
        {
            try
            {
                Description.Text = "Downloading " + System.IO.Path.GetFileNameWithoutExtension(Destination);
                Progress.Value = 0;
                TextProgress.Text = $"{Progress.Value}%";
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += WebClientDownloadProgressChanged;
                    client.DownloadFileAsync(new Uri(url), Destination);
                }
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // This is used to show progress on the ProgressBar
        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            TextProgress.Text = Progress.Value + "%";
        }

        // This is used to install mods
        private async Task InstallMods(string name)
        {
            try
            {
                // Directory.GetCurrenDirecotry doesn't have \ at the end
                string fullPath = Directory.GetCurrentDirectory() + @"\Mods\" + name;
                string directory = Directory.GetCurrentDirectory() + @"\Mods\" + System.IO.Path.GetFileNameWithoutExtension(name);
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Mods\" + System.IO.Path.GetFileNameWithoutExtension(name)))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Mods\" + System.IO.Path.GetFileNameWithoutExtension(name));
                };
                Description.Text = "Extracting " + System.IO.Path.GetFileNameWithoutExtension(name);
                await Extract(fullPath, directory);
                Description.Text = "Installing " + System.IO.Path.GetFileNameWithoutExtension(name);
                await Move(System.IO.Path.GetFileNameWithoutExtension(name), directory);
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // Used to extract files
        private async Task Extract(string fullPath, string directory)
        {
            try
            {
                using (var archive = ArchiveFactory.Open(fullPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(directory, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // Move files to installation
        private async Task Move(string name, string directory)
        {
            try
            {
                switch (name)
                {
                    default:
                        break;
                    case "ASI-Loader":
                        if (System.IO.Directory.Exists(directory))
                        {
                            if (File.Exists(directory + @"\dinput8.dll"))
                            {
                                File.Move(directory + @"\dinput8.dll", path + @"\dinput8.dll");
                            }
                        }
                        if (Directory.Exists(directory))
                        {
                            Directory.Delete(directory);
                        };
                        break;
                    case "EaglePatchAC1":
                        if (Directory.Exists(directory))
                        {
                            if (!Directory.Exists(path + @"\scripts"))
                            {
                                Directory.Move(directory, path + @"\scripts");
                            }
                            if (File.Exists(path + @"\scripts\Readme - EaglePatchAC1.txt"))
                            {
                                File.Delete(path + @"\scripts\Readme - EaglePatchAC1.txt");
                            }
                        }
                        break;
                    case "uMod":
                        if (System.IO.Directory.Exists(directory))
                        {
                            if (!Directory.Exists(path + @"\uMod"))
                            {
                                Directory.Move(directory, path + @"\uMod");
                            }
                        }
                        break;
                    case "PSButtons":
                        if (!Directory.Exists(path + @"\Mods"))
                        {
                            Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!Directory.Exists(path + @"\Mods\PS3Buttons"))
                        {
                            Directory.Move(directory, path + @"\Mods\PS3Buttons");
                        }
                        break;
                    case "Overhaul":
                        if (!Directory.Exists(path + @"\Mods"))
                        {
                            Directory.CreateDirectory(path + @"\Mods");
                        };
                        if (!Directory.Exists(path + @"\Mods\Overhaul"))
                        {
                            Directory.Move(directory, path + @"\Mods\Overhaul");
                        }
                        break;
                }
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task uModSetup()
        {
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(AppData + @"\uMod"))
            {
                Directory.CreateDirectory(AppData + @"\uMod");
                string ExecutableDirectory = path + @"\AssassinsCreed_Dx9.exe";
                char[] array = ExecutableDirectory.ToCharArray();
                List<char> charList = new List<char>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == 0)
                    {
                        charList.Add(array[i]);
                    }
                    else
                    {
                        charList.Add('\0');
                        charList.Add(array[i]);
                    }
                }
                charList.Add('\0');
                char[] charArray = charList.ToArray();
                string ExecutablePath = new string(charArray);
                using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9.txt"))
                {
                    sw.Write(ExecutablePath);
                }
            }
            else
            {
                string ExecutableDirectory = path + @"\AssassinsCreed_Dx9.exe";
                char[] array = ExecutableDirectory.ToCharArray();
                List<char> charList = new List<char>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == 0)
                    {
                        charList.Add(array[i]);
                    }
                    else
                    {
                        charList.Add('\0');
                        charList.Add(array[i]);
                    }
                }
                charList.Add('\0');
                char[] charArray = charList.ToArray();
                string ExecutablePath = new string(charArray);
                using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9.txt"))
                {
                    sw.Write(ExecutablePath);
                }
            }
            GC.Collect();
            await Task.Delay(10);
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
                if (!System.IO.Directory.Exists("Mods"))
                {
                    System.IO.Directory.CreateDirectory("Mods");
                };
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "Executable Files|AssassinsCreed_Dx9.exe";
                FileDialog.Title = "Select an Assassins Creed Executable";
                if (FileDialog.ShowDialog() == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                    Console.WriteLine(path);
                };
                using (StreamWriter sw = new StreamWriter(path + @"\Path.txt"))
                {
                    sw.WriteLine(path + @"\");
                };
                await ReadSources("https://raw.githubusercontent.com/shazzaam7/Assassins-Creed-Remastered-Installer/master/Sources.txt");
                for (int i = 0; i < Sources.Keys.Count; i++)
                {
                    KeyValuePair<string, string> keyValue = Sources.ElementAt(i);
                    //await DownloadFiles(keyValue.Value, @"Mods\"+ keyValue.Key, i);
                    await InstallMods(keyValue.Key);
                };
                Description.Text = "Setting up uMod";
                await uModSetup();
                Description.Text = "Cleaning up";
                Directory.Delete(Directory.GetCurrentDirectory() + @"\Mods", true);
                Progress.Value = 0;
                TextProgress.Text = "";
                Description.Text = "Installation Completed";
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
            OpenFileDialog FileDialog = new OpenFileDialog();
            FileDialog.Filter = "Executable Files|AssassinsCreed_Dx9.exe";
            FileDialog.Title = "Select an Assassins Creed Executable";
            if (FileDialog.ShowDialog() == true)
            {
                path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                Console.WriteLine(path);
            };
            if (File.Exists(path + @"\dinput8.dll"))
            {
                File.Delete(path + @"\dinput8.dll");
            };
            if (Directory.Exists(path + @"\Mods"))
            {
                Directory.Delete(path + @"\Mods",true);
            };
            if (Directory.Exists(path + @"\scripts"))
            {
                Directory.Delete(path + @"\scripts", true);
            };
            if (Directory.Exists(path + @"\uMod"))
            {
                Directory.Delete(path + @"\uMod", true);
            };
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\uMod"))
            {
                Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\uMod", true);
            }
            await Task.Delay(10);
        }
    }
}
