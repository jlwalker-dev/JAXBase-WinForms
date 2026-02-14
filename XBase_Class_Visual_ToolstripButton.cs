namespace JAXBase
{
    public class XBase_Class_Visual_ToolstripButton : XBase_Class
    {
        public ToolStripButton ToolstripButton => (ToolStripButton)me.nvObject!;
        public new string MyDefaultName { get; set; } = "tsbtn";

        public XBase_Class_Visual_ToolstripButton(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(null, "Toolbutton", string.IsNullOrWhiteSpace(name) ? MyDefaultName : name, false, UserObject.urw);
            me.nvObject = new ToolStripButton();

            ToolstripButton.ImageScaling = ToolStripItemImageScaling.None;
            ToolstripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolstripButton.ImageAlign = ContentAlignment.MiddleCenter;
        }



        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------


            return result;
        }


        public override int _SetMethod(string methodName, string SourceCode, string CompCode, string Type)
        {
            int result = base._SetMethod(methodName, SourceCode, CompCode, Type);

            if (result == 0 && string.IsNullOrWhiteSpace(SourceCode + CompCode) == false)
            {
                if (methodName.Equals("click", StringComparison.OrdinalIgnoreCase))
                {
                    ToolstripButton.Click -= My_Click;             // Unsubscribe so only one connection
                    ToolstripButton.Click += My_Click;             // Subscribe to make sure we have a connection
                }

                if (methodName.Equals("mousehover", StringComparison.OrdinalIgnoreCase))
                {
                    ToolstripButton.MouseHover -= My_MouseHover;
                    ToolstripButton.MouseHover += My_MouseHover;
                }

                if (methodName.Equals("mouseenter", StringComparison.OrdinalIgnoreCase))
                {
                    ToolstripButton.MouseEnter -= My_MouseEnter;
                    ToolstripButton.MouseEnter += My_MouseEnter;
                }

                if (methodName.Equals("mouseleave", StringComparison.OrdinalIgnoreCase))
                {
                    ToolstripButton.MouseLeave -= My_MouseLeave;
                    ToolstripButton.MouseLeave += My_MouseLeave;
                }

                if (methodName.Equals("mousedown", StringComparison.OrdinalIgnoreCase))
                {
                    ToolstripButton.MouseDown -= My_MouseDown;
                    ToolstripButton.MouseDown += My_MouseDown;
                }

                if (methodName.Equals("mouseup", StringComparison.OrdinalIgnoreCase))
                {
                    ToolstripButton.MouseDown -= My_MouseUp;
                    ToolstripButton.MouseDown += My_MouseUp;
                }
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
         *     -1   - Error encountered
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
                                ToolstripButton.BackColor = XClass_AuxCode.IntToColor(tk.AsInt());
                            else
                                result = 11;
                            break;

                        case "caption":
                            if (tk.Element.Type.Equals("C"))
                                ToolstripButton.Text = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "enabled":
                            if (tk.Element.Type.Equals("L"))
                                ToolstripButton.Enabled = tk.AsBool();
                            else
                                result = 11;
                            break;

                        case "fontcharset":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = tk.AsInt();
                                XClass_AuxCode.SetFont((IJAXClass)ToolstripButton);
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
                                UserProperties[propertyName].Element.Value = tk.AsBool();
                                XClass_AuxCode.SetFont((IJAXClass)ToolstripButton);
                            }
                            else
                                result = 11;
                            break;

                        case "fontname":
                            if (tk.Element.Type.Equals("C"))
                            {
                                UserProperties[propertyName].Element.Value = tk.AsString() ?? string.Empty;
                                XClass_AuxCode.SetFont((IJAXClass)ToolstripButton);
                            }
                            else
                                result = 11;
                            break;

                        case "fontsize":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = tk.AsInt();
                                XClass_AuxCode.SetFont((IJAXClass)ToolstripButton);
                            }
                            else
                                result = 11;
                            break;

                        case "forecolor":
                            if (tk.Element.Type.Equals("N"))
                                ToolstripButton.ForeColor = XClass_AuxCode.IntToColor(tk.AsInt());
                            else
                                result = 11;
                            break;

                        case "height":
                        case "width":
                            if (tk.Element.Type.Equals("N"))
                            {
                                tk.Element.Value = tk.AsInt() > 15 ? tk.AsInt() : 16;
                                tk.Element.Value = tk.AsInt() < 241 ? tk.AsInt() : 240;
                            }
                            else
                                result = 11;

                            break;

                        case "name":
                            if (tk.Element.Type.Equals("C"))
                                ToolstripButton.Name = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "picture":

                            if (tk.Element.Type.Equals("C"))
                            {
                                string iname = string.Empty;
                                string val = tk.AsString();
                                Image? picimage = null;

                                if (val.StartsWith("htt", StringComparison.OrdinalIgnoreCase))
                                {
                                    // URI
                                    result = 11;
                                }
                                else if (val.Contains('\\') || val.Contains(':'))
                                {
                                    // Filename
                                    iname = JAXLib.JustFName(val).ToLower();
                                    result = App.JaxImages.RegisterImage(val, iname, out _);
                                }
                                else
                                {
                                    // Expecting image name
                                    iname = val.ToLower();
                                }

                                // Set the image, resize it, and store it to the button
                                if (result == 0)
                                {
                                    picimage = App.JaxImages.GetSDImage(iname, out _);

                                    if (picimage is not null)
                                        picimage = App.JaxImages.ResizeImage(picimage, UserProperties["height"].AsInt(), UserProperties["width"].AsInt());

                                    ToolstripButton.Image = picimage;
                                    UserProperties["imagename"].Element.Value = iname;
                                }
                            }
                            else
                                result = 11;

                            break;

                        case "imagename":
                            if (tk.Element.Type.Equals("C"))
                            {
                                string iname = tk.AsString().ToLower();
                                if (App.JaxImages.RegisterImage(tk.AsString(), iname, out string imgName) == 0)
                                {
                                    Image? picimage = App.JaxImages.GetSDImage(iname, out imgName);

                                    if (picimage is not null)
                                        picimage = App.JaxImages.ResizeImage(picimage, UserProperties["height"].AsInt(), UserProperties["width"].AsInt());

                                    ToolstripButton.Image = picimage;
                                }
                            }
                            else
                                result = 11;

                            break;

                        case "righttoleft":
                            if (tk.Element.Type.Equals("L"))
                                ToolstripButton.RightToLeft = tk.AsBool() ? RightToLeft.Yes : RightToLeft.No;
                            else
                                result = 11;
                            break;


                        case "visible":
                            if (tk.Element.Type.Equals("L"))
                                ToolstripButton.Visible = tk.AsBool();
                            else
                                result = 11;
                            break;
                    }
                }
                else
                    result = 1559;

                // We don't save what we don't process
                // result>0 means an error occured so
                // don't save the value.
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

            methodName = methodName.ToLower();
            if (Methods.ContainsKey(methodName))
            {
                switch (methodName)
                {
                    case "setsize":
                        if (App.ParameterClassList[0].token.Element.Type.Equals("N"))
                        {
                            int v = App.ParameterClassList[0].token.AsInt();
                            v = v > 15 ? v : 16;
                            v = v < 241 ? v : 240;
                            UserProperties["height"].Element.Value = v;
                            UserProperties["width"].Element.Value = v;

                            ToolstripButton.Size = new Size(v, v);
                        }
                        else
                            result = 11;

                        break;

                    default:
                        result = base.DoDefault(methodName);
                        break;
                }
            }

            App.ParameterClassList.Clear();
            return result;
        }


        /*------------------------------------------------------------------------------------------*
         * Methods list
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
                "addproperty","refresh",
                "saveasclass","settooriginalvalue","setfocus","setsize",
                "writeexpression","writemethod","zorder"
                ];
        }


        /*------------------------------------------------------------------------------------------*
         * Events list
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","error",
                "mouseenter","mousehover","mouseleave","visiblechanged",
                "when"
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
                "baseclass,C,toolstrip","backcolor,r,0",
                "caption,c,","class,C,toolstrip","classlibrary,C,","comment,c,","controlcount,n,0",
                "enabled,l,.t.",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,r,0",
                "height,n,32",
                "imagename,c,",
                "name,c,",
                "objects,*,",
                "parent,o,","parentclass,c,","picture,c,",
                "righttoleft,L,false",
                "tag,c,","tooltip,c,",
                "visible,l,.t.",
                "width,n,32"
                ];
        }


        /*------------------------------------------------------------------------------------------*
         * Event handlerrs for Menu Items
         *------------------------------------------------------------------------------------------*/
        private void My_Click(object? sender, EventArgs e)
        {
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

        private void My_MouseDown(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("mousedown"))
                _CallMethod("mousedown");
        }

        private void My_MouseUp(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("mouseup"))
                _CallMethod("mouseup");
        }

    }
}
