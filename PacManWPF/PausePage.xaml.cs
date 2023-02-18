using PacManWPF.Game;
using PacManWPF.Game.Worlds;
using PacManWPF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Logica di interazione per PausePage.xaml
    /// </summary>
    public partial class PausePage : Page
    {
        public PausePage()
        {
            InitializeComponent();
            this.FillWorldsBox();
        }

        private void ClosePauseMenu(object sender, EventArgs e) => Close();

        public void Close()
        {
            if (WorldLoader.CurrentWorld is null)
            {
                Debug.Assert(StartPage.INSTANCE is not null);
                UIWindow.INSTANCE.SetPage(StartPage.INSTANCE);
                return;
            }
            else if (StartPage.INSTANCE is not null)
                StartPage.INSTANCE = null; // Clean up some memory

            UIWindow.INSTANCE.ResumeGame();
        }


        private void DropWorldsCache(object sender, EventArgs e)
        {
            WorldLoader.DropCache();
            FillWorldsBox();
        }

        private void FillWorldsBox()
        {
            this.worlds_box.Items.Clear();
            foreach (World world in WorldLoader.Worlds)
                this.worlds_box.Items.Add(world.Name);
        }

        private void OnWorldSelected(object sender, SelectionChangedEventArgs e)
        {
            if (this.worlds_box.SelectedIndex == -1)
                return;

            UIWindow.INSTANCE.SetPage(new GamePage(world_idx: this.worlds_box.SelectedIndex));
            UIWindow.INSTANCE.FreezeGame();

            GC.Collect(2, GCCollectionMode.Aggressive, true, true);
        }

        private static Assembly? asm = null;

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
            }
            catch (HttpRequestException)
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
            }
            catch (TargetInvocationException exc)
            {
                HandleOnlineExc(((AggregateException)exc.InnerException!.InnerException!).InnerExceptions[0]);
            }
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

        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Game.RuntimeSettingsHandler.SetVolume((int)e.NewValue);
            SoundEffectsPlayer.SetVolume(e.NewValue);
        }

        private void SetGraphicsQuality(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var opt = Game.RuntimeSettingsHandler.GRAPHIC_OPTIONS[(int)e.NewValue];
            Game.RuntimeSettingsHandler.SetGraphic((int)e.NewValue);

            if (GamePage.CurrentGrid is not null)
            {
                foreach (var img in GamePage.CurrentGrid!.Children.OfType<Image>())
                    RenderOptions.SetBitmapScalingMode(img, opt);
            }

            if (this.quality_label is null) // Not yet loaded when reading settings
                this.Loaded += (s, e) => this.quality_label!.Content = opt.ToString();
            else
                this.quality_label.Content = opt.ToString();
        }

        internal static void Open()
        {
            UIWindow.INSTANCE.SetPage(new PausePage());
        }
    }
}
