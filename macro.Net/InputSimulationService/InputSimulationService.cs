using macro.Net.Math;
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
        public InputSimulationService(Rand _rng, bool _debug)
        {
            Mouse = new(_rng, _debug);
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

        public async void ClickLeftMouseButton()
        {
            Keyboard.Mouse.LeftButtonClick();
        }

        public async void ClickRightMouseButton()
        {
            Keyboard.Mouse.RightButtonClick();
        }

        public async void ScrollDownOnce()
        {
            Keyboard.Mouse.VerticalScroll(-1);
        }

        public async void ScrollUpOnce()
        {
            Keyboard.Mouse.VerticalScroll(1);
        }

        private Mouse Mouse { get; set; }

        /// <summary>
        /// This class doesn't really care about inputting single characters realistically which would entail using keydown/keyup and pausing
        /// inbetween.
        /// </summary>
        private InputSimulator Keyboard { get; set; }
    }
}