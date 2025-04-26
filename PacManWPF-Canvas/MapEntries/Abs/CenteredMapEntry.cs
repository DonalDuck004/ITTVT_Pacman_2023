using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PacManWPF.Utils;

namespace PacManWPF_Canvas.MapEntries.Abs
{
    public abstract class CenteredMapEntry : MapEntry
    {
        protected Canvas Wrapper;

        public override void Init(Canvas canvas)
        {
            Wrapper = new Canvas();
            Wrapper.Children.Add(Element);
            Wrapper.RenderTransform = new TranslateTransform() { X = -Config.CHUNK_CS / 2, Y = -Config.CHUNK_CS / 2 }; // TODO ADD support for scale

            canvas.Children.Add(Wrapper);
        }
        public override void Deinit(Canvas canvas)
        {
            canvas.Children.Remove(Wrapper);
        }

        protected CenteredMapEntry(FrameworkElement element) : base(element)
        {
            Point.Update(0.5, 0.5);
        }
    }
}