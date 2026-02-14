/*------------------------------------------------------------------------------------------*
 * MenuItem Class
 * 
 * 2025.11.19 - JLW
 *      Tool strips for JAXBase.  Add a nice looking tool strip to your form
 *      using any kind of graphic, though ico an png are probably best.
 *      
 * 2025.12.11 - JLW
 *      Learning about icon/pic use in C# so I can finish this up and have
 *      the grid left before getting serious with the form designer bootstrap
 *      project which will be the kick-off of Version 0.6 developement.
 *      
 *      The AppClass has the ImageLibrary class which handles registration and
 *      access of all images.  Images are stored using the lowercase "stem.ext"
 *      of it's file name.
 * 
 *------------------------------------------------------------------------------------------*/
using ZXing;

namespace JAXBase
{
    public class XBase_Class_Visual_ToolStrip : XBase_Class_Visual
    {
        public ToolStrip Toolstrip => (ToolStrip)me.visualObject!;
        public new string MyDefaultName { get; set; } = "toolbar";

        public XBase_Class_Visual_ToolStrip(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new ToolStrip(), "Toolbar", string.IsNullOrWhiteSpace(name) ? MyDefaultName : name, true, UserObject.URW);
        }


        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------


            return result;
        }


        /*------------------------------------------------------------------------------------------*
         * Add an object to the end of the objects array
         *------------------------------------------------------------------------------------------*/
        public override int AddObject(JAXObjectWrapper value)
        {
            int err = 0;
            if (CanUseObjects == false)
                err = 3019;

            if (err == 0 && CanWriteObjects)
            {
                if (value is not null && value.nvObject is not null)
                {
                    if (JAXLib.InListC(value.BaseClass, "toolbutton", "separator"))
                    {
                        base.MakeNextDefaultName(value);
                        value.SetParent(me);

                        if (value.BaseClass.ToLower() == "separator")
                            Toolstrip.Items.Add((ToolStripSeparator)value.nvObject);
                        else
                            Toolstrip.Items.Add((ToolStripButton)value.nvObject);
                    }
                    else
                    {
                        err = 1903;
                    }
                }

                if (err == 0)
                {
                    UserProperties["objects"].Add(value!);
                    UserProperties["controlcount"].Element.Value = UserProperties["controlcount"].AsInt() + 1;
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
                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (JAXLib.Between(result, 1, 10))
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
        public new virtual int SetProperty(string propertyName, object objValue, int objIdx)
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
                                Toolstrip.BackColor = XClass_AuxCode.IntToColor(Convert.ToInt32(objValue));
                            else
                                result = 11;
                            break;

                        case "enabled":
                            if (tk.Element.Type.Equals("N"))
                                Toolstrip.Enabled = Convert.ToBoolean(objValue);
                            else
                                result = 11;
                            break;

                        case "fontcharset":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                XClass_AuxCode.SetFont((IJAXClass)Toolstrip);
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
                                XClass_AuxCode.SetFont((IJAXClass)Toolstrip);
                            }
                            else
                                result = 11;
                            break;

                        case "fontname":
                            if (tk.Element.Type.Equals("C"))
                            {
                                UserProperties[propertyName].Element.Value = objValue.ToString() ?? string.Empty;
                                XClass_AuxCode.SetFont((IJAXClass)Toolstrip);
                            }
                            else
                                result = 11;
                            break;

                        case "fontsize":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                XClass_AuxCode.SetFont((IJAXClass)Toolstrip);
                            }
                            else
                                result = 11;
                            break;

                        case "forecolor":
                            if (tk.Element.Type.Equals("N"))
                                Toolstrip.ForeColor = XClass_AuxCode.IntToColor(Convert.ToInt32(objValue));
                            else
                                result = 11;
                            break;

                        case "height":
                            if (tk.Element.Type.Equals("N"))
                                Toolstrip.Height = Convert.ToInt32(objValue);
                            else
                                result = 11;
                            break;

                        case "name":
                            if (tk.Element.Type.Equals("C"))
                                Toolstrip.Name = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "righttoleft":
                            if (tk.Element.Type.Equals("L"))
                                Toolstrip.RightToLeft = Convert.ToBoolean(objValue) ? RightToLeft.Yes : RightToLeft.No;
                            else
                                result = 11;
                            break;

                        case "vertical":
                            if (tk.Element.Type.Equals("L"))
                            {
                                bool vertical = tk.AsBool();

                                if (vertical)
                                {
                                    // Vertical toolbar on left side
                                    Toolstrip.Dock = DockStyle.Left;                    // Stick to the left
                                    Toolstrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                                }
                                else
                                {
                                    Toolstrip.Dock = DockStyle.Top;                     // Stick to the top
                                    Toolstrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                                }

                                JAXObjects.Token objects = new();
                                objects = UserProperties["objects"];

                                for (int i = 0; i < objects.Count; i++)
                                {
                                    JAXObjectWrapper? jow = objects._avalue[i].Value as JAXObjectWrapper;

                                    if (jow is not null && jow.Class.Equals("toolbutton", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Fix the settings of this button
                                        if (vertical)
                                        {

                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            break;

                        case "visible":
                            if (tk.Element.Type.Equals("L"))
                                Toolstrip.Visible = Convert.ToBoolean(objValue);
                            else
                                result = 11;
                            break;

                        case "width":
                            if (tk.Element.Type.Equals("N"))
                                Toolstrip.Width = Convert.ToInt32(objValue);
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
                "class,C,toolstrip","classlibrary,C,","comment,c,","controlcount,n,0",
                "dock,n,0",
                "enabled,l,.t.",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,r,0",
                "name,c,",
                "objects,*,",
                "parent,o,","parentclass,c,",
                "righttoleft,L,false",
                "tag,c,",
                "vertical,l,.f.",
                "visible,l,.t.",
                ];
        }


    }
}
