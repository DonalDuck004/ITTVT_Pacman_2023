using Point = System.Drawing.Point;


namespace PacManWPF.Game.PGs.Movers
{
    public class NoCachedAutoMover : AutoMover
    {
        public NoCachedAutoMover(string hash, Point StartPoint, Ghost ghost) : base(hash, StartPoint, ghost)
        {
        }

        public override bool NextFrame()
        {
            if (this.BaseNextFrame() is false)
                return false;


            if (!Pacman.INSTANCE.IsDrugged)
            {
                var way = GetWay(this.ghost.EffectivePosition, Pacman.INSTANCE.Position);
                this.Position = way.PopFirstNode().Point;
                this.stalking_way = null;
            }
            else{
                if (this.stalking_way == null || this.stalking_way.CountBefore == 0 || this.Position != this.ghost.EffectivePosition)
                    this.stalking_way = GetWay(this.ghost.EffectivePosition, PacmanGame.INSTANCE.FreeAreas[rnd.Next(PacmanGame.INSTANCE.FreeAreas.Count)]);
                this.Position = this.stalking_way.PopFirstNode().Point;
            }

            return true;
        }
    }


}
