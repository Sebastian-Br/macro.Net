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
        /// <param name="standard_deviation">The standard deviation.</param>
        /// <param name="min">The minimum result the caller wants to receive.</param>
        /// <param name="max">The maximum result the caller wants to receive.</param>
        /// <returns>A random std-distributed integer between min and max.</returns>
        public int GetStandardRand(int mean, double standard_deviation, int min, int max)
        {
            try
            {
                if(mean > max || mean < min)
                {
                    throw new InvalidOperationException("Mean can not be smaller than min or greater than max");
                }

                double d1 = 1.0 - r.NextDouble(); //uniform(0,1] random doubles
                double d2 = 1.0 - r.NextDouble();
                double normal_distribution = System.Math.Sqrt(-2.0 * System.Math.Log(d1)) * System.Math.Sin(2.0 * System.Math.PI * d2); //random normal(0,1) <-- this comment is bs!
                int normal_random_int = (int)(mean + standard_deviation * normal_distribution); //random normal(mean,stdDev^2)

                if (normal_random_int < min)
                    normal_random_int = min;
                if (normal_random_int > max)
                    normal_random_int = max;
                return normal_random_int;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetStandardRand(): " + e);
                return mean;
            }
        }

        /// <summary>
        /// Produces a distribution where around 36% of the values are in the range of [0.4-0.6].
        /// Around 7-8% of values are in the range of [0.0-0.2] and [0.8-1.0] each.
        /// </summary>
        /// <returns>A normally distributed double</returns>
        public double GetNextDouble()
        {
            double u = 1.0 - r.NextDouble();
            double v = 1.0 - r.NextDouble();
            double s = System.Math.Sqrt(u * u + v * v);

            while (true) // the time complexity here is O(let's not get too unlucky)
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
            if(r.Next(int.MaxValue) % 2 == 0) // randomly chooses to add or subtract
            {
                result = 0.5 + System.Math.Abs(s_half - 0.5); // adding/subtracting the absolute difference between 0.5 and s_half yields a distribution centered around 0.5
            }
            else
            {
                result = 0.5 - System.Math.Abs(s_half - 0.5);
            }
            return result;
        }

        /// <summary>
        /// The Random instance used to help create a normal distribution
        /// </summary>
        Random r { get; set; }
    }
}