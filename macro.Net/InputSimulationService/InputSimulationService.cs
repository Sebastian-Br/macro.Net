using macro.Net.Math;
using macro.Net.Wait;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace macro.Net.InputSimulationService
{
    /// <summary>
    /// Provides realistic mouse movement and simple keyboard input and mouse clicks
    /// </summary>
    public class InputSimulationService
    {
        /// <summary>
        /// Constructs a new InputSimulationService instance to provide realistic mouse movement as well as rudimentary keyboard input and mouse clicks
        /// </summary>
        /// <param name="_rng">The Random Number Generator shared between different classes</param>
        /// <param name="_debug">Whether to print (true) or not to print (false) debug messages</param>
        public InputSimulationService(Rand _rng, bool _debug)
        {
            Mouse = new(_rng, _debug);
            RNG = _rng;
            Keyboard = new();
        }

        /// <summary>
        /// Realistically moves the mouse to the input x,y position
        /// This function takes a small amount of time to execute
        /// </summary>
        /// <param name="x">The x coordinate of the target point on the screen</param>
        /// <param name="y">The y coordinate of the target point on the screen</param>
        public void MoveMouseToPosition(int x, int y)
        {
            Mouse.MouseMoveSimple(x, y);
        }

        /// <summary>
        /// Emulates key presses and types the string character by character
        /// This function doesn't resemble how a human would type very much, because it does not use keyup/keydown, which is a ToDo for the future.
        /// </summary>
        /// <param name="keystrokes">The text to type</param>
        public async void SendKeyStrokes(string keystrokes)
        {
            if (String.IsNullOrEmpty(keystrokes))
                return;

            char[] characters = keystrokes.ToCharArray();
            foreach(char c in characters)
            {
                WaitService.SmartWait(RNG.GetStandardRandInt(150, 50, 50, 250));
                Keyboard.Keyboard.TextEntry(c);
                WaitService.SmartWait(RNG.GetStandardRandInt(52, 20, 15, 90));
            }
        }

        public async void SingleClickLeftMouseButton()
        {
            Keyboard.Mouse.LeftButtonClick();
        }

        public async void SingleClickRightMouseButton()
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

        /// <summary>
        /// The instance that is used for human-like mouse movement; NOT for mouse clicks.
        /// </summary>
        private Mouse Mouse { get; set; }

        /// <summary>
        /// This class doesn't really care about inputting single characters realistically which would entail using keydown/keyup and pausing
        /// inbetween.
        /// </summary>
        private InputSimulator Keyboard { get; set; }

        private Math.Rand RNG { get; set; }
    }
}