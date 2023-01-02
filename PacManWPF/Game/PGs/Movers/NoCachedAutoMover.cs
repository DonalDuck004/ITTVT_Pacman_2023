using Point = System.Drawing.Point;


namespace PacManWPF.Game.PGs.Movers
{
    public class NoCachedAutoMover : Abs.BaseGhostMover
    {

        protected Point Position;
        protected Point StartPoint;

        public NoCachedAutoMover(string hash, Point StartPoint, Ghost ghost) : base(ghost)
        {
            this.StartPoint = StartPoint;
            this.Position = StartPoint;
        }

        public override Point GetStartPoint() => this.StartPoint;

        public override Point GetPos() => this.Position;

        protected bool BaseNextFrame() => base.NextFrame();

        public override bool NextFrame()
        {
            if (base.NextFrame() is false)
                return false;

            var way = GetWay(this.ghost.EffectivePosition, Pacman.INSTANCE.Position);
            this.Position = way.PopFirstNode().Point;

            return true;
        }
    }


}
