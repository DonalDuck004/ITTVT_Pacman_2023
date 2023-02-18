using PacManWPF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PacManWPF
{
    /// <summary>
    /// Logica di interazione per Page1.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        public static StartPage? INSTANCE = new();

        private StartPage()
        {
            InitializeComponent();
            StartPage.INSTANCE = this;
        }

        private void OpenGit(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/DonalDuck004/ITTVT_Pacman_2023") { UseShellExecute = true });
        }

        private void OpenTG(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://t.me/DonalDuck004") { UseShellExecute = true });
        }

        private void OpenKaze(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://t.me/Kaze94") { UseShellExecute = true });
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            // FillWorldsBox();
            // pause_menu_tab.IsSelected = true;
        }

        private void OpenWorldsFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", Config.WORLD_DIR);
        }

        private void OpenITTVTSite(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ittvt.edu.it/") { UseShellExecute = true });
        }
    }
}
