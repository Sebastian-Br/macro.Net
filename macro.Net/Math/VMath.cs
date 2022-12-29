namespace macro.Net.Math
{
    /// <summary>
    /// Provides functions for Vector-Math!
    /// </summary>
    static class VMath
    {
        /// <summary>
        /// Gets the Distance between two points in 2D-space.
        /// </summary>
        /// <param name="x_target">Point 1 x</param>
        /// <param name="y_target">Point 1 y</param>
        /// <param name="x_current">Point 2 x</param>
        /// <param name="y_current">Point 2 y</param>
        /// <returns>The distance between point 1 and 2</returns>
        public static int GetDistance(int x_target, int y_target, int x_current, int y_current)
        {
            int dX = x_target - x_current;
            int dY = y_target - y_current;
            int distance = (int)(System.Math.Sqrt(dX * dX + dY * dY));
            return distance;
        }

        /// <summary>
        /// Returns a low double value instead of zero to avoid division by zero
        /// </summary>
        /// <param name="i">Possible 0-value.</param>
        /// <returns>Returns input param if value is not zero. Otherwise returns 0.0000001</returns>
        public static double NotZero(double i)
        {
            if (i == 0)
                return 0.0000001;
            return i;
        }

        /// <summary>
        /// Gets the counter-clockwise (starting from 0° = to the right) angle between the current cursor position and the target
        /// file:///E:/Visual%20Studio/Projects/macro.Net/macro.Net/Math/VMath.cs.Doc/GetAngle.rtf
        /// </summary>
        /// <param name="cursor_x">X coordinate of the cursor</param>
        /// <param name="cursor_y">Y coordinate of the cursor</param>
        /// <param name="target_x">X coordinate of the target</param>
        /// <param name="target_y">Y coordinate of the target</param>
        /// <returns>Angle in °. If the points are the same, returns 90°</returns>
        public static double GetAngle(int cursor_x, int cursor_y, int target_x, int target_y)
        {
            if (target_x == cursor_x)
            {
                if (cursor_y < target_y)
                    return 270;
                else
                    return 90;
            }
            double angle = System.Math.Atan((target_y - cursor_y) / VMath.NotZero(target_x - cursor_x)) * ((double)180 / System.Math.PI);
            if (cursor_y < target_y && cursor_x < target_x) // Q2: p2 is above and to the left of p1
            {
                //Console.WriteLine("Q2");
                return opposite(180 - angle);
            }
            if (cursor_y > target_y && cursor_x < target_x) //Q3: point 2 is below and to the left of p1
            {
                //Console.WriteLine("Q3");
                return opposite(180 - angle);
            }
            if (cursor_y > target_y && cursor_x > target_x) //Q4: point 2 is below and to the right of p1
            {
                //Console.WriteLine("Q4");
                return opposite(360 - angle);
            }
            return opposite(-angle); // Q1
        }

        /// <summary>
        /// Returns the angle shifted by 180°. E.g.  30° -> 210°.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns>The angle shifted by 180°. E.g.  30° -> 210°.</returns>
        private static double opposite(double angle)
        {
            return (180 + angle) % 360;
        }

        /// <summary>
        /// Returns the absolute difference between two integers
        /// </summary>
        /// <param name="a">Integer 1</param>
        /// <param name="b">Integer 2</param>
        /// <returns></returns>
        public static int AbsDiff(int a, int b)
        {
            return System.Math.Abs(a - b);
        }

        /// <summary>
        /// Determines whether the point (x,y) is in a radius r around xTarget, yTarget with leeway of 2.
        /// </summary>
        /// <param name="x_circle">x Position of the circle's center.</param>
        /// <param name="y_circle">y Position of the circle's center.</param>
        /// <param name="x">x Position of a point.</param>
        /// <param name="y">y Position of a point.</param>
        /// <param name="r">The radius of the circle.</param>
        /// <returns>True: Point is in circle. False: Point is not in circle.</returns>
        public static bool IsInCircle(int x_circle, int y_circle, int x, int y, int r)
        {
            int dX = AbsDiff(x_circle, x);
            int dY = AbsDiff(y_circle, y);
            if (dX * dX + dY * dY <= r * r + 2)
                return true;
            return false;
        }
    }
}