using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using ColorD = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using PacManWPF.Game.PGs.Enums;


namespace PacManWPF.Utils
{

    static class ResourcesLoader
    {
        public const string CyanGhost = @"Images\cyan.png";
        public const string PinkGhost = @"Images\pink.png";
        public const string RedGhost = @"Images\red.png";
        public const string OrangeGhost = @"Images\orange.png";

        public const string GHOST_EYES_PATH = @"Images\eyes.png";
        public const string POWER_PELLET_PATH = @"Images\PowerPellet.png";
        public const string CHERRY_PATH = @"Images\Cherry.png";
        public const string STRAWBERRY_PATH = @"Images\Strawberry.png";
        public const string PEACH_PATH = @"Images\Peach.png";
        public const string GATE_PATH = @"Images\Gate.png";
        public const string PACMAN_PATH = @"Images\pacman.gif";
        public const string PAC_DOT_PATH = @"Images\PacDot.png";
        public const string SCARY_GHOST_PATH = @"Images\dead_ghost.png";
        public const string GALAXIAN_PATH = @"Images\Galaxian.png";
        public const string APPLE_PATH = @"Images\Apple.png";
        public const string MELON_PATH = @"Images\Melon.png";
        public const string BELL_PATH = @"Images\Bell.png";
        public const string KEY_PATH = @"Images\Key.png";

        private static Dictionary<CacheKey, BitmapImage> walls_cache = new();

        private static Dictionary<string, BitmapImage> cache = new();

        public static BitmapImage GhostEyes = GetImage(GHOST_EYES_PATH);

        public static BitmapImage PacMan = GetImage(PACMAN_PATH);

        public static BitmapImage ScaryGhost = GetImage(SCARY_GHOST_PATH);

        public static BitmapImage PacDot = GetImage(PAC_DOT_PATH);

        public static BitmapImage Peach = GetImage(PEACH_PATH);

        public static BitmapImage PowerPellet = GetImage(POWER_PELLET_PATH);

        public static BitmapImage Cherry = GetImage(CHERRY_PATH);

        public static BitmapImage Galaxian = GetImage(GALAXIAN_PATH);

        public static BitmapImage Apple = GetImage(APPLE_PATH);

        public static BitmapImage Melon = GetImage(MELON_PATH);

        public static BitmapImage Bell = GetImage(BELL_PATH);

        public static BitmapImage Key = GetImage(KEY_PATH);

        public static BitmapImage Strawberry = GetImage(STRAWBERRY_PATH);

        public static BitmapImage Gate = GetImage(GATE_PATH);

        public static BitmapImage EmptyImage = new();

        static ResourcesLoader()
        {
            Bitmap image = new(256, 256);
            MemoryStream buff = new();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            ResourcesLoader.EmptyImage.BeginInit();
            ResourcesLoader.EmptyImage.StreamSource = buff;
            ResourcesLoader.EmptyImage.EndInit();
        }

        public record CacheKey(Walls Block, ColorD PenColor);

        public static BitmapImage GetImage(GhostColors type)
        {
            return ResourcesLoader.GetImage(type is GhostColors.Cyan ? ResourcesLoader.CyanGhost :
                                            type is GhostColors.Pink ? ResourcesLoader.PinkGhost :
                                            type is GhostColors.Red  ? ResourcesLoader.RedGhost  :
                                                                       ResourcesLoader.OrangeGhost);
        }


        public static BitmapImage GetImage(Walls Block, ColorD PenColor) => GetImage(new CacheKey(Block, PenColor));

        public static BitmapImage GetImage(FoodTypes type)
        {
            if (type is FoodTypes.PacDot)
                return PacDot;
            else if (type is FoodTypes.PowerPellet)
                return PowerPellet;
            else if (type is FoodTypes.Cherry)
                return Cherry;
            else if (type is FoodTypes.Strawberry)
                return Strawberry;
            else if (type is FoodTypes.Peach)
                return Peach;
            else if (type is FoodTypes.Apple)
                return Apple;
            else if (type is FoodTypes.Melon)
                return Melon;
            else if (type is FoodTypes.Galaxian)
                return Galaxian;
            else if (type is FoodTypes.Bell)
                return Bell;
            else
                return Key;
        }


        public static BitmapImage GetImage(CacheKey key)
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
            MemoryStream buff = new();
            image.Save(buff, ImageFormat.Png);
            buff.Position = 0;
            bitmapImage.StreamSource = buff;
            bitmapImage.EndInit();

            return walls_cache[key] = bitmapImage;
        }

        public static BitmapImage GetImage(string name)
        {
            if (!cache.ContainsKey(name))
            {
                cache[name] = new BitmapImage();
                cache[name].BeginInit();
                cache[name].StreamSource = new FileStream(name, FileMode.Open, FileAccess.Read);
                cache[name].EndInit();
            }

            return cache[name];
        }
    }
}
