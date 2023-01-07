using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using PacManWPF.Game.Worlds;
using PacManWPF.Utils;

namespace PacManWPF
{
    public partial class UIWindow : Window
    {
        private const int SC_RESTORE = 0xF120;
        private const int SC_MAXIMIZE = 0xF030;
        private const int WM_EXITSIZEMOVE = 0x232;
        private const int WM_SYSCOMMAND = 0x112;
        // https://learn.microsoft.com/it-it/windows/win32/menurc/wm-syscommand
        private double oldWidth;
        private double oldHeight;
        public static readonly BitmapScalingMode[] GraphicOptions = { BitmapScalingMode.Unspecified, BitmapScalingMode.LowQuality, BitmapScalingMode.HighQuality, BitmapScalingMode.NearestNeighbor };

        public void AdaptToSize()
        {
            const double ALLOWED_THRESHOLD_X = 14.4;
            const double ALLOWED_THRESHOLD_Y = 33.2;

            double diff_x, diff_y;

            for (int i = 1; i < Config.Sizes.Length; i++)
            {
                diff_x = Config.Sizes[i][0] - this.Width;
                diff_y = Config.Sizes[i][1] - this.Height;

                if (diff_x > 0 && diff_y > 0)
                {
                    this.app_pages.Width = Config.Sizes[i - 1][0];
                    this.app_pages.Height = Config.Sizes[i - 1][1];

                    if (this.app_pages.Width >= this.MaxWidth || this.app_pages.Height >= this.MaxHeight)
                    {
                        int tmp = i - 1;
                        do
                        {
                            this.app_pages.Width = Config.Sizes[tmp][0];
                            this.app_pages.Height = Config.Sizes[tmp][1];
                            tmp--;
                        }
                        while (this.app_pages.Width >= this.MaxWidth || this.app_pages.Height >= this.MaxHeight);
                    }

                    if (this.Height < this.app_pages.Height + ALLOWED_THRESHOLD_Y)
                        this.Height = this.app_pages.Height + ALLOWED_THRESHOLD_Y;
                    if (this.Width < this.app_pages.Width + ALLOWED_THRESHOLD_X)
                        this.Width = this.app_pages.Width + ALLOWED_THRESHOLD_X;
                    return;
                }
            }
        }

        public void OnMaximize()
        {
            this.oldHeight = this.Height;
            this.oldWidth = this.Width;
            this.Width = this.MaxWidth;
            this.Height = this.MaxHeight;
            AdaptToSize();
        }

        public void OnRestore()
        {
            this.Width = this.oldWidth;
            this.Height = this.oldHeight;
            AdaptToSize();
        }

        public void OnLoad(object sender, EventArgs e)
        {
            WindowState old = this.WindowState;
            this.WindowState = WindowState.Maximized;
            this.MaxWidth = this.Width;
            this.MaxHeight = this.Height;
            this.WindowState = old;
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_EXITSIZEMOVE)
            {
                AdaptToSize();
                handled = true;
            }
            else if (msg == WM_SYSCOMMAND)
            {
                if (wParam.ToInt32() == SC_MAXIMIZE)
                {

                    OnMaximize();
                    handled = false;
                }
                else if (wParam.ToInt32() == SC_RESTORE)
                {

                    OnRestore();
                    handled = false;
                }

            }

            return IntPtr.Zero;
        }


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
            Application.Current.Shutdown();
        }

        private void SetAnimations(object sender, EventArgs e)
        {
            Game.RuntimeSettingsHandler.SetAnimations(true);
        }

        private void UnSetAnimations(object sender, EventArgs e)
        {
            Game.RuntimeSettingsHandler.SetAnimations(false);
        }

        private void OpenExt(object sender, RoutedEventArgs e)
        {
            new PacmanOnlineMaps.PlugWindow().ShowDialog();
        }
    }
}
