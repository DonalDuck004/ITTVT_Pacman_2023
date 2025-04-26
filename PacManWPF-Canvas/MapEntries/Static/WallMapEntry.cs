using System.Windows.Media;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Utils;

namespace PacManWPF_Canvas.MapEntries.Static
{
    public class WallMapEntry : MapEntry, IUnwalkable
    {
        protected override int Size { get; } = 32;
        public override int ZIndex { get; protected set; } = -1;

        public override void Resize(double Scale)
        {
            Refresh(Scale);

            Element.RenderTransform = new ScaleTransform(Size / Element.Width, Size / Element.Height);
        }

        public static WallMapEntry Empty(double x, double y)
        {
            var item = new WallMapEntry(Walls.Nothing, Colors.Transparent);
            item.Point.Update(x, y);
            return item;
        }

        public WallMapEntry(Walls walls, Color color) : base(ResourcesLoader.GetCanvasElement(walls, color))
        {

        }
    }
}