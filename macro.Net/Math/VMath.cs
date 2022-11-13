namespace macro.Net.Math
{
    internal class VMath
    {
        /// <summary>
        /// Gets the Distance between two points in 2D-space.
        /// </summary>
        /// <param name="xTarget">Point 1 x.</param>
        /// <param name="yTarget">Point 1 y.</param>
        /// <param name="xCurrent">Point 2 x.</param>
        /// <param name="yCurrent">Point 2 y.</param>
        /// <returns></returns>
        public static int GetDistance(int xTarget, int yTarget, int xCurrent, int yCurrent)
        {
            int dX = xTarget - xCurrent;
            int dY = yTarget - yCurrent;
            int ret = (int)(System.Math.Sqrt(dX * dX + dY * dY));
            return ret;
        }

        /// <summary>
        /// Returns a low double value instead of zero to avoid division by zero.
        /// </summary>
        /// <param name="i">Possible 0-value.</param>
        /// <returns>Returns input param if value is not zero. Otherwise returns 0.0000001.</returns>
        public static double NotZero(double i)
        {
            if (i == 0)
                return 0.0000001;
            return i;
        }

        /// <summary>
        /// Gets the counter-clockwise angle between two points P1, P2 from P1 to P2.
        /// </summary>
        /// <param name="x">P2 x.</param>
        /// <param name="y">P2 y.</param>
        /// <param name="X">P1 X.</param>
        /// <param name="Y">P1 Y.</param>
        /// <returns>Angle in °.</returns>
        public static double GetAngle(int x, int y, int X, int Y)
        {
            double angle = System.Math.Atan((Y - y) / VMath.NotZero(X - x)) * ((double)180 / System.Math.PI);
            if (X - x == 0) // replace with X == x?
            {
                if (y < Y)
                    return opposite(90); // 270
                else
                    return opposite(270); // 90
            }
            if (y < Y && x < X) // Q2
            {
                //Console.WriteLine("Q2");
                return opposite(180 - angle);
            }
            if (y > Y && x < X) //Q3
            {
                //Console.WriteLine("Q3");
                return opposite(180 - angle);
            }
            if (y > Y && x > X) //Q4
            {
                //Console.WriteLine("Q4");
                return opposite(360 - angle);
            }
            return opposite(-angle);
        }

        /// <summary>
        /// Returns the angle shifted by 180°. E.g.  30° -> 210°.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private static double opposite(double a)
        {
            return (180 + a) % 360;
        }

        /// <summary>
        /// Returns the absolute difference between two integers.
        /// </summary>
        /// <param name="a">Integer 1.</param>
        /// <param name="b">Integer 2.</param>
        /// <returns></returns>
        public static int AbsDiff(int a, int b)
        {
            return Abs(a - b);
        }

        /// <summary>
        /// Determines whether the point (x,y) is in a radius r around xTarget, yTarget with leeway of 2.
        /// </summary>
        /// <param name="xTarget">x Position of the circle's center.</param>
        /// <param name="yTarget">y Position of the circle's center.</param>
        /// <param name="x">x Position of a point.</param>
        /// <param name="y">y Position of a point.</param>
        /// <param name="r">The radius of the circle.</param>
        /// <returns>True: Point is in circle. False: Point is not in circle.</returns>
        public static bool isInCircle(int xTarget, int yTarget, int x, int y, int r)
        {
            int dX = AbsDiff(xTarget, x);
            int dY = AbsDiff(yTarget, y);
            if (dY >= 2 * r + 2 || dX >= 2 * r + 2) // should be r + 2?
                return false;
            if (dX * dX + dY * dY <= r * r + 2)
                return true;
            return false;
        }

        /// <summary>
        /// Determines for radius r, if any (x,y) relative to its centre are within the circle with leeway of 2.
        /// </summary>
        /// <param name="x">The x-coordinate of a point relative to the circle-centre.</param>
        /// <param name="y">The y-coordinate of a point relative to the circle-centre.</param>
        /// <param name="r">The radius.</param>
        /// <returns>True: Is in circle. False oterhwise.</returns>
        public static bool isInCenteredCircle(int x, int y, int r)
        {
            if (x * x + y * y <= r * r + 2)
                return true;
            return false;
        }

        /// <summary>
        /// Returns the absolute value, e.g. 2 -> 2. -3 -> 3.
        /// </summary>
        /// <param name="i">The value.</param>
        /// <returns>The absolute value.</returns>
        public static int Abs(int i)
        {
            if (i < 0)
                return -i;
            return i;
        }

        /// <summary>
        /// Returns the absolute value.
        /// </summary>
        /// <param name="i">Positive or negative double input.</param>
        /// <returns>Positive double output.</returns>
        public static double Abs(double i)
        {
            if (i < 0)
                return -i;
            return i;
        }
    }
}
