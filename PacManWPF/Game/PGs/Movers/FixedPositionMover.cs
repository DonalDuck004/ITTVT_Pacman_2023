using PacManWPF.Game.PGs.Movers.Abs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PacManWPF.Game.PGs.Movers
{
    public class FixedPositionMover : BaseGhostMover
    {
        protected new const Enums.GhostEngines EngineType = Enums.GhostEngines.Fixed;
        protected int schema_idx = 0;

        private bool Gone = false;

        protected Point StartPoint;

        public FixedPositionMover(Point start_point, Ghost self) : base(self)
        {
            this.StartPoint = start_point;
        }

        public override Point GetStartPoint() => StartPoint;

        public override Point GetPos() => StartPoint;

        public override bool NextFrame()
        {
            if (base.NextFrame() is false)
            {
                if (this.Gone)
                    this.Gone = false;
                return false;
            }

            if (this.Gone is false)
            {
                this.schema_idx = 0;
                this.Gone = true;
            }

            return false;
        }
    }
}
