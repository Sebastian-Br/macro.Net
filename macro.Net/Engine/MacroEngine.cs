using macro.Net.ImageDetection;
using macro.Net.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.Engine
{
    internal class MacroEngine
    {
        public MacroEngine()
        {
            ScreenShotSvc = new(80);
            OpticalCharacterRecognition = new("tessdata", 4, ScreenShotSvc);
            Image_Detector = new(ScreenShotSvc, "IMAGES");
        }
        private OCR.OCR OpticalCharacterRecognition { get; set; }

        private ScreenShotService ScreenShotSvc { get; set; }

        private ImageDetector Image_Detector { get; set; }
    }
}