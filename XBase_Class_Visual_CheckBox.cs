/*
 * CHECKBOX class
 * 
 */
using System.Drawing.Configuration;
using static JAXBase.JAXLabel;

namespace JAXBase
{
    public class XBase_Class_Visual_CheckBox : XBase_Class_Visual
    {
        public JAXCheckBox ChkBox => (JAXCheckBox)me.visualObject!;

        public XBase_Class_Visual_CheckBox(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new JAXCheckBox(), "Checkbox", "checkbox", true, UserObject.URW);
        }

        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = false;

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------
            if (InInit)
            {
                ChkBox.CheckedChanged += ChkBox_CheckedChanged;

                result = base.PostInit(callBack, parameterList);
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
         *      9   - Processed and saved, do not do anything else
         *      10  - Processed and saved
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
            int h, w;
            bool a, b;

            JAXObjects.Token tk = new();
            tk.Element.Value = objValue;    // Now we can type it easily!
            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                if (UserProperties.ContainsKey(propertyName))
                {
                    switch (propertyName)
                    {
                        // Intercept special handling of properties
                        case "autosize":
                            if (tk.Element.Type.Equals("L"))
                            {
                                a = tk.AsBool();
                                h = UserProperties["height"].AsInt();
                                b = UserProperties["wordwrap"].AsBool();
                                w = UserProperties["width"].AsInt();

                                if (b)
                                    SetWordWrap(a, b, h, w);
                                else
                                    ChkBox.AutoSize = a;
                            }
                            else
                                result = 11;
                            break;

                        case "bordercolor":
                            objValue = JAXUtilities.ReturnColorInt(objValue);
                            ChkBox.BorderColor = XClass_AuxCode.IntToColor((int)objValue);
                            UserProperties["bordercolor"].Element.Value = objValue;
                            result = 9; // do nothing else
                            break;

                        case "borderwidth":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (JAXLib.Between(tk.AsInt(), 0, 15))
                                    ChkBox.BorderWidth = tk.AsInt();
                                else
                                    result = 41;
                            }
                            else
                                result = 11;
                            break;

                        case "enabled":
                            if (tk.Element.Type.Equals("L"))
                            {
                                ChkBox.Enabled = tk.AsBool();
                                ChkBox.Image = App.JaxImages.GetSDImage(UserProperties[ChkBox.Enabled ? "picture" : "disabledpicure"].AsString(), out _);
                            }
                            else
                                result = 11;
                            break;

                        case "height":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0)
                                    result = 41;
                                else
                                {
                                    a = UserProperties["autosize"].AsBool();
                                    h = tk.AsInt();
                                    b = UserProperties["wordwrap"].AsBool();
                                    w = UserProperties["width"].AsInt();

                                    if (b)
                                        SetWordWrap(a, b, h, w);
                                    else
                                        ChkBox.Height = h;
                                }
                            }
                            else
                                result = 11;
                            break;

                        case "picture":
                            result = 1999;
                            break;

                        case "pictureposition":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (JAXLib.Between(tk.AsInt(), 0, 14))
                                {
                                    objValue = tk.AsInt();

                                    ChkBox.ImageAlign = tk.AsInt() switch
                                    {
                                        0 => System.Drawing.ContentAlignment.MiddleLeft,
                                        1 => System.Drawing.ContentAlignment.MiddleRight,
                                        2 => System.Drawing.ContentAlignment.TopCenter,
                                        3 => System.Drawing.ContentAlignment.BottomCenter,
                                        4 => System.Drawing.ContentAlignment.TopLeft,
                                        5 => System.Drawing.ContentAlignment.TopRight,
                                        6 => System.Drawing.ContentAlignment.BottomLeft,
                                        7 => System.Drawing.ContentAlignment.BottomRight,
                                        8 => System.Drawing.ContentAlignment.TopCenter,
                                        9 => System.Drawing.ContentAlignment.TopCenter,
                                        10 => System.Drawing.ContentAlignment.BottomCenter,
                                        11 => System.Drawing.ContentAlignment.BottomCenter,
                                        12 => System.Drawing.ContentAlignment.MiddleCenter,
                                        13 => System.Drawing.ContentAlignment.MiddleLeft,
                                        _ => System.Drawing.ContentAlignment.TopCenter
                                    };

                                    ChkBox.TextAlign = tk.AsInt() switch
                                    {
                                        0 => System.Drawing.ContentAlignment.MiddleRight,
                                        1 => System.Drawing.ContentAlignment.MiddleLeft,
                                        2 => System.Drawing.ContentAlignment.BottomCenter,
                                        3 => System.Drawing.ContentAlignment.TopCenter,
                                        4 => System.Drawing.ContentAlignment.TopRight,
                                        5 => System.Drawing.ContentAlignment.TopLeft,
                                        6 => System.Drawing.ContentAlignment.BottomRight,
                                        7 => System.Drawing.ContentAlignment.BottomLeft,
                                        8 => System.Drawing.ContentAlignment.BottomRight,
                                        9 => System.Drawing.ContentAlignment.BottomLeft,
                                        10 => System.Drawing.ContentAlignment.TopRight,
                                        11 => System.Drawing.ContentAlignment.TopLeft,
                                        12 => System.Drawing.ContentAlignment.MiddleCenter,
                                        13 => System.Drawing.ContentAlignment.MiddleRight,
                                        _ => System.Drawing.ContentAlignment.MiddleCenter
                                    };

                                    ChkBox.TextImageRelation = tk.AsInt() switch
                                    {
                                        0 => System.Windows.Forms.TextImageRelation.ImageBeforeText,
                                        1 => System.Windows.Forms.TextImageRelation.ImageBeforeText,
                                        2 => System.Windows.Forms.TextImageRelation.ImageBeforeText,
                                        3 => System.Windows.Forms.TextImageRelation.TextBeforeImage,
                                        4 => System.Windows.Forms.TextImageRelation.TextBeforeImage,
                                        5 => System.Windows.Forms.TextImageRelation.TextBeforeImage,
                                        6 => System.Windows.Forms.TextImageRelation.ImageAboveText,
                                        7 => System.Windows.Forms.TextImageRelation.ImageAboveText,
                                        8 => System.Windows.Forms.TextImageRelation.ImageAboveText,
                                        9 => System.Windows.Forms.TextImageRelation.TextAboveImage,
                                        10 => System.Windows.Forms.TextImageRelation.TextAboveImage,
                                        11 => System.Windows.Forms.TextImageRelation.TextAboveImage,
                                        12 => System.Windows.Forms.TextImageRelation.Overlay,
                                        13 => System.Windows.Forms.TextImageRelation.ImageBeforeText,
                                        _ => System.Windows.Forms.TextImageRelation.Overlay,
                                    };

                                    // Clear text for option 14
                                    if (tk.AsInt() == 14)
                                        ChkBox.Text = "";
                                    else
                                        ChkBox.Text = UserProperties["caption"].AsString();
                                }
                                else
                                    result = 41;
                            }
                            else
                                result = 11;

                            break;

                        case "width":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0)
                                    result = 41;
                                else
                                {
                                    a = UserProperties["autosize"].AsBool();
                                    b = UserProperties["wordwrap"].AsBool();
                                    h = UserProperties["height"].AsInt();
                                    w = tk.AsInt();

                                    if (b)
                                        SetWordWrap(a, b, h, w);
                                    else
                                        ChkBox.Width = w;
                                }
                            }
                            else
                                result = 11;
                            break;

                        case "wordwrap":
                            if (tk.Element.Type.Equals("L"))
                            {
                                a = UserProperties["autosize"].AsBool();
                                b = tk.AsBool();
                                h = UserProperties["height"].AsInt();
                                w = Convert.ToInt32(objValue);

                                SetWordWrap(a, b, h, w);
                            }
                            else
                                result = 11;
                            break;

                        case "value":
                            isProgrammaticChange = true;

                            if ("LN".Contains(tk.Element.Type))
                            {
                                ChkBox.Checked = tk.Element.Type.Equals("L") ? tk.AsBool() : tk.AsInt() != 0;

                                if (ChkBox.Checked)
                                {
                                    if (tk.Element.Type.Equals("N"))
                                        objValue = 1;
                                    else
                                        objValue = true;
                                }
                                else
                                {
                                    if (tk.Element.Type.Equals("N"))
                                        objValue = 0;
                                    else
                                        objValue = false;
                                }
                            }

                            isProgrammaticChange = false;
                            break;

                        default:
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result, 0, 10))
                    {
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

            if (result > 0)
            {
                _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);

                result = -1;
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

            // Get the property and fill in the value
            //resultToken.CopyFrom(UserProperties[propertyName]);
            if (UserProperties.ContainsKey(propertyName))
            {
                switch (propertyName)
                {
                    // Intercept special handling of properties
                    case "***":
                        result = 1559;
                        break;

                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (JAXLib.Between(result, 1, 10))
                {
                    // First, we double check to make sure that the property exists
                    if (result < 9)
                    {
                        result = 0;

                        // Visual object common property handler
                        switch (propertyName)
                        {
                            default:
                                returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
                                break;
                        }
                    }
                    else
                        result = 0;
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
                "addproperty", "drag", "move", "readexpression", "readmethod", "refresh", "resettodefault",
                "saveasclass", "settooriginalvalue", "setfocus", "showwhatsthis", "writeexpression", "writemethod", "zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","dragdrop","dragover","error","gotfocus","init",
                "interactivechagnge","keypress","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "programmaticchange","rangehigh","rangelow","rightclick","uienable","valid","visiblechanged","when"
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
                "alignment,n,3","anchor,n,0","autosize,L,",
                "backcolor,R,16777215","backstyle,n,1","BaseClass,C,checkbox","bordercolor,R,0","borderwidth,n,0",
                "caption,c,","centered,L,.F.","Class,C,checkboxbox","ClassLibrary,C,","Comment,C,","controlsource,c,","checkedpicture,c,",
                "disabledbackcolor,R!,15790320","disabledforecolor,R!,7171437","disabledpicture,c,","downpicture,c,",
                "Enabled,L,true",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false",
                "height,n,21",
                "left,N,0",
                "name,c,text1",
                "originalvalue,,",
                "parent,o,","parentclass,C,","picture,c,","pictureposition,n,13",
                "readonly,l,false","righttoleft,L,false",
                "selectonentry,l,f","selectedbackcolor,R,14120960","selectedforecolor,R,16777215",
                "setoriginalwhen,n,0",
                "tabindex,n,1","tabstop,l,true","tag,C,","top,N,0","tooltiptext,c,",
                "uncheckedpicture,c,",
                "value,l,.F.","visible,l,true",
                "width,N,0","wordwrap,l,.F."
                ];
        }

        /*
         * Word wrap handler for .Net
         */
        public void SetWordWrap(bool autosize, bool wrap, int h, int w)
        {
            if (wrap)
            {
                // Wrap in the maximum area
                ChkBox.Height = h;
                ChkBox.Width = w;
                ChkBox.MaximumSize = new Size(w, h);
                ChkBox.AutoSize = true;
            }
            else
            {
                if (autosize)
                {
                    ChkBox.Height = h;
                    ChkBox.Size = new Size(0, h);
                    ChkBox.AutoSize = true;
                }
                else
                {
                    ChkBox.AutoSize = false;
                    ChkBox.Height = h;
                    ChkBox.Width = w;
                }
            }
        }


        // ------------------------------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------------------------------
        private void ChkBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (isProgrammaticChange)
                _CallMethod("programmaticchange");
            else
                _CallMethod("interactivechange");
        }

        public override void MyObj_MouseDown(object? sender, MouseEventArgs e)
        {
            // Change image when mouse button is pressed
            _CallMethod("mousedown");
            ChkBox.Image = App.JaxImages.GetSDImage(UserProperties["downpicture"].AsString(), out _);
        }

        public override void MyObj_MouseUp(object? sender, MouseEventArgs e)
        {
            // Revert image when mouse button is released
            _CallMethod("mouseup");
            ChkBox.Image = App.JaxImages.GetSDImage(UserProperties["picture"].AsString(), out _);
        }
    }
}

