using System.Drawing;

namespace PacManWPF.Game.PGs.Movers
{
    public class CyclicSchemaMover : Abs.SchemaBasedMover
    {
        public CyclicSchemaMover(Point[] schema) : base(schema)
        {

        }

        public override void Move(Ghost self)
        {
            Point LastPos = GetPos();

            schema_idx++;
            if (schema_idx == schema.Length)
                schema_idx = 0;


            if (GetPos() == LastPos)
                NextFrame(self);
        }
    }
}
