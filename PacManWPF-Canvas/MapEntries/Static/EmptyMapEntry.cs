using System.Windows.Controls;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Static
{
    public class EmptyMapEntry : IMapEntry, IUnwalkable
    {
        public DoublePoint Point { get; init; } = DoublePoint.ZERO;

        public static readonly EmptyMapEntry INSTANCE = new EmptyMapEntry();
        public void BeginDraw(Canvas canvas, double Scale) { }
        public void Process(GameMapTransaction transaction) { }
        public void Init(Canvas canvas) { }
        public void Deinit(Canvas canvas) { }

        public void Resize(double Scale) { }
    }
}