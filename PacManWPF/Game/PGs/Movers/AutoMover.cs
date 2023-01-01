using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Point = System.Drawing.Point;


namespace PacManWPF.Game.PGs.Movers
{

    public class AutoMover : NoCachedAutoMover
    {
        protected List<Point>? stalking_way = null;


        public AutoMover(string hash, Point StartPoint, Ghost ghost) : base(hash, StartPoint, ghost) // TODO LIMIT LEN
        {
        }




        public override bool NextFrame()
        {
            if (this.BaseNextFrame() is false)
                return false;

            if (this.stalking_way is null || this.stalking_way.Count == 0 || this.stalking_way[0] != this.ghost.EffectivePosition)
            {
                this.stalking_way = GetWay(this.ghost.EffectivePosition, Pacman.INSTANCE.Position);
                this.stalking_way.RemoveAt(0);
            }

            this.Position = this.stalking_way[0];
            this.stalking_way.RemoveAt(0);

            return true;
        }
    }


}
