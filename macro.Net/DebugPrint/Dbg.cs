using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.DebugPrint
{
    public static class Dbg
    {
        /// <summary>
        /// Prints the message if debug is true
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="debug">The debug parameter</param>
        public static void Print(string message, bool debug)
        {
            if(debug)
                Console.WriteLine(message);
        }
    }
}