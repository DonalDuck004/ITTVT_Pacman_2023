using System.Drawing;

namespace PacManWPF.Game.PGs.Movers
{
    public class CyclicSchemaMover : Abs.SchemaBasedMover
    {
        public CyclicSchemaMover(Point[] schema, Ghost self) : base(schema, self)
        {

        }

        public override bool Move()
        {
            Point LastPos = GetPos();

            schema_idx++;
            if (schema_idx == schema.Length)
                schema_idx = 0;


            if (GetPos() == LastPos)
                return NextFrame();

            return true;
        }
    }
}
