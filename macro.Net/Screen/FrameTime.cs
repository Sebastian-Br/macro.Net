using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;

namespace macro.Net.Screen
{
    class FrameTime
    {
        /// <summary>
        /// The previous content of the Desktop (Screenshot).
        /// </summary>
        private Graphics PreviousFrame { get; set; }

        /// <summary>
        /// The current Screenshot.
        /// </summary>
        private Graphics CurrentFrame { get; set; }

        /// <summary>
        /// This gets set to true when you want to shut down the master thread.
        /// </summary>
        private bool ShutdownScheduled { get; set; }

        /// <summary>
        /// The amount of iterations on this CPU per millisecond.
        /// </summary>
        private int IterationsPerMS { get; set; }

        /// <summary>
        /// The average Frame Time [ms] of the last 5 seconds.
        /// </summary>
        private int AverageRecentFrameTime { get; set; }

        /// <summary>
        /// E.g. if AverageRecentFrameTime = 10, a 15ms Frame will proc the followup-procedure.
        /// </summary>
        public int LagCutoffPercentage { get; set; }

        /// <summary>
        /// Internal, temporary Bitmap variable.
        /// </summary>
        private Bitmap ScreenshotBm { get; set; }

        private GraphicsDescriptor ImageGraphicsDescriptor { get; set; }

        /// <summary>
        /// The size of the on-screen image you wish to record.
        /// </summary>
        private Size ImageSize { get; set; }

        public FrameTime(GraphicsDescriptor _GraphicsDescriptor)
        {
            ImageGraphicsDescriptor = _GraphicsDescriptor;
            ImageSize = new(ImageGraphicsDescriptor.Xsize, ImageGraphicsDescriptor.Ysize);
            ShutdownScheduled = false;
            InitSpinIterations1MS();
            ScreenshotBm = new Bitmap(100, 2, PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// Saves the content of the screen to a 'Graphics'-object.
        /// This presumes that your screen Height and Width are properly configured.
        /// </summary>
        /// <returns></returns>
        public Graphics GetFrame()
        {
            Stopwatch clock = new(); clock.Start();
            const int X = 100; // 240, 480, 960
            const int Y = 2;
            Bitmap Screenshot = new Bitmap(X, Y, PixelFormat.Format32bppArgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too.
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            // Console.WriteLine("GetFrame() Execution-Time1: " + clock.ElapsedMilliseconds + " [ms]."); this is always 0 ms.
            //Size s = new(ScreenWidth, ScreenHeight);
            gScreenshot.CopyFromScreen(960 - X / 2, 540 - Y / 2, 0, 0, ImageSize);
            Console.WriteLine("GetFrame() Execution-Time2: " + clock.ElapsedMilliseconds + " [ms].");
            //Screenshot.Save("Screen" + DateTime.Now.Year + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".png", ImageFormat.Png);
            clock.Stop();
            Console.WriteLine("GetFrame() Execution-Time3: " + clock.ElapsedMilliseconds + " [ms].");
            //ShutdownScheduled = true; //TEST CODE
            return gScreenshot;
        }


        public static byte[] DbgGetFrameAsBitmapByteArray()
        {
            Bitmap Screenshot = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            // Console.WriteLine("GetFrame() Execution-Time1: " + clock.ElapsedMilliseconds + " [ms]."); this is always 0 ms.
            //Size s = new(ScreenWidth, ScreenHeight);
            Size s = new(1920, 1080);
            gScreenshot.CopyFromScreen(0, 0, 0, 0, s);
            return ToByteArray(Screenshot, ImageFormat.Bmp);
        }

        private static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// This function sets the RecentAverageFrameTime member.
        /// Call this only after IterationsPerMS has been initialized.
        /// </summary>
        private async Task GetRecentAverageFrameTime()
        {

            Stopwatch clock = new();
            long currentFrameTime = 0;
            const int PollsPerSecond = 4;
            const int RecentSeconds = 5;
            const int ArraySize = PollsPerSecond * RecentSeconds;
            long[] recentFrameTimes = new long[ArraySize];
            for (int i = 0; i < ArraySize; i++)
            {
                recentFrameTimes[i] = 0;
            }

            Graphics prevFrame = GetFrame();
            Graphics currFrame = GetFrame();
            int loopCounter = 0;
            while (!ShutdownScheduled)
            {
                prevFrame = GetFrame();
                currFrame = GetFrame();
                clock.Restart();
                while (PreviousFrame == CurrentFrame)
                {
                    System.Threading.Thread.SpinWait(IterationsPerMS); //this waits for ~1 MS.
                    currFrame = GetFrame();
                }

                clock.Stop();
                currentFrameTime = clock.ElapsedMilliseconds;
                Console.WriteLine("GetRecentAverageFrameTime: currentFrameTime = " + currentFrameTime);
                recentFrameTimes[loopCounter] = currentFrameTime;
                loopCounter++;
                if (loopCounter >= ArraySize)
                {
                    loopCounter = 0; // this could be done with '%' but I don't trust MS's mod definition.
                }

                AverageRecentFrameTime = ((int)recentFrameTimes.Average());
                Console.WriteLine("GetRecentAverageFrameTime(): AverageRecentFrameTime = " + AverageRecentFrameTime);
                System.Threading.Thread.Sleep((int)(1000 / PollsPerSecond));
            }
            return;
        }

        public async Task StartMasterThread()
        {
            Task.Run(() => GetRecentAverageFrameTime().ConfigureAwait(continueOnCapturedContext: false)); //starts the Avg-Frame-Time-Thread.
            System.Threading.Thread.Sleep(5500);
            Stopwatch clock = new();
            long currentFrameTime = 0;
            while (!ShutdownScheduled)
            {
                CurrentFrame = GetFrame();
                PreviousFrame = GetFrame();
                clock.Restart();
                while (PreviousFrame == CurrentFrame)
                {
                    //System.Threading.Thread.SpinWait(IterationsPerMS); //this waits for ~1 MS.
                    CurrentFrame = GetFrame();
                }

                clock.Stop();
                currentFrameTime = clock.ElapsedMilliseconds;
                Console.WriteLine("MASTER-THREAD: currentFrameTime = " + currentFrameTime);
                if (currentFrameTime >= (1 + (LagCutoffPercentage / 100)) * AverageRecentFrameTime)
                {
                    Console.WriteLine("MASTER-THREAD: currentFrameTime = " + currentFrameTime + " >> AverageRecentFrameTime = " + AverageRecentFrameTime);
                    Console.Beep(1000, 2);
                }
                else
                {
                    //Console.WriteLine("MASTER-THREAD: OK!");
                }
            }
            return;
        }

        /// <summary>
        /// To construe an accurate timer, we need to use the SpinWait() method.
        /// However, this method only accepts the 'iterations' input, and CPU speed impacts wait time.
        /// As a result the actual duration of a SpinWait() has to be assessed first.
        /// </summary>
        /// <returns>The amount of iterations such that SpinWait waits for 1 ms.</returns>
        private void InitSpinIterations1MS()
        {
            int iterations = 0;
            double lastDuration = 0.001; // duration of the last wait in [ms].
            double lastThreeAvg = 0;
            const int N = 50;
            double[] lastN = new double[N];
            for (int i = 0; i < N; i++)
                lastN[i] = 0;
            int arrayCounter = 0;
            while (lastThreeAvg < 0.5) //half a MS
            {
                iterations += 5;
                long startTicks = Stopwatch.GetTimestamp();
                System.Threading.Thread.SpinWait(iterations);
                long endTicks = Stopwatch.GetTimestamp();
                Console.WriteLine("InitSpinIterations1MS(): #Ticks = " + (endTicks - startTicks) + " @Iterations= " + iterations);
                lastDuration = (endTicks - startTicks) / 10000.0;
                lastN[arrayCounter % N] = lastDuration;
                Console.WriteLine("InitSpinIterations1MS(): lastDuration = " + lastDuration + " [ms].");
                arrayCounter++;
                lastThreeAvg = lastN.Average();
                Console.WriteLine("LastNAvg()= " + lastThreeAvg + " [ms].");
            }

            IterationsPerMS = iterations;
        }

        /// <summary>
        /// Call this method to shutdown the Thread associated with this object.
        /// </summary>
        public void StopMasterThread()
        {
            ShutdownScheduled = true;
        }
    }

    public class GraphicsDescriptor
    {
        public GraphicsDescriptor(int _Xsource, int _Ysource, int _Xsize, int _Ysize)
        {
            Xsource = _Xsource;
            Ysource = _Ysource;
            Xsize = _Xsize;
            Ysize = _Ysize;
        }
        public int Xsource { get; set; }
        public int Ysource { get; set; }
        public int Xsize { get; set; }
        public int Ysize { get; set; }
    }
}