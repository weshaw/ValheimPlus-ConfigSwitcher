using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfigSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string baseDir = Environment.CurrentDirectory;
        private string[] configs;
        private string[] configNames;
        private int currentSelection;
        private JObject settings;
        public MainWindow()
        {
            InitializeComponent();
            InitSettings();
        }

        private void InitSettings()
        {
            settings = new JObject();
            LoadSettings();
            var bepInExPath = Setting("BepInExPath");
            if (bepInExPath == "" || !Directory.Exists(bepInExPath))
            {
                var bepinexpath = "";
                var pathAr = baseDir.Split("\\Valheim\\");
                if(pathAr.Length>0)
                {
                    bepinexpath = pathAr[0] + "\\Valheim\\BepInEx";
                }
                if(bepinexpath != "" && Directory.Exists(bepinexpath))
                {
                    Setting("BepInExPath", bepinexpath);
                }
            }
            GetConfigFiles();
            var selected = Setting("Selected");
            if(selected == "" && configNames.Length > 0)
            {
                selected = configNames[0];
                Setting("Selected", selected);
            }
            if(selected != "")
            {
                this.configsComboBox.SelectedValue = selected;
            }
            foreach (var name in configNames)
            {
                this.configsComboBox.Items.Add(name);
            }
            
        }

        private void ChangeConfig(object sender, SelectionChangedEventArgs args)
        {
            var bepInExPath = Setting("BepInExPath");
            var comboBox = sender as ComboBox;
            currentSelection = comboBox.SelectedIndex;
            var path = configs[currentSelection];
            File.Copy(path, bepInExPath + "\\config\\valheim_plus.cfg", true);
            Setting("Selected", configNames[currentSelection]);
        }

        public bool GetConfigFiles()
        {
            var bepInExPath = Setting("BepInExPath");
            if (bepInExPath == "" || !Directory.Exists(bepInExPath))
                return false;

            var filePath = bepInExPath + "\\config";
            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string[] filePaths = Directory.GetFiles(@filePath);
            configs = filePaths.Where(x => x.EndsWith(".cfg") && x.Contains("valheim_plus.") && !x.Contains("valheim_plus.cfg")).ToArray(); ;
            if(configs.Length == 0 && File.Exists(filePath+ "\\valheim_plus.cfg"))
            {
                var origpath = filePath + "\\valheim_plus.Original.cfg";
                File.Copy(filePath + "\\valheim_plus.cfg", origpath, true);
                configs = new List<string> { origpath }.ToArray();
            }

            configNames = configs.Select(x => {
                var spos = x.LastIndexOf("valheim_plus.") + 13;
                var epos = x.LastIndexOf(".cfg");
                return x.Substring(spos, epos - spos);
            }).ToArray();
            return configs.Length > 0;
        }

        private string SettingsPath()
        {
            return baseDir + "\\ConfigSwitcherSettings.json";
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath()))
                return;

            using (StreamReader r = new StreamReader(SettingsPath()))
            {
                var json = r.ReadToEnd();
                settings = JObject.Parse(json);
            }
        }

        private void SaveSettings()
        {
            File.WriteAllText(SettingsPath(), settings.ToString());
        }

        private string Setting(string name)
        {
            var result = "";
            if (settings.ContainsKey(name))
            {
                result = settings[name].ToString();
            }
            return result;
        }

        private void Setting(string name, string value)
        {
            settings[name] = value;
            SaveSettings();
        }
    }
}
