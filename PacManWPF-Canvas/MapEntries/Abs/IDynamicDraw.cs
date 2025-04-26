using System.Windows.Controls;

namespace PacManWPF_Canvas.MapEntries.Abs
{
    public interface IDynamicDraw
    {
        public void Draw(Canvas canvas, double Scale);
    }
}