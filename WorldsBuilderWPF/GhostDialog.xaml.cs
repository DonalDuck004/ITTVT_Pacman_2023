using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using Rectangle = System.Windows.Shapes.Rectangle;
using Color = System.Windows.Media.Color;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Logica di interazione per GhostPicker.xaml
    /// </summary>
    /// 

    public enum GhostEngines
    {
        CachedAutoMover,
        Cyclic,
        NextToBack,
        NoCachedAutoMover,
        OneTime
    }

    public partial class GhostDialog : Window
    {
      
        public static Dictionary<GhostColors, GhostDialog> SINGLETONS { get; private set; } = new();

        public GhostColors Color;
        public int ArrayIdx => (int)Color;
        public int? X;
        public int? Y;
        public bool Listening = false;
        public List<System.Drawing.Point> positions = new();
        private Color[] scale = { Colors.Green, Colors.DarkGreen, Colors.Blue, Colors.DarkBlue };

        private ImageSource rec_img;

        private DispatcherTimer RecAnimator = new DispatcherTimer()
        {
            Interval = new TimeSpan(TimeSpan.TicksPerSecond / 2)
        };
        public GhostEngines CurrentEngine => (GhostEngines)((ComboBoxItem)this.engines.SelectedItem).Tag;
        private Action<GhostDialog> cbk;

        public GhostDialog(GhostColors color, Action<GhostDialog> cbk)
        {
            if (GhostDialog.SINGLETONS.ContainsKey(color))
                throw new Exception("Instance is not null");
            
            InitializeComponent();
            GhostDialog.SINGLETONS[color] = this;
            this.Color = color; 
            this.Title = $"Ghost {color} - Schema";
            this.RecAnimator.Tick += new EventHandler(OnRecTick);
            this.rec_img = StartRecImg.Source;
            this.cbk = cbk;
            foreach (var item in (GhostEngines[])Enum.GetValues(typeof(GhostEngines)))
                this.engines.Items.Add(new ComboBoxItem() { Content = item.ToString(), Tag = item });

            this.engines.SelectedIndex = 0;
        }

        private void OnRecTick(object? sender, EventArgs e)
        {
            if (object.ReferenceEquals(this.StartRecImg.Source, MainWindow.EmptyImage))
                this.StartRecImg.Source = this.rec_img;
            else
                this.StartRecImg.Source = MainWindow.EmptyImage;
        }

        public void RecPos(int X, int Y)
        {
            positions.Add(new (X, Y));
            var ceil = (Rectangle)matrix.Children[Y * 33 + X];
            var tmp = (SolidColorBrush)ceil.Fill;
            int colorIdx = tmp.Color == Colors.Red ? -1 : Array.IndexOf(scale, tmp.Color);
            colorIdx++;
            if (colorIdx == scale.Length)
                colorIdx = 0;

            ((Rectangle)matrix.Children[Y * 33 + X]).Fill = new SolidColorBrush(scale[colorIdx]);
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            var n = int.Parse(((TextBox)sender).Text + e.Text);
            e.Handled = !(0 <= n && n <= (((TextBox)sender).Name == "wp_Y" ? 14 : 32));
        }

        private void OnCloseButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnClose(object? sender, EventArgs e)
        {
            this.RecAnimator.Stop();
            this.Listening = false;
            this.cbk.Invoke(this);
            SINGLETONS.Remove(this.Color);
        }


        private void StartRec(object sender, MouseButtonEventArgs e)
        {
            if (this.CurrentEngine is GhostEngines.CachedAutoMover || this.CurrentEngine is GhostEngines.CachedAutoMover )
            {
                MessageBox.Show("Non puoi registrare in modalita auto");
                return;
            }

            this.Listening = true;
            MainWindow.INSTANCE.Activate();

            if (!RecAnimator.IsEnabled)
                RecAnimator.Start();
        }

        private void StopRec(object sender, MouseButtonEventArgs e)
        {
            this.Listening = false;
            this.RecAnimator.Stop();
            this.StartRecImg.Source = this.rec_img;
        }

        private void OnEngineChanged(object sender, RoutedEventArgs e)
        {
            this.positions.Clear();
            foreach(var control in matrix.Children.OfType<Rectangle>())
                control.Fill = new SolidColorBrush(Colors.Red);
        }

        private void OnKeydown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
                this.Close();
        }
    }
}
