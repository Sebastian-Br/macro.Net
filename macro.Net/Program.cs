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


ClickWin10HomeButton().ExecuteGraph();

Console.WriteLine("EOF");
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