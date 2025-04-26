using System.Windows;
using System.Windows.Controls;
using PacManWPF.Utils;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Abs
{
    public abstract class MapEntry : IMapEntry
    {
        protected virtual int Size { get; } = 24;
        public abstract int ZIndex { get; protected set; }
        public DoublePoint Point { get; init; }

        public virtual void Process(GameMapTransaction transaction) { }

        public virtual void BeginDraw(Canvas canvas, double Scale)
        {
            Refresh(Scale);

            if (this is IDynamicDraw dynamic_draw)
                dynamic_draw.Draw(canvas, Scale / Config.CHUNK_CS);
        }

        public virtual void Init(Canvas canvas)
        {
            canvas.Children.Add(Element);
        }
        public virtual void Deinit(Canvas canvas)
        {
            canvas.Children.Remove(Element);
        }

        protected virtual void Refresh(double Scale)
        {
            Canvas.SetLeft(Element, Point.X * Scale);
            Canvas.SetTop(Element, Point.Y * Scale);
            Panel.SetZIndex(Element, ZIndex);
        }

        public virtual void Resize(double Scale)
        {
            Refresh(Scale);

            Element.Width = Size * Scale;
            Element.Height = Size * Scale;
        }

        public object? Tag { get; set; } = null;

        protected FrameworkElement Element;

        protected MapEntry(FrameworkElement element)
        {
            Point = DoublePoint.ZERO;
            Element = element;
        }
    }
}