﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShotTaker
{
    public class Configuration
    {
        public Configuration()
        {
            DefaultImagesPath = "";
        }
        public string DefaultImagesPath { get; set; }
    }
}
