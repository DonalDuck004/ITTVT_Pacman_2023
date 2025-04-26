using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PacManWPF.Utils;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.MapEntries.Static;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Utils;

namespace PacManWPF_Canvas.Worlds
{
    public class GameMap
    {
        public Canvas Canvas { get; private set; } = new Canvas() { Background = new SolidColorBrush(Colors.Black), ClipToBounds = true };

        internal List<IMapEntry> Entries = new List<IMapEntry>();
        internal Dictionary<IntPoint, List<IMapEntry>> MapState;
        public int Ticks { get; private set; } = 0;
        public int Frames { get; private set; } = 0;
        public double MultiplyBy { get; private set; } = 1;
        private bool Paused = false;
        private bool TickedOnce = false;

        private GameMapTransactionFactory TransactionFactory { get; set; }

        public GameMap()
        {
            Entries.Add(new PacmanMapEntry());

            var sp1 = new SpawnPointMapEntry();
            var sp2 = new SpawnPointMapEntry();
            var sp3 = new SpawnPointMapEntry();
            var sp4 = new GateSpawnPointMapEntry();

            var full_bottom = Walls.Bottom | Walls.SmallBottom;
            var full_top = Walls.Top | Walls.SmallTop;
            var full_right = Walls.Right | Walls.SmallRight;
            var full_left = Walls.Left | Walls.SmallLeft;
            var full_external_left_top = Walls.CurveLeft | Walls.SmallCurveLeft;
            var full_external_left_bottom = Walls.CurveBottom | Walls.SmallCurveBottom;
            var full_external_right_top = Walls.CurveTop | Walls.SmallCurveTop;
            var full_external_right_bottom = Walls.CurveRight | Walls.SmallCurveRight;

            #region map building
            var tmp_x = 0D;
            var tmp_y = 0D;
            Entries.Add(WallMapEntry.Empty(tmp_x++, tmp_y)); ;
            Entries.Add(new WallMapEntry(full_external_right_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_external_left_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_right | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(WallMapEntry.Empty(tmp_x++, tmp_y)); ;
            Entries.Add(new WallMapEntry(full_bottom | full_top | full_left, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top | full_right, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(WallMapEntry.Empty(tmp_x++, tmp_y)); ;
            Entries.Add(new WallMapEntry(full_bottom | full_right | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_external_right_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_bottom | full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_external_left_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 1D;
            Entries.Add(new WallMapEntry(full_external_right_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_bottom | full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 2D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 3D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_external_right_top | full_external_left_top | full_bottom, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_external_left_bottom | full_external_left_top | full_right, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 4D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_external_right_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_external_right_top | full_external_right_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 5D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_top | full_external_right_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_right | full_top | full_external_left_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_top | full_external_right_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_right | full_top | full_external_left_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 6D;
            Entries.Add(new WallMapEntry(full_right | full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom | full_external_right_top, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_right | full_bottom | full_external_left_top, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom | full_external_right_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_right | full_bottom | full_external_left_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            sp4.Point.Update(tmp_x++, tmp_y);
            Entries.Add(sp4);
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom | full_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_right | full_external_left_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 7D;
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            sp1.Point.Update(tmp_x++, tmp_y);
            Entries.Add(sp1);
            sp2.Point.Update(tmp_x++, tmp_y);
            Entries.Add(sp2); 
            sp3.Point.Update(tmp_x++, tmp_y);
            Entries.Add(sp3);
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            tmp_x = 0D;
            tmp_y = 8D;
            Entries.Add(new WallMapEntry(full_right | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom | full_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_external_left_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 9D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 10D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_right | full_left, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_external_right_top | full_external_right_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 11D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 12D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_right | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left | full_external_right_top | full_bottom, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_external_left_top | full_external_right_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_bottom | full_top, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.MediumPurple) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left | full_bottom, Colors.GreenYellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_right | full_left | full_bottom, Colors.Red) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_left, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_bottom | full_right, Colors.Yellow) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 13D;
            Entries.Add(new WallMapEntry(full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new FoodMapEntry(FoodTypes.PowerPellet) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            tmp_x = 0D;
            tmp_y = 14D;
            Entries.Add(new WallMapEntry(full_external_right_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top | full_right, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new FoodMapEntry(FoodTypes.PacDot) { Point = new DoublePoint(tmp_x++ + 0.5, tmp_y + 0.5) });
            Entries.Add(new WallMapEntry(full_top | full_left, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            Entries.Add(new WallMapEntry(full_external_left_top, Colors.Blue) { Point = new DoublePoint(tmp_x++, tmp_y ) });
            #endregion

            Entries.Add(new GhostMapEntry(GhostColor.Cyan, sp1));
            Entries.Add(new GhostMapEntry(GhostColor.Orange, sp2));
            Entries.Add(new GhostMapEntry(GhostColor.Pink, sp3));
            Entries.Add(new GhostMapEntry(GhostColor.Red, sp4));

            foreach (var item in Entries) // temp
                item.Init(Canvas);

            MapState = (from Y in Enumerable.Range(0, Config.CHUNK_HC)
                        from X in Enumerable.Range(0, Config.CHUNK_WC)
                        let point = new IntPoint(X, Y)
                        group EmptyMapEntry.INSTANCE by point into g
                        select g).ToDictionary(x => x.Key, x => x.ToList<IMapEntry>());

            Canvas.SizeChanged += (s, e) => SizeChanged();
            Canvas.MinWidth = Config.EXCPETED_WIDTH;
            Canvas.MinHeight = Config.EXCPETED_HEIGHT;
            Canvas.MaxWidth = Config.EXCPETED_WIDTH; // TODO Add scale support
            Canvas.MaxHeight = Config.EXCPETED_HEIGHT;
            Canvas.Visibility = Visibility.Hidden;
        }

        private void SizeChanged()
        {
            MultiplyBy = Canvas.ActualWidth / Config.CHUNK_WC;
            foreach (var entry in Entries)
                entry.Resize(MultiplyBy / Config.CHUNK_CS);
        }

        public void Load()
        {
            SizeChanged();
        }

        private GameMapTransaction BeginTick(KeyboardStatus status)
        {
            foreach (var entry in MapState)
                entry.Value.Clear();

            foreach (var entry in Entries)
                MapState[entry.Point.ToIntFloorPoint().Normalize()].Add(entry);
     
            this.TransactionFactory ??= new GameMapTransactionFactory(this);
            return this.TransactionFactory.build(status);
        }


        public Direction? Process(KeyboardStatus status)
        {
            ++Ticks;

            var transaction = BeginTick(status);

            foreach (var item in Entries)
                item.Process(transaction);


            if (Random.Shared.Next(64) == 0)
            {
                var point = new IntPoint(Random.Shared.Next(Config.CHUNK_WC - 1),
                                         Random.Shared.Next(Config.CHUNK_HC - 1));

                if (transaction.At(point).Count == 0) {
                    var food = new FoodMapEntry();
                    food.Point.Update(point.X + 0.5, point.Y + 0.5);
                    food.Point.Normalize();
                    food.Resize(MultiplyBy / Config.CHUNK_CS);
                    transaction.Add(food);
                }
            }

            transaction.Flush();

            if (!TickedOnce)
            {
                TickedOnce = true;
                Canvas.Visibility = Visibility.Visible;
            }

            return transaction.SuggestedFallback;
        }

        public void Frame()
        {
            if (!TickedOnce)
                return;

            ++Frames;

            foreach (var item in Entries)
                item.BeginDraw(Canvas, MultiplyBy);
        }

        public void ResetStats()
        {
            Frames = Ticks = 0;
        }

        public void Pause()
        {
            Paused = true;
        }

        public void Resume()
        {
            Paused = false;
        }
    }
}