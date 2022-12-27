using macro.Net.Engine;
using macro.Net.ImageDetection;
using macro.Net.ImageProcessing;
using macro.Net.Math;
using macro.Net.OCR;
using macro.Net.Screen;
using macro.Net.Templates;
using System.Diagnostics;
using System.Drawing.Text;

using (Process proc = Process.GetCurrentProcess())
    proc.PriorityClass = ProcessPriorityClass.High;

await Task.Delay(4000);
ClickGoogleRecaptcha_ByImage().ExecuteGraph();

//Console.WriteLine("EOF");
while (true)
{
    await Task.Delay(10000);
}

MacroEngine ClickWin10HomeButton()
{
    MacroEngine m = new("tessdata", "IMAGES", true);
    Rectangle default_roi_screen = new Rectangle(0, 0, 1920, 1080);

    ActionTemplate root = new(900, 500);
    m.SetRootActionTemplate(root);
    MatchTemplate WinHomeButton = new(m, default_roi_screen, "Win10HfromFS.JPG", true, "WinHomeButton");
    m.AddMatchTemplate(WinHomeButton);
    ActionTemplate move_mouse_to_winhomebutton = new(ActionTemplate.MouseEvent.MoveMouse, "WinHomeButton");
    m.AddActionTemplate(move_mouse_to_winhomebutton);
    ActionTemplate clickLMB = new(ActionTemplate.MouseEvent.SingleClickLeftMouseButton, "");
    m.AddActionTemplate(clickLMB);
    return m;
}

// Clicks https://www.google.com/recaptcha/api2/demo . Only works when the zoom level is 100%
MacroEngine ClickGoogleRecaptcha_ByImage()
{
    MacroEngine m = new("tessdata", "IMAGES", true);
    Rectangle default_roi_screen = new Rectangle(0, 0, 1920, 1080);
    
    ActionTemplate root = new(900, 500);
    m.SetRootActionTemplate(root);
    MatchTemplate not_a_robot_button = new(m, default_roi_screen, "notarobot.bmp", true, "norobot");
    m.AddMatchTemplate(not_a_robot_button);

    ActionTemplate move_mouse_to_notarobot_button = new(ActionTemplate.MouseEvent.MoveMouse, "norobot");
    m.AddActionTemplate(move_mouse_to_notarobot_button);
    ActionTemplate clickLMB = new(ActionTemplate.MouseEvent.SingleClickLeftMouseButton, "");
    m.AddActionTemplate(clickLMB);
    ActionTemplate wait_for_ui_to_expand = new(ActionTemplate.WaitEvent.SimplyWait, 2000);
    m.AddActionTemplate(wait_for_ui_to_expand);

    MatchTemplate check_for_image_captcha = new(m, default_roi_screen, "bsod.bmp", true, "");
    m.AddMatchTemplate(check_for_image_captcha);

    MatchTemplate image_captcha_success = new MatchTemplate(m, "NotARobot_Success.bmp", true, "");
    ActionTemplate wait_for_user_to_solve_captcha = new("Please solve the image captcha.", image_captcha_success, true, false);
    m.AddActionTemplate(wait_for_user_to_solve_captcha);

    ActionTemplate wait_for_ui_to_expand_2 = new(ActionTemplate.WaitEvent.SimplyWait, 1500);
    m.AddActionTemplate(wait_for_ui_to_expand_2);
    MatchTemplate submit_ui = new(m, default_roi_screen, "SubmitButton.bmp", true, "submit_ui");
    m.AddMatchTemplate(submit_ui);
    ActionTemplate move_mouse_to_submit_button = new(ActionTemplate.MouseEvent.MoveMouse, "submit_ui");
    m.AddActionTemplate(move_mouse_to_submit_button);
    ActionTemplate clickLMB2 = new(ActionTemplate.MouseEvent.SingleClickLeftMouseButton, "");
    m.AddActionTemplate(clickLMB2);
    return m;
}