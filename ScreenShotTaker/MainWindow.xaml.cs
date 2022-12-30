using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ScreenShotTaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Can_Take_Screenshot = true;
        Configuration configuration { get; set; }
        public MainWindow()
        {
            string appsettingscontent = File.ReadAllText(@"appsettings.json"); // the default image folder is stored in the appsettings file
            configuration = JsonConvert.DeserializeObject<Configuration>(appsettingscontent);
            InitializeComponent();
            TextBoxDefaultPath.Text = configuration.DefaultImagesPath;
            Task.Run(() => Check_Screenshot_Hotkey());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var set_default_folder_dialogue = new CommonOpenFileDialog();
            set_default_folder_dialogue.Title = "Select a default folder";
            set_default_folder_dialogue.IsFolderPicker = true;
            set_default_folder_dialogue.InitialDirectory = TextBoxDefaultPath.Text;

            set_default_folder_dialogue.AddToMostRecentlyUsedList = false;
            set_default_folder_dialogue.AllowNonFileSystemItems = false;
            set_default_folder_dialogue.DefaultDirectory = TextBoxDefaultPath.Text;
            set_default_folder_dialogue.EnsureFileExists = true;
            set_default_folder_dialogue.EnsurePathExists = true;
            set_default_folder_dialogue.EnsureReadOnly = false;
            set_default_folder_dialogue.EnsureValidNames = true;
            set_default_folder_dialogue.Multiselect = false;
            set_default_folder_dialogue.ShowPlacesList = true;

            if (set_default_folder_dialogue.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folder = set_default_folder_dialogue.FileName;
                TextBoxDefaultPath.Text = folder;
                configuration.DefaultImagesPath = folder;
                string jsonString = JsonConvert.SerializeObject(configuration, Formatting.Indented);
                File.WriteAllText("appsettings.json", jsonString);
            }
        }

        /// <summary>
        /// Continuously checks whether the hotkey (LCONTROL) to take a screenshot is pressed
        /// </summary>
        /// <returns></returns>
        public async Task Check_Screenshot_Hotkey()
        {
            System.Drawing.Size ScreenSize = new System.Drawing.Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            while (true)
            {
                if(Can_Take_Screenshot)
                {
                    if(Keyboard.IsKeyDown(162)) // 162 = LCONTROL. https://mirametrics.com/help/mira_pro_x64_script_8/source/getkeystate.htm
                    {
                        Can_Take_Screenshot = false;
                        Bitmap Screenshot = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too
                        Graphics gScreenshot = Graphics.FromImage(Screenshot);
                        gScreenshot.CopyFromScreen(0, 0, 0, 0, ScreenSize);
                        Microsoft.Win32.OpenFileDialog openFileDialog = new();
                        openFileDialog.InitialDirectory = configuration.DefaultImagesPath;
                        openFileDialog.CheckFileExists = false;
                        openFileDialog.Multiselect= false;
                        openFileDialog.Filter = "image files .bmp|*.bmp";
                        openFileDialog.DefaultExt = ".bmp";
                        bool? result = openFileDialog.ShowDialog();
                        if (result == true)
                        {
                            Screenshot.Save(openFileDialog.FileName);
                        }

                        await Task.Delay(200);
                        Can_Take_Screenshot = true;
                    }
                }

                await Task.Delay(200);
            }
        }
    }
}