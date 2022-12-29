using System.Drawing;

namespace PacManWPF.Game.PGs.Movers.Abs
{
    public abstract class SchemaBasedMover : BaseGhostMover
    {
        protected int schema_idx = 0;

        protected Point[] schema;

        public SchemaBasedMover(Point[] schema)
        {
            this.schema = schema;
        }

        public override Point GetPos()
        {
            return schema[schema_idx];
        }

        public abstract void Move(Ghost self);

        public override void NextFrame(Ghost self)
        {
            Move(self);
        }
    }
}