using System.Drawing;

namespace PacManWPF.Game.PGs.Movers.Abs
{
    public abstract class SchemaBasedMover : BaseGhostMover
    {
        protected int schema_idx = 0;

        private bool Gone = false;

        protected Point[] schema;

        public SchemaBasedMover(Point[] schema, Ghost self) : base(self)
        {
            this.schema = schema;
        }

        public override Point GetStartPoint()
        {
            return schema[0];
        }

        public override Point GetPos()
        {
            return schema[schema_idx];
        }

        public abstract bool Move();

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

            return Move();
        }
    }
}