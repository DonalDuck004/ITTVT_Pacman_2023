using System.Diagnostics;

namespace PacManWPF_Canvas.Points
{
    public class LinkedPoints<T> where T: class
    {
        public required T Point { init; get; }
        public LinkedPoints<T>? Top { set; get; }
        public LinkedPoints<T>? Left { set; get; }
        public LinkedPoints<T>? Bottom { set; get; }
        public LinkedPoints<T>? Right { set; get; }

        public LinkedPoints<T>? GetTheOnlyNear(T except)
        {
            if (object.ReferenceEquals(except, this.Top?.Point))
                return (Left is not null && Right is null && Bottom is null) ? Left :
                       (Right is not null && Left is null && Bottom is null) ? Right :
                       (Bottom is not null && Right is null && Left is null) ? Bottom : null;

            if (object.ReferenceEquals(except, this.Left?.Point))
                return (Top is not null && Right is null && Bottom is null) ? Top :
                       (Right is not null && Top is null && Bottom is null) ? Right :
                       (Bottom is not null && Right is null && Top is null) ? Bottom : null;

            if (object.ReferenceEquals(except, this.Right?.Point))
                return (Top is not null && Left is null && Bottom is null) ? Top :
                       (Left is not null && Top is null && Bottom is null) ? Left :
                       (Bottom is not null && Left is null && Top is null) ? Bottom : null;

            if (object.ReferenceEquals(except, this.Bottom?.Point))
                return (Top is not null && Left is null && Right is null) ? Top :
                       (Left is not null && Top is null && Right is null) ? Left :
                       (Right is not null && Left is null && Top is null) ? Right : null;
            return null;

        }
    }
}