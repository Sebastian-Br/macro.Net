using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.Wait
{
    public static class WaitService
    {
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