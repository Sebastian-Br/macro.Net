# macro.Net

UI automation framework for Windows platforms.

Prerequisites:
  * Only tested on a 1920x1080 screen with the Tesseract OCR versions present in this repository
  * Somewhat CPU/memory intensive, as multiple instances of Tesseract are used to parse images simultaneously (tested on an i7 8700K 6x3.7 Ghz)
  
Benefits:
  * Interacts with UI more like a human would, which is useful in environments where automation is otherwise suppressed
  * Can deal with slight noise when recognizing text (though images are generally considered to be noise free)
