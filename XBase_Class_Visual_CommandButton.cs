namespace JAXBase
{
    public class XBase_Class_Visual_CommandButton : XBase_Class_Visual
    {
        public Button btn => (Button)me.visualObject!;

        public XBase_Class_Visual_CommandButton(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new Button(), "CommandButton", "cmdbutton", true, UserObject.URW);
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------

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
                                    btn.AutoSize = a;
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
                                        btn.Height = h;
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
                                        btn.Width = w;
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
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result,0,10))
                    {
                        if (result < 9)
                        {
                            // First, we check to make sure that the property exists
                            if (UserProperties.ContainsKey(propertyName))
                            {
                                // Visual object common property handler
                                switch (propertyName)
                                {
                                    case "test":
                                        break;
                                }


                                // Did we process it?
                                if (result == 0)
                                   UserProperties[propertyName].Element.Value = objValue;
                            }
                            else
                                result = 1559;
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
                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }
            }
            else
                result = 1559;

            if (JAXLib.Between(result, 1, 10))
            {
                // First, we double check to make sure that the property exists
                if (UserProperties.ContainsKey(propertyName))
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


        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
                "addproperty","move","readexpression","readmethod","refresh","resettodefault",
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
                "click","destroy","error","gotfocus",
                "init","keypress","load","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "rightclick","valid","visiblechanged","visiblechanged","when"
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
                "alignment,n,0","anchor,n,0","autosize,l,false","backcolor,R,16777215","BaseClass,C,commandbutton",
                "caption,c,Option1","Class,C,commandbutton","ClassLibrary,C,",
                "Comment,C,",
                "disabledpicture,c,","downpicture,c,",
                "Enabled,L,true",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false","forcolor,R,0",
                "Height,N,0",
                "left,N,0",
                "name,c,command",
                "originalvalue,,",
                "parent,o,","parentclass,C,","picture,c,","picturemargin,n,0","pictureposition,n,13","picturespacing,n,0",
                "righttoleft,L,false",
                "setoriginalwhen,n,0",
                "tabindex,n,1","tabstop,l,true","tag,C,","tooltiptext,c,",
                "top,N,0",
                "value,N,1","visible,l,true",
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
                btn.Height = h;
                btn.Width = w;
                btn.MaximumSize = new Size(w, h);
                btn.AutoSize = true;
            }
            else
            {
                if (autosize)
                {
                    btn.Height = h;
                    btn.Size = new Size(0, h);
                    btn.AutoSize = true;
                }
                else
                {
                    btn.AutoSize = false;
                    btn.Height = h;
                    btn.Width = w;
                }
            }
        }
    }
}
