using System.Windows.Controls;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Abs
{
    public interface IMapEntry
    {
        public DoublePoint Point { get; init; }
        public void Process(GameMapTransaction transaction);
        public void BeginDraw(Canvas canvas, double Scale);

        public void Init(Canvas canvas);
        public void Deinit(Canvas canvas);
        public void Resize(double Scale);
    }
}