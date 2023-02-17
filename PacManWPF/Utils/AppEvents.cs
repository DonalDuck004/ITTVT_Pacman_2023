using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using PacManWPF.Game;
using PacManWPF.Game.Worlds;
using PacManWPF.Utils;

namespace PacManWPF
{
    public partial class UIWindow : Window
    {
        public static readonly BitmapScalingMode[] GraphicOptions = { BitmapScalingMode.Unspecified, BitmapScalingMode.LowQuality, BitmapScalingMode.HighQuality, BitmapScalingMode.NearestNeighbor };


        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Game.RuntimeSettingsHandler.SetVolume((int)e.NewValue);
            SoundEffectsPlayer.SetVolume(e.NewValue);
        }

        private void SetGraphicsQuality(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var opt = GraphicOptions[(int)e.NewValue];
            Game.RuntimeSettingsHandler.SetGraphic((int)e.NewValue);

            foreach (var img in this.game_grid.Children.OfType<System.Windows.Controls.Image>())
                RenderOptions.SetBitmapScalingMode(img, opt);

            if (this.quality_label is null) // Not yet loaded when reading settings
                this.Loaded += (s, e) => this.quality_label!.Content = opt.ToString();
            else
                this.quality_label.Content = opt.ToString();
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
            FillWorldsBox();
            pause_menu_tab.IsSelected = true;
        }

        private void OpenWorldsFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", Config.WORLD_DIR);
        }

        private void OpenITTVTSite(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ittvt.edu.it/") { UseShellExecute = true });
        }

        private void ClosePauseMenu(object sender, EventArgs e)
        {
            if (WorldLoader.CurrentWorld is null)
            {
                this.start_game_tab.IsSelected = true;
                return;
            }

            this.game_tab.IsSelected = true;
            CloseMenu();
        }

        private void FillWorldsBox()
        {
            this.worlds_box.Items.Clear();
            foreach (World world in WorldLoader.Worlds)
                this.worlds_box.Items.Add(world.Name);
        }

        private void DropWorldsCache(object sender, EventArgs e)
        {
            WorldLoader.DropCache();
            FillWorldsBox();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SoundEffectsPlayer.StopAll();
            // TODO Wait for scoredb thread
            Environment.Exit(0);
        }

        private void SetAnimations(object sender, EventArgs e)
        {
            Game.RuntimeSettingsHandler.SetAnimations(true);
        }

        private void UnSetAnimations(object sender, EventArgs e)
        {
            Game.RuntimeSettingsHandler.SetAnimations(false);
        }

        private void SetMaximizedStartup(object sender, EventArgs e)
        {
            Game.RuntimeSettingsHandler.SetMaximizedStartup(true);
        }

        private void UnSetMaximizedStartup(object sender, EventArgs e)
        {
            Game.RuntimeSettingsHandler.SetMaximizedStartup(false);
        }


        Assembly asm = null;

        private void LoadAsm()
        {
            string path = RuntimeSettingsHandler.ONLINE_DLL;
            try
            {
                asm = Assembly.LoadFile(path);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("DLL Non Trovato", $"{path} non trovato.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleOnlineExc(Exception e)
        {
            try
            {
                throw e;
            }catch (HttpRequestException)
            {
                MessageBox.Show("Impossibile collegarsi al server...");
            }
        }

        private void OpenExt(object sender, RoutedEventArgs e)
        {
            if (asm is null)
                LoadAsm();

            if (asm is null)
                return;


            Type t = asm.GetType("PacmanOnlineMaps.PlugWindow")!;
            var methodInfo = t.GetMethod("ShowDialog")!;
            try
            {
                methodInfo.Invoke(Activator.CreateInstance(t), new object[] { });
            }catch(TargetInvocationException exc)
            {
                HandleOnlineExc(((AggregateException)exc.InnerException!.InnerException!).InnerExceptions[0]);
            }
        }
    }
}
