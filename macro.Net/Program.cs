using macro.Net.OCR;



macro.Net.Screen.Paint p = new(0.2, 3000); // low opacity = high transparency
//await p.DrawContainingRectangle(new Rectangle() { X = 1000, Y = 100, Width = 200, Height = 30 });
Console.WriteLine("2");

while(true)
{
    await Task.Delay(10000);
}