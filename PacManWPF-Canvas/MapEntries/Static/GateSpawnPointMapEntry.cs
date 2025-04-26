using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.Utils;

namespace PacManWPF_Canvas.MapEntries.Static
{
    public class GateSpawnPointMapEntry : MapEntry, ISpawnPointMapEntry
    {
        protected override int Size { get; } = 32;
        public override int ZIndex { get; protected set; } = -1;

        public override void Resize(double Scale)
        {
            Refresh(Scale);

            Element.RenderTransform = new ScaleTransform(Size / Element.Width, Size / Element.Height);
        }


        public GateSpawnPointMapEntry() : base(new Image() { Source = ResourcesLoader.Gate })
        {
            ((Image)this.Element).ImageFailed += (s, e) => Debugger.Break();
        }
    }
}