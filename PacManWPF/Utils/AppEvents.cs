using System;
using System.Windows;
using System.Windows.Interop;
using PacManWPF.Utils;

namespace PacManWPF
{
    public partial class MainWindow : Window
    {
        private const int SC_RESTORE = 0xF120;
        private const int SC_MAXIMIZE = 0xF030;
        private const int WM_EXITSIZEMOVE = 0x232;
        private const int WM_SYSCOMMAND = 0x112;
        // https://learn.microsoft.com/it-it/windows/win32/menurc/wm-syscommand
        private double oldWidth;
        private double oldHeight;

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
            SoundEffectsPlayer.SetVolume(e.NewValue);
        }
    }
}
