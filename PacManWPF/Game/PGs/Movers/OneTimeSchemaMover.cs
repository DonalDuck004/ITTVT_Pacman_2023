using System.Drawing;

namespace PacManWPF.Game.PGs.Movers
{
    public class OneTimeSchemaMover : Abs.SchemaBasedMover
    {
        public OneTimeSchemaMover(Point[] schema, Ghost self) : base(schema, self)
        {

        }

        public override bool Move()
        {
            Point LastPos = GetPos();

            if (schema_idx == schema.Length - 1)
                return false;

            schema_idx++;

            if (GetPos() == LastPos)
                return NextFrame();

            return true;
        }
    }
}
