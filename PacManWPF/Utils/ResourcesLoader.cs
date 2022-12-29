using System;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using Color = System.Windows.Media.Color;
using ColorD = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace PacManWPF.Utils
{
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

    class WallStream : MemoryStream
    {
    }


    static class ResourcesLoader
    {
        public const string CyanGhost = @"Images\cyan.png";
        public const string PinkGhost = @"Images\pink.png";
        public const string RedGhost = @"Images\red.png";
        public const string OrangeGhost = @"Images\orange.png";

        public const string GHOST_EYES_PATH = @"Images\eyes.png";
        public const string DRUG_PATH = @"Images\Drug.png";
        public const string PACMAN_PATH = @"Images\pacman_open.png";
        public const string PACMAN_P_CLOSED_PATH = @"Images\pacman_partially_closed.png";
        public const string PACMAN_CLOSED_PATH = @"Images\pacman_closed.png";
        public const string SMALL_POINT_PATH = @"Images\Point.png";
        public const string SCARY_GHOST_PATH = @"Images\dead_ghost.png";

        private static Dictionary<CacheKey, ImageBrush> walls_cache = new();

        public static string[] PacManAnimationPaths = { PACMAN_PATH, PACMAN_P_CLOSED_PATH, PACMAN_CLOSED_PATH };


        private static Dictionary<string, ImageBrush> cache = new();

        public static ImageBrush GhostEyes
        {
            get => GetImage(GHOST_EYES_PATH);

        }

        public static ImageBrush PacMan
        {
            get => GetImage(PACMAN_PATH);
        }
        // static Random rnd = new Random();

        public static ImageBrush ScaryGhost
        {
            get => GetImage(SCARY_GHOST_PATH); // Paths[rnd.Next(Paths.Length)]);
        }

        public static ImageBrush SmallPoint
        {
            get => GetImage(SMALL_POINT_PATH);
        }

        public static ImageBrush Drug
        {
            get => GetImage(DRUG_PATH);
        }
        public record CacheKey(Walls Block, ColorD PenColor);
       
        public static ImageBrush GetImage(Walls Block, ColorD PenColor) => GetImage(new CacheKey(Block, PenColor));

        public static ImageBrush GetImage(CacheKey key)
        {
            if (walls_cache.ContainsKey(key) && false)
                return walls_cache[key];

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

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            WallStream buff = new();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            bitmapImage.StreamSource = buff;
            bitmapImage.EndInit();

            return walls_cache[key] = new ImageBrush(bitmapImage);
        }

        public static ImageBrush GetImage(string name)
        {
            if (!cache.ContainsKey(name))
                cache[name] = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(name, UriKind.Relative)),
                };

            return cache[name];
        }
    }
}
