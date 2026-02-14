/*------------------------------------------------------------------------------------------*
 * Menu class
 * 
 * 2025.11.17 - JLW 
 *      Created and started working on it.  Not a difficult class since most of the work
 *      is just adding MenuItems and Separators.  This is a visual class but doesn't
 *      support a lot of events or methods as it's more of a pallette than a full visual
 *      control.
 *------------------------------------------------------------------------------------------*/
using System.Windows.Controls;
using System.Windows.Forms;
using ZXing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace JAXBase
{
    public class XBase_Class_Visual_Menu : XBase_Class_Visual
    {
        public MenuStrip MenuObj => (MenuStrip)me.visualObject!;
        public new string MyDefaultName { get; set; } = "menu";

        public XBase_Class_Visual_Menu(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new MenuStrip(), "Menu", "menu", true, UserObject.URW);
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
            if (CanUseObjects == false) err = 3019;

            if (err == 0 && CanWriteObjects)
            {
                if (me.visualObject is not null)
                {
                    // Add the menu item to the menu
                    if (value.nvObject is null)
                        err = 1901;
                    else if (JAXLib.InListC(value.BaseClass, "menuitem", "separator"))
                    {
                        value.SetParent(me);
                        base.MakeNextDefaultName(value);

                        if (value.BaseClass.ToLower() == "separator")
                            MenuObj.Items.Add((ToolStripMenuItem)value.nvObject);
                        else
                        {
                            ToolStripMenuItem obj = (ToolStripMenuItem)value.nvObject;

                            // Make changes for vertical/horizontal
                            if (GetProperty("vertical", out JAXObjects.Token tk) == 0)
                            {
                                if (tk.AsBool())
                                {
                                    obj.Margin = new Padding(0, 0, 0, 1);
                                    obj.Width = MenuObj.ClientSize.Width;

                                }
                                else
                                {
                                    obj.Margin = new Padding(0, 0, 0, 0);
                                    obj.Height = MenuObj.ClientSize.Width;
                                }
                            }

                            MenuObj.Items.Add(obj);
                        }
                    }
                    else
                        err = 1903;
                }

                if (err == 0)
                {
                    UserProperties["objects"].Add(value);
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
         * Handle the commmon properties by calling the base and then
         * handle the special cases.
         * 
         * Return result from XBase_Visual_Class
         *      0   - Successfully proccessed
         *      1   - Did not process
         *      2   - Requires special processing
         *     -1   - Error code
         * 
         * 
         * Return from here
         *      0   - Successfully processed
         *     -1   - Error Code
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
                                MenuObj.BackColor = XClass_AuxCode.IntToColor(Convert.ToInt32(objValue));
                            else
                                result = 11;
                            break;

                        case "caption":
                            if (tk.Element.Type.Equals("C"))
                                MenuObj.Text = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "enabled":
                            if (tk.Element.Type.Equals("N"))
                                MenuObj.Enabled = Convert.ToBoolean(objValue);
                            else
                                result = 11;
                            break;

                        case "fontcharset":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                XClass_AuxCode.SetFont((IJAXClass)MenuObj);
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
                                XClass_AuxCode.SetFont((IJAXClass)MenuObj);
                            }
                            else
                                result = 11;
                            break;

                        case "fontname":
                            if (tk.Element.Type.Equals("C"))
                            {
                                UserProperties[propertyName].Element.Value = objValue.ToString() ?? string.Empty;
                                XClass_AuxCode.SetFont((IJAXClass)MenuObj);
                            }
                            else
                                result = 11;
                            break;

                        case "fontsize":
                            if (tk.Element.Type.Equals("N"))
                            {
                                UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                XClass_AuxCode.SetFont((IJAXClass)MenuObj);
                            }
                            else
                                result = 11;
                            break;

                        case "forecolor":
                            if (tk.Element.Type.Equals("N"))
                                MenuObj.ForeColor = XClass_AuxCode.IntToColor(Convert.ToInt32(objValue));
                            else
                                result = 11;
                            break;

                        case "name":
                            if (tk.Element.Type.Equals("C"))
                                MenuObj.Name = objValue.ToString() ?? string.Empty;
                            else
                                result = 11;
                            break;

                        case "righttoleft":
                            if (tk.Element.Type.Equals("L"))
                                MenuObj.RightToLeft = Convert.ToBoolean(objValue) ? RightToLeft.Yes : RightToLeft.No;
                            else
                                result = 11;
                            break;

                        case "visible":
                            if (tk.Element.Type.Equals("L"))
                                MenuObj.Visible = Convert.ToBoolean(objValue);
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
                "addobject","addproperty","mousehover","move","readexpression","readmethod","refresh","resettodefault",
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
                "destroy","error","init","load","visiblechanged","when"
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
                "baseclass,C,menu","backcolor,r,0",
                "caption,c,","class,C,menu","classlibrary,C,","comment,c,","controlcount,n,0",
                "enabled,l,.t.",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,r,0",
                "name,c,",
                "objects,*,",
                "parent,o,","parentclass,c,","picture,c,",
                "righttoleft,L,false",
                "tag,c,","tooltiptext,c,",
                "visible,l,.t.",
                ];
        }
    }
}
