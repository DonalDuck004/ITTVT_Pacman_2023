using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;

using Pen = System.Drawing.Pen;
using Matrix = System.Windows.Media.Matrix;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using System.Runtime.CompilerServices;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public const int X_COUNT = 33;
        public const int Y_COUNT = 15;
        public const int SP_IMAGES_COUNT = 5;


        Image[][] game_ceils;
        int? filler_idx = null;
        private WrapPanel? CurrentWP => filler_idx is null ? null : (WrapPanel)this.viewer.Children[filler_idx.Value];
        private Image? CurrentImage => filler_idx is null ? null : (Image)CurrentWP!.Children[0];

        Image PacmanCeil;

#pragma warning disable CS8618
        public static MainWindow INSTANCE { get; private set; }
#pragma warning restore CS8618

        public record CacheKey(Walls Block, Color PenColor);
        private static Dictionary<CacheKey, BitmapImage> cache = new();

        private const string DRUG_PATH = @".\Assets\Images\PowerPellet.png";
        private const string POINT_PATH = @".\Assets\Images\PacDot.png";
        private const string GATE_PATH = @".\Assets\Images\Gate.png";
        private const string PACMAN_PATH = @".\Assets\Images\Pacman.png";
        private const string RED_PATH = @".\Assets\Images\Red.png";
        private const string PINK_PATH = @".\Assets\Images\Pink.png";
        private const string CYAN_PATH = @".\Assets\Images\Cyan.png";
        private const string ORANGE_PATH = @".\Assets\Images\Orange.png";
        private const string RECORD_PATH = @".\Assets\Images\Record.png";

        public static BitmapImage EmptyImage = new();
        public static BitmapImage UnspawnableImage = new();
        public static BitmapImage PowerPelletImage = new(new Uri(DRUG_PATH, UriKind.Relative));
        public static BitmapImage PacDotImage = new(new Uri(POINT_PATH, UriKind.Relative));
        public static BitmapImage GateImage = new(new Uri(GATE_PATH, UriKind.Relative));
        public static BitmapImage PacmanImage = new(new Uri(PACMAN_PATH, UriKind.Relative));
        public static BitmapImage RedImage = new(new Uri(RED_PATH, UriKind.Relative));
        public static BitmapImage PinkImage = new(new Uri(PINK_PATH, UriKind.Relative));
        public static BitmapImage CyanImage = new(new Uri(CYAN_PATH, UriKind.Relative));
        public static BitmapImage OrangeImage = new(new Uri(ORANGE_PATH, UriKind.Relative));
        public static BitmapImage RecordImage = new(new Uri(RECORD_PATH, UriKind.Relative));
        public static Image FocusEffect = new();


        private GhostControl[] ghosts = new GhostControl[4];
        private List<CacheKey> images = new();
        private Image? ActiveImg = null;


        public MainWindow()
        {
            MainWindow.INSTANCE = this;
            InitializeComponent();
            game_ceils = this.game_grid.Children.OfType<Image>().Split(X_COUNT).ToArray();

            Random rnd = new();
            Bitmap image = new(256, 256);
            MemoryStream buff = new();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            MainWindow.EmptyImage.BeginInit();
            MainWindow.EmptyImage.StreamSource = buff;
            MainWindow.EmptyImage.EndInit();

            image = new(256, 256);
            var g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.FromArgb(125, 0, 102, 204)), 0, 0, 256, 256);
            buff = new();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            var pd = new BitmapImage();
            pd.BeginInit();
            pd.StreamSource = buff;
            MainWindow.FocusEffect.Source = pd;
            pd.EndInit();

            game_grid.Children.Add(MainWindow.FocusEffect);
            Grid.SetZIndex(MainWindow.FocusEffect, -1);
            MainWindow.FocusEffect.Visibility = Visibility.Hidden;

            foreach (var row in game_ceils)
            {
                foreach (var col in row)
                {
                    col.Source = MainWindow.EmptyImage;
                    col.Tag = Types.Tag.EMPTY;
                }
            }

            this.PacmanCeil = new Image() { Source = PacmanImage };
            game_grid.Children.Add(this.PacmanCeil);
            this.PacmanCeil.MouseLeftButtonDown += OnFocus;
            Grid.SetColumn(this.PacmanCeil, 0);

            this.ghosts[0] = new(new(), GhostColors.Red); // TODO creare oggetto Image, mettere sopra
            this.ghosts[0].image.Source = RedImage;
            this.ghosts[0].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[0].image);
            Grid.SetColumn(this.ghosts[0].image, 1);


            this.ghosts[1] = new(new(), GhostColors.Pink);
            this.ghosts[1].image.Source = PinkImage;
            this.ghosts[1].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[1].image);
            Grid.SetColumn(this.ghosts[1].image, 2);


            this.ghosts[2] = new(new(), GhostColors.Orange);
            this.ghosts[2].image.Source = OrangeImage;
            this.ghosts[2].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[2].image);
            Grid.SetColumn(this.ghosts[2].image, 3);


            this.ghosts[3] = new(new(), GhostColors.Cyan);
            this.ghosts[3].image.Source = CyanImage;
            this.ghosts[3].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[3].image);
            Grid.SetColumn(this.ghosts[3].image, 4);

            var wp = new WrapPanel();
            var tmp = new Image() { Source = PowerPelletImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = Types.Tag.POWER_PELLET;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);

            wp = new WrapPanel();
            tmp = new Image() { Source = PacDotImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = Types.Tag.PAC_DOT;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);

            wp = new WrapPanel();
            tmp = new Image() { Source = GateImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = Types.Tag.GATE;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);

            wp = new WrapPanel();
            tmp = new Image() { Source = EmptyImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = Types.Tag.EMPTY;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);


            image = new(256, 256);
            var graphics = Graphics.FromImage(image);
            Pen pen = new(Color.Red, 12);
            graphics.DrawLine(pen, 0, 0, 255, 255);
            graphics.DrawLine(pen, 255, 0, 0, 255);
            UnspawnableImage.BeginInit();
            buff = new MemoryStream();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            UnspawnableImage.StreamSource = buff;
            UnspawnableImage.EndInit();

            wp = new WrapPanel();
            tmp = new Image() { Source = UnspawnableImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = Types.Tag.UNSPAWNABLE;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);
            ChoiceFiller(tmp);

            var arr = this.tabs.Items.OfType<TabItem>().Skip(1).ToArray();

            for (int i = 0; i < arr.Length - 1; i++)
                arr[i].Content = ghosts[i];

            this.KeyDown += new KeyEventHandler(OnKeyDown);

            foreach (var item in new System.Windows.Media.Color[] {Colors.Black, Colors.White})
            {
                this.color_box.Items.Add(new ComboBoxItemColor()
                {
                    Content = item.ToString(),
                    Tag = item,
                    ColorFill = new SolidColorBrush(item),
                });
            }

            this.color_box.SelectedIndex = 0;
        }

        private void ClickEvent(object sender, MouseButtonEventArgs e)
        {
            var img = e.Source as Image;
            if (img is null)
                return;

            if (object.ReferenceEquals(img, PacmanCeil) ||
                object.ReferenceEquals(img, this.ghosts[0].image) ||
                object.ReferenceEquals(img, this.ghosts[1].image) ||
                object.ReferenceEquals(img, this.ghosts[2].image) ||
                object.ReferenceEquals(img, this.ghosts[3].image))
                return;


            img.Source = this.CurrentImage!.Source;
            img.Tag = this.CurrentImage!.Tag;
        }

        public static BitmapImage GetImage(Walls Block, Color PenColor) => GetImage(new CacheKey(Block, PenColor));
        public static BitmapImage GetImage(CacheKey key)
        {
            if (cache.ContainsKey(key) && false)
                return cache[key];

            Bitmap image = new(256, 256);
            Graphics graphics = Graphics.FromImage(image);
            Pen pen = new(key.PenColor, 8);
            #region External
            if (key.Block.HasFlag(Walls.Top))
            {
                graphics.DrawLine(pen, 48, 32, 208, 32);
                if (key.Block.HasFlag(Walls.Right))
                    graphics.DrawArc(pen, 191, 32, 32, 32, 270, 90);
                else
                    graphics.DrawLine(pen, 208, 32, 256, 32);

                if (key.Block.HasFlag(Walls.Left))
                    graphics.DrawArc(pen, 32, 32, 32, 32, -90, -90);
                else
                    graphics.DrawLine(pen, 0, 32, 48, 32);
            }
            if (key.Block.HasFlag(Walls.Left))
            {
                graphics.DrawLine(pen, 32, 48, 32, 208);
                if (!key.Block.HasFlag(Walls.Bottom))
                    graphics.DrawLine(pen, 32, 48, 32, 256);
                if (!key.Block.HasFlag(Walls.Top))
                    graphics.DrawLine(pen, 32, 0, 32, 208);
            }
            if (key.Block.HasFlag(Walls.Bottom))
            {
                graphics.DrawLine(pen, 48, 224, 208, 224);
                if (key.Block.HasFlag(Walls.Right))
                    graphics.DrawArc(pen, 191, 191, 32, 32, -270, -90);
                else
                    graphics.DrawLine(pen, 208, 224, 256, 224);

                if (key.Block.HasFlag(Walls.Left))
                    graphics.DrawArc(pen, 32, 191, 32, 32, -270, 90);
                else
                    graphics.DrawLine(pen, 0, 224, 48, 224);
            }
            if (key.Block.HasFlag(Walls.Right))
            {
                graphics.DrawLine(pen, 224, 48, 224, 208);
                if (!key.Block.HasFlag(Walls.Bottom))
                    graphics.DrawLine(pen, 224, 48, 224, 256);
                if (!key.Block.HasFlag(Walls.Top))
                    graphics.DrawLine(pen, 224, 0, 224, 208);
            }
            #endregion

            #region internal
            if (key.Block.HasFlag(Walls.SmallTop))
            {
                graphics.DrawLine(pen, 80, 64, 176, 64);

                if (key.Block.HasFlag(Walls.SmallRight))
                    graphics.DrawArc(pen, 159, 64, 32, 32, 270, 90);
                else if (key.Block.HasFlag(Walls.Right))
                    graphics.DrawLine(pen, 176, 64, 192, 64);
                else
                    graphics.DrawLine(pen, 176, 64, 256, 64);


                if (key.Block.HasFlag(Walls.SmallLeft))
                    graphics.DrawArc(pen, 64, 64, 32, 32, -90, -90);
                else if (key.Block.HasFlag(Walls.Left))
                    graphics.DrawLine(pen, 64, 64, 80, 64);
                else
                    graphics.DrawLine(pen, 0, 64, 176, 64);
            }
            if (key.Block.HasFlag(Walls.SmallLeft))
            {
                graphics.DrawLine(pen, 64, 80, 64, 176);


                if (!key.Block.HasFlag(Walls.Bottom) && !key.Block.HasFlag(Walls.SmallBottom))
                    graphics.DrawLine(pen, 64, 176, 64, 256);
                else if (!key.Block.HasFlag(Walls.SmallBottom))
                    graphics.DrawLine(pen, 64, 176, 64, 192);

                if (!key.Block.HasFlag(Walls.Top) && !key.Block.HasFlag(Walls.SmallTop))
                    graphics.DrawLine(pen, 64, 0, 64, 80);
                else if (!key.Block.HasFlag(Walls.SmallTop))
                    graphics.DrawLine(pen, 64, 64, 64, 80);

            }
            if (key.Block.HasFlag(Walls.SmallBottom))
            {
                graphics.DrawLine(pen, 80, 192, 176, 192);

                if (key.Block.HasFlag(Walls.SmallRight))
                    graphics.DrawArc(pen, 159, 159, 32, 32, -270, -90);
                else if (key.Block.HasFlag(Walls.Right))
                    graphics.DrawLine(pen, 176, 192, 192, 192);
                else
                    graphics.DrawLine(pen, 176, 192, 256, 192);

                if (key.Block.HasFlag(Walls.SmallLeft))
                    graphics.DrawArc(pen, 64, 159, 32, 32, -270, 90);
                else if (key.Block.HasFlag(Walls.Left))
                    graphics.DrawLine(pen, 64, 192, 80, 192);
                else
                    graphics.DrawLine(pen, 0, 192, 80, 192);
            }
            if (key.Block.HasFlag(Walls.SmallRight))
            {
                graphics.DrawLine(pen, 192, 80, 192, 176);

                if (!key.Block.HasFlag(Walls.SmallBottom) && !key.Block.HasFlag(Walls.Bottom))
                    graphics.DrawLine(pen, 192, 176, 192, 256);
                else if (!key.Block.HasFlag(Walls.SmallBottom))
                    graphics.DrawLine(pen, 192, 176, 192, 192);

                if (!key.Block.HasFlag(Walls.SmallTop) && !key.Block.HasFlag(Walls.Top))
                    graphics.DrawLine(pen, 192, 80, 192, 0);
                else if (!key.Block.HasFlag(Walls.SmallTop))
                    graphics.DrawLine(pen, 192, 80, 192, 64);
            }
            #endregion

            #region External Curve
            // Circular
            //  ^ ->
            //  |  |
            //  <- x
            if (key.Block.HasFlag(Walls.CurveTop))
            {
                graphics.DrawArc(pen, 223, 0, 32, 32, 90, 90);
                graphics.DrawLine(pen, 224, 0, 224, 17);
                graphics.DrawLine(pen, 239, 32, 256, 32);
            }
            if (key.Block.HasFlag(Walls.CurveLeft))
            {
                graphics.DrawArc(pen, 0, 0, 32, 32, -270, -90);
                graphics.DrawLine(pen, 32, 0, 32, 17);
                graphics.DrawLine(pen, 0, 32, 17, 32);
            }
            if (key.Block.HasFlag(Walls.CurveBottom))
            {
                graphics.DrawArc(pen, 0, 223, 32, 33, 270, 90);
                graphics.DrawLine(pen, 32, 240, 32, 256);
                graphics.DrawLine(pen, 0, 224, 17, 224);
            }
            if (key.Block.HasFlag(Walls.CurveRight))
            {
                graphics.DrawArc(pen, 224, 224, 32, 32, 270, -90);
                graphics.DrawLine(pen, 224, 240, 224, 256);
                graphics.DrawLine(pen, 240, 224, 256, 224);
            }
            #endregion

            #region Internal Curve
            if (key.Block.HasFlag(Walls.SmallCurveTop))
            {
                graphics.DrawArc(pen, 191, 32, 32, 32, 90, 90);
                graphics.DrawLine(pen, 192, 0, 192, 49);
                graphics.DrawLine(pen, 256, 64, 207, 64);
            }
            if (key.Block.HasFlag(Walls.SmallCurveLeft))
            {
                graphics.DrawArc(pen, 32, 32, 32, 32, -270, -90);
                graphics.DrawLine(pen, 64, 0, 64, 49);
                graphics.DrawLine(pen, 0, 64, 49, 64);
            }
            if (key.Block.HasFlag(Walls.SmallCurveBottom))
            {
                graphics.DrawArc(pen, 32, 191, 32, 32, 270, 90);
                graphics.DrawLine(pen, 64, 207, 64, 256);
                graphics.DrawLine(pen, 0, 192, 49, 192);
            }
            if (key.Block.HasFlag(Walls.SmallCurveRight))
            {
                graphics.DrawArc(pen, 191, 191, 32, 32, 270, -90);
                graphics.DrawLine(pen, 192, 207, 192, 256);
                graphics.DrawLine(pen, 207, 192, 256, 192);
            }

            #endregion

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            MemoryStream buff = new();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            bitmapImage.StreamSource = buff;
            bitmapImage.EndInit();

            return cache[key] = bitmapImage;
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
            e.Handled = !(0 <= n && n <= ((((TextBox)sender).Tag.ToString() == "Y" ? Y_COUNT : X_COUNT) - 1));
        }

        private void OnNewWall(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Picker picker = new Picker();
            picker.ShowDialog();
            if (!picker.HasResult)
                return;

            var key = new CacheKey(Picker.Result, Picker.PenColor);
            if (images.Contains(key) || Picker.Result is Walls.Nothing)
            {
                ChoiceFiller((Image)((WrapPanel)viewer.Children[images.IndexOf(key) + SP_IMAGES_COUNT]).Children[0]);
                return;
            }

            images.Add(key);
            var tmp = new Image();
            RenderOptions.SetBitmapScalingMode(tmp, BitmapScalingMode.HighQuality);
            tmp.MouseLeftButtonDown += Setter;
            tmp.BeginInit();
            tmp.Source = Picker.Result is Walls.Nothing ? MainWindow.EmptyImage : GetImage(Picker.Result, Picker.PenColor);
            tmp.Tag = Picker.Result is Walls.Nothing ? Types.Tag.EMPTY : new Types.WallTag(Picker.Result, Picker.PenColor);
            tmp.EndInit();

            var wp = new WrapPanel();
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);
            ChoiceFiller(tmp);
        }

        private void ChoiceFiller(Image image)
        {
            if (this.filler_idx is not null)
                this.CurrentWP!.Background = null;

            this.filler_idx = this.viewer.Children.IndexOf((WrapPanel)image.Parent);
            this.CurrentWP!.Background = new SolidColorBrush(Colors.Wheat);

        }

        private void Setter(object sender, RoutedEventArgs e) => ChoiceFiller((Image)sender);

        private System.Drawing.Point Move(Key key, Image img)
        {
            var row = Grid.GetRow(img);
            var col = Grid.GetColumn(img);
            if (key is Key.S)
                row++;
            else if (key is Key.W)
                row--;
            else if (key is Key.A)
                col--;
            else
                col++;

            if (row == -1)
                row = 14;
            else if (row == 15)
                row = 0;

            if (col == 33)
                col = 0;
            else if (col == -1)
                col = 32;


            Grid.SetRow(MainWindow.FocusEffect, row);
            Grid.SetColumn(MainWindow.FocusEffect, col);
            Grid.SetZIndex(MainWindow.FocusEffect, -1);

            Grid.SetRow(img, row);
            Grid.SetColumn(img, col);


            return new(col, row);
        }

        private int LargestZIndexAt(int x, int y)
        {
            return this.game_grid.Children.OfType<Image>().Where(i => Grid.GetRow(i) == y && Grid.GetColumn(i) == x).Select(i => Grid.GetZIndex(i)).Max();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is not Key.S && e.Key is not Key.W && e.Key is not Key.A && e.Key is not Key.D)
                return;

            System.Drawing.Point r;

            if (this.ActiveImg is not null)
            {

                r = this.Move(e.Key, this.ActiveImg);

                if (ReferenceEquals(this.PacmanCeil, this.ActiveImg))
                    Grid.SetZIndex(this.PacmanCeil, LargestZIndexAt(r.X, r.Y) + 1);
                else
                {
                    foreach (var ghost in this.ghosts)
                        if (ReferenceEquals(ghost.image, this.ActiveImg))
                        {
                            Grid.SetZIndex(this.ActiveImg, LargestZIndexAt(r.X, r.Y) + 1);
                            ghost.RecPos(r.X, r.Y);
                            break;
                        }
                }

            }

            foreach (var ghost in this.ghosts)
            {
                if (ghost.IsInRec && !ReferenceEquals(ghost.image, this.ActiveImg))
                {
                    r = this.Move(e.Key, ghost.image);
                    Grid.SetZIndex(ghost.image, LargestZIndexAt(r.X, r.Y) + 1);
                    ghost.RecPos(r.X, r.Y);
                }
            }

            e.Handled = true;
        }

        private void SaveBtn(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void OutFocus(object sender, RoutedEventArgs e)
        {
            if (this.ActiveImg is null)
                return;

            if (!ReferenceEquals(((FrameworkElement)e.Source).Parent, game_grid))
            {
                e.Handled = true;
                MainWindow.FocusEffect.Visibility = Visibility.Hidden;
                this.ActiveImg = null;
            }
        }

        private void OnFocus(object sender, RoutedEventArgs e)
        {

            e.Handled = true;
            if (object.ReferenceEquals(e.Source, FocusEffect))
                return;

            this.ActiveImg = (Image)e.Source;
            this.ActiveImg.Focus();
            MainWindow.FocusEffect.Visibility = Visibility.Visible;
            Grid.SetColumn(MainWindow.FocusEffect, Grid.GetColumn(this.ActiveImg));
            Grid.SetRow(MainWindow.FocusEffect, Grid.GetRow(this.ActiveImg));
        }

        private void ShowGridLines(object sender, RoutedEventArgs e)
        {
            this.game_grid.ShowGridLines = true;
        }

        private void HideGridLines(object sender, RoutedEventArgs e)
        {
            this.game_grid.ShowGridLines = false;
        }

        private void ChangeBackground(object sender, RoutedEventArgs e)
        {
            var color = (System.Windows.Media.Color)((sender as ComboBox)!.SelectedItem as ComboBoxItemColor)!.Tag;
            this.game_grid.Background = new SolidColorBrush(color);
        }
    }
}
