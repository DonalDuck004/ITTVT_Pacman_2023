using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Logica di interazione per GhostControl.xaml
    /// </summary>
    public partial class GhostControl : UserControl
    {
        public Image image;
        public GhostColors color;
        public List<System.Drawing.Point> positions;
        public bool IsInRec;
        public GhostEngines CurrentEngine => (GhostEngines)((ComboBoxItem)this.engines.SelectedItem).Tag;
        private DispatcherTimer RecAnimator = new DispatcherTimer()
        {
            Interval = new TimeSpan(TimeSpan.TicksPerSecond / 2)
        };
        private Color[] scale = { Colors.Green, Colors.DarkGreen, Colors.Blue, Colors.DarkBlue };
        private static int init_x = 1;

        public GhostControl(Image image,
                     GhostColors color
                     )
        {
            InitializeComponent();
            this.image = image;
            this.positions = new();
            this.color = color;
            this.IsInRec = false;
            this.RecAnimator.Tick += new EventHandler(OnRecTick);

            foreach (var item in (GhostEngines[])Enum.GetValues(typeof(GhostEngines)))
            {
                if (item is GhostEngines._NULL)
                    continue;

                this.engines.Items.Add(new ComboBoxItem() { Content = item.ToString(), Tag = item });
            }

            this.engines.SelectedIndex = 0;
            this.x_txt.Text = init_x++.ToString();
        }

        private void OnGhostEngineChanged(object sender, RoutedEventArgs e)
        {
            if (!this.CurrentEngine.SupportsSchema())
                ClearGrid();
        }

        private void GhostStartRec(object sender, MouseButtonEventArgs e)
        {
            if (!this.CurrentEngine.SupportsSchema())
            {
                MessageBox.Show("Questo engine non supporta gli schemi");
                return;
            }

            this.IsInRec = true;
            MainWindow.INSTANCE.Activate();
            this.x_txt.IsEnabled = false;
            this.y_txt.IsEnabled = false;
            this.confirm_btn.IsEnabled = false;

            if (!RecAnimator.IsEnabled)
                RecAnimator.Start();
        }

        private void GhostStopRec(object sender, MouseButtonEventArgs e)
        {
            this.IsInRec = false;
            this.RecAnimator.Stop();
            this.StartRecImg.Source = MainWindow.RecordImage;

            this.x_txt.IsEnabled = true;
            this.y_txt.IsEnabled = true;
            this.confirm_btn.IsEnabled = true;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            var n = int.Parse(((TextBox)sender).Text + e.Text);
            
            e.Handled = !(0 <= n && n <= ((((TextBox)sender).Tag.ToString() == "Y" ? MainWindow.Y_COUNT : MainWindow.X_COUNT) - 1));
        }

        public void RecPos(int X, int Y)
        {
            x_txt.Text = X.ToString();
            y_txt.Text = Y.ToString();

            if (!this.IsInRec)
                return;

            positions.Add(new(X, Y));


            var ceil = (Rectangle)matrix.Children[Y * MainWindow.X_COUNT + X];
            var tmp = (SolidColorBrush)ceil.Fill;
            int colorIdx = tmp.Color == Colors.Red ? -1 : Array.IndexOf(scale, tmp.Color);
            if (++colorIdx == scale.Length)
                colorIdx = 0;

            ((Rectangle)matrix.Children[Y * MainWindow.X_COUNT + X]).Fill = new SolidColorBrush(scale[colorIdx]);
        }

        private void OnRecTick(object? sender, EventArgs e)
        {
            if (object.ReferenceEquals(this.StartRecImg.Source, MainWindow.EmptyImage))
                this.StartRecImg.Source = MainWindow.RecordImage;
            else
                this.StartRecImg.Source = MainWindow.EmptyImage;
        }

        private void OnSetPos(object sender, RoutedEventArgs e)
        {
            if (x_txt.Text == "" || y_txt.Text == "")
                return;

            this.SetPos(int.Parse(x_txt.Text), int.Parse(y_txt.Text), from_e: true);
        }

        public void ClearGrid()
        {
            this.positions.Clear();

            var filler = new SolidColorBrush(Colors.Red);
            foreach (var item in this.matrix.Children.OfType<Rectangle>())
                item.Fill = filler;
        }

        public void SetPositions(List<System.Drawing.Point> positions, GhostEngines gh)
        {
            Debug.Assert(gh.SupportsSchema());
            this.SetEngine(gh);

            this.positions = positions;

            var filler = new SolidColorBrush(scale[0]);
            foreach (var item in positions)
                ((Rectangle)matrix.Children[item.Y * MainWindow.X_COUNT + item.X]).Fill = filler;

            this.SetPos(positions[0]);
        }

        public void SetPosition(System.Drawing.Point point, GhostEngines gh)
        {
            Debug.Assert(!gh.SupportsSchema());
            this.SetEngine(gh);

            this.SetPos(point);
        }

        private void SetEngine(GhostEngines gh)
        {
            for (int i = 0; i < this.engines.Items.Count; i++)
                if ((GhostEngines)((ComboBoxItem)this.engines.Items[i]).Tag == gh)
                {
                    this.engines.SelectedIndex = i;
                    return;
                } // I can cast GhostEngines to int, but i wanna be safe lol
        }

        private void SetPos(System.Drawing.Point point, bool from_e = false) => SetPos(point.X, point.Y, from_e);

        private void SetPos(int X, int Y, bool from_e = false)
        {
            Grid.SetColumn(this.image, X);
            Grid.SetRow(this.image, Y);

            if (from_e is false)
            {
                this.x_txt.Text = X.ToString();
                this.y_txt.Text = Y.ToString();
            }
        }

        private void OnClear(object sender, RoutedEventArgs e) => this.ClearGrid();
    }
}
