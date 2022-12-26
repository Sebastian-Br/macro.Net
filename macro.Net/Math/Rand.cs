/*
 * 
 * Optional: Add StdRand function for Double & without min/max.
 */

namespace macro.Net.Math // to avoid namespace colission
{
    public class Rand
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
                if(mean > max || mean < min)
                {
                    throw new InvalidOperationException("Mean can not be smaller than min or larger than max");
                }

                double d1 = 1.0 - r.NextDouble(); //uniform(0,1] random doubles
                double d2 = 1.0 - r.NextDouble();
                double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(d1)) * System.Math.Sin(2.0 * System.Math.PI * d2); //random normal(0,1)
                double drandNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
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

        /// <summary>
        /// Returns a normally distributed double.
        /// Produces a distribution where around 36% of the values are in the range of [0.4-0.6].
        /// Around 7-8% of values are in the range of [0.0-0.2] and [0.8-1.0] each.
        /// </summary>
        /// <returns></returns>
        public double GetNextDouble()
        {
            double u = 1.0 - r.NextDouble();
            double v = 1.0 - r.NextDouble();
            double s = System.Math.Sqrt(u * u + v * v);

            while (true)
            {
                if(s <= 1)
                {
                    break;
                }
                else
                {
                    u = 1.0 - r.NextDouble();
                    v = 1.0 - r.NextDouble();
                    s = System.Math.Sqrt(u * u + v * v);
                }
            }

            double s_half = s / 2; // [0-0.5] most of the values here are in the 0.4-0.5 range
            double result;
            if(r.Next(int.MaxValue) % 2 == 0)
            {
                result = 0.5 + System.Math.Abs(s_half - 0.5);
            }
            else
            {
                result = 0.5 - System.Math.Abs(s_half - 0.5);
            }
            return result;
        }

        Random r { get; set; }
    }
}