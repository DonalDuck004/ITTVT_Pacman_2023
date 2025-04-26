using System.Numerics;

namespace PacManWPF_Canvas.Points.Abs
{

    public abstract class BasePoint<T> : ReadonlyBasePoint<T>, ICloneable where T : INumber<T>
    {

        public BasePoint(T X, T Y): base(X, Y) { }

        protected virtual void DropCachedValue()
        {
            this.NormalizedValue = null;
        }

        public void SetX(T x)
        {
            this.X = x;
            this.DropCachedValue();
        }

        public void SetY(T y)
        {
            this.Y = y;
            this.DropCachedValue();
        }

        public void Update(T x, T y)
        {
            this.X = x;
            this.Y = y;

            this.DropCachedValue();
        }

        public void Update(BasePoint<T> other)
        {
            this.X = other.X;
            this.Y = other.Y;

            this.DropCachedValue();
        }

        protected override abstract BasePoint<T> _Normalize(T max_x, T max_y);

        public override abstract object Clone();

        public ReadonlyBasePoint<T> AsReadonlyMask()
        {
            return this;
        }

        public (T X, T Y) AsReadonlyTuple()
        {
            return (this.X, this.Y);
        }
    }
}