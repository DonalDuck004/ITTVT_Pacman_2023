using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Forms;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Logica di interazione per Picker.xaml
    /// </summary>
    public partial class Picker : Window
    {
        public static Walls Result = Walls.Nothing;
        public static System.Drawing.Color PenColor = System.Drawing.Color.Blue;
        public bool HasResult = false;

        public Picker()
        {
            InitializeComponent();

            if (Result is not Walls.Nothing) {
                if (Result.HasFlag(Walls.Top))
                    this.top_flag.IsChecked = true;
                if (Result.HasFlag(Walls.Right))
                    this.right_flag.IsChecked = true;
                if (Result.HasFlag(Walls.Left))
                    this.left_flag.IsChecked = true;
                if (Result.HasFlag(Walls.Bottom))
                    this.bottom_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallTop))
                    this.small_top_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallRight))
                    this.small_right_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallLeft))
                    this.small_left_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallBottom))
                    this.small_bottom_flag.IsChecked = true;
                if (Result.HasFlag(Walls.CurveTop))
                    this.curve_top_flag.IsChecked = true;
                if (Result.HasFlag(Walls.CurveRight))
                    this.curve_right_flag.IsChecked = true;
                if (Result.HasFlag(Walls.CurveLeft))
                    this.curve_left_flag.IsChecked = true;
                if (Result.HasFlag(Walls.CurveBottom))
                    this.curve_bottom_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallCurveTop))
                    this.small_curve_top_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallCurveRight))
                    this.small_curve_right_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallCurveLeft))
                    this.small_curve_left_flag.IsChecked = true;
                if (Result.HasFlag(Walls.SmallCurveBottom))
                    this.small_curve_bottom_flag.IsChecked = true;
            }
            UpdateImage();
        }


        private void UpdateImage()
        {
            Result = Walls.Nothing;
            if (this.top_flag.IsChecked is true)
                Result |= Walls.Top;
            if (this.right_flag.IsChecked is true)
                Result |= Walls.Right;
            if (this.left_flag.IsChecked is true)
                Result |= Walls.Left;
            if (this.bottom_flag.IsChecked is true)
                Result |= Walls.Bottom;

            if (this.small_top_flag.IsChecked is true)
                Result |= Walls.SmallTop;
            if (this.small_right_flag.IsChecked is true)
                Result |= Walls.SmallRight;
            if (this.small_left_flag.IsChecked is true)
                Result |= Walls.SmallLeft;
            if (this.small_bottom_flag.IsChecked is true)
                Result |= Walls.SmallBottom;

            if (this.curve_top_flag.IsChecked is true)
                Result |= Walls.CurveTop;
            if (this.curve_right_flag.IsChecked is true)
                Result |= Walls.CurveRight;
            if (this.curve_left_flag.IsChecked is true)
                Result |= Walls.CurveLeft;
            if (this.curve_bottom_flag.IsChecked is true)
                Result |= Walls.CurveBottom;

            if (this.small_curve_top_flag.IsChecked is true)
                Result |= Walls.SmallCurveTop;
            if (this.small_curve_right_flag.IsChecked is true)
                Result |= Walls.SmallCurveRight;
            if (this.small_curve_left_flag.IsChecked is true)
                Result |= Walls.SmallCurveLeft;
            if (this.small_curve_bottom_flag.IsChecked is true)
                Result |= Walls.SmallCurveBottom;

            target_image.Source = MainWindow.GetImage(Result, PenColor);
        }

        private void CheckBoxClicked(object sender, RoutedEventArgs e) => UpdateImage();
        
        private void ColorChoice(object sender, RoutedEventArgs e)
        {
            var cd = new ColorDialog()
            {
                FullOpen = true,
                Color = PenColor
            };

            cd.ShowDialog();
            PenColor = cd.Color;
            UpdateImage();
        }


        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            this.HasResult = true;
            this.Close();
        }

        private void ClearPreview(object sender, RoutedEventArgs e)
        {
            this.top_flag.IsChecked = false;
            this.right_flag.IsChecked = false;
            this.left_flag.IsChecked = false;
            this.bottom_flag.IsChecked = false;
            this.small_top_flag.IsChecked = false;
            this.small_right_flag.IsChecked = false;
            this.small_left_flag.IsChecked = false;
            this.small_bottom_flag.IsChecked = false;
            this.curve_top_flag.IsChecked = false;
            this.curve_right_flag.IsChecked = false;
            this.curve_left_flag.IsChecked = false;
            this.curve_bottom_flag.IsChecked = false;
            this.small_curve_top_flag.IsChecked = false;
            this.small_curve_right_flag.IsChecked = false;
            this.small_curve_left_flag.IsChecked = false;
            this.small_curve_bottom_flag.IsChecked = false;
            UpdateImage();
        }
    }
}
