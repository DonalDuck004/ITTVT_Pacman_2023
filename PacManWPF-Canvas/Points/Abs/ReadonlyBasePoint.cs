using System.Numerics;

namespace PacManWPF_Canvas.Points.Abs
{
    public abstract class ReadonlyBasePoint<T> : ICloneable where T : INumber<T>
    {
        protected ReadonlyBasePoint<T>? NormalizedValue { get; set; } = null;
      
        public T X { get; protected set; }
        public T Y { get; protected set; }

        public ReadonlyBasePoint(T X, T Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static bool operator ==(ReadonlyBasePoint<T>? point1, ReadonlyBasePoint<T>? point2)
        {
            if (point1 is null || point2 is null) return false;

            return point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y);
        }


        public static bool operator !=(ReadonlyBasePoint<T>? point1, ReadonlyBasePoint<T>? point2)
        {
            return !(point1 == point2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ReadonlyBasePoint<T> other) return false;
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("Point", typeof(T).Name, X, Y);
        }

        public override string ToString()
        {
            return $"Point{{X={X}, Y={Y}}}";
        }

        protected abstract ReadonlyBasePoint<T> _Normalize(T max_x, T max_y);

        public virtual ReadonlyBasePoint<T> Normalize(T max_x = default, T max_y = default)
        {
            return this.NormalizedValue ??= this._Normalize(max_x, max_y);
        }

        public abstract object Clone();

    }
}