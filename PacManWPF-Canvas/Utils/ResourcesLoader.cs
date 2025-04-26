using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;


using UIPath = System.Windows.Shapes.Path;

namespace PacManWPF_Canvas.Utils
{
    static class ResourcesLoader
    {
        public const string GHOST_EYES_PATH = @"Assets\Images\eyes.png";
        public const string POWER_PELLET_PATH = @"Assets\Images\PowerPellet.png";
        public const string CHERRY_PATH = @"Assets\Images\Cherry.png";
        public const string STRAWBERRY_PATH = @"Assets\Images\Strawberry.png";
        public const string PEACH_PATH = @"Assets\Images\Peach.png";
        public const string GATE_PATH = @"Assets\Images\gate.png";
        public const string PACMAN_PATH = @"Assets\Images\pacman.gif";
        public const string PAC_DOT_PATH = @"Assets\Images\PacDot.png";
        public const string SCARY_GHOST_PATH = @"Assets\Images\dead_ghost.png";
        public const string GALAXIAN_PATH = @"Assets\Images\Galaxian.png";
        public const string APPLE_PATH = @"Assets\Images\Apple.png";
        public const string MELON_PATH = @"Assets\Images\Melon.png";
        public const string BELL_PATH = @"Assets\Images\Bell.png";
        public const string KEY_PATH = @"Assets\Images\Key.png";

        private static Dictionary<string, BitmapImage> Cache = new();

        public static BitmapImage GhostEyes = GetImage(GHOST_EYES_PATH);

        public static BitmapImage Pacman = GetImage(PACMAN_PATH);

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

        private static Dictionary<CacheKey, Canvas> WallCache = new();

        public static Uri GetUri(string path)
        {
            return new Uri("pack://application:,,,/PacmanWPF-Canvas;component/" + path);
        }


        public static BitmapImage GetImage(string path)
        {
            if (!Cache.ContainsKey(path))
                Cache[path] = new BitmapImage(GetUri(path));

            return Cache[path];
        }

        public static BitmapImage GetImage(FoodTypes type)
        {
            if (type is FoodTypes.PacDot)
                return PacDot;
            if (type is FoodTypes.PowerPellet)
                return PowerPellet;
            if (type is FoodTypes.Cherry)
                return Cherry;
            if (type is FoodTypes.Strawberry)
                return Strawberry;
            if (type is FoodTypes.Peach)
                return Peach;
            if (type is FoodTypes.Apple)
                return Apple;
            if (type is FoodTypes.Melon)
                return Melon;
            if (type is FoodTypes.Galaxian)
                return Galaxian;
            if (type is FoodTypes.Bell)
                return Bell;
            return Key;
        }


        public record CacheKey(Walls Block, Color PenColor);


        public static Canvas GetCanvasElement(Walls Block, Color PenColor)
        {
            return GetCanvasElement(new CacheKey(Block, PenColor));
        }

        public static Canvas GetCanvasElement(CacheKey key)
        {
            const double SCALE = 256 / 64;

            var wallCanvas = new Canvas
            {
                Width = 256 / SCALE,
                Height = 256 / SCALE
            };

            var penThickness = 10D / SCALE;
            var penBrush = new SolidColorBrush(key.PenColor);

            // Helper to add a line to the canvas
            void AddLine(double x1, double y1, double x2, double y2)
            {
                x1 /= SCALE; y1 /= SCALE; x2 /= SCALE; y2 /= SCALE;
                var line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = penBrush,
                    StrokeThickness = penThickness
                };
                wallCanvas.Children.Add(line);
            }

            void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle)
            {
                x /= SCALE; y /= SCALE; width /= SCALE; height /= SCALE;
                var adjustedStartAngle = 180D + startAngle;
                var adjustedSweepAngle = -sweepAngle;

                var arcFigure = new PathFigure
                {
                    StartPoint = CalculateArcPoint(x, y, width, height, adjustedStartAngle)
                };

                arcFigure.Segments.Add(new ArcSegment
                {
                    Point = CalculateArcPoint(x, y, width, height, adjustedStartAngle + adjustedSweepAngle),
                    Size = new Size(width / 2, height / 2),
                    SweepDirection = adjustedSweepAngle > 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
                    IsLargeArc = Math.Abs(adjustedSweepAngle) > 180
                });

                var arcPath = new UIPath
                {
                    Data = new PathGeometry([arcFigure]),
                    Stroke = penBrush,
                    StrokeThickness = penThickness
                };
                wallCanvas.Children.Add(arcPath);
            }


            Point CalculateArcPoint(double x, double y, double width, double height, double angle)
            {
                var radians = Math.PI * angle / 180.0;
                return new(
                    x + width / 2 + (width / 2) * Math.Cos(radians),
                    y + height / 2 - (height / 2) * Math.Sin(radians)
                );
            }


            #region External
            if (key.Block.HasFlag(Walls.Top))
            {
                AddLine(48, 32, 208, 32);
                if (key.Block.HasFlag(Walls.Right))
                    AddArc(191, 32, 32, 32, 270, 90);
                else
                    AddLine(208, 32, 256, 32);

                if (key.Block.HasFlag(Walls.Left))
                    AddArc(32, 32, 32, 32, -90, -90);
                else
                    AddLine(0, 32, 48, 32);
            }
            if (key.Block.HasFlag(Walls.Left))
            {
                AddLine(32, 48, 32, 208);
                if (!key.Block.HasFlag(Walls.Bottom))
                    AddLine(32, 48, 32, 256);
                if (!key.Block.HasFlag(Walls.Top))
                    AddLine(32, 0, 32, 208);
            }
            if (key.Block.HasFlag(Walls.Bottom))
            {
                AddLine(48, 224, 208, 224);
                if (key.Block.HasFlag(Walls.Right))
                    AddArc(191, 191, 32, 32, -270, -90);
                else
                    AddLine(208, 224, 256, 224);

                if (key.Block.HasFlag(Walls.Left))
                    AddArc(32, 191, 32, 32, -270, 90);
                else
                    AddLine(0, 224, 48, 224);
            }
            if (key.Block.HasFlag(Walls.Right))
            {
                AddLine(224, 48, 224, 208);
                if (!key.Block.HasFlag(Walls.Bottom))
                    AddLine(224, 48, 224, 256);
                if (!key.Block.HasFlag(Walls.Top))
                    AddLine(224, 0, 224, 208);
            }
            #endregion

            #region internal
            if (key.Block.HasFlag(Walls.SmallTop))
            {
                AddLine(80, 64, 176, 64);

                if (key.Block.HasFlag(Walls.SmallRight))
                    AddArc(159, 64, 32, 32, 270, 90);
                else if (key.Block.HasFlag(Walls.Right))
                    AddLine(176, 64, 192, 64);
                else
                    AddLine(176, 64, 256, 64);


                if (key.Block.HasFlag(Walls.SmallLeft))
                    AddArc(64, 64, 32, 32, -90, -90);
                else if (key.Block.HasFlag(Walls.Left))
                    AddLine(64, 64, 80, 64);
                else
                    AddLine(0, 64, 176, 64);
            }
            if (key.Block.HasFlag(Walls.SmallLeft))
            {
                AddLine(64, 80, 64, 176);


                if (!key.Block.HasFlag(Walls.Bottom) && !key.Block.HasFlag(Walls.SmallBottom))
                    AddLine(64, 176, 64, 256);
                else if (!key.Block.HasFlag(Walls.SmallBottom))
                    AddLine(64, 176, 64, 192);

                if (!key.Block.HasFlag(Walls.Top) && !key.Block.HasFlag(Walls.SmallTop))
                    AddLine(64, 0, 64, 80);
                else if (!key.Block.HasFlag(Walls.SmallTop))
                    AddLine(64, 64, 64, 80);

            }
            if (key.Block.HasFlag(Walls.SmallBottom))
            {
                AddLine(80, 192, 176, 192);

                if (key.Block.HasFlag(Walls.SmallRight))
                    AddArc(159, 159, 32, 32, -270, -90);
                else if (key.Block.HasFlag(Walls.Right))
                    AddLine(176, 192, 192, 192);
                else
                    AddLine(176, 192, 256, 192);

                if (key.Block.HasFlag(Walls.SmallLeft))
                    AddArc(64, 159, 32, 32, -270, 90);
                else if (key.Block.HasFlag(Walls.Left))
                    AddLine(64, 192, 80, 192);
                else
                    AddLine(0, 192, 80, 192);
            }
            if (key.Block.HasFlag(Walls.SmallRight))
            {
                AddLine(192, 80, 192, 176);

                if (!key.Block.HasFlag(Walls.SmallBottom) && !key.Block.HasFlag(Walls.Bottom))
                    AddLine(192, 176, 192, 256);
                else if (!key.Block.HasFlag(Walls.SmallBottom))
                    AddLine(192, 176, 192, 192);

                if (!key.Block.HasFlag(Walls.SmallTop) && !key.Block.HasFlag(Walls.Top))
                    AddLine(192, 80, 192, 0);
                else if (!key.Block.HasFlag(Walls.SmallTop))
                    AddLine(192, 80, 192, 64);
            }
            #endregion

            #region External Curve
            // Circular
            //  ^ ->
            //  |  |
            //  <- x
            if (key.Block.HasFlag(Walls.CurveTop))
            {
                AddArc(223, 0, 32, 32, 90, 90);
                AddLine(224, 0, 224, 17);
                AddLine(239, 32, 256, 32);
            }
            if (key.Block.HasFlag(Walls.CurveLeft))
            {
                AddArc(0, 0, 32, 32, -270, -90);
                AddLine(32, 0, 32, 17);
                AddLine(0, 32, 17, 32);
            }
            if (key.Block.HasFlag(Walls.CurveBottom))
            {
                AddArc(0, 223, 32, 33, 270, 90);
                AddLine(32, 240, 32, 256);
                AddLine(0, 224, 17, 224);
            }
            if (key.Block.HasFlag(Walls.CurveRight))
            {
                AddArc(224, 224, 32, 32, 270, -90);
                AddLine(224, 240, 224, 256);
                AddLine(240, 224, 256, 224);
            }
            #endregion

            #region Internal Curve
            if (key.Block.HasFlag(Walls.SmallCurveTop))
            {
                AddArc(191, 32, 32, 32, 90, 90);
                AddLine(192, 0, 192, 49);
                AddLine(256, 64, 207, 64);
            }
            if (key.Block.HasFlag(Walls.SmallCurveLeft))
            {
                AddArc(32, 32, 32, 32, -270, -90);
                AddLine(64, 0, 64, 49);
                AddLine(0, 64, 49, 64);
            }
            if (key.Block.HasFlag(Walls.SmallCurveBottom))
            {
                AddArc(32, 191, 32, 32, 270, 90);
                AddLine(64, 207, 64, 256);
                AddLine(0, 192, 49, 192);
            }
            if (key.Block.HasFlag(Walls.SmallCurveRight))
            {
                AddArc(191, 191, 32, 32, 270, -90);
                AddLine(192, 207, 192, 256);
                AddLine(207, 192, 256, 192);
            }

            #endregion
            return wallCanvas;
        }
    }
}
