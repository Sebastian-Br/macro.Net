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

namespace macro.Net.InputSimulationService
{
    public class Mouse
    {
        public Mouse(Rand _rng, bool _debug)
        {
            rd = new Random();
            ScreenWidthMinus1 = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 1;
            ScreenHeightMinus1 = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 1;
            point_for_MouseMoveSimple = new();
            Debug = _debug;
            RNG = _rng;
        }

        [DllImport("User32.Dll")]
        private static extern long SetCursorPos(int x, int y);
        [DllImport("User32.Dll")]
        private static extern bool GetCursorPos(ref POINT p); // top left corner = 0,0

        private static Random rd;

        private Rand RNG { get; set; }

        private POINT point_for_MouseMoveSimple;

        private int ScreenWidthMinus1 { get; set; }
        private int ScreenHeightMinus1 { get; set; }

        private bool Debug { get; set; }

        public void MouseMoveSimple(int x, int y)
        {
            GetCursorPos(ref point_for_MouseMoveSimple);
            int time_ms_left = findTimeToTargetMs(VMath.GetDistance(x, y, point_for_MouseMoveSimple.x, point_for_MouseMoveSimple.y));
            MouseMoveSimple_Rec(x, y, time_ms_left, 0);
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
            int xPos = p.x + dX;
            int yPos = p.y + dY;
            if (xPos < 0)
                xPos = 0;
            if (yPos < 0)
                yPos = 0;
            if (xPos > ScreenWidthMinus1)
                xPos = ScreenWidthMinus1;
            if (yPos > ScreenHeightMinus1)
                yPos = ScreenHeightMinus1;
            //dbg("Moved from (" + p.x + "," + p.y + ") to (" + xPos + "," + yPos + ")");
            SetCursorPos(xPos, yPos);
        }

        /// <summary>
        /// Recursively moves the mouse to the target (X,Y) location.
        /// Set a realistic timeMsLeft! Mouse movement should not be too quick.
        /// The timeMsLeft-parameter gets passed to the next recursion and decreases over time,
        /// as each move-step has a wait-duration associated with it.
        /// The lastDevAngle gets passed to the next recursion to generate more fluent movement;
        /// a.k.a. this param changes only a bit per recursion.
        /// </summary>
        /// <param name="X">The target X coordinate.</param>
        /// <param name="Y">The target Y coordinate.</param>
        /// <param name="timeMsLeft">The time, in Milliseconds, left to move the cursor to that location.</param>
        /// <param name="lastDevAngle">The deviation in deg° from the shortest path of the previous call.</param>
        private void MouseMoveSimple_Rec(int X, int Y, int timeMsLeft, double lastDevAngle)
        {
            GetCursorPos(ref point_for_MouseMoveSimple);
            if (VMath.isInCircle(X, Y, point_for_MouseMoveSimple.x, point_for_MouseMoveSimple.y, 4))
            {
                Dbg.Print("MouseMoveSimple() - Reached target!", Debug);
                SmartWait(timeMsLeft - 1);
                return;
            }
            //dbg("MouseMove2(): @(" + p.x + "," + p.y + ")" + " aiming@(" + X + "," + Y +")");
            int px_distance = VMath.GetDistance(X, Y, point_for_MouseMoveSimple.x, point_for_MouseMoveSimple.y);
            if (timeMsLeft == 0) // prevent division by 0
                timeMsLeft = 1;
            double pxSpeedMin = px_distance / timeMsLeft;
            //dbg("MouseMove2(): Chose minspeed: " + pxSpeedMin + " [px/ms]");
            int movementDuration = RNG.GetStandardRand(2, 0.75, 1, 3); // using GetStandardRand(2, 0.75, 1, 3) yields 2:50%, 1/3:25%
            if (timeMsLeft > 97 + rd.Next(-6, 18) && rd.Next(0, 99) == 8)
            {
                movementDuration = 3 + RNG.GetStandardRand(9, 4, 1, 18); // micro-wait.
                Dbg.Print("MicroWait!", Debug);
                goto loc_wait;
            }
            //dbg("MouseMove2(): Chose movementDuration: " + movementDuration);
            int pxMovement = movementDuration * (int)pxSpeedMin;
            if (pxMovement == 0)
                pxMovement = 1;
            //dbg("MouseMove2(): Chose pxMovement: " + pxMovement);
            int radiusAroundTarget = px_distance - pxMovement;
            double angle = VMath.GetAngle(point_for_MouseMoveSimple.x, point_for_MouseMoveSimple.y, X, Y);
            //dbg("MouseMove2(): Actual Angle: " + angle);
            lastDevAngle = CheckLastDevAngle(lastDevAngle);
            double angleDeviation = (2 * rd.Next(-1, 2)) * RNG.GetNextDouble() + lastDevAngle; // +-18 deg
            lastDevAngle = angleDeviation;
            angle += angleDeviation;
            //dbg("MouseMove2(): Movement Angle: " + angle);
            //dbg("MouseMove2(): angleDeviation: " + angleDeviation);
            int newdx = 0; int newdy = 0;
            if (radiusAroundTarget <= 4)
                radiusAroundTarget = 4;
            int pResult = paintLineToCircle(ref newdx, ref newdy, point_for_MouseMoveSimple.x, point_for_MouseMoveSimple.y, angle, X, Y, radiusAroundTarget);
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
            SmartWait(movementDuration);
            MouseMoveSimple_Rec(X, Y, timeMsLeft - movementDuration, lastDevAngle);
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
        /// <param name="lastDevAngle"></param>
        private double CheckLastDevAngle(double lastDevAngle)
        {
            double new_deviation_angle = 0;
            if (lastDevAngle > 16)
                new_deviation_angle = 16;
            else if (lastDevAngle < -16)
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
            int divisor_rand_seed = RNG.GetStandardRand(5, 3, 0, 10);
            divisor += ((double)divisor_rand_seed) * 0.1d;
            int ret = 2 + rd.Next(0, 3) + rd.Next(1, pxDistance % 31 + pxDistance / 7) + (int)((double)pxDistance / divisor);
            return ret;
        }

        /// <summary>
        /// Changes ref parameters newDx, newDy such that the mouse is moved closer to the next circle.
        /// </summary>
        /// <param name="newDx"></param>
        /// <param name="newDy"></param>
        /// <param name="x">Current mouse position X</param>
        /// <param name="y">Current mouse position Y</param>
        /// <param name="angle"></param>
        /// <param name="X">X coordinate of the circle</param>
        /// <param name="Y">Y coordinate of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        private int paintLineToCircle(ref int newDx, ref int newDy, int x, int y, double angle, int X, int Y, int r)
        {
            double slopeX = System.Math.Tan(angle / (((double)180 / System.Math.PI)));
            double slopeY = System.Math.Tan((angle - 90.0) / (((double)180 / System.Math.PI)));
            bool walkX = VMath.Abs(slopeX) <= VMath.Abs(slopeY);
            while (!VMath.isInCircle(X, Y, x + newDx, y + newDy, r))
            {
                if (walkX)
                {
                    if (x > X)
                        newDx--;
                    else
                        newDx++;
                    newDy = -(int)(((double)newDx) * slopeX);
                }
                else
                {
                    if (y < Y)
                        newDy++;
                    else
                        newDy--;
                    newDx = (int)(((double)newDy) * slopeY);
                }
            }
            if (newDy == 0)
                return 2;
            if (newDx == 0)
                return 1;
            return 0;
        }

        /// <summary>
        /// Wait 'ms' milliseconds.
        /// This function is very accurate, unlike Sleep() which delegates the Thread
        /// </summary>
        /// <param name="ms">Milliseconds.</param>
        private static void SmartWait(int ms)
        {
            System.Diagnostics.Stopwatch watch = new();
            watch.Start();
            while (watch.ElapsedMilliseconds < ms)
            {
                System.Threading.Thread.SpinWait(4);
            }
            watch.Stop();
        }
    }
}