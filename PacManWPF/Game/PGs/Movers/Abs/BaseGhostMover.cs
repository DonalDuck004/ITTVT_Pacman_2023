using System.Drawing;

namespace PacManWPF.Game.PGs.Movers.Abs
{
    public interface IGhostMover
    {
        Point GetPos();
        void NextFrame(Ghost self);
    }

    public abstract class BaseGhostMover : IGhostMover
    {
        public abstract Point GetPos();
        public abstract void NextFrame(Ghost self);
    }
}