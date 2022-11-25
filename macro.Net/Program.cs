using macro.Net.OCR;
using macro.Net.Screen;
using System.Diagnostics;

using (Process proc = Process.GetCurrentProcess())
    proc.PriorityClass = ProcessPriorityClass.High;

OCR ocr = new("E:\\Visual Studio\\Projects\\macro.Net\\tessdata", 4);
Console.WriteLine("finished loading tessData");
await Task.Delay(2000);
Stopwatch clock = new(); clock.Start();
TextMatch awaitMatch = await ocr.GetFirstWordFromScreenTiles("await", StringComparison.InvariantCultureIgnoreCase);
clock.Stop();
Console.WriteLine("GetFirstWordPosition() resolved in " + clock.ElapsedMilliseconds + " [ms]");
Paint p = new(0.34, 5000); // low opacity = high transparency
await p.DrawContainingRectangle(awaitMatch.MatchRect);


Console.WriteLine("2");

while(true)
{
    await Task.Delay(10000);
}