using macro.Net.ImageDetection;
using macro.Net.Math;
using macro.Net.OCR;
using macro.Net.Screen;
using macro.Net.Templates;
using macro.Net.Wait;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using macro.Net.DebugPrint;

namespace macro.Net.Engine
{
    public class MacroEngine
    {
        public MacroEngine(string _tessdata_dir, string _image_dir, bool _debug)
        {
            RNG = new();
            ScreenShotSvc = new(80);
            OpticalCharacterRecognition = new(_tessdata_dir, 4, ScreenShotSvc, _debug);
            Image_Detector = new(ScreenShotSvc, _image_dir, _debug);
            InputSim = new(RNG, _debug);
            AllActionTemplates = new();
            AllMatchTemplates= new();
            ActionRegionsOfInterestDictionary = new();
            DebugMode = _debug;
        }

        private bool DebugMode { get; set; }
        private OCR.OCR OpticalCharacterRecognition { get; set; }

        public OCR.OCR GetOCR()
        {
            return OpticalCharacterRecognition;
        }

        private ScreenShotService ScreenShotSvc { get; set; }

        private ImageDetector Image_Detector { get; set; }

        public ImageDetector GetImageDetector()
        {
            return Image_Detector;
        }

        private InputSimulationService.InputSimulationService InputSim { get; set; }

        private Rand RNG { get; set; }

        private ActionTemplate RootActionTemplate { get; set; }

        /// <summary>
        /// Used when adding Templates.
        /// If the most recent element in the graph is an ActionTemplate, this is not null, and MostRecentMatchTemplate is null.
        /// </summary>
        private ActionTemplate MostRecentActionTemplate { get; set; }

        /// <summary>
        /// Used when adding Templates.
        /// If the most recent element in the graph is a MatchTemplate, this is not null, and MostRecentActionTemplate is null.
        /// </summary>
        private MatchTemplate MostRecentMatchTemplate { get; set; }

        private MatchTemplate CurrentMatchTemplate { get; set; }

        private ActionTemplate CurrentActionTemplate { get; set; }

        private List<ActionTemplate> AllActionTemplates { get; set; } // used when setting the reset a flag for cycle-detection
        private List<MatchTemplate> AllMatchTemplates { get; set; } // used when setting the reset a flag for cycle-detection

        private Dictionary<string, List<Rectangle>> ActionRegionsOfInterestDictionary { get; set; }

        private void AddOrUpdateActionRectanglesOfMatchTemplateInDictionary(MatchTemplate m)
        {
            if (m.GetDictionaryKey() == "")
                return;
            if(m.ImageMatches != null)
            {
                if(m.ImageMatches.Count > 0)
                {
                    List<Rectangle> action_rectangles = new();
                    foreach(ImageMatch im in m.ImageMatches)
                    {
                        action_rectangles.Add(im.ActionRectangle);
                    }

                    AddToOrUpdateDictionary(m.GetDictionaryKey(), action_rectangles);
                }
            }
            else if(m.TextMatches != null)
            {
                if(m.TextMatches.Count > 0)
                {
                    List<Rectangle> action_rectangles = new();
                    foreach (TextMatch tm in m.TextMatches)
                    {
                        action_rectangles.Add(tm.ActionRectangle);
                    }

                    AddToOrUpdateDictionary(m.GetDictionaryKey(), action_rectangles);
                }
            }
        }

        private void AddToOrUpdateDictionary(string key, List<Rectangle> action_regions_of_interest)
        {
            if(!ActionRegionsOfInterestDictionary.ContainsKey(key))
            {
                ActionRegionsOfInterestDictionary.Add(key, action_regions_of_interest);
            }
            else
            {
                ActionRegionsOfInterestDictionary.Remove(key);
                ActionRegionsOfInterestDictionary.Add(key, action_regions_of_interest);
            }
        }

        public void SetRootActionTemplate(ActionTemplate action_template)
        {
            RootActionTemplate = action_template;
            MostRecentActionTemplate = action_template;
            CurrentActionTemplate = action_template;
            MostRecentMatchTemplate = null;
            AllActionTemplates.Add(action_template);
        }

        /// <summary>
        /// An ActionTemplate can be added after both an ActionTemplate and a MatchTemplate.
        /// </summary>
        /// <param name="action_template">The ActionTemplate to be added as a child element.</param>
        public void AddActionTemplate(ActionTemplate action_template)
        {
            if(MostRecentActionTemplate != null)
            {
                MostRecentActionTemplate.SetChildAction(action_template);
                MostRecentActionTemplate = action_template;
                MostRecentMatchTemplate = null;
                AllActionTemplates.Add(action_template);
            }
            else if (MostRecentMatchTemplate!= null)
            {
                MostRecentMatchTemplate.SetChildAction(action_template);
                MostRecentActionTemplate = action_template;
                MostRecentMatchTemplate = null;
                AllActionTemplates.Add(action_template);
            }
            else
            {
                throw new Exception("No previous element!");
            }
        }

        /// <summary>
        /// An ActionTemplate can be added after both an ActionTemplate and a MatchTemplate.
        /// </summary>
        /// <param name="match_template">The ActionTemplate to be added as a child element.</param>
        public void AddMatchTemplate(MatchTemplate match_template)
        {
            if (MostRecentActionTemplate != null)
            {
                MostRecentActionTemplate.SetChildMatchTemplate(match_template);
                MostRecentMatchTemplate = match_template;
                MostRecentActionTemplate = null;
                AllMatchTemplates.Add(match_template);
            }
            else if (MostRecentMatchTemplate != null)
            {
                throw new Exception("Can't add a MatchTemplate after another MatchTemplate! (yet)");
            }
            else
            {
                throw new Exception("No previous element!");
            }
        }

        private void ResetCycleCheckFlagsOnTemplates()
        {
            foreach(ActionTemplate a in AllActionTemplates)
            {
                a.CycleCheck_Visited = false;
            }

            foreach(MatchTemplate m in AllMatchTemplates)
            {
                m.CycleCheck_Visited = false;
            }
        }

        private async Task ExecuteAction(ActionTemplate action_template)
        {
            if(action_template.GetKeyboardInput() != null)
            {
                InputSim.SendKeyStrokes(action_template.GetKeyboardInput());
            }
            else if(action_template.GetMouseMoveTarget() != null)
            {
                Point p = action_template.GetMouseMoveTarget().Value;
                InputSim.MoveMouseToPosition(p.X, p.Y);
            }
            else if(action_template.GetMouseScrollVertically() != 0)
            {
                int scroll_value = action_template.GetMouseScrollVertically();
                if(scroll_value < 0) // scrolling down - negative scroll value
                {
                    scroll_value = -1 * scroll_value; // after this, scroll_value is positive
                    if(scroll_value < 1000) // don't accept scroll_value s that are too high
                    {
                        WaitService.SmartWait(RNG.GetStandardRand(210, 50, 110, 500)); // a small wait period before starting to scroll
                        while(scroll_value > 0)
                        {
                            InputSim.ScrollDownOnce();
                            WaitService.SmartWait(RNG.GetStandardRand(124, 25, 89, 301)); //this should be done inside the input sim class
                            scroll_value--;
                        }
                    }
                }
                else // scroll up
                {
                    if (scroll_value > 1000) // don't accept scroll_value s that are too high
                    {
                        WaitService.SmartWait(RNG.GetStandardRand(220, 49, 112, 501));
                        while (scroll_value > 0)
                        {
                            InputSim.ScrollDownOnce();
                            WaitService.SmartWait(RNG.GetStandardRand(124, 25, 89, 301));
                            scroll_value--;
                        }
                    }
                }
            }
            else if(action_template.GetMouseClick() != 0)
            {
                ActionTemplate.MouseEvent mouse_event = action_template.GetMouseClick();
                if(mouse_event == ActionTemplate.MouseEvent.MoveMouse)
                {
                    if(ActionRegionsOfInterestDictionary.TryGetValue(action_template.DictionaryKey, out List<Rectangle> action_targets))
                    {
                        Rectangle target_boundaries = action_targets.First(); //TODO: Currently, this only moves the mouse to the first target
                        int x_min = target_boundaries.X;
                        int x_max = target_boundaries.Right - 1;
                        int y_min = target_boundaries.Y;
                        int y_max = target_boundaries.Bottom - 1;

                        int x_target = RNG.GetStandardRand((x_min + x_max) / 2, System.Math.Abs(x_max - x_min)/2, x_min, x_max);
                        int y_target = RNG.GetStandardRand((y_min + y_max) / 2, System.Math.Abs(y_max - y_min)/2, y_min, y_max);
                        InputSim.MoveMouseToPosition(x_target, y_target);
                    }
                }
                else if(mouse_event == ActionTemplate.MouseEvent.SingleClickLeftMouseButton)
                {
                    InputSim.ClickLeftMouseButton();
                }
                else if(mouse_event == ActionTemplate.MouseEvent.SingleClickRightMouseButton)
                {
                    InputSim.ClickRightMouseButton();
                }
                else if (mouse_event == ActionTemplate.MouseEvent.SingleClickMiddleMouseButton)
                {
                    throw new NotImplementedException("Middle mouse button clicks are not supported as of yet");
                }
            }
            else if (action_template.GetWaitEvent() != 0)
            {
                ActionTemplate.WaitEvent wait_event = action_template.GetWaitEvent();
                if(wait_event == ActionTemplate.WaitEvent.SimplyWait)
                {
                    await action_template.Wait();
                }
                else if (wait_event == ActionTemplate.WaitEvent.WaitAndDoodle)
                {
                    throw new NotImplementedException("WaitAndDoodle is not yet implemented!");
                }
                else if (wait_event == ActionTemplate.WaitEvent.WaitForUserToHelp)
                {
                    if(action_template.UserHelpMessage_UseMsgBox)
                    {
                        MessageBox.Show(action_template.UserHelpMessage, "The Application requires your help!");
                    }
                    if(action_template.UserHelpMessage_UseTTS)
                    {
                        throw new NotImplementedException("Text-To-Speech is not yet implemented!");
                    }

                    while (true)
                    {
                        if(await action_template.UserHelpSuccessCondition.Test() == true)
                        {
                            break;
                        }

                        await Task.Delay(300);
                    }
                }
                else
                {
                    throw new NotImplementedException("Invalid Wait Action!");
                }
            }
            else
            {
                throw new InvalidEnumArgumentException("There was no Action associated with this ActionTemplate!");
            }
        }

        public async void ExecuteGraph()
        {
            bool next = true; // true: next action or match-template exists. false: none exist and the function will return.
            bool current_template_is_match_template = false;

            while(next)
            {
                if(current_template_is_match_template)
                {
                    if(CurrentMatchTemplate == null)
                    {
                        Console.WriteLine("Reached end of graph");
                        return;
                    }

                    Dbg.Print("Attempting to Match " + CurrentMatchTemplate.FileName + " / MatchWord: " + CurrentMatchTemplate.SearchForWord, DebugMode);
                    bool match_success = await CurrentMatchTemplate.Test(); // a match is successful if one match was found
                    if(!match_success)
                    {
                        Console.WriteLine("CurrentMatchTemplate.Test(): False/Match was not found! Skipping to next MatchTemplate!");
                        if (!SkipToNextMatchTemplate())
                        {
                            Console.WriteLine("No subsequent MatchTemplate has been found! Exiting");
                            return;
                        }
                        goto location_end_of_execute_graph_loop;
                    }
                    else
                    {
                        AddOrUpdateActionRectanglesOfMatchTemplateInDictionary(CurrentMatchTemplate);
                    }

                    current_template_is_match_template = false;
                    SetCurrentTemplate(CurrentMatchTemplate.GetChildAction());
                }
                else // this is an action template
                {
                    if (CurrentActionTemplate == null)
                    {
                        Console.WriteLine("Reached the end of the graph");
                        return;
                    }

                    await ExecuteAction(CurrentActionTemplate);

                    if (CurrentActionTemplate.GetChildAction() != null)
                    {
                        SetCurrentTemplate(CurrentActionTemplate.GetChildAction());
                    }
                    else if(CurrentActionTemplate.GetChildMatchTemplate() != null)
                    {
                        current_template_is_match_template = true;
                        SetCurrentTemplate(CurrentActionTemplate.GetChildMatchTemplate());
                    }
                    else
                    {
                        Console.WriteLine("Reached the end of the graph (Action had no children)");
                        return;
                    }
                }
            location_end_of_execute_graph_loop:;
            }
        }

        /// <summary>
        /// Skips to the next MatchTemplate. Is called when the current match template produced no matches.
        /// Update this when 
        /// </summary>
        /// <returns></returns>
        private bool SkipToNextMatchTemplate()
        {
            ActionTemplate tmp_action;
            if (CurrentMatchTemplate.GetChildAction() != null) // currently, a MatchTemplate can only have an ActionTemplate child.
            {
                CurrentMatchTemplate.CycleCheck_Visited = true;
                tmp_action = CurrentMatchTemplate.GetChildAction();
            }
            else
            {
                Dbg.Print("SkipToNextMatchTemplate(): The CurrentMatchTemplate had no ChildAction", DebugMode);
                return false;
            }

            while(true)
            {
                if(tmp_action != null)
                {
                    if (tmp_action.CycleCheck_Visited)
                    {
                        Console.WriteLine("SkipToNextMatchTemplate() Cycle! Already visited this ActionTemplate");
                        ResetCycleCheckFlagsOnTemplates();
                        return false;
                    }

                    if (tmp_action.GetChildMatchTemplate() != null)
                    {
                        MatchTemplate tmp_match = tmp_action.GetChildMatchTemplate();
                        if(!tmp_match.CycleCheck_Visited) //i.e. this is the initial MatchTemplate whence the search was started
                        {
                            SetCurrentTemplate(tmp_match);
                            ResetCycleCheckFlagsOnTemplates();
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("SkipToNextMatchTemplate() Cycle! Already visited this ActionTemplate");
                            ResetCycleCheckFlagsOnTemplates();
                            return false;
                        }
                    }
                    else
                    {
                        tmp_action = tmp_action.GetChildAction();
                    }
                }
                else // both are null!
                {
                    Dbg.Print("SkipToNextMatchTemplate() tmp_action was null!", DebugMode);
                    return false;
                }
            }
        }

        private void SetCurrentTemplate(ActionTemplate action_template)
        {
            CurrentMatchTemplate = null;
            CurrentActionTemplate = action_template;
        }

        private void SetCurrentTemplate(MatchTemplate match_template)
        {
            CurrentMatchTemplate = match_template;
            CurrentActionTemplate = null;
        }
    }
}