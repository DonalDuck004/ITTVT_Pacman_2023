using PacManWPF.Utils;
using PacManWPF_Canvas.Points.Abs;

namespace PacManWPF_Canvas.Points
{
    public class IntPoint : BasePoint<int>
    {
        public static IntPoint ZERO => new IntPoint(0, 0);

        public IntPoint(int X, int Y) : base(X, Y) { }

        protected override IntPoint _Normalize(int max_x, int max_y)
        {
            if (max_x <= 0 || max_y <= 0)
                throw new ArgumentException("max_x and max_y must be greater than 0");

            while (X < 0)
                X += max_x;
            while (X >= max_x)
                X -= max_x;
            while (Y < 0)
                Y += max_y;
            while (Y >= max_y)
                Y -= max_y;
            return this;
        }

        public override IntPoint Normalize(int max_x = Config.CHUNK_WC, int max_y = Config.CHUNK_HC)
        {
            return (IntPoint)base.Normalize(max_x, max_y);
        }

        public override IntPoint Clone()
        {
            return new IntPoint(X, Y);
        }
    }
}