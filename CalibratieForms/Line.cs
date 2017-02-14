using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CalibratieForms
{
    //Originally: http://www.artiom.pro/2013/06/c-find-intersections-of-two-line-by.html

    public class LineEquation
    {
        public LineEquation(Point start, Point end)
        {
            Start = start;
            End = end;

            A = End.Y - Start.Y;
            B = Start.X - End.X;
            C = A * Start.X + B * Start.Y;
        }

        public Point Start { get; private set; }
        public Point End { get; private set; }

        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }

        public Point? GetIntersectionWithLine(LineEquation otherLine)
        {
            double determinant = A * otherLine.B - otherLine.A * B;

            if (determinant.IsZero()) //lines are parallel
                return default(Point?);

            //Cramer's Rule

            double x = (otherLine.B * C - B * otherLine.C) / determinant;
            double y = (A * otherLine.C - otherLine.A * C) / determinant;

            Point intersectionPoint = new Point(x, y);

            return intersectionPoint;
        }

        public Point? GetIntersectionWithLineSegment(LineEquation otherLine)
        {
            Point? intersectionPoint = GetIntersectionWithLine(otherLine);

            if (intersectionPoint.HasValue &&
                intersectionPoint.Value.IsBetweenTwoPoints(otherLine.Start, otherLine.End))
                return intersectionPoint;

            return default(Point?);
        }

        //i didnt review this one for correctness
        public LineEquation GetIntersectionWithLineForRay(Rect rectangle)
        {
            LineEquation intersectionLine;

            if (Start == End)
                return null;

            IEnumerable<LineEquation> lines = rectangle.LineSegments();
            intersectionLine = new LineEquation(new Point(0, 0), new Point(0, 0));
            var intersections = new Dictionary<LineEquation, Point>();
            foreach (LineEquation equation in lines)
            {
                Point? intersectionPoint = GetIntersectionWithLineSegment(equation);

                if (intersectionPoint.HasValue)
                    intersections[equation] = intersectionPoint.Value;
            }

            if (!intersections.Any())
                return null;

            var intersectionPoints = new SortedDictionary<double, Point>();
            foreach (var intersection in intersections)
            {
                if (End.IsBetweenTwoPoints(Start, intersection.Value) ||
                    intersection.Value.IsBetweenTwoPoints(Start, End))
                {
                    double distanceToPoint = Start.DistanceToPoint(intersection.Value);
                    intersectionPoints[distanceToPoint] = intersection.Value;
                }
            }

            if (intersectionPoints.Count == 1)
            {
                Point endPoint = intersectionPoints.First().Value;
                intersectionLine = new LineEquation(Start, endPoint);

                return intersectionLine;
            }

            if (intersectionPoints.Count == 2)
            {
                Point start = intersectionPoints.First().Value;
                Point end = intersectionPoints.Last().Value;
                intersectionLine = new LineEquation(start, end);

                return intersectionLine;
            }

            return null;
        }

        public override string ToString()
        {
            return "[" + Start + "], [" + End + "]";
        }
    }

    public static class DoubleExtensions
    {
        //SOURCE: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Precision.cs
        //        https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Precision.Equality.cs
        //        http://referencesource.microsoft.com/#WindowsBase/Shared/MS/Internal/DoubleUtil.cs
        //        http://stackoverflow.com/questions/2411392/double-epsilon-for-equality-greater-than-less-than-less-than-or-equal-to-gre
        
        /// <summary>
        /// The smallest positive number that when SUBTRACTED from 1D yields a result different from 1D.
        /// The value is derived from 2^(-53) = 1.1102230246251565e-16, where IEEE 754 binary64 &quot;double precision&quot; floating point numbers have a significand precision that utilize 53 bits.
        ///
        /// This number has the following properties:
        ///     (1 - NegativeMachineEpsilon) &lt; 1 and
        ///     (1 + NegativeMachineEpsilon) == 1
        /// </summary>
        public const double NegativeMachineEpsilon = 1.1102230246251565e-16D; //Math.Pow(2, -53);

        /// <summary>
        /// The smallest positive number that when ADDED to 1D yields a result different from 1D.
        /// The value is derived from 2 * 2^(-53) = 2.2204460492503131e-16, where IEEE 754 binary64 &quot;double precision&quot; floating point numbers have a significand precision that utilize 53 bits.
        /// 
        /// This number has the following properties:
        ///     (1 - PositiveDoublePrecision) &lt; 1 and
        ///     (1 + PositiveDoublePrecision) &gt; 1
        /// </summary>
        public const double PositiveMachineEpsilon = 2D * NegativeMachineEpsilon;

        /// <summary>
        /// The smallest positive number that when SUBTRACTED from 1D yields a result different from 1D.
        /// 
        /// This number has the following properties:
        ///     (1 - NegativeMachineEpsilon) &lt; 1 and
        ///     (1 + NegativeMachineEpsilon) == 1
        /// </summary>
        public static readonly double MeasuredNegativeMachineEpsilon = MeasureNegativeMachineEpsilon();

        private static double MeasureNegativeMachineEpsilon()
        {
            double epsilon = 1D;

            do
            {
                double nextEpsilon = epsilon / 2D;

                if ((1D - nextEpsilon) == 1D) //if nextEpsilon is too small
                    return epsilon;

                epsilon = nextEpsilon;
            }
            while (true);
        }

        /// <summary>
        /// The smallest positive number that when ADDED to 1D yields a result different from 1D.
        /// 
        /// This number has the following properties:
        ///     (1 - PositiveDoublePrecision) &lt; 1 and
        ///     (1 + PositiveDoublePrecision) &gt; 1
        /// </summary>
        public static readonly double MeasuredPositiveMachineEpsilon = MeasurePositiveMachineEpsilon();

        private static double MeasurePositiveMachineEpsilon()
        {
            double epsilon = 1D;

            do
            {
                double nextEpsilon = epsilon / 2D;

                if ((1D + nextEpsilon) == 1D) //if nextEpsilon is too small
                    return epsilon;

                epsilon = nextEpsilon;
            }
            while (true);
        }

        const double DefaultDoubleAccuracy = NegativeMachineEpsilon * 10D;

        public static bool IsClose(this double value1, double value2)
        {
            return IsClose(value1, value2, DefaultDoubleAccuracy);
        }

        public static bool IsClose(this double value1, double value2, double maximumAbsoluteError)
        {
            if (double.IsInfinity(value1) || double.IsInfinity(value2))
                return value1 == value2;

            if (double.IsNaN(value1) || double.IsNaN(value2))
                return false;

            double delta = value1 - value2;

            //return Math.Abs(delta) <= maximumAbsoluteError;

            if (delta > maximumAbsoluteError ||
                delta < -maximumAbsoluteError)
                return false;

            return true;
        }

        public static bool LessThan(this double value1, double value2)
        {
            return (value1 < value2) && !IsClose(value1, value2);
        }

        public static bool GreaterThan(this double value1, double value2)
        {
            return (value1 > value2) && !IsClose(value1, value2);
        }

        public static bool LessThanOrClose(this double value1, double value2)
        {
            return (value1 < value2) || IsClose(value1, value2);
        }

        public static bool GreaterThanOrClose(this double value1, double value2)
        {
            return (value1 > value2) || IsClose(value1, value2);
        }

        public static bool IsOne(this double value)
        {
            double delta = value - 1D;

            //return Math.Abs(delta) <= PositiveMachineEpsilon;

            if (delta > PositiveMachineEpsilon ||
                delta < -PositiveMachineEpsilon)
                return false;

            return true;
        }

        public static bool IsZero(this double value)
        {
            //return Math.Abs(value) <= PositiveMachineEpsilon;

            if (value > PositiveMachineEpsilon ||
                value < -PositiveMachineEpsilon)
                return false;

            return true;
        }
    }

    public static class PointExtensions
    {
        public static double DistanceToPoint(this Point point, Point point2)
        {
            return Math.Sqrt((point2.X - point.X) * (point2.X - point.X) + (point2.Y - point.Y) * (point2.Y - point.Y));
        }

        public static double SquaredDistanceToPoint(this Point point, Point point2)
        {
            return (point2.X - point.X) * (point2.X - point.X) + (point2.Y - point.Y) * (point2.Y - point.Y);
        }

        public static bool IsBetweenTwoPoints(this Point targetPoint, Point point1, Point point2)
        {
            double minX = Math.Min(point1.X, point2.X);
            double minY = Math.Min(point1.Y, point2.Y);
            double maxX = Math.Max(point1.X, point2.X);
            double maxY = Math.Max(point1.Y, point2.Y);

            double targetX = targetPoint.X;
            double targetY = targetPoint.Y;

            return minX.LessThanOrClose(targetX)
                  && targetX.LessThanOrClose(maxX)
                  && minY.LessThanOrClose(targetY)
                  && targetY.LessThanOrClose(maxY);
        }
    }

    public static class RectExtentions
    {
        //improved name from original
        public static IEnumerable<LineEquation> LineSegments(this Rect rectangle)
        {
            var lines = new List<LineEquation>
            {
                new LineEquation(new Point(rectangle.X, rectangle.Y),
                                 new Point(rectangle.X, rectangle.Y + rectangle.Height)),

                new LineEquation(new Point(rectangle.X, rectangle.Y + rectangle.Height),
                                 new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height)),

                new LineEquation(new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
                                 new Point(rectangle.X + rectangle.Width, rectangle.Y)),

                new LineEquation(new Point(rectangle.X + rectangle.Width, rectangle.Y),
                                 new Point(rectangle.X, rectangle.Y)),
            };

            return lines;
        }

        //improved from original at http://www.codeproject.com/Tips/403031/Extension-methods-for-finding-centers-of-a-rectang

        /// <summary>
        /// Returns the center point of the rectangle
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Center point of the rectangle</returns>
        public static Point Center(this Rect r)
        {
            return new Point(r.Left + (r.Width / 2D), r.Top + (r.Height / 2D));
        }
        /// <summary>
        /// Returns the center right point of the rectangle
        /// i.e. the right hand edge, centered vertically.
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Center right point of the rectangle</returns>
        public static Point CenterRight(this Rect r)
        {
            return new Point(r.Right, r.Top + (r.Height / 2D));
        }
        /// <summary>
        /// Returns the center left point of the rectangle
        /// i.e. the left hand edge, centered vertically.
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Center left point of the rectangle</returns>
        public static Point CenterLeft(this Rect r)
        {
            return new Point(r.Left, r.Top + (r.Height / 2D));
        }
        /// <summary>
        /// Returns the center bottom point of the rectangle
        /// i.e. the bottom edge, centered horizontally.
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Center bottom point of the rectangle</returns>
        public static Point CenterBottom(this Rect r)
        {
            return new Point(r.Left + (r.Width / 2D), r.Bottom);
        }
        /// <summary>
        /// Returns the center top point of the rectangle
        /// i.e. the topedge, centered horizontally.
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Center top point of the rectangle</returns>
        public static Point CenterTop(this Rect r)
        {
            return new Point(r.Left + (r.Width / 2D), r.Top);
        }
    }
}
