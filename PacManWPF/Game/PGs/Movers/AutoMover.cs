using PacManWPF.Game.PGs.Movers.Abs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Point = System.Drawing.Point;


namespace PacManWPF.Game.PGs.Movers
{

    public class AutoMover : Abs.BaseGhostMover
    {
        protected ChainNode? stalking_way = null;
        protected Point Position;
        protected Point StartPoint;
        protected Random rnd = new();
        protected bool was_drugged = false;

        public AutoMover(Point StartPoint, Ghost ghost) : base(ghost) // TODO LIMIT LEN
        {
            this.StartPoint = StartPoint;
            this.Position = StartPoint;
        }

        public override Point GetStartPoint() => this.StartPoint;
        public override Point GetPos() => this.Position;
        protected bool BaseNextFrame() => base.NextFrame();

        public override bool NextFrame()
        {
            if (this.BaseNextFrame() is false)
                return false;


            bool get_escape = Pacman.INSTANCE.IsDrugged && !this.was_drugged;
            this.was_drugged = Pacman.INSTANCE.IsDrugged;

            if (get_escape || this.stalking_way is null || this.stalking_way.CountBefore == 0 || this.Position != this.ghost.EffectivePosition)
                this.stalking_way = GetWay(this.ghost.EffectivePosition, get_escape ?
                                                                         PacmanGame.INSTANCE.FreeAreas[rnd.Next(PacmanGame.INSTANCE.FreeAreas.Count)] : 
                                                                         Pacman.INSTANCE.Position);

            this.Position = this.stalking_way.PopFirstNode().Point;

            return true;
        }
    }


}
