/*
 * 
 * Optional: Add StdRand function for Double & without min/max.
 */

namespace macro.Net.Math // to avoid namespace colission
{
    internal class Rand
    {
        public Rand()
        {
            r = new Random();
        }

        /// <summary>
        /// Get a random integer from a standard-distribution.
        /// </summary>
        /// <param name="mean">The centre/peak, 'mean', of the standard-distribution.</param>
        /// <param name="stdDev">The standard deviation.</param>
        /// <param name="min">The minimum result the caller wants to receive.</param>
        /// <param name="max">The maximum result the caller wants to receive.</param>
        /// <returns>A random std-distributed integer between min and max.</returns>
        public int GetStandardRand(int mean, double stdDev, int min, int max)
        {
            try
            {
                double d1 = 1.0 - r.NextDouble(); //uniform(0,1] random doubles
                double d2 = 1.0 - r.NextDouble();
                double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(d1)) * System.Math.Sin(2.0 * System.Math.PI * d2); //random normal(0,1)
                double drandNormal = (mean + stdDev * randStdNormal); //random normal(mean,stdDev^2)
                int randNormal = (int)drandNormal;

                if (drandNormal - randNormal >= 0.5) // round up.
                    randNormal++;

                if (randNormal < min)
                    randNormal = min;
                if (randNormal > max)
                    randNormal = max;
                return randNormal;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetStandardRand(): " + e);
                return mean;
            }

        }

        Random r { get; set; }
    }
}