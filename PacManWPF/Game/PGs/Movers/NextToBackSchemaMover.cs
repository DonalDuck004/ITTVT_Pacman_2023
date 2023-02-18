using System.Drawing;

namespace PacManWPF.Game.PGs.Movers
{
    public class NextToBackSchemaMover : Abs.SchemaBasedMover
    {
        protected new const Enums.GhostEngines EngineType = Enums.GhostEngines.NextToBack;
        private bool increment = true;
    
        public NextToBackSchemaMover(Point[] schema, Ghost self) : base(schema, self)
        {
        }

        public override bool Move()
        {
            Point LastPos = GetPos();

            if (increment)
            {
                schema_idx++;
                if (schema_idx == schema.Length)
                {
                    schema_idx = schema.Length - 1;
                    increment = false;
                }
            }
            else
            {
                schema_idx--;
                if (schema_idx == -1)
                {
                    schema_idx = 0;
                    increment = true;
                }
            }

            if (GetPos() == LastPos)
                NextFrame();

            return true;
        }
    }
}
