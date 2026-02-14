namespace JAXBase
{
    internal class XBase_Class_Visual_OptionButton : XBase_Class_Visual
    {
        public RadioButton optBtn => (RadioButton)me.visualObject!;

        public XBase_Class_Visual_OptionButton(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new RadioButton(), "OptionButton", "option", true, UserObject.URW);
        }

        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------

            if (InInit)
            {
                optBtn.CheckedChanged += OptBtn_CheckedChanged;
            }

            return result;
        }

        private void OptBtn_CheckedChanged(object? sender, EventArgs e)
        {
            if (isProgrammaticChange)
                _CallMethod("programmaticchange");
            else
                _CallMethod("interactivechange");
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
        public new virtual int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            int h, w;
            bool a, b;
            propertyName = propertyName.ToLower();
            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;

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
                            a = (bool)objValue;
                            h = UserProperties["height"].AsInt();
                            b = UserProperties["wordwrap"].AsBool();
                            w = UserProperties["width"].AsInt();

                            if (b)
                                SetWordWrap(a, b, h, w);
                            else
                                optBtn.AutoSize = a;
                            break;

                        case "height":
                            a = UserProperties["autosize"].AsBool();
                            h = Convert.ToInt32(objValue);
                            b = UserProperties["wordwrap"].AsBool();
                            w = UserProperties["width"].AsInt();

                            if (b)
                                SetWordWrap(a, b, h, w);
                            else
                                optBtn.Height = h;
                            break;

                        case "width":
                            a = UserProperties["autosize"].AsBool();
                            b = UserProperties["wordwrap"].AsBool();
                            h = UserProperties["height"].AsInt();
                            w = Convert.ToInt32(objValue);

                            if (b)
                                SetWordWrap(a, b, h, w);
                            else
                                optBtn.Width = w;
                            break;

                        case "wordwrap":
                            a = UserProperties["autosize"].AsBool();
                            b = (bool)objValue;
                            h = UserProperties["height"].AsInt();
                            w = Convert.ToInt32(objValue);

                            SetWordWrap(a, b, h, w);
                            break;

                        case "value":
                            isProgrammaticChange = true;
                            if ("LN".Contains(objtk.Element.Type))
                            {
                                optBtn.Checked = objtk.Element.Type.Equals("L") ? objtk.AsBool() : Math.Abs(Math.Truncate(objtk.AsDouble() + 0.99D)) > 0;

                                if (optBtn.Checked)
                                {
                                    if (objtk.Element.Type.Equals("N"))
                                        objValue = 1;
                                    else
                                        objValue = true;
                                }
                                else
                                {
                                    if (objtk.Element.Type.Equals("N"))
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
                        // Do we skip?
                        if (result < 9)
                        {
                            result = 0;

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

            if (UserProperties.ContainsKey(propertyName))
            {
                // Get the property and fill in the value
                returnToken.CopyFrom(UserProperties[propertyName]);

                switch (propertyName)
                {
                    // Intercept special handling of properties
                    case "autosize":
                    case "height":
                    case "width":
                    case "wordwrap":
                        returnToken.Element.Value = UserProperties[propertyName].Element.Value;
                        break;

                    case "value":
                        if (UserProperties[propertyName].Element.Type.Equals("N"))
                            returnToken.Element.Value = optBtn.Checked ? 1 : 0;
                        else
                            returnToken.Element.Value = optBtn.Checked;
                        break;

                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (JAXLib.Between(result, 1, 10))
                {
                    result = 0;

                    // Visual object common property handler
                    switch (propertyName.ToLower())
                    {
                        default:
                            returnToken.Element.Value = UserProperties[propertyName.ToLower()].Element.Value;
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
                "addproperty","drag","move","readexpression","readmethod","refresh","resettodefault",
                "saveasclass","settooriginalvalue","setfocus","writeexpression","writemethod","zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","error","gotfocus",
                "init","keypress","load","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "rightclick","valid","visiblechanged","when"
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
                "alignment,n,0","anchor,n,0","autosize,l,false","backcolor,n,16777215","backstyle,n,1",
                "BaseClass,C,commandbutton",
                "caption,c,Option1",
                "Class,C,Grid","ClassLibrary,C,",
                "Comment,C,",
                "disabledbackcolor,n!,16777215","disabledforecolor,n!,7171437","disabledpicture,c,","downpicture,c,",
                "Enabled,L,true","forcolor,n,0",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false",
                "Height,N,0",
                "left,N,0",
                "name,c,command",
                "originalvalue,,",
                "parent,o,","parentclass,C,","picture,c,","picturemargin,n,0","pictureposition,n,13","picturespacing,n,0",
                "righttoleft,L,false",
                "setoriginalwhen,n,0",
                "tabindex,n,1","tabstop,l,true","tag,C,","top,N,0","tooltiptext,c,",
                "value,,1","visible,l,true",
                "width,N,10","wordwrap,l,false"
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
                optBtn.Height = h;
                optBtn.Width = w;
                optBtn.MaximumSize = new Size(w, h);
                optBtn.AutoSize = true;
            }
            else
            {
                if (autosize)
                {
                    optBtn.Height = h;
                    optBtn.Size = new Size(0, h);
                    optBtn.AutoSize = true;
                }
                else
                {
                    optBtn.AutoSize = false;
                    optBtn.Height = h;
                    optBtn.Width = w;
                }
            }
        }
    }
}