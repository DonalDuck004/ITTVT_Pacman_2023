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

using Pen = System.Drawing.Pen;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Microsoft.Win32;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    [Flags]
    public enum Walls
    {
        Nothing =           0b000000000000000,

        Top =               0b000000000000001,
        Left =              0b000000000000010,
        Right =             0b000000000000100,
        Bottom =            0b000000000001000,
        
        SmallTop =          0b000000000010000,
        SmallLeft =         0b000000000100000,
        SmallRight =        0b000000001000000,
        SmallBottom =       0b000000010000000,


        CurveTop =          0b000000100000000,
        CurveLeft =         0b000001000000000,
        CurveRight =        0b000010000000000,
        CurveBottom =       0b000100000000000,


        SmallCurveTop =    0b0001000000000000,
        SmallCurveLeft =   0b0010000000000000,
        SmallCurveRight =  0b0100000000000000,
        SmallCurveBottom = 0b1000000000000000,
    }

    public enum Tags
    {
        Wall,
        PacDot,
        PowerPellet,
        Empty,
        Unspawnable,
        Gate
    }

    public record class Tag(Tags tag)
    {
        public static Tag PAC_DOT = new(Tags.PacDot);
        public static Tag POWER_PELLET = new(Tags.PowerPellet);
        public static Tag EMPTY = new(Tags.Empty);
        public static Tag GATE = new(Tags.Gate);
        public static Tag UNSPAWNABLE = new(Tags.Unspawnable);
    }

    public record WallTag(Walls wall, Color color) : Tag(Tags.Wall);

    public class GhostData
    {
        public Image image;
        public GhostEngines engine;
        public List<System.Drawing.Point>? positions;

        public GhostData(Image image,
                        GhostEngines engine, 
                        List<System.Drawing.Point>? positions)
        {
            this.image = image;
            this.engine = engine;
            this.positions = positions;
        }
    }

    public enum GhostColors
    {
        Red,
        Pink,
        Cyan,
        Orange
    }
    public partial class MainWindow : Window
    {
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
        public static BitmapImage EmptyImage = new();
        public static BitmapImage UnspawnableImage = new();
        public static BitmapImage PowerPelletImage = new(new Uri(DRUG_PATH, UriKind.Relative));
        public static BitmapImage PacDotImage = new(new Uri(POINT_PATH, UriKind.Relative));
        public static BitmapImage GateImage = new(new Uri(GATE_PATH, UriKind.Relative));
        public static Image FocusEffect = new();


        private GhostData[] ghosts = new GhostData[4];
        private List<CacheKey> images = new();
        private Image? ActiveImg = null;


        public MainWindow()
        {
            MainWindow.INSTANCE = this;
            InitializeComponent();
            game_ceils = this.game_grid.Children.OfType<Image>().Split(33).ToArray();

            Random rnd = new Random();
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
                    col.Tag = WorldsBuilderWPF.Tag.EMPTY;
                }
            }

            this.PacmanCeil = new Image() { Source = Pacman.Source };
            game_grid.Children.Add(this.PacmanCeil);
            this.PacmanCeil.MouseLeftButtonDown += OnFocus;
            Grid.SetColumn(this.PacmanCeil, 0);
            Grid.SetZIndex(this.PacmanCeil, 1);

            this.ghosts[0] = new(new(), GhostEngines.NoCachedAutoMover, null); // TODO creare oggetto Image, mettere sopra
            this.ghosts[0].image.Source = Red.Source;
            this.ghosts[0].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[0].image);
            Grid.SetColumn(this.ghosts[0].image, 1);


            this.ghosts[1] = new(new(), GhostEngines.NoCachedAutoMover, null);
            this.ghosts[1].image.Source = Pink.Source;
            this.ghosts[1].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[1].image);
            Grid.SetColumn(this.ghosts[1].image, 2);

            this.ghosts[2] = new(new(), GhostEngines.NoCachedAutoMover, null);
            this.ghosts[2].image.Source = Cyan.Source;
            this.ghosts[2].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[2].image);
            Grid.SetColumn(this.ghosts[2].image, 3);

            this.ghosts[3] = new(new(), GhostEngines.NoCachedAutoMover, null);
            this.ghosts[3].image.Source = Orange.Source;
            this.ghosts[3].image.MouseLeftButtonDown += OnFocus;
            game_grid.Children.Add(this.ghosts[3].image);
            Grid.SetColumn(this.ghosts[3].image, 4);

            this.filler_idx = 3;

            var wp = new WrapPanel();
            var tmp = new Image() { Source = PowerPelletImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = WorldsBuilderWPF.Tag.POWER_PELLET;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);

            wp = new WrapPanel();
            tmp = new Image() { Source = PacDotImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = WorldsBuilderWPF.Tag.PAC_DOT;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);

            wp = new WrapPanel();
            tmp = new Image() { Source = GateImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = WorldsBuilderWPF.Tag.GATE;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);

            wp = new WrapPanel();
            tmp = new Image() { Source = EmptyImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = WorldsBuilderWPF.Tag.EMPTY;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp); 
            ChoiceFiller(tmp);


            image = new(256, 256);
            var graphics = Graphics.FromImage(image);
            Pen pen = new(Color.Red, 12);
            graphics.DrawLine(pen, 0, 0, 255, 255);
            graphics.DrawLine(pen, 255, 0, 255, 0);
            UnspawnableImage.BeginInit();
            buff = new MemoryStream();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            UnspawnableImage.StreamSource = buff;
            UnspawnableImage.EndInit();

            wp = new WrapPanel();
            tmp = new Image() { Source = UnspawnableImage };
            tmp.MouseLeftButtonDown += Setter;
            tmp.Tag = WorldsBuilderWPF.Tag.UNSPAWNABLE;
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);
            ChoiceFiller(tmp);
        }

        private void ClickEvent(object sender, MouseButtonEventArgs e)
        {
            var img = (Image)e.Source;
            if (PacmanDialog.SINGLETON is not null && !PacmanDialog.SINGLETON.IsHidden)
            {
                if (object.ReferenceEquals(e.Source, this.ghosts[0].image) ||
                    object.ReferenceEquals(e.Source, this.ghosts[1].image) ||
                    object.ReferenceEquals(e.Source, this.ghosts[2].image) ||
                    object.ReferenceEquals(e.Source, this.ghosts[3].image))
                        return;

                PacmanDialog.SINGLETON.Activate();
                PacmanDialog.SINGLETON.SetPos(Grid.GetColumn(img), Grid.GetRow(img));
                return;
            }

            if (object.ReferenceEquals(img, PacmanCeil)    ||
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
                else if(!key.Block.HasFlag(Walls.SmallTop))
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

        private void ChoiceFiller(Image image)
        {
            if (this.filler_idx is not null)
                this.CurrentWP!.Background = null;

            this.filler_idx = this.viewer.Children.IndexOf((WrapPanel)image.Parent);
            this.CurrentWP!.Background = new SolidColorBrush(Colors.Wheat);
        }

        private void Setter(object sender, RoutedEventArgs e) => ChoiceFiller((Image)sender);

        private void PacmanSetPos(object sender, MouseButtonEventArgs e)
        {
            if (PacmanDialog.SINGLETON is not null)
            {
                PacmanDialog.SINGLETON.Reload();
                return;
            }

            var tmp = new PacmanDialog(SetPacman);
            tmp.Show();
        }

        private void SetPacman()
        {
#nullable disable
            this.Activate();
            if (PacmanDialog.SINGLETON.X is not null && PacmanDialog.SINGLETON.Y is not null)
            {
                Grid.SetColumn(this.PacmanCeil, (int)PacmanDialog.SINGLETON.X);
                Grid.SetRow(this.PacmanCeil, (int)PacmanDialog.SINGLETON.Y);
            }
            var transform = Matrix.Identity;
            transform.RotateAt(PacmanDialog.SINGLETON.D * 90, 0.5, 0.5);
            this.PacmanCeil.LayoutTransform = new MatrixTransform(transform);

            PacmanDialog.SINGLETON.Activate();
#nullable restore
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Picker picker = new Picker();
            picker.ShowDialog();
            if (!picker.HasResult)
                return;

            var key = new CacheKey(Picker.Result, Picker.PenColor);
            if (images.Contains(key) // TODO Colore
                || Picker.Result == Walls.Nothing)
            {
                ChoiceFiller((Image)((WrapPanel)viewer.Children[images.IndexOf(key) + 4]).Children[0]);
                return;
            }

            images.Add(key);
            var tmp = new Image();
            RenderOptions.SetBitmapScalingMode(tmp, BitmapScalingMode.HighQuality);
            tmp.MouseLeftButtonDown += Setter;
            tmp.BeginInit();
            tmp.Source = Picker.Result is Walls.Nothing ? MainWindow.EmptyImage : GetImage(Picker.Result, Picker.PenColor); 
            tmp.Tag = Picker.Result is Walls.Nothing ? WorldsBuilderWPF.Tag.EMPTY : new WallTag(Picker.Result, Picker.PenColor);
            tmp.EndInit();

            var wp = new WrapPanel();
            wp.Children.Add(tmp);
            viewer.Children.Add(wp);
            filler_idx = viewer.Children.Count - 1;
            ChoiceFiller(tmp);
        }

        private System.Drawing.Point Move(Key key, Image img)
        {
            var row = Grid.GetRow(img);
            var col = Grid.GetColumn(img);
            if (key is Key.Down)
                row++;
            else if (key is Key.Up)
                row--;
            else if (key is Key.Left)
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

            Grid.SetRow(img, row);
            Grid.SetColumn(img, col);

            Grid.SetRow(MainWindow.FocusEffect, row);
            Grid.SetColumn(MainWindow.FocusEffect, col);

            return new(col, row);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.Left && e.Key != Key.Right)
                return;

            System.Drawing.Point r;

            if (this.ActiveImg is not null)
            {

                r = this.Move(e.Key, this.ActiveImg);

                if (ReferenceEquals(this.Pacman, this.ActiveImg))
                    Grid.SetZIndex(MainWindow.FocusEffect, Grid.GetZIndex(MainWindow.FocusEffect) + 1);
                else
                {
                    foreach (var item in GhostDialog.SINGLETONS.Values)
                        if (ReferenceEquals(this.ghosts[item.ArrayIdx].image, this.ActiveImg))
                        {
                            Grid.SetZIndex(this.ActiveImg, Grid.GetZIndex(this.ActiveImg) + 1);
                            if (item.Listening)
                                item.RecPos(r.X, r.Y);
                        }
                }

            }

            foreach (var item in GhostDialog.SINGLETONS.Values)
            {
                if (item.Listening && !ReferenceEquals(this.ghosts[item.ArrayIdx].image, this.ActiveImg))
                {
                    r = this.Move(e.Key, this.ghosts[item.ArrayIdx].image);
                    Grid.SetZIndex(this.ghosts[item.ArrayIdx].image, Grid.GetZIndex(this.ghosts[item.ArrayIdx].image) + 1);
                    item.RecPos(r.X, r.Y);
                }
            }
        }

        private void RecordGhost(object sender, MouseButtonEventArgs e)
        {
            string name = ((Image)sender).Name;
            var color = (GhostColors)((Image)sender).Tag;
            GhostDialog? dialog;

            if (GhostDialog.SINGLETONS.TryGetValue(color, out dialog))
            {
                dialog.Activate();
                return;
            }

            dialog = new GhostDialog(color, OnGhostDialogClosed);
            dialog.Show();
        }

        public void OnGhostDialogClosed(GhostDialog ghost)
        {
            this.ghosts[(int)ghost.Color].engine = ghost.CurrentEngine;
            this.ghosts[(int)ghost.Color].positions = ghost.positions;
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
            
            this.ActiveImg = (Image)e.Source;

            MainWindow.FocusEffect.Visibility = Visibility.Visible;
            Grid.SetColumn(MainWindow.FocusEffect, Grid.GetColumn(this.ActiveImg));
            Grid.SetRow(MainWindow.FocusEffect, Grid.GetRow(this.ActiveImg));
        }

        private void OnLoadClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new() { Multiselect = false, CheckFileExists = true, CheckPathExists = true };
            dialog.ShowDialog();
            if (dialog.SafeFileName == "")
                return;

            BinaryReader reader = new(new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read));
            Grid.SetRow(this.PacmanCeil, reader.ReadInt32());
            Grid.SetRow(this.PacmanCeil, reader.ReadInt32());
            reader.ReadInt32(); // TODO: Rotation
            int i = 0;
            Walls walls;
            Color color;

            foreach (var item in this.game_grid.Children.OfType<Image>())
            {

                if (object.ReferenceEquals(item, this.ghosts[0].image) ||
                    object.ReferenceEquals(item, this.ghosts[1].image) ||
                    object.ReferenceEquals(item, this.ghosts[2].image) ||
                    object.ReferenceEquals(item, this.ghosts[3].image) ||
                    object.ReferenceEquals(item, this.PacmanCeil) ||
                    object.ReferenceEquals(item, FocusEffect))
                    continue;

                switch (reader.ReadInt32())
                {
                    case -1:
                        item.Source = MainWindow.PacDotImage;
                        item.Tag = WallTag.PAC_DOT;
                        break;
                    case -2:
                        item.Source = MainWindow.PowerPelletImage;
                        item.Tag = WallTag.POWER_PELLET;
                        break;
                    case -3:
                        item.Source = MainWindow.EmptyImage;
                        item.Tag = WallTag.EMPTY;
                        break;
                    case -4:
                        walls = (Walls)reader.ReadInt32();
                        color = Color.FromArgb(reader.ReadByte(),
                                               reader.ReadByte(),
                                               reader.ReadByte());
                        item.Source = MainWindow.GetImage(walls, color);
                        item.Tag = new WallTag(walls, color);
                        break;
                    case -5:
                        item.Source = MainWindow.GateImage;
                        item.Tag = WallTag.GATE;
                        break;
                    case -6:
                        item.Source = MainWindow.UnspawnableImage;
                        item.Tag = WallTag.UNSPAWNABLE;
                        break;
                    default:
                        continue;
                        // throw new Exception();
                }
                i++;

            }
            reader.Close();
        }

        private record World(byte[] byte_map, string title, string[] tags, byte[] preview);

        private void UpLoad(object sender, RoutedEventArgs e)
        {
            var window = new InputWindow();
            if (window.ShowDialog() is false)
                return;

            var title = window.title_box.Text;
            var tags = window.tags_box.Text.Split("; ");

            HttpClient client = new();

            BinaryWriter st = new(new MemoryStream());
            this.DumpWorld(st);
            st.BaseStream.Position = 0;
           
            var world = new World(((MemoryStream)st.BaseStream).ToArray(), title, tags, this.GetWindowScreen());
            st.Close();
            var content = new StringContent(JsonSerializer.Serialize(world), Encoding.UTF8, "application/json");

            var a = client.PostAsync("http://localhost:8000/add_world", content).Result;
        }

        public byte[] GetWindowScreen()
        {

            RenderTargetBitmap renderTargetBitmap = new((int)this.game_grid.ActualWidth, (int)this.game_grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(this.game_grid);
            PngBitmapEncoder pngImage = new();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (var stream = new MemoryStream())
            {
                pngImage.Save(stream);
                return stream.ToArray();
            }
        }

        public void DumpWorld(BinaryWriter output_stream)
        {

            output_stream.Write(Grid.GetColumn(this.PacmanCeil));
            output_stream.Write(Grid.GetRow(this.PacmanCeil));
            output_stream.Write(PacmanDialog.SINGLETON is null ? 0 : PacmanDialog.SINGLETON.D);

            int c = 0;
            foreach (var field in this.game_grid.Children.OfType<Image>())
            {
                if (object.ReferenceEquals(field, this.ghosts[0].image) ||
                    object.ReferenceEquals(field, this.ghosts[1].image) ||
                    object.ReferenceEquals(field, this.ghosts[2].image) ||
                    object.ReferenceEquals(field, this.ghosts[3].image) ||
                    object.ReferenceEquals(field, this.PacmanCeil) || 
                    object.ReferenceEquals(field, MainWindow.FocusEffect))
                    continue;
                c++;


                if (((Tag)field.Tag).tag is Tags.PacDot)
                    output_stream.Write(-1);
                else if (((Tag)field.Tag).tag is Tags.PowerPellet)
                    output_stream.Write(-2);
                else if (((Tag)field.Tag).tag is Tags.Empty)
                    output_stream.Write(-3);
                else if (field.Tag is WallTag wt)
                {
                    output_stream.Write(-4);
                    output_stream.Write((int)wt.wall);
                    output_stream.Write(wt.color.R);
                    output_stream.Write(wt.color.G);
                    output_stream.Write(wt.color.B);
                }
                else if (((Tag)field.Tag).tag is Tags.Gate)
                    output_stream.Write(-5);
                else if (((Tag)field.Tag).tag is Tags.Unspawnable)
                    output_stream.Write(-6);
                else
                    throw new Exception("Invalid field");
            }

            foreach (var ghost in this.ghosts)
            {
                output_stream.Write((int)ghost.engine);

                if (ghost.engine.SupportsSchema())
                {
                    Debug.Assert(ghost.positions is not null);
                    output_stream.Write(ghost.positions.Count);
                    foreach (var item in ghost.positions)
                    {
                        output_stream.Write(item.X);
                        output_stream.Write(item.Y);
                    }
                }
                else
                {
                    output_stream.Write(Grid.GetColumn(ghost.image));
                    output_stream.Write(Grid.GetRow(ghost.image));
                }

            }
        }


        public void OnClosed(object sender, EventArgs e)
        {
            if (PacmanDialog.SINGLETON is not null)
                PacmanDialog.SINGLETON.Close();

            Application.Current.Shutdown(0);
        }

        public void Save()
        {
            SaveFileDialog dialog = new();
            dialog.ShowDialog();
            if (dialog.SafeFileName == "")
                return;

            BinaryWriter stream = new(new FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.Write));

            this.DumpWorld(stream);
            stream.Close();
        }
    }
}
