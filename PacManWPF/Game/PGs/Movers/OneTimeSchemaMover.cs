using System.Drawing;

namespace PacManWPF.Game.PGs.Movers
{
    public class OneTimeSchemaMover : Abs.SchemaBasedMover
    {
        public OneTimeSchemaMover(Point[] schema) : base(schema)
        {

        }

        public override void Move(Ghost self)
        {
            Point LastPos = GetPos();

            if (schema_idx == schema.Length - 1)
                return;

            schema_idx++;

            if (GetPos() == LastPos)
                NextFrame(self);
        }
    }
}
