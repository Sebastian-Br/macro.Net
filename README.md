# macro.Net

UI automation framework for Windows platforms. Can locate images and text on the screen efficiently, move the mouse

Prerequisites:
  * Only tested on a 1920x1080 screen with the Tesseract OCR version present in this repository
  * Somewhat CPU/memory intensive, as multiple instances of Tesseract are used to parse images simultaneously (tested on an i7 8700K 6x3.7 Ghz)
  
Benefits:
  * Interacts with UI more like a human would, which is useful in environments where automation is otherwise suppressed
  * Can deal with slight noise when recognizing text (though images are generally considered to be basically noise free)
</br>
This is primarily a showcase project and is far from complete. It's intended to show how I write and document code, but it does offer some functionality; e.g. it can locate Google's "I'm not a Robot" Button, move the mouse to it similar to how a human would, click and pass the test with ease.
</br>
Currently, you have to define this set of instructions (recognizing an image/text 'MatchTemplate's, moving/clicking the mouse 'ActionTemplate's) programmatically and construct a graph that describes the order in which Match/ActionTemplates are to be executed and how they are related. This is sort of sluggish and ideally, Macros would be defined in a UI where the author would have a much better overview of how their Macro works. This however is beyond the scope of this demo project and I would probably opt for Unity to create the Graph Designer UI in the future.
</br>
A MatchTemplate can match either an image or a text on the screen. If it is to match an image, a path to a suitable image has to be provided. Keep in mind that the Snip-Tool shipped with Windows can not be used to take suitable screenshots (those are blurry and hard to recognize); the ScreenShotTaker tool has been designed for this purpose (simply resize its screenshots with MS-Paint).
</br>
Part of the project is also a tool that updates file references used to document the project's code. You can include (local) links to word (*.rtf) files in the comment section when documenting a function, class, or other piece of code. Within that word file, you may specify the code more in depth as would be possible inside the code editor. This saves space and makes it easier to structure code. After cloning the project, run the UpdateDocumentationReferences.py script to update all of the local file links, such that you can open the word files by shift+left-clicking on the links from inside Visual Studio.
