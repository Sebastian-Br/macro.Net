using macro.Net.ImageDetection;
using macro.Net.OCR;
using macro.Net.Screen;
using System.Diagnostics;

using (Process proc = Process.GetCurrentProcess())
    proc.PriorityClass = ProcessPriorityClass.High;

ScreenShotService screenShotService = new();
OCR ocr = new("E:\\Visual Studio\\Projects\\macro.Net\\tessdata", 4, screenShotService);
Console.WriteLine("finished loading tessData");
ImageDetector imageDetector = new(screenShotService, "IMAGES");
await Task.Delay(2000);

Console.WriteLine("2");

while(true)
{
    await Task.Delay(10000);
}