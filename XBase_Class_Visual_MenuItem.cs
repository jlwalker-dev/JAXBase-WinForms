/*------------------------------------------------------------------------------------------*
 * MenuItem Class
 * 
 * 2025.11.17 - JLW
 *      Created this component of the menu class.  You add menu items to a menu
 *      by creating a object of this class and adding it to the menu object.
 *      You can also add menuitem objects to this object to create sub-menus.
 *      
 *      Not actually a visual object, but a component of the menu which is
 *      a visual object.
 *      
 *      Limited properties, events and methods at this time as I'm just looking 
 *      for basic functionality.
 *      
 * 2025.12.11 - JLW
 *      First full success with building and use a menu!
 *      Took a few days and google searches to learn more about events in C#
 *      than I really thought I would ever need to learn.  It's so much more
 *      complicated than XBase!  While .Net seems to have more capabilities, 
 *      XBase works just fine with the simpler interface.
 *      
 *------------------------------------------------------------------------------------------*/
using ZXing;

namespace JAXBase
{
    public class XBase_Class_Visual_MenuItem : XBase_Class
    {
        public ToolStripMenuItem Menuitem => (ToolStripMenuItem)me.nvObject!;
        public new string MyDefaultName { get; set; } = "mitem";

        public XBase_Class_Visual_MenuItem(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(null, "MenuItem", "menuitem", false, UserObject.URW);

            // Objects that are not visual need to be manually saved into
            // the nvObject (non-visual object) variable for later use.
            me.nvObject = new ToolStripMenuItem();
        }

        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------
            Menuitem.Tag = UserProperties["classid"].AsString();
            return result;
        }


        /*------------------------------------------------------------------------------------------*
         * Override so that we have a place to register menu item events.
         * Only one set of subscriptions are allowed, so unsubscribe any previous subscriptions
         * and create a new one.  Also, only subscribe if an event has code in it.
         *------------------------------------------------------------------------------------------*/
        public override int _SetMethod(string methodName, string SourceCode, string CompCode, string Type)
        {
            int i= base._SetMethod(methodName, SourceCode, CompCode, Type);

            if (i==0 && string.IsNullOrWhiteSpace(SourceCode+CompCode)==false)
            {
                if (methodName.Equals("click",StringComparison.OrdinalIgnoreCase))
                {
                    Menuitem.Click -= My_Click;             // Unsubscribe so only one connection
                    Menuitem.Click += My_Click;             // Subscribe to make sure we have a connection
                }

                if (methodName.Equals("mousehover", StringComparison.OrdinalIgnoreCase))
                {
                    Menuitem.MouseHover -= My_MouseHover;
                    Menuitem.MouseHover += My_MouseHover;
                }

                if (methodName.Equals("mouseenter", StringComparison.OrdinalIgnoreCase))
                {
                    Menuitem.MouseEnter -= My_MouseEnter;
                    Menuitem.MouseEnter += My_MouseEnter;
                }

                if (methodName.Equals("mouseleave", StringComparison.OrdinalIgnoreCase))
                {
                    Menuitem.MouseLeave -= My_MouseLeave;
                    Menuitem.MouseLeave += My_MouseLeave;
                }

                if (methodName.Equals("mousedown", StringComparison.OrdinalIgnoreCase))
                {
                    Menuitem.MouseDown -= My_MouseDown;
                    Menuitem.MouseDown += My_MouseDown;
                }

                if (methodName.Equals("mouseup", StringComparison.OrdinalIgnoreCase))
                {
                    Menuitem.MouseDown -= My_MouseUp;
                    Menuitem.MouseDown += My_MouseUp;
                }
            }
            return i;
        }

        /*------------------------------------------------------------------------------------------*
         * Add an object to the end of the objects array
         *------------------------------------------------------------------------------------------*/
        public override int AddObject(JAXObjectWrapper value)
        {
            int err = 0;
            if (CanUseObjects == false) err = 3019;

            if (err == 0 && CanWriteObjects)
            {
                if (value.nvObject is not null)
                {
                    if (JAXLib.InListC(value.BaseClass, "Menuitem", "separator"))
                    {
                        var aa = me;
                        value.SetParent(me);

                        App.DebugLog($"Menu-Item - {me.Name}.addobject - {value.Name}");
                        if (value.BaseClass.ToLower() == "separator")
                            Menuitem.DropDownItems.Add((ToolStripMenuItem)value.nvObject);
                        else
                        {
                            value.GetProperty("classid", out JAXObjects.Token tk);
                            me.GetProperty("classid", out JAXObjects.Token tkm);

                            App.DebugLog($"Adding menuitem {tk.AsString()} to menuitem {tkm.AsString()}");
                            Menuitem.DropDownItems.Add((ToolStripMenuItem)value.nvObject);
                        }

                        // TODO - Update the parent information
                    }
                    else
                        err = 1903;
                }
                else
                    err = 1902;

                if (err == 0)
                {
                    UserProperties["objects"].Add(value);
                    PostInit(me, []);
                }
            }
            else
                err = 3019;

            if (err > 0)
            {
                _AddError(err, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(err, $"{err}|", string.Empty);
            }

            return err > 0 ? -1 : UserProperties["objects"]._avalue.Count;
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
         *      -1  - Error Code
         *      
         *------------------------------------------------------------------------------------------*/
        public override int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            JAXObjects.Token tk = new();
            tk.Element.Value = objValue;
            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                if (UserProperties.ContainsKey(propertyName))
                    {
                    // Intercept special handling of properties
                    switch (propertyName)
                    {
                        // Intercept special handling of properties
                        case "backcolor":
                            if (tk.Element.Type.Equals("N"))
                                Menuitem.BackColor = XClass_AuxCode.IntToColor(Convert.ToInt32(objValue));
                            else
                                result = 11;
                            break;

                        case "caption":
                            if (tk.Element.Type.Equals("C"))
                                Menuitem.Text = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "enabled":
                            if (tk.Element.Type.Equals("N"))
                                Menuitem.Enabled = Convert.ToBoolean(objValue);
                            else
                                result = 11;
                            break;

                        case "fontcharset":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                XClass_AuxCode.SetFont((IJAXClass)Menuitem);
                            }
                            else
                                result = 11;
                            break;

                        case "fontbold":
                        case "fontitalic":
                        case "fontoutline":
                        case "fontshadow":
                        case "fontstrikethrough":
                        case "fontunderline":
                            if (tk.Element.Type.Equals("C"))
                            {
                                UserProperties[propertyName].Element.Value = (bool)objValue;
                                XClass_AuxCode.SetFont((IJAXClass)Menuitem);
                            }
                            else
                                result = 11;
                            break;

                        case "fontname":
                            if (tk.Element.Type.Equals("C"))
                            {
                                UserProperties[propertyName].Element.Value = objValue.ToString() ?? string.Empty;
                                XClass_AuxCode.SetFont((IJAXClass)Menuitem);
                            }
                            else
                                result = 11;
                            break;

                        case "fontsize":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                XClass_AuxCode.SetFont((IJAXClass)Menuitem);
                            }
                            else
                                result = 11;
                            break;

                        case "forecolor":
                            if (tk.Element.Type.Equals("N"))
                                Menuitem.ForeColor = XClass_AuxCode.IntToColor(Convert.ToInt32(objValue));
                            else
                                result = 11;
                            break;

                        case "name":
                            if (tk.Element.Type.Equals("C"))
                                Menuitem.Name = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "righttoleft":
                            if (tk.Element.Type.Equals("L"))
                                Menuitem.RightToLeft = Convert.ToBoolean(objValue) ? RightToLeft.Yes : RightToLeft.No;
                            else
                                result = 11;
                            break;

                        case "visible":
                            if (tk.Element.Type.Equals("L"))
                                Menuitem.Visible = Convert.ToBoolean(objValue);
                            else
                                result = 11;
                            break;
                    }
                }
                else
                    result = 1559;

                // We don't save what we don't process
                if (result == 0)
                {
                    // We processed it, so save the property to the dictionary
                    // Ignore the CA1854 as it won't put the value into the property
                    if (UserProperties.ContainsKey(propertyName))
                        UserProperties[propertyName].Element.Value = objValue;
                    else
                        result = 1559;
                }
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



        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
                "addproperty","addobject","refresh","removeobject",
                "saveasclass","settooriginalvalue","setfocus",
                "writeexpression","writemethod","zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","error","mousedown","mouseenter","mousehover","mouseleave","mouseup","visiblechanged","when"
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
                "baseclass,C,menuitem","backcolor,r,0",
                "caption,c,","class,C,menuitem","classlibrary,C,","comment,c,","controlcount,n,0",
                "enabled,l,.t.",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,r,0",
                "name,c,",
                "objects,*,",
                "parent,o,","parentclass,c,","picture,c,",
                "righttoleft,L,false",
                "tag,c,","tooltip,c,",
                "visible,l,.t.",
                ];
        }



        /*------------------------------------------------------------------------------------------*
         * Event handlers for Menu Items
         *------------------------------------------------------------------------------------------*/
        private void My_Click(object? sender, EventArgs e)
        {
            // At this time ignore if it's a mouse or a key click.
            // Save that for version 2
            if (Methods.ContainsKey("click"))
                _CallMethod("click");
        }

        private void My_MouseHover(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("mousehover"))
                _CallMethod("mousehover");
        }

        private void My_MouseEnter(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("mouseenter"))
                _CallMethod("mouseenter");
        }

        private void My_MouseLeave(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("mouseleave"))
                _CallMethod("mouseleave");
        }

        private void My_MouseDown(object? sender, MouseEventArgs e)
        {
            if (Methods.ContainsKey("mousedown"))
                _CallMethod("mousedown");
        }

        private void My_MouseUp(object? sender, MouseEventArgs e)
        {
            if (Methods.ContainsKey("mousedown"))
                _CallMethod("mousedown");
        }
    }
}
