using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace macro.Net.InputSimulationService
{
    public class InputSimulationService
    {
        public InputSimulationService()
        {
            Mouse = new();
            Keyboard = new();
        } 
        public void MoveMouseToPosition(int x, int y)
        {
            Mouse.MouseMoveSimple(x, y);
        }

        public async void SendKeyStrokes(string KeyStrokes)
        {
            if (KeyStrokes == null)
                return;
            if (KeyStrokes.Length == 0)
                return;

            char[] characters = KeyStrokes.ToCharArray();
            int wait_time_ms = 0;
            foreach(char c in characters)
            {
                Keyboard.Keyboard.TextEntry(c);
            }

        }

        private Mouse Mouse { get; set; }

        /// <summary>
        /// This class doesn't really care about inputting single characters realistically which would entail using keydown/keyup and pausing
        /// inbetween.
        /// </summary>
        private InputSimulator Keyboard { get; set; }
    }
}