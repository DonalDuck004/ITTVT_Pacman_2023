using System.Drawing;

namespace PacManWPF.Game.PGs.Movers.Abs
{
    public interface IGhostMover
    {
        Point GetPos();
        bool NextFrame();
    }
}