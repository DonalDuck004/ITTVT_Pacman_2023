#define EXPERIMENTAL_CACHE

using PacManWPF.Utils;
using PacManWPF_Canvas.Points.Abs;

namespace PacManWPF_Canvas.Points
{
    public class DoublePoint : BasePoint<double>
    {
        public static DoublePoint ZERO => new DoublePoint(0D, 0D);


        public DoublePoint(double X, double Y) : base(X, Y)
        {
        }

        public IntPoint ToIntFloorPoint()
        {
            return new IntPoint((int)Math.Floor(X), (int)Math.Floor(Y));
        }

        protected override DoublePoint _Normalize(double max_x, double max_y)
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

        public override DoublePoint Normalize(double max_x = Config.CHUNK_WC, double max_y = Config.CHUNK_HC)
        {
            return (DoublePoint)base.Normalize(max_x, max_y);
        }

        public override DoublePoint Clone()
        {
            return new DoublePoint(X, Y);
        }
    }
}