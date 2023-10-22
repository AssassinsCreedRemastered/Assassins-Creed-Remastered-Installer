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
using IWshRuntimeLibrary;
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
                    await client.DownloadFileTaskAsync(new Uri(url), Destination);
                }
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            /*
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    Description.Text = "Downloading " + System.IO.Path.GetFileNameWithoutExtension(Destination);
                    Progress.Value = 0;
                    TextProgress.Text = $"{Progress.Value}%";
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        using (FileStream fs = new FileStream(Destination, FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fs);
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
            }*/
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
                string fullPath = Directory.GetCurrentDirectory() + @"\Installation Files\" + name;
                string directory = Directory.GetCurrentDirectory() + @"\Installation Files\" + System.IO.Path.GetFileNameWithoutExtension(name);
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Installation Files\" + System.IO.Path.GetFileNameWithoutExtension(name)))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Installation Files\" + System.IO.Path.GetFileNameWithoutExtension(name));
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
                    case "ASI-Loader":
                        if (System.IO.Directory.Exists(directory))
                        {
                            if (System.IO.File.Exists(directory + @"\dinput8.dll"))
                            {
                                //System.IO.File.Move(directory + @"\dinput8.dll", path + @"\dinput8.dll", true);
                                System.IO.File.Copy(directory + @"\dinput8.dll", path + @"\dinput8.dll", true);
                            }
                        }
                        break;
                    case "EaglePatchAC1":
                        if (Directory.Exists(directory))
                        {
                            if (!Directory.Exists(path + @"\scripts"))
                            {
                                //Directory.Move(directory, path + @"\scripts");
                                System.IO.File.Copy(directory + @"\EaglePatchAC1.asi", path + @"\scripts\EaglePatchAC1.asi");
                                System.IO.File.Copy(directory + @"\EaglePatchAC1.ini", path + @"\scripts\EaglePatchAC1.ini");
                            }
                            if (System.IO.File.Exists(path + @"\scripts\Readme - EaglePatchAC1.txt"))
                            {
                                System.IO.File.Delete(path + @"\scripts\Readme - EaglePatchAC1.txt");
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
                    case "AssassinsCreed_Dx9":
                        if (Directory.Exists(directory))
                        {
                            if (System.IO.File.Exists(path + @"\AssassinsCreed_Dx9.exe"))
                            {
                                System.IO.File.Move(path + @"\AssassinsCreed_Dx9.exe", path + @"\AssassinsCreed_Dx9.exe.bkp");
                                System.IO.File.Copy(directory + @"\AssassinsCreed_Dx9.exe", path + @"\AssassinsCreed_Dx9.exe");
                            }
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
                    case "ReShade":
                        if (Directory.Exists(directory))
                        {
                            foreach (string file in Directory.GetFiles(directory))
                            {
                                if (!System.IO.File.Exists(path + @"\" + System.IO.Path.GetFileName(file)))
                                {
                                    System.IO.File.Copy(file, path + @"\" + System.IO.Path.GetFileName(file), true);
                                }
                            }
                            foreach (string dir in Directory.GetDirectories(directory))
                            {
                                if (!Directory.Exists(path + @"\" + System.IO.Path.GetFileName(dir)))
                                {
                                    Directory.Move(dir, path + @"\" + System.IO.Path.GetFileName(dir));
                                }
                            }
                        }
                        break;
                    case "Launcher":
                        if (Directory.Exists(directory))
                        {
                            if (System.IO.File.Exists(directory + @"\Assassins Creed Remastered Launcher.exe"))
                            {
                                System.IO.File.Copy(directory + @"\Assassins Creed Remastered Launcher.exe", path + @"\Assassins Creed Remastered Launcher.exe", true);
                            }
                        }
                        break;
                    case "Updater":
                        if (Directory.Exists(directory))
                        {
                            if (System.IO.File.Exists(directory + @"\Assassins Creed Remastered Launcher Updater.exe"))
                            {
                                System.IO.File.Copy(directory + @"\Assassins Creed Remastered Launcher Updater.exe", path + @"\Assassins Creed Remastered Launcher Updater.exe",true);
                            }
                        }
                        break;
                    default:
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

        // Setup uMod for the first time
        private async Task uModSetup()
        {
            try
            {
                await uModAppData();
                await Task.Delay(10);
                await uModSaveFile();
                GC.Collect();
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // Configuration of AppData file for uMod
        private async Task uModAppData()
        {
            try
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
                    if (!System.IO.File.Exists(AppData + @"\uMod\uMod_DX9.txt"))
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
                        using (StreamReader sr = new StreamReader(AppData + @"\uMod\uMod_DX9.txt"))
                        {
                            using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9temp.txt"))
                            {
                                string line = sr.ReadLine();
                                while (line != null)
                                {

                                    if (line == '\0'.ToString())
                                    {
                                        sw.Write(line);
                                    }
                                    else
                                    {
                                        sw.Write(line + "\n");
                                    }
                                    line = sr.ReadLine();
                                }
                                sw.Write(ExecutablePath);
                            }
                        }
                        System.IO.File.Delete(AppData + @"\uMod\uMod_DX9.txt");
                        System.IO.File.Move(AppData + @"\uMod\uMod_DX9temp.txt", AppData + @"\uMod\uMod_DX9.txt");
                    }
                }
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task uModSaveFile()
        {
            try
            {
                if (!Directory.Exists(path + @"\uMod\templates"))
                {
                    Directory.CreateDirectory(path + @"\uMod\templates");
                }
                if (!System.IO.File.Exists(path + @"\uMod\Status.txt"))
                {
                    using (StreamWriter sw = new StreamWriter(path + @"\uMod\Status.txt"))
                    {
                        sw.Write("Enabled=1");
                    }
                }
                using (StreamWriter sw = new StreamWriter(path + @"\uMod\templates\ac1.txt"))
                {
                    sw.Write("SaveAllTextures:0\n");
                    sw.Write("SaveSingleTexture:0\n");
                    sw.Write("FontColour:255,0,0\n");
                    sw.Write("TextureColour:0,255,0\n");
                    sw.Write("Add_true:" + path + @"\Mods\Overhaul\Overhaul Fixed For ReShade.tpf" + "\n");
                }
                string saveFile = path + @"\AssassinsCreed_Dx9.exe" + "|" + path + @"\uMod\templates\ac1.txt";
                char[] array = saveFile.ToCharArray();
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
                string SaveFilePATH = new string(charArray);
                using (StreamWriter sw = new StreamWriter(path + @"\uMod\uMod_SaveFiles.txt"))
                {
                    sw.Write(SaveFilePATH);
                }
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task CreateShortcut()
        {
            try
            {
                if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed Remastered.lnk"))
                {
                    MessageBoxResult result = MessageBox.Show("Do you want to create shortcut?", "Confirmation", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        WshShell shell = new WshShell();
                        string SearchLocation = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                        string ShortcutLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed Remastered.lnk";
                        IWshShortcut Shortcut = shell.CreateShortcut(ShortcutLocation);
                        Shortcut.Description = "Shortcut for Assassin's Creed Remastered";
                        Shortcut.IconLocation = $"{path + @"\Assassins Creed Remastered Launcher.exe"}, {0}";
                        Shortcut.TargetPath = path + @"\Assassins Creed Remastered Launcher.exe";
                        Shortcut.Save();
                        System.IO.File.Copy(ShortcutLocation, SearchLocation + @"\Assassin's Creed Remastered.lnk");
                    }
                }
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task RemoveGameFromuMod()
        {
            try
            {
                string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                bool isFirstLine = true;
                // AssassinsCreed_Dx9.exe
                using (StreamReader sr = new StreamReader(AppData + @"\uMod\uMod_DX9.txt"))
                {
                    using (StreamWriter sw = new StreamWriter(AppData + @"\uMod\uMod_DX9temp.txt"))
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            if (line == '\0'.ToString() && !line.EndsWith("AssassinsCreed_Dx9.exe"))
                            {
                                if (isFirstLine)
                                {
                                    sw.Write(line.TrimStart('\0'));
                                    isFirstLine = false;
                                }
                                else
                                {
                                    sw.Write(line);
                                }
                            }
                            else if (!line.EndsWith("AssassinsCreed_Dx9.exe"))
                            {
                                if (isFirstLine)
                                {
                                    sw.Write(line.TrimStart('\0') + "\n");
                                    isFirstLine = false;
                                }
                                else
                                {
                                    sw.Write(line + "\n");
                                }
                            }
                            line = sr.ReadLine();
                        }
                    }
                }
                System.IO.File.Delete(AppData + @"\uMod\uMod_DX9.txt");
                System.IO.File.Move(AppData + @"\uMod\uMod_DX9temp.txt", AppData + @"\uMod\uMod_DX9.txt");
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
                throw;
            }
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
                if (FileDialog.ShowDialog() == true)
                {
                    path = System.IO.Path.GetDirectoryName(FileDialog.FileName);
                }
                else
                {
                    return;
                };
                if (!System.IO.Directory.Exists("Installation Files"))
                {
                    System.IO.Directory.CreateDirectory("Installation Files");
                };
                if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\"))
                {
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\");
                };
                if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\Assassin's Creed\"))
                {
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\Assassin's Creed\");
                };
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\Assassin's Creed\Path.txt"))
                {
                    sw.WriteLine(path);
                };
                await ReadSources("https://raw.githubusercontent.com/AssassinsCreedRemastered/Assassins-Creed-Remastered-Mods/main/Sources.txt");
                for (int i = 0; i < Sources.Keys.Count; i++)
                {
                    KeyValuePair<string, string> keyValue = Sources.ElementAt(i);
                    if (!System.IO.File.Exists(Directory.GetCurrentDirectory() + @"\Installation Files\" + keyValue.Key))
                    {
                        await DownloadFiles(keyValue.Value, @"Installation Files\" + keyValue.Key, i);
                    }
                    await InstallMods(keyValue.Key);
                };
                Description.Text = "Setting up uMod";
                await uModSetup();
                await CreateShortcut();
                Description.Text = "Cleaning up";
                Directory.Delete(Directory.GetCurrentDirectory() + @"\Installation Files", true);
                Progress.Value = 0;
                TextProgress.Text = "";
                Description.Text = "Installation Completed";
                MessageBox.Show("Installation completed.");
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
            } else
            {
                return;
            }
            try
            {
                // Delete Ultimate ASI Loader
                if (System.IO.File.Exists(path + @"\dinput8.dll"))
                {
                    System.IO.File.Delete(path + @"\dinput8.dll");
                };

                // Delete Mods Folder that has PS3 Buttons and Overhaul Mod
                if (Directory.Exists(path + @"\Mods"))
                {
                    Directory.Delete(path + @"\Mods", true);
                };

                // Delete scripts folder that has EaglePatch
                if (Directory.Exists(path + @"\scripts"))
                {
                    Directory.Delete(path + @"\scripts", true);
                };

                // Delete uMod
                if (Directory.Exists(path + @"\uMod"))
                {
                    Directory.Delete(path + @"\uMod", true);
                };
                // Delete uMod settings
                MessageBoxResult result = MessageBox.Show("Do you want to delete all of uMod settings?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\uMod"))
                    {
                        Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\uMod", true);
                    }
                }
                else
                {
                    RemoveGameFromuMod();
                }
                if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\Assassin's Creed\Path.txt"))
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\Assassin's Creed\Path.txt");
                }

                // Restore Original non LAA executable
                if (System.IO.File.Exists(path + @"\AssassinsCreed_Dx9.exe.bkp"))
                {
                    System.IO.File.Delete(path + @"\AssassinsCreed_Dx9.exe");
                    System.IO.File.Move(path + @"\AssassinsCreed_Dx9.exe.bkp", path + @"\AssassinsCreed_Dx9.exe");
                }

                // Delete ReShade
                if (System.IO.File.Exists(path + @"\d3d9.dll"))
                {
                    System.IO.File.Delete(path + @"\d3d9.dll");
                };
                if (System.IO.File.Exists(path + @"\d3d9.log"))
                {
                    System.IO.File.Delete(path + @"\d3d9.log");
                };
                if (System.IO.File.Exists(path + @"\dxgi.dll"))
                {
                    System.IO.File.Delete(path + @"\dxgi.dll");
                };
                if (System.IO.File.Exists(path + @"\ReShade.ini"))
                {
                    System.IO.File.Delete(path + @"\ReShade.ini");
                };
                if (Directory.Exists(path + @"\ReShade"))
                {
                    Directory.Delete(path + @"\ReShade", true);
                };

                // Delete Launcher
                if (System.IO.File.Exists(path + @"\AssassinsCreedRemasteredLauncher.exe"))
                {
                    System.IO.File.Delete(path + @"\AssassinsCreedRemasteredLauncher.exe");
                };
                if (System.IO.File.Exists(path + @"\Assassins Creed Remastered Launcher.exe"))
                {
                    System.IO.File.Delete(path + @"\Assassins Creed Remastered Launcher.exe");
                };
                if (System.IO.File.Exists(path + @"\icon.ico"))
                {
                    System.IO.File.Delete(path + @"\icon.ico");
                };

                // Delete Launcher Updater
                if (System.IO.File.Exists(path + @"\Assassins Creed Remastered Launcher Updater.exe"))
                {
                    System.IO.File.Delete(path + @"\Assassins Creed Remastered Launcher Updater.exe");
                };

                // Delete Shortcut
                if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed Remastered.lnk"))
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Assassin's Creed Remastered.lnk");
                }

                if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Assassin's Creed Remastered.lnk"))
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Assassin's Creed Remastered.lnk");
                }
                Description.Text = "Uninstallation Completed";
                MessageBox.Show("Uninstallation completed.");
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}
