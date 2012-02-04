using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.JanRapp.General.Utilities
{
    [System.Diagnostics.DebuggerDisplay("{X},{Y}")]
    public struct Int32Vector
    {
        public Int32Vector(int xAndY)
        {

            this.X = xAndY;
            this.Y = xAndY;
        }
        public Int32Vector(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X;
        public int Y;

        public int LenghtSquared
        {
            get{ return X * X + Y * Y; }
        }

        public double Lenght
        {
            get { return Math.Sqrt(LenghtSquared); }
        }

        public static Int32Vector Add(Int32Vector v1, Int32Vector v2)
        {
            v1.X += v2.X;
            v1.Y += v2.Y;
            return v1;
        }
        public static Int32Vector Subtract(Int32Vector v1, Int32Vector v2)
        {
            v1.X -= v2.X;
            v1.Y -= v2.Y;
            return v1;
        }
        public static Int32Vector Negate(Int32Vector v)
        {
            v.X = -v.X;
            v.Y = -v.Y;
            return v;
        }
        public static Int32Vector Multiply(Int32Vector v, int scalar)
        {
            v.X *= scalar;
            v.Y *= scalar;
            return v;
        }
        public static Int32Vector Multiply(int scalar, Int32Vector v)
        {
            v.X *= scalar;
            v.Y *= scalar;
            return v;
        }
        public static Int32Vector Divide(Int32Vector v, int scalar)
        {
            v.X /= scalar;
            v.Y /= scalar;
            return v;
        }
        public static Int32Vector Remainer(Int32Vector v, int scalar)
        {
            v.X %= scalar;
            v.Y %= scalar;
            return v;
        }
        public static bool Equals(Int32Vector v1, Int32Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static Int32Vector operator -(Int32Vector vector)
        {
            return Int32Vector.Negate(vector);
        }
        public static Int32Vector operator -(Int32Vector vector1, Int32Vector vector2)
        {
            return Int32Vector.Subtract(vector1, vector2);
        }
        public static bool operator !=(Int32Vector vector1, Int32Vector vector2)
        {
            return !Int32Vector.Equals(vector1, vector2);
        }
        public static Int32Vector operator *(int scalar, Int32Vector vector)
        {
            return Int32Vector.Multiply(scalar, vector);
        }
        public static Int32Vector operator *(Int32Vector vector, int scalar)
        {
            return Int32Vector.Multiply(vector, scalar);
        }
        public static Int32Vector operator /(Int32Vector vector, int scalar)
        {
            return Int32Vector.Divide(vector, scalar);
        }
        public static Int32Vector operator %(Int32Vector vector, int scalar)
        {
            return Int32Vector.Remainer(vector, scalar);
        }
        public static Int32Vector operator +(Int32Vector vector1, Int32Vector vector2)
        {
            return Int32Vector.Add(vector1, vector2);
        }
        public static bool operator ==(Int32Vector vector1, Int32Vector vector2)
        {
            return Int32Vector.Equals(vector1, vector2);
        }
        public static explicit operator Int32Vector(System.Windows.Vector v)
        {
            return new Int32Vector((int)v.X, (int)v.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Int32Vector)
            {
                Int32Vector vec = (Int32Vector)obj;

                return vec.X == this.X && vec.Y == this.Y;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TraverseBLABLA
    {
        public struct Section
        {
            public int Top;
            public int Bottom;
            public int RowStartAtTop;
            public int RowEndAtTop;
            public float ChangeOfRowStart;
            public float ChangeOfRowEnd;
            public PointOfBla Point;
        }

        public class PointOfBla
        {
            public PointOfBla(Int32Vector vector)
            {
                this.Vector = vector;
            }
            public Int32Vector Vector;
            public PointOfBla Above;
            public PointOfBla Below;
            public PointOfBla ConnectedToAbvoe;
            public PointOfBla ConnectedToBelow;
        }

        public static Section Bla(PointOfBla p)
        {
            int top = p.Vector.Y;
            Int32Vector topChangeVec = p.ConnectedToBelow.Vector - p.Vector;
            float topChange;
            if (topChangeVec.Y == 0)
                topChange = 0;
            else
                topChange = (float)topChangeVec.X / (float)topChangeVec.Y;

            PointOfBla bottomPoint = p.Below;
            int bottom = bottomPoint.Vector.Y;
            Int32Vector bottomChangeVec = bottomPoint.Vector - bottomPoint.ConnectedToAbvoe.Vector;
            float bottomChange;
            if (bottomChangeVec.Y == 0)
                bottomChange = 0;
            else
                bottomChange = (float)bottomChangeVec.X / (float)bottomChangeVec.Y;


            float startOfTop = p.Vector.X;
            float startOfBottom = bottomPoint.ConnectedToAbvoe.Vector.X + (top - bottomPoint.ConnectedToAbvoe.Vector.Y) * bottomChange;

            float rowStart;
            float rowEnd;
            float startChange;
            float endChange;
            if (startOfBottom > startOfTop)
            {
                rowStart = startOfTop;
                rowEnd = startOfBottom;
                startChange = topChange;
                endChange = bottomChange;
            }
            else
            {
                rowStart = startOfBottom;
                rowEnd = startOfTop;
                startChange = bottomChange;
                endChange = topChange;            
            }

            Section s = new Section();

            s.Top = top;
            s.Bottom = bottom;
            s.RowStartAtTop = (int)(rowStart + 1);
            s.RowEndAtTop = (int)(rowEnd);
            s.ChangeOfRowStart = startChange;
            s.ChangeOfRowEnd = endChange;
            s.Point = p;

            return s;
        }

        public static Section[] GetSections(Int32Vector[] vectors)
        {
            if (vectors.Length != 4)
                throw new InvalidOperationException(); // Not Sure If anything else would work

            PointOfBla[] pointsBla = new PointOfBla[vectors.Length];

            for (int i = 0; i < vectors.Length; i++)
			{
                pointsBla[i] = new PointOfBla(vectors[i]);
			}

            for (int i = 0; i < vectors.Length; i++)
            {
                PointOfBla connected1, connected2;

                if (i == 0)
                    connected1 = pointsBla[vectors.Length - 1];
                else
                    connected1 = pointsBla[i - 1];

                if (i == vectors.Length - 1)
                    connected2 = pointsBla[0];
                else
                    connected2 = pointsBla[i + 1];

                if (connected1.Vector.Y < connected2.Vector.Y)//smaller is higher
                {
                    pointsBla[i].ConnectedToAbvoe = connected1;
                    pointsBla[i].ConnectedToBelow = connected2;
                }
                else
                {
                    pointsBla[i].ConnectedToAbvoe = connected2;
                    pointsBla[i].ConnectedToBelow = connected1;
                }
            }

            var pointsFromTopToBottom = pointsBla.OrderBy(p => p.Vector.Y).ToArray();

            for (int i = 0; i < pointsFromTopToBottom.Length; i++)
            {
                pointsFromTopToBottom[i].Above = i == 0 ? null : pointsFromTopToBottom[i - 1];
                pointsFromTopToBottom[i].Below = 
                    i == pointsFromTopToBottom.Length - 1 ? null :
                        pointsFromTopToBottom[i + 1];
            }

            Section[] sections = new Section[vectors.Length - 1];

            for (int i = 0; i < sections.Length; i++)
            {
                sections[i] = TraverseBLABLA.Bla(pointsFromTopToBottom[i]);
            }

            return sections;
        }

        public class Section2
        {
            public Section2(Int32Vector top, Int32Vector bottom)
            {
                this.Top = top;
                this.Bottom = bottom;
                Int32Vector changeVec = bottom - top;
                this.Change = (double)changeVec.X / (double)changeVec.Y;
            }

            public readonly Int32Vector Top;
            public readonly Int32Vector Bottom;
            public readonly double Change;

            public int XPosAt(int yPos)
            {
                return this.Top.X + (int)Math.Round((yPos - this.Top.Y) * this.Change);
            }

            public bool VerticalOverlap(Section2 other, out Tuple<int, int> vertikalIntersection)
            {
                if (this.Bottom.Y > other.Top.Y && this.Top.Y < other.Bottom.Y)
                {
                    int top, bottom;

                    if (this.Top.Y > other.Top.Y)
                        top = this.Top.Y;
                    else
                        top = other.Top.Y;

                    if (this.Bottom.Y < other.Bottom.Y)
                        bottom = this.Bottom.Y;
                    else
                        bottom = other.Bottom.Y;

                    vertikalIntersection = new Tuple<int, int>(top, bottom);

                    return true;
                }

                vertikalIntersection = null;
                return false;
            }

            public bool? IsClearlyLeftOf(Section2 other, Tuple<int, int> vertikalIntersection)
            {
                int atTop1 = this.XPosAt(vertikalIntersection.Item1);
                int atTop2 = other.XPosAt(vertikalIntersection.Item1);
                int atBottom1 = this.XPosAt(vertikalIntersection.Item2);
                int atBottom2 = other.XPosAt(vertikalIntersection.Item2);

                if (atTop1 < atTop2 && atBottom1 > atBottom2 || atTop1 > atTop2 && atBottom1 < atBottom2)
                    return null;

                if (atTop1 == atTop2 && atBottom1 == atBottom2)
                    return null;

                if(atTop1 < atTop2 || atBottom1 < atBottom2)
                    return true;
            
                if(atTop1 > atTop2 || atBottom1 > atBottom2)
                    return false;

                return null;
            }
        }

        public static Section2[][] GetSections2(Int32Vector[] vectors)
        {
            List<Section2> sections = new List<Section2>();

            for (int i = 0; i < vectors.Length; i++)
            {
                Int32Vector connected1, connected2;

                if (i == 0)
                    connected1 = vectors[vectors.Length - 1];
                else
                    connected1 = vectors[i - 1];

                if (i == vectors.Length - 1)
                    connected2 = vectors[0];
                else
                    connected2 = vectors[i + 1];

                if (connected1.Y > vectors[i].Y)
                    sections.Add(new Section2(vectors[i], connected1));

                if (connected2.Y > vectors[i].Y)
                    sections.Add(new Section2(vectors[i], connected2));

            }

            List<Section2> allLeft = new List<Section2>();
            List<Section2> allRight = new List<Section2>();

            foreach (Section2 outer in sections)
            {
                foreach (Section2 inner in sections)
                {
                    Tuple<int,int> verticalOnverlap;
                    if(inner.VerticalOverlap(outer, out verticalOnverlap))
                    {
                        bool? b = inner.IsClearlyLeftOf(outer, verticalOnverlap);
                        if(b == true)
                            allLeft.Add(inner);
                        if(b == false)
                            allRight.Add(inner);
                    }
                }
            }

            var left = allLeft.Distinct().Select( section => Tuple.Create(section, true)).ToArray();
            var right = allRight.Distinct().Select( section => Tuple.Create(section, false)).ToArray();
            var all = left.Concat(right).OrderBy(s => s.Item1.Top.Y).ToArray();

            return new[] {allLeft.Distinct().OrderBy(s=> s.Top.Y).ToArray(),
                allRight.Distinct().OrderBy(s=> s.Top.Y).ToArray()};
        }
    }

    public static class NumberUtil
    {
        //Use T4 for that!!!

        public static double Clamp(double value, double bound1, double bound2)
        {
            var lowLimit = bound1 < bound2 ? bound1 : bound2;
            var hightLimit = bound1 > bound2 ? bound1 : bound2;

            if (value < lowLimit)
                value = lowLimit;
            if (value > hightLimit) 
                value = hightLimit;

            return value;
        }

        public static double Max(params double[] values)
        {
            double max = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }

        public static double Min(params double[] values)
        {
            double min = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }

            return min;
        }

        public static int Clamp(int value, int bound1, int bound2)
        {
            var lowLimit = bound1 < bound2 ? bound1 : bound2;
            var hightLimit = bound1 > bound2 ? bound1 : bound2;

            if (value < lowLimit)
                value = lowLimit;
            if (value > hightLimit)
                value = hightLimit;

            return value;
        }

        public static int Max(params int[] values)
        {
            int max = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }

        public static int Min(params int[] values)
        {
            int min = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }

            return min;
        }

    }
}
