namespace JAXBase
{
    public class XBase_Class_Visual_Label : XBase_Class_Visual
    {
        public JAXLabel lbl => (JAXLabel)me.visualObject!;

        public XBase_Class_Visual_Label(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new JAXLabel(), "Label", "label", true, UserObject.URW);
        }

        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------

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
            int baseresult = 0;
            int result = 0;
            int w, h;
            bool a, b;

            JAXObjects.Token tk = new();
            tk.Element.Value = objValue;    // Now we can type it easily!
            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                // Do we need to process this property?
                // First, we doublecheck to make sure that the property exists
                if (UserProperties.ContainsKey(propertyName))
                {
                    // Visual object common property handler
                    switch (propertyName.ToLower())
                    {
                        case "baseclass":
                        case "class":
                        case "classlibrary":
                        case "parent":
                        case "parentclass":
                            baseresult = 1533;
                            break;

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
                                    lbl.AutoSize = a;
                            }
                            else
                                result = 11;
                            break;

                        case "height":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0)
                                    result = 9999;
                                else
                                {
                                    a = UserProperties["autosize"].AsBool();
                                    h = tk.AsInt();
                                    b = UserProperties["wordwrap"].AsBool();
                                    w = UserProperties["width"].AsInt();

                                    if (b)
                                        SetWordWrap(a, b, h, w);
                                    else
                                        lbl.Height = h;
                                }
                            }
                            else
                                result = 11;
                            break;

                        case "width":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0)
                                    result = 9999;
                                else
                                {
                                    a = UserProperties["autosize"].AsBool();
                                    b = UserProperties["wordwrap"].AsBool();
                                    h = UserProperties["height"].AsInt();
                                    w = tk.AsInt();

                                    if (b)
                                        SetWordWrap(a, b, h, w);
                                    else
                                        lbl.Width = w;
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

                        default:
                            baseresult = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Did we process it?
                    if (baseresult < 2)
                    {
                        // We processed it or just need to save the property (perhaps again)
                        // Ignore the CA1854 as it won't put the value into the property
                        if (UserProperties.ContainsKey(propertyName))
                        {
                            UserProperties[propertyName].Element.Value = objValue;
                            result = 0;
                        }
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

                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (result > 0 && result < 11)
                {
                    returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;

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


        public override string[] JAXMethods()
        {
            return [
                "addproperty", "drag", "move", "readexpression", "readmethod", "refresh",
                "resettodefault", "saveasclass", "setfocus", "showwhatsthis", "writeexpression", "writemethod", "zorder"
                ];
        }

        public override string[] JAXEvents()
        {
            return [
                "click","dblclick","destroy","dragdrop","dragover","error","gotfocus",
                "init","interactivechagnge","keypress","load","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "programmaticchange","rangehigh","rangelow","rightclick","uienable","valid","visiblechanged","when"
            ];
        }

        public override string[] JAXProperties()
        {
            return [
                "alignment,n,0","anchor,n,0","autosize,l,false",
                "backcolor,n,16777215","backstyle,n,1","BaseClass,C,label","borderstyle,n,0",
                "caption,c,","Class,C,label","ClassLibrary,C,","Comment,C,",
                "disabledbackcolor,n,15790320","disabledforecolor,n,7171437",
                "Enabled,L,true",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false",
                "FontName,C,","FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false",
                "forcolor,n,0",
                "height,n,21",
                "left,N,0",
                "name,c,label1",
                "parent,o,","parentclass,C,",
                "righttoleft,L,false",
                "tabindex,n,1","tabstop,l,false","tag,C,","top,N,0","tooltiptext,c,",
                "visible,l,true","width,N,100","wordwrap,l,false"
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
                lbl.MaximumSize = new Size(w, h);
                lbl.AutoSize = true;
            }
            else
            {
                if (autosize)
                {
                    lbl.Size = new Size(0, h);
                    lbl.AutoSize = true;
                }
                else
                {
                    lbl.AutoSize = false;
                    lbl.Height = h;
                    lbl.Width = w;
                }
            }
        }
    }
}
