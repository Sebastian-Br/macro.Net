using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.Templates
{
    /// <summary>
    /// This class describes the actions that can be taken after the UI you intend to interact with has been located.
    /// The class itself only describes which actions are supported. The implementation can be found in the MacroEngine.cs file.
    /// Dragging is currently not supported.
    /// </summary>
    public class ActionTemplate
    {
        /// <summary>
        /// When this is not null, input this text.
        /// </summary>
        private string KeyboardInput { get; set; }

        /// <summary>
        /// When this is not null, move the mouse to this point.
        /// </summary>
        private Point? MouseMoveTarget { get; set; }

        /// <summary>
        /// Positive values = Scroll up that many times. Negative values = Scroll down that many times.
        /// </summary>
        private int MouseScrollVertically { get; set; }

        private MouseEvent Mouse_Click { get; set; }

        private WaitEvent Wait_Event { get; set; }

        private int WaitDurationMs { get; set; }

        private ActionTemplate ChildAction { get; set; }

        private MatchTemplate ChildMatchTemplate { get; set; }

        /// <summary>
        /// Temporarily turns true when checking for a cycle (when skipping to the next Match Template)
        /// Is set to false after the search
        /// </summary>
        public bool CycleCheck_Visited { get; set; }

        public string DictionaryKey { get; set; }

        public string UserHelpMessage { get; set; }

        public bool UserHelpMessage_UseMsgBox { get; set; }
        public bool UserHelpMessage_UseTTS { get; set; }

        public MatchTemplate UserHelpSuccessCondition { get; set; }

        public void SetChildAction(ActionTemplate _child_action)
        {
            ChildAction = _child_action;
        }

        public ActionTemplate GetChildAction()
        {
            return ChildAction;
        }

        public void SetChildMatchTemplate(MatchTemplate _child_match_template)
        {
            ChildMatchTemplate = _child_match_template;
        }

        public MatchTemplate GetChildMatchTemplate()
        {
            return ChildMatchTemplate;
        }

        public ActionTemplate(string keyboardInput, string dictionaryKey)
        {
            KeyboardInput = keyboardInput;
            CycleCheck_Visited = false;
            DictionaryKey = dictionaryKey;
        }

        /// <summary>
        /// Construct an ActionTemplate to move the mouse to a fixed position on the screen.
        /// </summary>
        /// <param name="mouse_destination_x"></param>
        /// <param name="mouse_destination_y"></param>
        public ActionTemplate(int mouse_destination_x, int mouse_destination_y)
        {
            MouseMoveTarget = new(mouse_destination_x, mouse_destination_y);
            CycleCheck_Visited = false;
        }

        public ActionTemplate(int mouse_scroll_vertical, string dictionaryKey)
        {
            MouseScrollVertically = mouse_scroll_vertical;
            CycleCheck_Visited = false;
            DictionaryKey = dictionaryKey;
        }

        public ActionTemplate(MouseEvent mouse_event, string dictionaryKey)
        {
            Mouse_Click = mouse_event;
            CycleCheck_Visited = false;
            DictionaryKey = dictionaryKey;
        }

        public ActionTemplate(WaitEvent wait_event, int wait_duration_ms)
        {
            Wait_Event = wait_event;
            CycleCheck_Visited = false;
            WaitDurationMs = wait_duration_ms;
        }

        /// <summary>
        /// This constructor sets up an ActionTemplate to wait for the user to help the application.
        /// </summary>
        /// <param name="wait_user_help_msg"></param>
        /// <param name="end_wait_condition"></param>
        public ActionTemplate(string wait_user_help_msg, MatchTemplate end_wait_condition, bool userHelpMessage_UseMsgBox, bool userHelpMessage_UseTTS)
        {
            Wait_Event = WaitEvent.WaitForUserToHelp;
            CycleCheck_Visited = false;
            UserHelpMessage = wait_user_help_msg;
            UserHelpSuccessCondition = end_wait_condition;
            UserHelpMessage_UseMsgBox = userHelpMessage_UseMsgBox;
            UserHelpMessage_UseTTS = userHelpMessage_UseTTS;
        }

        public enum MouseEvent
        {
            SingleClickLeftMouseButton = 1,
            SingleClickRightMouseButton = 2,
            SingleClickMiddleMouseButton = 3,
            MoveMouse = 4
        };

        public enum WaitEvent
        {
            SimplyWait = 1,
            WaitAndDoodle = 2, //TBD
            WaitForUserToHelp = 3
        }

        public string GetKeyboardInput()
        {
            return KeyboardInput;
        }

        public Point? GetMouseMoveTarget()
        {
            return MouseMoveTarget;
        }

        public int GetMouseScrollVertically()
        {
            return MouseScrollVertically;
        }

        public MouseEvent GetMouseClick()
        {
            return Mouse_Click;
        }
        public WaitEvent GetWaitEvent()
        {
            return Wait_Event;
        }

        public async Task Wait()
        {
            await Task.Delay(WaitDurationMs);
        }
    }
}