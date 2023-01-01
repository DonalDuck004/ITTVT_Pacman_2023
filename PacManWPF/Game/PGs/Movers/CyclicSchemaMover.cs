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

            this.schema_idx++;
            if (this.schema_idx == this.schema.Length)
                this.schema_idx = 0;


            if (GetPos() == LastPos)
                return NextFrame();

            return true;
        }
    }
}
