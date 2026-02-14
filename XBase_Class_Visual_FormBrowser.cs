/*------------------------------------------------------------------------------------------*
 * FormBrowser Visual Class
 * 
 * Putting all of my ideas and theories to the test.  I should be able to whip
 * together a beautiful browse form that will kick the pants off of any legacy
 * XBase system's browse window.
 *      
 * User can not do the following which is normally allowed:
 *      Change the Name of any child objects created at instantiation
 *      TODO - Remove any child objects created at instantiation
 * 
 *------------------------------------------------------------------------------------------*/
namespace JAXBase
{
    public class XBase_Class_Visual_FormBrowser : XBase_Class_Visual
    {
        public Form Form => (Form)me.visualObject!;
        string MainMenuName = string.Empty;

        public XBase_Class_Visual_FormBrowser(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new Form(), "browser", "browser", true, UserObject.URW);
            me.THISFORM = me;
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of Form properties
            // ----------------------------------------

            // Set Datasession if it hasn't been done already
            if (UserProperties["datasession"].AsInt() > 1 && UserProperties["datasessionid"].AsInt() < 2)
                UserProperties["datasessionid"].Element.Value = App.CreateNewDataSession(App.SystemCounter());

            // Set up and add the grid
            JAXObjectWrapper grd = new(App, "grid", "grid", []);

            if (grd.thisObject is not null)
            {
                grd.visualObject!.Dock = DockStyle.None;
                grd.thisObject.SetProperty("top", 0, 0);
                grd.thisObject.SetProperty("left", 0, 0);
                grd.thisObject.SetProperty("width", 400, 0);
                grd.thisObject.SetProperty("height", 400, 0);
                grd.thisObject.SetProperty("columncount", 1, 0);
                grd.thisObject.SetProperty("visible", true, 0);
                grd.thisObject.SetProperty("anchor", 15, 0);
                grd.Protected = JAXObjectWrapper.Protection.URd;
                grd.visualObject.BringToFront();
                AddObject(grd);
            }

            // Set up and add the command button group
            /*
            JAXObjectWrapper bgrp = new(App, "commandgroup", "cmdgroup", []);
            if (bgrp.thisObject is not null)
            {
                bgrp.thisObject.UserProperties["top"].Element.Value = 355;
                bgrp.thisObject.UserProperties["left"].Element.Value = 0;
                bgrp.thisObject.UserProperties["width"].Element.Value = 400;
                bgrp.thisObject.UserProperties["height"].Element.Value = 40;
                bgrp.thisObject.UserProperties["anchor"].Element.Value = 13;
                bgrp.thisObject.UserProperties["buttoncount"].Element.Value = 5;
                //bgrp.thisObject.UserProperties["name"].Protected = true;
                bgrp.thisObject.UserProperties["visible"].Element.Value = false;
                bgrp.Protected = JAXObjectWrapper.Protection.URd;
                //bgrp.thisObject.UserProperties[""].Element.Value =
                AddObject(bgrp);
            }

            // Add to the VISIBLECHANGED event
            */

            // Set up and add the refresh timer
            /*
            JAXObjectWrapper btmr = new(App, "timer", "timer", []);
            if (bgrp.thisObject is not null)
            {
                btmr.thisObject.UserProperties["top"].Element.Value = 0;
                btmr.thisObject.UserProperties["left"].Element.Value = 0;
                btmr.thisObject.UserProperties["name"].Protected = true;
                btmr.thisObject.UserProperties["interval"].Element.Value = 5
                btmr.thisObject.UserProperties["enabled"].Element.Value = false;
                btmr.Protected = JAXObjectWrapper.Protection.URd;

                //btmr.thisObject.UserProperties[""].Element.Value = 
                //btmr.thisObject.UserProperties[""].Element.Value = 
                //btmr.thisObject.UserProperties[""].Element.Value = 
                AddObject(btmr);
            }
            */

            return result;
        }


        /*
         * Handle any cases that need special processing when
         * adding a new object to the form
         */
        public override int AddObject(JAXObjectWrapper value)
        {
            int result = base.AddObject(value);

            if (result >= 0)
            {
                // Looking for the first menu attached to the
                // form to make it the Main menu strip
                if (value.BaseClass.Equals("menu", StringComparison.OrdinalIgnoreCase) && Form.MainMenuStrip is null && value.visualObject is not null)
                {
                    Form.MainMenuStrip = (MenuStrip)value.visualObject;
                    MainMenuName = Form.MainMenuStrip.Name;
                }
                else if (value.BaseClass.Equals("toolbar", StringComparison.OrdinalIgnoreCase) && Form.MainMenuStrip != null)
                {
                    // pull the menu and add it again to set it on top of the toolbox
                    for (int i = 0; i < Form.Controls.Count; i++)
                    {
                        Type tp = Form.Controls[i].GetType();
                        string name = Form.Controls[i].Name;

                        if (tp == typeof(MenuStrip) && name.Equals(MainMenuName, StringComparison.OrdinalIgnoreCase))
                        {
                            System.Windows.Forms.Control mnu = Form.Controls[i];
                            Form.Controls.RemoveAt(i);
                            Form.Controls.Add(mnu);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /*
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
         *     -1   - Error Code
         * 
         */
        public override int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            propertyName = propertyName.ToLower();

            JAXObjectWrapper? jow;
            int idx;

            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;
            App.DebugLog($"MyObj={Form.Name} FORM.{propertyName}={objtk.AsString()}");

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                // Do we need to process this property?
                // First, we doublecheck to make sure that the property exists
                if (UserProperties.ContainsKey(propertyName))
                {
                    // Visual object common property handler
                    switch (propertyName)
                    {
                        case "activecontrol":
                        case "activeform":
                        case "baseclass":
                        case "class":
                        case "classlibrary":
                        case "controlcount":
                        case "controls":
                        case "datasessionid":
                        case "hwnd":
                        case "objects":
                        case "parent":
                        case "parentclass":
                            result = 1533;
                            break;

                        case "alwaysontop":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                Form.TopMost = objtk.AsBool();
                            }
                            else
                                result = 11;
                            break;

                        case "autocenter":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                if (objtk.AsBool())
                                    Form.StartPosition = FormStartPosition.CenterScreen;
                                else
                                    Form.StartPosition = FormStartPosition.WindowsDefaultLocation;
                            }
                            else
                                result = 11;
                            break;

                        case "borderstyle":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                Form.FormBorderStyle = objtk.AsInt() switch
                                {
                                    1 => FormBorderStyle.FixedSingle,
                                    2 => FormBorderStyle.FixedDialog,
                                    3 => FormBorderStyle.Sizable,
                                    _ => 0
                                };
                            }
                            else
                                result = 11;
                            break;

                        case "controlbox":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                Form.ControlBox = objtk.AsBool();
                            }
                            else
                                result = 11;
                            break;

                        case "datasession":
                            // Read only at runtime
                            if (App.RuntimeFlag == false)
                            {
                                if (objtk.Element.Type.Equals("N"))
                                {
                                    int v = objtk.AsInt();
                                    objValue = v > 1 ? 2 : 1;
                                }
                            }
                            else
                                result = 11;
                            break;

                        case "height":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                objValue = objtk.AsInt() + 40;
                                result = base.SetProperty(propertyName, objValue, objIdx);
                            }
                            else
                                result = 11;
                            break;

                        case "icon":
                            if (objtk.Element.Type.Equals("C"))
                            {
                                string iconName = objValue.ToString() ?? string.Empty;
                                if (File.Exists(iconName))
                                {
                                    Form.Icon = new Icon(iconName);
                                    // Alternate - read in icon as base 64 string
                                    // MyObj.Icon = Utilities.StringToICO(string)
                                }
                                else
                                    objValue = string.Empty;    // invalid icon names are ignored
                            }
                            else
                                result = 11;
                            break;

                        case "keypreview":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                Form.KeyPreview = objtk.AsBool();
                            }
                            else
                                result = 11;
                            break;

                        case "maxbutton":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                Form.MaximizeBox = objtk.AsBool();
                            }
                            else
                                result = 11;
                            break;

                        case "mdiform":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                Form.IsMdiContainer = objtk.AsBool();
                            }
                            else
                                result = 11;
                            break;

                        case "minbutton":
                            if (objtk.Element.Type.Equals("L"))
                            {
                                Form.MinimizeBox = objtk.AsBool();
                            }
                            else
                                result = 11;
                            break;

                        case "mousepointer":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                // TODO - add custom icons 16-99
                                Form.Cursor = objtk.AsInt() switch
                                {
                                    1 => Cursors.Arrow,
                                    2 => Cursors.Cross,
                                    3 => Cursors.IBeam,
                                    4 => Cursors.NoMove2D,
                                    5 => Cursors.SizeAll,
                                    6 => Cursors.SizeNESW,
                                    7 => Cursors.SizeNS,
                                    8 => Cursors.SizeNWSE,
                                    9 => Cursors.SizeWE,
                                    10 => Cursors.UpArrow,
                                    11 => Cursors.WaitCursor,
                                    12 => Cursors.AppStarting,
                                    13 => Cursors.Hand,
                                    14 => Cursors.Help,
                                    15 => Cursors.No,
                                    _ => Cursors.Default
                                };
                            }
                            else
                                result = 11;
                            break;

                        case "picture":
                            if (objtk.Element.Type.Equals("C"))
                            {
                                string picname = objtk.AsString().Trim();
                                string imagename = JAXLib.JustFName(picname.ToLower());
                                if (imagename.Equals(picname) == false)
                                {
                                    if (picname.StartsWith("http:") || picname.StartsWith("https:"))
                                    {
                                        // URI
                                        result = 11;
                                    }
                                    else
                                    {
                                        // Does the image exist?
                                        if (App.JaxImages.HasImage(imagename) == false)
                                            App.JaxImages.RegisterImage(picname, imagename, out _);
                                    }
                                }

                                // If it exists, load it
                                if (App.JaxImages.HasImage(imagename))
                                    Form.BackgroundImage = App.JaxImages.GetSDImage(imagename, out _);
                            }
                            else
                                result = 11;
                            break;

                        case "recordsource":
                            jow = GetObject("grid", out idx);
                            if (jow is not null && idx > 0)
                                jow.SetProperty("recordsource", objtk.Element.Value);
                            break;

                        case "recordsourcetype":
                            jow = GetObject("grid", out idx);
                            if (jow is not null && idx > 0)
                                jow.SetProperty("recordsourcetype", objtk.Element.Value);
                            break;

                        case "scrollbars":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                Form.HorizontalScroll.Visible = (objtk.AsInt() & 1) > 0;
                                Form.VerticalScroll.Visible = (objtk.AsInt() & 2) > 0;
                                objValue = objtk.AsInt() & 3;
                            }
                            else
                                result = 11;
                            break;

                        case "showwindow":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                switch (objtk.AsInt())
                                {
                                    case 0:
                                        Form.TopLevel = false;
                                        // Make part of desktop
                                        break;
                                    case 1:
                                        Form.TopLevel = false;
                                        // Make part of another form
                                        break;
                                    case 2:
                                        Form.TopLevel = true;
                                        break;

                                    default:
                                        // Ignore what was sent
                                        objValue = UserProperties["showwindow"].AsInt();
                                        break;
                                }
                            }
                            else
                                result = 11;
                            break;

                        case "width":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                objValue = objtk.AsInt() + 16;
                                result = base.SetProperty(propertyName, objValue, objIdx);
                            }
                            else
                                result = 11;
                            break;


                        case "windowstate":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                switch (objtk.AsInt())
                                {
                                    case 1:
                                        Form.WindowState = FormWindowState.Minimized;
                                        objValue = 1;
                                        break;
                                    case 2:
                                        Form.WindowState = FormWindowState.Maximized;
                                        objValue = 2;
                                        break;
                                    case 0:
                                        objValue = 0;
                                        Form.WindowState = FormWindowState.Normal;
                                        break;
                                }
                            }
                            else
                                result = 11;
                            break;

                        default:
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Did we process it?
                    if (JAXLib.Between(result, 0, 10))
                    {
                        result = 0;

                        // We processed it or just need to save the property (perhaps again)
                        // Ignore the CA1854 as it won't put the value into the property
                        if (UserProperties.ContainsKey(propertyName))
                            UserProperties[propertyName].Element.Value = objValue;
                        else
                            result = 1559;
                    }
                }
                else
                    result = 1559;
            }

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

        public override int GetProperty(string propertyName, int idx, out JAXObjects.Token returnToken)
        {
            propertyName = propertyName.ToLower();
            returnToken = new();
            int result = 0;

            if (UserProperties.ContainsKey(propertyName))
            {
                // Get the property and fill in the value
                returnToken.CopyFrom(UserProperties[propertyName]);

                result = base.GetProperty(propertyName, idx, out returnToken);

                if (result > 0 && result < 100)
                {
                    result = 0;

                    // Visual object common property handler
                    switch (propertyName)
                    {
                        case "activecontrol":
                            if (Form.ActiveControl is not null)
                            {
                                System.Windows.Forms.Control activeControl = Form.ActiveControl;
                                string activeName = activeControl.Name;
                                int count = UserProperties["objects"].Count;

                                for (int i = 0; i < count; i++)
                                {
                                    UserProperties["objects"].SetElement(i, 1);
                                    JAXObjectWrapper o = (JAXObjectWrapper)UserProperties["objects"].Element.Value;

                                    if (o.GetProperty("name", out JAXObjects.Token tk) == 0)
                                    {
                                        returnToken.Element.Value = o;
                                        break;
                                    }
                                }
                            }
                            else
                                returnToken.Element.MakeNull();

                            break;

                        case "activeform":
                            break;

                        case "alwaysontop":
                            returnToken.Element.Value = Form.TopMost;
                            break;

                        case "autocenter":
                            returnToken.Element.Value = (Form.StartPosition == FormStartPosition.CenterScreen || Form.StartPosition == FormStartPosition.CenterParent);
                            break;

                        case "borderstyle":
                            returnToken.Element.Value = Form.FormBorderStyle switch
                            {
                                FormBorderStyle.FixedSingle => 1,
                                FormBorderStyle.FixedDialog => 2,
                                FormBorderStyle.Sizable => 3,
                                _ => 0
                            };
                            break;

                        case "controlbox":
                            returnToken.Element.Value = Form.ControlBox;
                            break;

                        case "keypreview":
                            returnToken.Element.Value = Form.KeyPreview;
                            break;

                        case "maxbutton":
                            returnToken.Element.Value = Form.MaximizeBox;
                            break;

                        case "minbutton":
                            returnToken.Element.Value = Form.MinimizeBox;
                            break;

                        case "scrollbars":
                            if (Form.HorizontalScroll.Visible && Form.VerticalScroll.Visible) returnToken.Element.Value = 3;
                            else if (Form.HorizontalScroll.Visible) returnToken.Element.Value = 1;
                            else if (Form.VerticalScroll.Visible) returnToken.Element.Value = 2;
                            else returnToken.Element.Value = 0;
                            break;


                        case "showwindow":
                            if (Form.TopLevel)
                                returnToken.Element.Value = 2;
                            else if (Form.MdiParent is not null)
                            {
                                if (Form.MdiParent.IsMdiContainer)
                                    returnToken.Element.Value = 0;
                                else
                                    returnToken.Element.Value = 1;
                            }
                            else
                            {
                                returnToken.Element.Value = 2;
                            }

                            break;


                        case "windowstate":
                            returnToken.Element.Value = Form.WindowState switch
                            {
                                FormWindowState.Minimized => 1,
                                FormWindowState.Maximized => 2,
                                _ => 0
                            };
                            break;
                    }
                }
            }
            else
                result = 1559;

            if (result > 0)
            {
                _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);

                result = -1;
            }
            return result;
        }


        public override string[] JAXMethods()
        {
            return ["addobject", "addproperty", "box", "circle", "cls", "dock", "draw", "getdockstate", "hide", "line",
            "move", "newobject", "pset", "point", "print", "readexpression", "readmethod", "refresh", "release",
            "resettodefault", "saveas", "saveasclass", "setall","setfocus","setmousepointer","setviewport", "show", "showwhatsthis",
            "textheight", "textwidth","whatsthismode", "writeexpression", "writemethod", "zorder"];
        }

        public override string[] JAXEvents()
        {
            return
                [
                "activate","afterdock","beforedock","click","dblclick","dblrightclick","deactivate","destroy","dragdrop","dragover","error",
                "gotfocus","init","keypress","load","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "moved","paint","queryunload","resize","rightclick","scrolled","undock","unload","visiblechanged"
                ];
        }

        /*
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
         */
        public override string[] JAXProperties()
        {
            return [
                "activecontrol,N,0","activeform,N!,0","alwaysontop,L,false",
                "autocenter,L,false",
                "backcolor,R,15790320","bindcontrols,L,true","borderstyle,N,3",
                "baseclass,C!,broswer","class,C!,browser",
                "caption,C,browser","classlibrary,C$,","closable,L,true",
                "comment,C,","controlbox,L,true","controlcount,N,0",
                "datasession,N,1","datasessionid,N!,1",
                "Enabled,L,true",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false",
                "FontItalic,L,false","FontName,C,","FontSize,N,0",
                "FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,R,0",
                "Height,N,440","hwnd,N!,0",
                "icon,C,",
                "keypreview,L,false",
                "left,N,0","lockscreen,L,false",
                "maxbutton,L,true","maxheight,N,-1","maxwidth,N,-1","minbutton,L,true",
                "minheight,N,-1","minwidth,N,-1","mousepointer,n,0","moveable,L,true",
                "name,C,form",
                "objects,*,",
                "parent,o$,","parentclass,C$,","picture,C,",
                "recordsource,c,","recordsourcetype,n,-1","righttoleft,L,false",
                "shownintaskbar,L,true","showwindow,N,0",
                "tag,C,","tabindex,N,1","tabstop,L,true","top,N,0",
                "visible,L,true",
                "width,N,416","windowstate,N,0","windowtype,N,0"
                ];
        }


        /*
         * SHOW has an override which selects the table in the current
         * work area (if one exists) if the recordsourcetype is set to -1.
         */
        public override int DoDefault(string methodName)
        {
            int results = 0;
            string msg = string.Empty;
            methodName = methodName.ToLower();

            try
            {
                if (Methods.ContainsKey(methodName))
                {
                    string cCode = Methods[methodName].CompiledCode;

                    // Create a new App.Levels and execute the code
                    if (cCode.Length > 0)
                        results = base._CallMethod(methodName);
                    else
                    {
                        switch (methodName)
                        {
                            case "show":
                                JAXDirectDBF wa = App.CurrentDS.CurrentWA;
                                if (wa.DbfInfo is not null && wa.DbfInfo.DBFStream is not null)
                                {
                                    if (UserProperties["recordsourcetype"].AsInt() < 0)
                                    {
                                        // Load up the grid with the current table
                                        JAXObjectWrapper? grid = GetObject("grid", out int idx);

                                        if (idx >= 0 && grid is not null)
                                        {
                                            grid.SetProperty("recordsource", wa.DbfInfo.Alias);
                                            grid.SetProperty("recordsourcetype", 1);
                                            grid.SetProperty("scrollbars", 3);
                                            grid.MethodCall("autofit");
                                            grid.visualObject!.Refresh();
                                            Form.Show();
                                            Form.Refresh();
                                        }
                                        else
                                            results = 1901;
                                    }
                                    else
                                        results = 1901;
                                }
                                break;

                            default:
                                results = base.DoDefault(methodName);
                                break;
                        }
                    }
                }
                else
                    results = 1559;

            }
            catch (Exception ex)
            {
                results = 9999;
                msg = ex.Message;
            }

            if (results > 0)
            {
                _AddError(results, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(results, $"{results}|{msg}", string.Empty);

                results = -1;
            }

            return results;
        }
    }
}
