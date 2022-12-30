using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using macro.Net.Math;
using macro.Net.DebugPrint;
using System.Security.Cryptography;
using macro.Net.Wait;

namespace macro.Net.InputSimulationService
{
    /// <summary>
    /// This class provides realistic mouse movement somewhat resembling how a human would move the mouse
    /// </summary>
    public class Mouse
    {
        public Mouse(Rand _rng, bool _debug)
        {
            rd = new Random();
            ScreenWidthMinus1 = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 1;
            ScreenHeightMinus1 = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 1;
            point_cursor_position = new();
            Debug = _debug;
            RNG = _rng;
        }

        [DllImport("User32.Dll")]
        private static extern long SetCursorPos(int x, int y);
        [DllImport("User32.Dll")]
        private static extern bool GetCursorPos(ref POINT p); // top left corner = 0,0

        private static Random rd;

        private Rand RNG { get; set; }

        private POINT point_cursor_position;

        private int ScreenWidthMinus1 { get; set; }
        private int ScreenHeightMinus1 { get; set; }

        private bool Debug { get; set; }

        /// <summary>
        /// Moves the mouse in a natural way (with jitter, and not overly fast)
        /// </summary>
        /// <param name="target_x">The target X coordinate on the screen</param>
        /// <param name="target_y">The target Y coordinate on the screen</param>
        public void MouseMoveSimple(int target_x, int target_y)
        {
            GetCursorPos(ref point_cursor_position);
            int time_ms_left = findTimeToTargetMs(VMath.GetDistance(target_x, target_y, point_cursor_position.x, point_cursor_position.y));
            MouseMoveSimple_Rec(target_x, target_y, time_ms_left, 0);
        }

        /// <summary>
        /// Moves the mouse-cursor by an offset.
        /// </summary>
        /// <param name="dX">Change in X-direction (to the right).</param>
        /// <param name="dY">Change in Y-direction (downwards).</param>
        private void MMove(int dX, int dY)
        {
            POINT p = new POINT();
            //dbg("MMove() dX " + dX + " dY " + dY);
            GetCursorPos(ref p);
            int new_x_position = p.x + dX;
            int new_y_position = p.y + dY;
            if (new_x_position < 0)
                new_x_position = 0;
            if (new_y_position < 0)
                new_y_position = 0;
            if (new_x_position > ScreenWidthMinus1)
                new_x_position = ScreenWidthMinus1;
            if (new_y_position > ScreenHeightMinus1)
                new_y_position = ScreenHeightMinus1;
            //dbg("Moved from (" + p.x + "," + p.y + ") to (" + xPos + "," + yPos + ")");
            SetCursorPos(new_x_position, new_y_position);
        }

        /// <summary>
        /// Recursively moves the mouse to the target (X,Y) location.
        /// Set a realistic timeMsLeft! Mouse movement should not be too quick.
        /// The timeMsLeft-parameter gets passed to the next recursion and decreases over time,
        /// as each move-step has a wait-duration associated with it.
        /// The lastDevAngle gets passed to the next recursion to generate more fluent movement;
        /// a.k.a. this param changes only a bit per recursion.
        /// </summary>
        /// <param name="target_x">The target X coordinate.</param>
        /// <param name="target_Y">The target Y coordinate.</param>
        /// <param name="time_ms_left">The time, in Milliseconds, left to move the cursor to that location.</param>
        /// <param name="last_deviation_angle">The deviation in deg° from the shortest path of the previous call.</param>
        private void MouseMoveSimple_Rec(int target_x, int target_Y, int time_ms_left, double last_deviation_angle)
        {
            GetCursorPos(ref point_cursor_position);
            if(VMath.IsInCircle(target_x, target_Y, point_cursor_position.x, point_cursor_position.y, 4))
            {
                Dbg.Print("MouseMoveSimple() - Reached target!", Debug);
                WaitService.SmartWait(time_ms_left - 1);
                return;
            }
            //dbg("MouseMove2(): @(" + p.x + "," + p.y + ")" + " aiming@(" + X + "," + Y +")");
            int px_distance = VMath.GetDistance(target_x, target_Y, point_cursor_position.x, point_cursor_position.y);
            if (time_ms_left == 0) // prevent division by 0
                time_ms_left = 1;
            double px_per_ms_speed_min = px_distance / time_ms_left; // the minimum speed required to get to the target in time
            //dbg("MouseMove2(): Chose minspeed: " + pxSpeedMin + " [px/ms]");
            int movement_duration; 
            if (time_ms_left > 97 + rd.Next(-6, 18) && rd.Next(0, 99) == 8)
            {
                movement_duration = 3 + RNG.GetStandardRandInt(9, 5, 1, 18); // micro-wait.
                Dbg.Print("MicroWait!", Debug);
                goto loc_wait;
            }
            else
            {
                movement_duration = RNG.GetStandardRandInt(2, 0.75, 1, 3); // using GetStandardRand(2, 0.75, 1, 3) yields 2:50%, 1/3:25%
            }
            //dbg("MouseMove2(): Chose movementDuration: " + movementDuration);
            int movement_distance_px = movement_duration * (int)px_per_ms_speed_min;
            if (movement_distance_px == 0)
                movement_distance_px = 1;
            //dbg("MouseMove2(): Chose pxMovement: " + pxMovement);
            int radius_around_target = px_distance - movement_distance_px;
            double angle = VMath.GetAngle(point_cursor_position.x, point_cursor_position.y, target_x, target_Y);
            //dbg("MouseMove2(): Actual Angle: " + angle);
            last_deviation_angle = CheckLastDevAngle(last_deviation_angle);
            double deviation_angle = (2 * rd.Next(-1, 2)) * RNG.GetNextDouble() + last_deviation_angle; // +-18 deg
            last_deviation_angle = deviation_angle;
            angle += deviation_angle;
            //dbg("MouseMove2(): Movement Angle: " + angle);
            //dbg("MouseMove2(): angleDeviation: " + angleDeviation);
            int newdx = 0; int newdy = 0;
            if (radius_around_target <= 4)
                radius_around_target = 4;
            int pResult = PaintLineToCircle(ref newdx, ref newdy, point_cursor_position.x, point_cursor_position.y, angle, target_x, target_Y, radius_around_target);
            if (pResult == 1) // newdx was 0, thus newdx will be randomized to make the mouse jitter
            {
                if (rd.Next(0, 3) == 0)
                    newdx += rd.Next(-1, 2);
            }
            if (pResult == 2) // newdy was 0, thus newdy will be randomized to make the mouse jitter
            {
                if (rd.Next(0, 3) == 0)
                    newdy += rd.Next(-1, 2);
            }
            //dbg("Chose newdX " + newdx + " newdy " + newdy);
            MMove(newdx, newdy);
        loc_wait:
            WaitService.SmartWait(movement_duration);
            MouseMoveSimple_Rec(target_x, target_Y, time_ms_left - movement_duration, last_deviation_angle);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }

        /// <summary>
        /// Checks the angle by which the last movement deviated from the ideal, shortest path.
        /// This angle should not be too large, and should not change discontinuously.
        /// </summary>
        /// <param name="last_deviation_angle">The last deviation angle</param>
        private double CheckLastDevAngle(double last_deviation_angle)
        {
            double new_deviation_angle = 0;
            if (last_deviation_angle > 16)
                new_deviation_angle = 16;
            else if (last_deviation_angle < -16)
                new_deviation_angle = -16;

            return new_deviation_angle;
        }

        /// <summary>
        /// Gets the time that the next series of mouse-moves should occupy.
        /// </summary>
        /// <param name="pxDistance">The distance between the current and target mouse position.</param>
        /// <returns>The time in milliseconds that the movement should take.</returns>
        private int findTimeToTargetMs(int pxDistance)
        {
            double divisor = 2.4;
            int divisor_rand_seed = RNG.GetStandardRandInt(5, 3, 0, 10);
            divisor += ((double)divisor_rand_seed) * 0.1d;
            int ret = 2 + rd.Next(0, 3) + rd.Next(1, pxDistance % 31 + pxDistance / 7) + (int)((double)pxDistance / divisor);
            return ret;
        }

        /// <summary>
        /// Changes ref parameters newDx, newDy such that the mouse is moved closer to the next circle.
        /// </summary>
        /// <param name="newDx">The change in the x direction</param>
        /// <param name="newDy">The change in the y direction</param>
        /// <param name="x_mouse">Current mouse position X</param>
        /// <param name="y_mouse">Current mouse position Y</param>
        /// <param name="angle">The angle from the current mouse position to the center of circle</param>
        /// <param name="x_circle">X coordinate of the circle</param>
        /// <param name="y_circle">Y coordinate of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns>2 if newDy would be 0. 1 if newDx would be 0. Returns 0 otherwise</returns>
        private int PaintLineToCircle(ref int newDx, ref int newDy, int x_mouse, int y_mouse, double angle, int x_circle, int y_circle, int r)
        {
            double slope_x = System.Math.Tan(angle / (((double)180 / System.Math.PI)));
            double slope_y = System.Math.Tan((angle - 90.0) / (((double)180 / System.Math.PI)));
            bool walkX = System.Math.Abs(slope_x) <= System.Math.Abs(slope_y);
            while (!VMath.IsInCircle(x_circle, y_circle, x_mouse + newDx, y_mouse + newDy, r))
            {
                if (walkX)
                {
                    if (x_mouse > x_circle)
                        newDx--;
                    else
                        newDx++;
                    newDy = -(int)(((double)newDx) * slope_x);
                }
                else
                {
                    if (y_mouse < y_circle)
                        newDy++;
                    else
                        newDy--;
                    newDx = (int)(((double)newDy) * slope_y);
                }
            }
            if (newDy == 0)
                return 2;
            if (newDx == 0)
                return 1;
            return 0;
        }
    }
}