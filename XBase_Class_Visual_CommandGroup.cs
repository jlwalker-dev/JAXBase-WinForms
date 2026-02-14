/*------------------------------------------------------------------------------------------*
 * Command Group
 * 
 *  * TODO - ButtonLayout (add)
 *      3 - Vertical stack auto size borders
 *      4 - Horizontal stack auto size borders
 *------------------------------------------------------------------------------------------*/
namespace JAXBase
{
    public class XBase_Class_Visual_CommandGroup : XBase_Class_Visual
    {
        List<Button> MyObjList = [];

        public Panel cmdGroup => (Panel)me.visualObject!;

        public XBase_Class_Visual_CommandGroup(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new Panel(), "CommandGroup", "cmdgroup", true, UserObject.URW);
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // Do we need to add buttons?
            if (InInit)
            {
                // ----------------------------------------
                // Final setup of properties
                // ----------------------------------------
                SetProperty("height", 40, 0);
                SetProperty("width", 174, 0);
                SetProperty("borderstyle", 1, 0);   // Set up the border
                SetProperty("borderwidth", 1, 0);   // Set up the border
                SetProperty("bordercolor", "100,100,100", 0);
                SetProperty("buttonlayout", 1, 0);  // Horizontal layout
                SetProperty("buttoncount", 2, 0);   // Start with 2 buttons
                FixSpacing();
            }
            return result;
        }

        /*------------------------------------------------------------------------------------------*
         * Handle the commmon properties by calling the base and then
         * handle the special cases.
         * 
         * Return result from XBase_Visual_Class
         *      0   - Successfully proccessed
         *      1   - Did not process
         *      2   - Requires special processing
         *      >10 - Error code
         * 
         * 
         * Return from here
         *      0   - Successfully processed
         *      >0  - Error Code
         *      
         *------------------------------------------------------------------------------------------*/
        public override int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;
            propertyName = propertyName.ToLower();
            int temp;
            int spacing;

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                if (UserProperties.ContainsKey(propertyName))
                {
                    switch (propertyName)
                    {
                        case "bordercolor":
                            UserProperties[propertyName].Element.Value = JAXUtilities.ReturnColorInt(objValue);
                            result = 9;
                            break;

                        case "borderwidth":
                            if (objtk.Element.Type.Equals("N") == false) throw new Exception("11|");
                            if (JAXLib.Between(objtk.AsInt(), 0, 1))
                            {
                                // Set the border width - TODO
                                UserProperties[propertyName].Element.Value = objtk.AsInt();
                            }
                            else
                                throw new Exception("11|");
                            break;

                        case "borderstyle":
                            if (objtk.Element.Type.Equals("N") == false) throw new Exception("11|");
                            if (JAXLib.Between(objtk.AsInt(), 0, 1))
                            {
                                UserProperties[propertyName].Element.Value = objtk.AsInt();
                                cmdGroup.BorderStyle = objtk.AsInt() == 0 ? BorderStyle.None : BorderStyle.FixedSingle;
                            }
                            else
                                throw new Exception("11|");
                            break;

                        case "buttonlayout":
                            if (objtk.Element.Type.Equals("N") == false) throw new Exception("11|");
                            if (JAXLib.Between(objtk.AsInt(), 0, 2))
                                UserProperties[propertyName].Element.Value = objValue;
                            else
                                throw new Exception("11|");

                            FixSpacing();
                            break;

                        // Intercept special handling of properties
                        case "buttoncount":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                JAXObjects.Token bc = UserProperties["objects"];
                                if (objtk.AsInt() < 1) throw new Exception("9999|");

                                int desiredButtonCount = objtk.AsInt();
                                int currentButtonCount = 0;

                                // FIRST, make sure we have a correct count
                                for (int i = 0; i < bc.Count; i++)
                                {
                                    if (bc._avalue[i].Value is JAXObjectWrapper)
                                    {
                                        JAXObjectWrapper btn = (JAXObjectWrapper)bc._avalue[i].Value;
                                        if (btn.BaseClass.Equals("commandbutton", StringComparison.OrdinalIgnoreCase))
                                            currentButtonCount++;
                                    }
                                }

                                // Second, do we need to knock some buttons out of Objects?
                                int ii = bc.Count - 1;
                                while (ii >= 0 && currentButtonCount > desiredButtonCount)
                                {
                                    if (bc._avalue[ii].Value is JAXObjectWrapper)
                                    {
                                        JAXObjectWrapper btn = (JAXObjectWrapper)bc._avalue[ii].Value;
                                        if (btn.BaseClass.Equals("commandbutton", StringComparison.OrdinalIgnoreCase))
                                        {
                                            bc.RemoveAt(ii);
                                            cmdGroup.Controls.RemoveAt(cmdGroup.Controls.Count - 1);
                                            ii--;
                                            currentButtonCount--;
                                        }
                                    }
                                }

                                spacing = UserProperties["spacing"].AsInt();

                                // Finallyu, do we need to add some to the end?
                                while (currentButtonCount < desiredButtonCount)
                                {
                                    JAXObjectWrapper obut = new(App, "commandbutton", $"command{bc.Count + 1}", []);
                                    obut.SetProperty("autosize", true);
                                    obut.SetProperty("caption", $"Command{bc.Count + 1}");
                                    obut.SetProperty("visible", true);
                                    obut.SetParent(me);
                                    bc.Add(obut);
                                    cmdGroup.Controls.Add(obut.visualObject);
                                    currentButtonCount++;
                                }

                                UserProperties["controlcount"].Element.Value = bc.Col;
                                UserProperties["buttoncount"].Element.Value = currentButtonCount;
                                result = 9;
                                FixSpacing();
                            }
                            else
                                result = 11;
                            break;


                        case "height":
                            // Make sure we have a valid value
                            if (objtk.Element.Type.Equals("N"))
                                temp = objtk.AsInt();
                            else
                                throw new Exception("11|");

                            objValue = temp < 30 ? 30 : temp;
                            objtk.Element.Value = objValue;

                            result = base.SetProperty(propertyName, objValue, objIdx);

                            // Update all buttons appropriately
                            FixSpacing();
                            break;

                        case "spacing":
                            if (objtk.Element.Type.Equals("N"))
                                temp = objtk.AsInt();
                            else
                                throw new Exception("11|");

                            // Check value to make sure it's in the acceptable range and save it
                            temp = JAXLib.Between(temp, 0, 255) ? temp : throw new Exception("11|");
                            UserProperties[propertyName].Element.Value = temp;

                            // Fix button spacing
                            FixSpacing();
                            break;

                        case "width":
                            // Make sure we have a valid value
                            if (objtk.Element.Type.Equals("N"))
                                temp = objtk.AsInt();
                            else
                                throw new Exception("11|");

                            objValue = temp < 40 ? 40 : temp;
                            objtk.Element.Value = objValue;

                            result = base.SetProperty(propertyName, objValue, objIdx);

                            // Update all buttons appropriately
                            FixSpacing();
                            break;

                        default:
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result, 0, 10))
                    {

                        // 9 & 10 skips further processing
                        if (result < 9)
                        {
                            result = 0;

                            // First, we check to make sure that the property exists
                            // Visual object common property handler
                            switch (propertyName.ToLower())
                            {
                                case "***":
                                    break;
                            }


                            // Did we process it?
                            if (result == 0)
                            {
                                // We processed it or just need to save the property (perhaps again)
                                // Ignore the CA1854 as it won't put the value into the property
                                if (UserProperties.ContainsKey(propertyName))
                                    UserProperties[propertyName].Element.Value = objValue;
                                else
                                    result = 1559;
                            }
                        }
                        else
                            result = 0;
                    }
                }
                else
                    result = 1559;
            }

            if (result > 0)
            {
                _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);


                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);

                result = -1;
            }

            // Refresh everything
            cmdGroup.Refresh();
            return result;
        }


        // Adjust property of all buttons in group
        private void SetAllButtons(string propertyName, JAXObjects.Token objtk)
        {
            JAXObjects.Token otk = UserProperties["objects"];
            for (int i = 0; i < otk._avalue.Count; i++)
            {
                // Protection, but should always be true
                if (otk._avalue[i].Value is JAXObjectWrapper)
                {
                    // If it's a button then adjust the property
                    JAXObjectWrapper itk = (JAXObjectWrapper)otk._avalue[i].Value;
                    if (itk.BaseClass.Equals("commandbutton", StringComparison.OrdinalIgnoreCase))
                        SetObjectProperty(i, propertyName, objtk);
                }
            }
        }

        /*
         * Fix the spacing
         * ButtonLayout
         *      0 - Vertical stack - resize buttons to fit panel area
         *      1 - Horizontal stack - resize buttons to fit panel area
         *      2 - FreeForm - resize panel to fit buttons and reposition the buttons into the panel
         *                     leaving their spacing between each other exactly the same.
         */
        private int FixSpacing()
        {
            int result = 0;

            try
            {
                JAXObjects.Token otk = UserProperties["objects"];
                int spacing = UserProperties["spacing"].AsInt();
                int temp = spacing;

                // Current dimensions of panel
                int clft = UserProperties["left"].AsInt();
                int ctop = UserProperties["top"].AsInt();
                int cwid = UserProperties["width"].AsInt();
                int chgt = UserProperties["height"].AsInt();

                // Used for BL=2
                int top = 0;
                int lft = 0;

                // used for all 3 modes
                int hgt = 0;
                int wth = 0;
                int btncount = UserProperties["buttoncount"].AsInt();

                // If there are buttons...
                if (btncount > 0)
                {
                    // Skip this section if freeform
                    if (UserProperties["buttonlayout"].AsInt() == 2)
                    {
                        // set them way out there
                        hgt = 2147483647;
                        wth = 2147483647;
                    }
                    else
                    {
                        if (UserProperties["buttonlayout"].AsInt() == 0)
                        {
                            // Vertical
                            hgt = UserProperties["height"].AsInt();
                            hgt = (hgt - 6 * btncount - spacing * 2) / btncount;
                            hgt = (hgt < 23) ? 23 : hgt;
                            wth = UserProperties["width"].AsInt() - spacing * 2;
                            wth = (wth < 25) ? 25 : wth;
                        }
                        else
                        {
                            // Horizontal
                            wth = UserProperties["width"].AsInt();
                            wth = (wth - 6 * btncount - spacing * 2) / btncount;
                            wth = (wth < 23) ? 23 : wth;
                            hgt = UserProperties["height"].AsInt() - spacing * 2;
                            hgt = (hgt < 25) ? 25 : hgt;
                        }
                    }

                    // Temp now becomes the current top or left position for the next button
                    for (int i = 0; i < otk._avalue.Count; i++)
                    {
                        // Spacing between buttons is spacing * 2
                        if (i > 0) temp += spacing;

                        // Protection, but should always be true
                        if (otk._avalue[i].Value is JAXObjectWrapper)
                        {
                            // If it's a command button and autosize = .T.
                            JAXObjectWrapper itk = (JAXObjectWrapper)otk._avalue[i].Value;
                            if (itk.BaseClass.Equals("commandbutton", StringComparison.OrdinalIgnoreCase))
                            {
                                if (UserProperties["buttonlayout"].AsInt() == 2)
                                {
                                    // Freeform layout
                                    // Get the current button location and dimensions
                                    int t = itk.thisObject!.UserProperties["top"].AsInt();
                                    int l = itk.thisObject!.UserProperties["left"].AsInt();
                                    int h = itk.thisObject!.UserProperties["height"].AsInt();
                                    int w = itk.thisObject!.UserProperties["width"].AsInt();

                                    lft = (clft + lft) > clft + l ? l : clft;
                                    top = (ctop + top) > ctop + t ? t : top;
                                    wth = (clft + lft + wth) > (l + w) ? l + w : wth;
                                    hgt = (ctop + chgt + hgt) > (t + h) ? t + h : hgt;
                                }
                                else
                                {
                                    itk.SetProperty("autosize", false);
                                    if (UserProperties["buttonlayout"].AsInt() == 0)
                                    {
                                        // Vertical layout
                                        itk.SetProperty("top", temp);
                                        itk.SetProperty("left", spacing);
                                        itk.SetProperty("width", wth);
                                        itk.SetProperty("height", hgt);

                                        //  advance temp for the next one
                                        temp += hgt + spacing;
                                    }
                                    else
                                    {
                                        // Horizontal layout
                                        itk.SetProperty("left", temp);
                                        itk.SetProperty("top", spacing);
                                        itk.SetProperty("width", wth);
                                        itk.SetProperty("height", hgt);

                                        //  advance temp for the next one
                                        temp += wth + spacing;
                                    }
                                }
                            }
                            else
                            {
                                result = 1903;
                                App.SetError(result, $"1903|{i}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                            }
                        }
                    }

                    // Now adjust the panel height/width and
                    // make sure the butten area is moved
                    // into the panel client area.
                    if (UserProperties["buttonlayout"].AsInt() == 2)
                    {
                        UserProperties["height"].Element.Value = hgt + spacing * 2;
                        UserProperties["width"].Element.Value = wth + spacing * 2;

                        // Set up the relative movement of all
                        // buttons to fit into the client area
                        int tfix = spacing - top;
                        int lfix = spacing - lft;

                        // Move all button tops and lefts so everything
                        // ends up in the client area.
                        for (int i = 0; i < otk._avalue.Count; i++)
                        {
                            // Protection, but should always be true
                            if (otk._avalue[i].Value is JAXObjectWrapper)
                            {
                                // If it's a command button and autosize = .T.
                                JAXObjectWrapper itk = (JAXObjectWrapper)otk._avalue[i].Value;
                                if (itk.BaseClass.Equals("commandbutton", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (itk.GetProperty("top", 0, out JAXObjects.Token t) == 0)
                                    {
                                        if (itk.GetProperty("left", 0, out JAXObjects.Token l) == 0)
                                        {
                                            itk.SetProperty("top", t.AsInt() + tfix);
                                            itk.SetProperty("left", l.AsInt() + lfix);
                                        }
                                        else
                                        {
                                            // Remark on the problem
                                            result = 1559;
                                            App.SetError(result, $"1559|LEFT", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                                        }
                                    }
                                    else
                                    {
                                        // Remark on the problem
                                        result = 1559;
                                        App.SetError(result, $"1559|TOP", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.SetError(9999, $"9999|{ex.Message}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                result = 9999;
            }

            return result;
        }

        /*
         * Resize actions depend on the buttonlayout property and autosize of buttons and container
         *  0 - Align all buttons vertically, adjusting their size and position within the border of the command group
         *  1 - Align all buttons horizontally, adjusting their size and position within the border of the command group.
         *  2 - Adjust the border of the command group to fit around the buttons if container is autosize.
         */
        public override int DoDefault(string methodName)
        {
            int result = 1559;

            if (Methods.ContainsKey(methodName))
            {
                result = 0;
                methodName = methodName.ToLower();
                JAXObjects.Token tk = new();

                switch (methodName)
                {
                    case "resize":
                        FixSpacing();
                        break;

                    default:
                        result = base.DoDefault(methodName);
                        break;
                }
            }

            return result;
        }

        /*------------------------------------------------------------------------------------------*
         * GetProperty method returns 
         *      0 = Successfully returning value
         *     -1 = Error code
         *------------------------------------------------------------------------------------------*/
        public override int GetProperty(string propertyName, int idx, out JAXObjects.Token returnToken)
        {
            int result = 0;
            returnToken = new();
            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName))
            {
                // Get the property and fill in the value
                returnToken.CopyFrom(UserProperties[propertyName]);

                switch (propertyName)
                {
                    // Intercept special handling of properties
                    case "buttoncount":
                        returnToken.Element.Value = UserProperties["objects"].Count;
                        break;

                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (JAXLib.Between(result, 1, 10))
                {
                    // Visual object common property handler
                    switch (propertyName)
                    {
                        default:
                            returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
                            result = 0;
                            break;
                    }
                }
            }
            else
                result = 1559;

            if (result > 10)
            {
                _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);

                result = -1;
            }
            else
                result = 0;

            return result;
        }


        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
               "addproperty", "addobject", "drag", "move", "readexpression", "readmethod", "removeobject", "refresh",
               "saveasclass", "showwhatsthis", "writeexpression", "writemethod", "zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","error",
                "init","interactivechagnge","keypress","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "programmaticchange","resize","rightclick","valid","visiblechanged","when"
                ];
        }

        /*------------------------------------------------------------------------------------------*
             * property data types
             *      C = Character
             *      N = Numeric         I=Integer       R=Color
             *      D = Date
             *      T = DateTime
             *      L = Logical         LY = Yes/No logical
             *      
             *      Attributes
             *          ! Protected - can't change after initialization
             *          $ Special Handling - do not auto process
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXProperties()
        {
            return
                [
                "anchor,n,0","autosize,l,true",
                "backcolor,R,15790320","backstyle,n,1","BaseClass,C,Grid","bordercolor,R,100|100|100",
                "borderstyle,n,1","borderwidth,n,1","buttoncount,n,0","buttonlayout,n,0",
                "Class,C,Grid","ClassLibrary,C,","Comment,C,","controlcount,n,0",
                "Enabled,L,true",
                "Height,N,40",
                "left,N,0",
                "name,c,",
                "objects,*,",
                "parent,o,","parentclass,C,",
                "righttoleft,L,false",
                "setoriginalwhen,n,0","spacing,n,6",
                "tabstop,L,true","tabindex,n,1","tag,C,","tooltiptext,c,","top,n,0",
                "value,n,1","visible,l,true","width,N,150"
                ];
        }
    }
}
