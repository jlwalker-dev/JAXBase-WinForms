/*------------------------------------------------------------------------------------------*
 * Form Visual Class
 * 
 * 2025.11.02 - JLW
 *      Reworked the form property to make it subclass the XBase_Class_Visual class.
 *      Much happier with this design, but it's taking some effort to understand the
 *      heirarchy.  I've discoverd that the actual object has to be in the 
 *      JAXObjectWrapper to make things work properly.  I guess that makes sense since
 *      everything is covered by the JAXObjectWrapper and can't exist without it.  Also
 *      it provides a common ground for all classes.
 *      
 * 2025.11.18 - JLW
 *      The menu class is now laid out and I'm going to start working on how to tie it
 *      in when assigned to a form.  I'll only allow one menu per object.  Don't know 
 *      if I'll change that later, because Google says it's possible to add a menu to 
 *      a container or pageframe.  THAT will allow multiple menus to be added without 
 *      ever worrying about cross-over or interference.  In fact, that might be the 
 *      best way to add menus to forms in the first place, but I'll just give an few
 *      examples in the docs and let the user decide how to implement.
 *      
 *      When I get to the tool bars, I am going to allow multiples and they can be docked 
 *      to any side of the form AND as many as desired.  I'll also allow toolbars to be 
 *      added to containers and pageframes.  Again, that might be the best way to do it.
 *      
 *------------------------------------------------------------------------------------------*/
namespace JAXBase
{
    public class XBase_Class_Visual_Form : XBase_Class_Visual
    {
        public Form Form => (Form)me.visualObject!;
        string MainMenuName = string.Empty;

        int widthDelta = 0;
        int heightDelta = 0;

        public XBase_Class_Visual_Form(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new Form(), "Form", "form", true, UserObject.URW);
            me.THISFORM = me;

            // Calculate how much is being stolen
            widthDelta = Form.Width - Form.ClientSize.Width;
            heightDelta = Form.Height - Form.ClientSize.Height;
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
         *      9   - Processed and saved, do not do anything else
         *      10  - Processed and saved
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
                        case "baseclass":
                        case "class":
                        case "classlibrary":
                        case "controlcount":
                        case "controls":
                        case "datasessionid":
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
                                if (objtk.AsInt() < 0)
                                    result = 41;
                                else
                                {
                                    objValue = objtk.AsInt() + heightDelta;
                                    result = base.SetProperty(propertyName, objValue, objIdx);
                                }
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
                                if (objtk.AsInt() < 0)
                                    result = 41;
                                else
                                {
                                    objValue = objtk.AsInt() + widthDelta;
                                    result = base.SetProperty(propertyName, objValue, objIdx);
                                }
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
                        // 9 & 10 skips the save
                        if (result < 9)
                        {
                            // We processed it or just need to save the property (perhaps again)
                            // Ignore the CA1854 as it won't put the value into the property
                            UserProperties[propertyName].Element.Value = objValue;
                        }

                        result = 0;
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
            int result = 9;

            // First, we check to make sure that the property exists
            if (UserProperties.ContainsKey(propertyName))
            {
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

                    case "height":
                        returnToken.Element.Value = Form.Height - heightDelta;
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


                    case "width":
                        returnToken.Element.Value = Form.Width - widthDelta;
                        break;

                    case "windowstate":
                        returnToken.Element.Value = Form.WindowState switch
                        {
                            FormWindowState.Minimized => 1,
                            FormWindowState.Maximized => 2,
                            _ => 0
                        };
                        break;

                    default:
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        result = result == 0 ? 9 : result;
                        break;
                }


                if (JAXLib.Between(result, 0, 10))
                {
                    if (result < 9)
                    {
                        // Get the base information
                        returnToken.CopyFrom(UserProperties[propertyName]);
                    }

                    result = 0;
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


        public override int DoDefault(string methodName)
        {
            int result = 0;

            // Process the object's native method if appropriate
            switch (methodName.ToLower())
            {
                default:
                    result = base.DoDefault(methodName);
                    break;
            }

            return result;
        }

        public override string[] JAXMethods()
        {
            return ["addobject", "addproperty", "box", "circle", "cls", "dock", "draw", "getdockstate", "hide", "line",
            "move", "newobject", "pset", "point", "print", "readexpression", "readmethod", "refresh", "release",
            "removeobject", "resettodefault", "saveas", "saveasclass", "setall","setfocus","setmousepointer","setviewport", "show", "showwhatsthis",
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
                "activecontrol,N,0","alwaysontop,L,false",
                "autocenter,L,false",
                "backcolor,R,15790320","bindcontrols,L,true","bordercolor,R,0","borderstyle,N,3","borderwidth,n,0",
                "baseclass,C!,form","class,C!,Form",
                "caption,C,Form","classlibrary,C$,","closable,L,true",
                "comment,C,","controlbox,L,true","controlcount,N,0",
                "datasession,N,1","datasessionid,N!,1",
                "Enabled,L,true",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false",
                "FontItalic,L,false","FontName,C,","FontSize,N,0",
                "FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,R,0",
                "Height,N,0",
                "icon,C,",
                "keypreview,L,false",
                "left,N,0","lockscreen,L,false",
                "maxbutton,L,true","maxheight,N,-1","maxwidth,N,-1","minbutton,L,true",
                "minheight,N,-1","minwidth,N,-1","mousepointer,n,0","moveable,L,true",
                "name,C,form",
                "objects,*,",
                "parent,o$,","parentclass,C$,","picture,C,",
                "righttoleft,L,false",
                "shownintaskbar,L,true","showwindow,N,0",
                "tag,C,","tabindex,N,1","tabstop,L,true","top,N,0","tooltiptext,c,",
                "visible,L,true",
                "width,N,0","windowstate,N,0","windowtype,N,0"
                ];
        }
    }
}
