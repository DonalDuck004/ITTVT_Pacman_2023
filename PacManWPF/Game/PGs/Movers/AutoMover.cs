using PacManWPF.Game.PGs.Movers.Abs;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Point = System.Drawing.Point;


namespace PacManWPF.Game.PGs.Movers
{

    public class AutoMover : NoCachedAutoMover
    {
        protected ChainNode? stalking_way = null;


        public AutoMover(string hash, Point StartPoint, Ghost ghost) : base(hash, StartPoint, ghost) // TODO LIMIT LEN
        {
        }




        public override bool NextFrame()
        {
            if (this.BaseNextFrame() is false)
                return false;

            if (this.stalking_way is null || this.stalking_way.CountBefore == 0 || this.Position != this.ghost.EffectivePosition)
                this.stalking_way = GetWay(this.ghost.EffectivePosition, Pacman.INSTANCE.Position);

            this.Position = this.stalking_way.PopFirstNode().Point;

            return true;
        }
    }


}
