using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PacManWPF.Game.PGs;
using System.Windows.Controls;

namespace PacManWPF.Utils
{

    public static class Extensions
    {
        public static bool IsDrug(this Rectangle? ceil)
        {
            return ceil is not null && ceil.Fill is not null &&
                ((BitmapImage)((ImageBrush)ceil.Fill).ImageSource).UriSource == ((BitmapImage)ResourcesLoader.Drug.ImageSource).UriSource;
        }
        public static bool IsDrug(this ImageBrush? ceil)
        {
            return ceil is not null && ((BitmapImage)ceil.ImageSource).UriSource == ((BitmapImage)ResourcesLoader.Drug.ImageSource).UriSource;
        }

        public static bool IsPoint(this Rectangle? ceil)
        {
            return ceil is not null && ceil.Fill is not null &&
                ((BitmapImage)((ImageBrush)ceil.Fill).ImageSource).UriSource == ((BitmapImage)ResourcesLoader.SmallPoint.ImageSource).UriSource;
        }

        public static bool IsEmpty(this ImageBrush? ceil)
        {
            return ceil is null;
        }

        public static bool IsEmpty(this Rectangle? ceil)
        {
            return ceil is null || ceil.Fill is null;
        }

        public static bool IsPoint(this ImageBrush? ceil)
        {

            return ceil is not null && ((BitmapImage)ceil.ImageSource).UriSource == ((BitmapImage)ResourcesLoader.SmallPoint.ImageSource).UriSource;
        }

        public static bool IsPacman(this Rectangle? ceil)
        {
            if (Pacman.INSTANCE is null)
                throw new Exception("Pacman not instantiated");

            return ceil is not null && ceil.Fill is not null &&
               ((BitmapImage)((ImageBrush)ceil.Fill).ImageSource).UriSource == ((BitmapImage)((ImageBrush)Pacman.INSTANCE.CeilObject.Fill).ImageSource).UriSource;
        }

        public static bool IsPacman(this ImageBrush? ceil)
        {
            if (Pacman.INSTANCE is null)
                throw new Exception("Pacman not instantiated");

            return ceil is not null &&
               ((BitmapImage)ceil.ImageSource).UriSource == ((BitmapImage)((ImageBrush)Pacman.INSTANCE.CeilObject.Fill).ImageSource).UriSource;
        }


        public static bool IsScariedGhost(this Rectangle? ceil)
        {
            return ceil is not null && ceil.Fill is not null &&
                ((BitmapImage)((ImageBrush)ceil.Fill).ImageSource).UriSource == ((BitmapImage)ResourcesLoader.ScaryGhost.ImageSource).UriSource;
        }

        public static bool IsScariedGhost(this ImageBrush? ceil)
        {
            return ceil is not null && ((BitmapImage)ceil.ImageSource).UriSource == ((BitmapImage)ResourcesLoader.ScaryGhost.ImageSource).UriSource;
        }

        public static bool IsDiedGhost(this Rectangle? ceil)
        {
            return ceil is not null && ceil.Fill is not null &&
                ((BitmapImage)((ImageBrush)ceil.Fill).ImageSource).UriSource == ((BitmapImage)ResourcesLoader.GhostEyes.ImageSource).UriSource;
        }

        public static bool IsDiedGhost(this ImageBrush? ceil)
        {
            return ceil is not null && ((BitmapImage)ceil.ImageSource).UriSource == ((BitmapImage)ResourcesLoader.GhostEyes.ImageSource).UriSource;
        }

        public static bool IsWall(this Rectangle? ceil)
        {
            return ceil is not null && IsWall((ImageBrush)ceil.Fill);
        }

        public static bool IsWall(this ImageBrush? ceil)
        {
            if (ceil is null)
                return false;

            var tmp = ((BitmapImage)ceil.ImageSource).StreamSource;
            return tmp is not null && tmp.GetType() == typeof(WallStream);
        }

        public static Rectangle CeilAt(this Rectangle[][] array, System.Drawing.Point point) => array.CeilAt((int)point.X, (int)point.Y);


        public static Rectangle CeilAt(this Rectangle[][] array, int x, int y)
        {
            return array[y][x];
        }


        public static string ZFill(this string src, int len, char filler = '0')
        {
            if (src.Length > len)
                return src;

            return new string(filler, len - src.Length) + src;
        }

        public static IEnumerable<T[]> Split<T>(this IEnumerable<T> array, int size)
        {
            T[] rt = array.ToArray();
            int to = rt.Length / size;

            for (int i = 0; i < to; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }

        public static Rectangle? RectangleAtCeil(this Grid grid, System.Drawing.Point point) => RectangleAtCeil(grid, point.X, point.Y);
        public static Rectangle? RectangleAtCeil(this Grid grid, int x, int y)
        {
            return grid.Children[y * Config.CHUNK_WC + x] as Rectangle;
        }
    }
}
