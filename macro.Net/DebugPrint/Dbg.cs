using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.DebugPrint
{
    public static class Dbg
    {
        public static void Print(string message, bool debug)
        {
            if(debug)
                Console.WriteLine(message);
        }
    }
}