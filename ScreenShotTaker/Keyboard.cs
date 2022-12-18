using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShotTaker
{
    public static class Keyboard
    {
        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        /// <summary>
        /// https://stackoverflow.com/questions/1100285/how-to-detect-the-currently-pressed-key
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static KeyStates GetKeyStateFromCode(int code)
        {
            KeyStates state = KeyStates.None;
            short retVal = GetKeyState(code);

            //If the high-order bit is 1, the key is down
            //otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        public static bool IsKeyDown(int key)
        {
            return 1 == ((int)GetKeyStateFromCode(key) & 1);
        }

        public static bool IsKeyToggled(int key)
        {
            return 2 == ((int)GetKeyStateFromCode(key) & 2);
        }
    }
}
