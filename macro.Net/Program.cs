using macro.Net.ImageDetection;
using macro.Net.ImageProcessing;
using macro.Net.OCR;
using macro.Net.Screen;
using System.Diagnostics;

using (Process proc = Process.GetCurrentProcess())
    proc.PriorityClass = ProcessPriorityClass.High;

ScreenShotService screenShotService = new();
OCR ocr = new("tessdata", 4, screenShotService);
Console.WriteLine("finished loading tessData");
ImageDetector imageDetector = new(screenShotService, "IMAGES");
await Task.Delay(2000);

Stopwatch s = new();
s.Start();
TextMatch m = await ocr.GetFirstWordFromScreenTiles("await", StringComparison.InvariantCultureIgnoreCase);
s.Stop();
Console.WriteLine("Textmatch time elapsed: " + s.ElapsedMilliseconds);
Paint p = new(0.45, 7000);
if(m != null)
{
    p.DrawContainingRectangle(m.MatchRect);
}
else { Console.WriteLine("await not found!"); }

Console.WriteLine("2");

byte[] winFile = screenShotService.DbgGetFileAsByteArray_24BppArgb("IMAGES\\FFfromFS.JPG"); // pos on screen: 12, 1048
byte[] screen = screenShotService.GetFullScreenAsBmpByteArray_24bppRgb();
//byte[] screen = screenShotService.DbgGetFileAsByteArray_24BppArgb("IMAGES\\FullScreen.JPG");
ImageProcessor IP = new();
s.Restart();
Rectangle? r = IP.FindFirstImageInImage_24bppRGB(screen, 1920, 1080, winFile, 26, 24, 0);
s.Stop();
Console.WriteLine("ImageMatch time elapsed: " + s.ElapsedMilliseconds);
if (r != null)
{
    p.DrawContainingRectangle(r.Value);
}
else
{
    Console.WriteLine("Image not found!");
}

Console.WriteLine("3");

while (true)
{
    await Task.Delay(10000);
}