/*------------------------------------------------------------------------------------------*
 * This is the Visual base class - All visual classes will subclass from here.
 * 
 * 2025.11.02 - JLW
 *      Created this as a base class for all visual objects.
 *      
 * 2025.11.18 - JLW
 *      Starting to tie things together so that the form and its objects
 *      will work as a unit and not as a bunch of separate objects.
 *      
 *------------------------------------------------------------------------------------------*/
using ZXing.QrCode.Internal;

namespace JAXBase
{
    public class XBase_Class_Visual : XBase_Class
    {

        public XBase_Class_Visual(JAXObjectWrapper jow, string name) : base(jow, name) { }

        /*
         * This handles the most common properties for visual controls.
         * 
         * Return INT result
         *      0   - Successfully proccessed
         *      1   - Was not found - not yet processed
         *      2   - Requires special handling, did not process
         *      3   - Not a class property
         *      9   - Processed and saved, do not do anything else
         *      10  - Processed and saved
         *      >10 - Error code
         *      
         */
        public override int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            int val;

            System.Windows.Forms.Control? MyObj = me.visualObject;

            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;
            App.DebugLog($"MyObj={((MyObj is null) ? "null" : MyObj.Name.ToUpper())} VISUAL.{propertyName}={objtk.AsString()}");

            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                // First, we check to make sure that the property exists
                if (UserProperties.ContainsKey(propertyName))
                {
                    if (UserProperties[propertyName].Protected)
                    {
                        // It's protected - leave it alone
                        result = 1533;
                    }
                    else if (UserProperties[propertyName].SpecialHandling)
                    {
                        // Special handling - pass it back
                        result = 2;
                    }
                    else if (MyObj is null)
                    {
                        // MyObj has to have something in it to
                        // process as a visual object.  This is
                        // most likely an error
                        result = 1901;
                    }
                    else
                    {
                        if (UserProperties[propertyName].ClassProperty)
                        {
                            result = 0;

                            // Visual object common property handler.  Only the common properties
                            // that are registered to the current class will be executed.
                            switch (propertyName)
                            {
                                case "activeform":
                                case "activecontrol":
                                case "controlcount":
                                case "hwnd":
                                case "objects":
                                case "parent":
                                case "parentclass":
                                    result = 1533;
                                    break;

                                case "anchor":
                                    MyObj.Anchor = XClass_AuxCode.IntToAnchor(Convert.ToInt32(objValue));
                                    break;


                                case "autosize":
                                    MyObj.AutoSize = (bool)objValue;
                                    break;

                                case "backcolor":
                                    objValue = JAXUtilities.ReturnColorInt(objValue);
                                    MyObj.BackColor = XClass_AuxCode.IntToColor((int)objValue);
                                    UserProperties["backcolor"].Element.Value = objValue;
                                    result = 9; // do nothing else
                                    break;

                                case "backstyle":
                                    if (Convert.ToInt32(objValue) == 0)
                                        MyObj.BackColor = Color.Transparent;
                                    else
                                    {
                                        objValue = 1D;  // If not zero then it must be 1
                                        MyObj.BackColor = XClass_AuxCode.IntToColor(UserProperties["backcolor"].AsInt());
                                    }
                                    break;

                                case "caption":
                                    if (objtk.Element.Type.Equals("C") == false) result = 11;
                                    MyObj.Text = objtk.AsString();
                                    break;

                                case "closable":
                                    break;

                                case "dock":
                                    if (objtk.Element.Type.Equals("N"))
                                    {
                                        MyObj.Dock = objtk.AsInt() switch
                                        {
                                            1 => DockStyle.Left,
                                            2 => DockStyle.Bottom,
                                            3 => DockStyle.Right,
                                            4 => DockStyle.Fill,
                                            _ => DockStyle.Top
                                        };

                                        objValue = JAXLib.Between(objtk.AsInt(), 0, 4) ? objtk.AsInt() : 0;
                                    }
                                    break;

                                case "enabled":
                                    MyObj.Enabled = (bool)objValue;
                                    break;

                                case "fontbold":
                                case "fontcharset":
                                case "fontitalic":
                                case "fontoutline":
                                case "fontshadow":
                                case "fontstrikethrough":
                                case "fontunderline":
                                    UserProperties[propertyName].Element.Value = (bool)objValue;
                                    XClass_AuxCode.SetFont((IJAXClass)MyObj);
                                    break;

                                case "fontname":
                                    UserProperties[propertyName].Element.Value = objValue.ToString() ?? string.Empty;
                                    XClass_AuxCode.SetFont((IJAXClass)MyObj);
                                    break;

                                case "fontsize":
                                    UserProperties[propertyName].Element.Value = Convert.ToInt32(objValue);
                                    XClass_AuxCode.SetFont((IJAXClass)MyObj);
                                    break;

                                case "forecolor":
                                    objValue = JAXUtilities.ReturnColorInt(objValue);
                                    MyObj.ForeColor = XClass_AuxCode.IntToColor((int)objValue);
                                    UserProperties["forecolor"].Element.Value = objValue;
                                    result = 9; // do nothing else
                                    break;

                                case "height":
                                    MyObj.Height = Convert.ToInt32(objValue);
                                    break;


                                case "left":
                                    MyObj.Left = Convert.ToInt32(objValue);
                                    break;

                                case "lockscreen":
                                    UserProperties[propertyName].Element.Value = (bool)objValue;
                                    if ((bool)objValue)
                                        MyObj.SuspendLayout();
                                    else
                                        MyObj.ResumeLayout();
                                    break;

                                case "maxheight":
                                    val = Convert.ToInt32(objValue);
                                    MyObj.MaximumSize = new System.Drawing.Size(MyObj.MaximumSize.Width, val < 1 ? 0 : val);
                                    break;

                                case "maxwidth":
                                    val = Convert.ToInt32(objValue);
                                    MyObj.MinimumSize = new System.Drawing.Size(val < 1 ? 0 : val, MyObj.MaximumSize.Height);
                                    break;

                                case "minheight":
                                    val = Convert.ToInt32(objValue);
                                    MyObj.MinimumSize = new System.Drawing.Size(MyObj.MinimumSize.Width, val < 1 ? 0 : val);
                                    break;

                                case "minwidth":
                                    val = Convert.ToInt32(objValue);
                                    MyObj.MinimumSize = new System.Drawing.Size(val < 1 ? 0 : val, MyObj.MinimumSize.Height);
                                    break;

                                case "name":
                                    MyObj.Name = objValue.ToString() ?? "X" + App.SystemCounter();
                                    me.SetName(MyObj.Name);
                                    break;

                                case "righttoleft":
                                    MyObj.RightToLeft = (bool)objValue ? System.Windows.Forms.RightToLeft.Yes : System.Windows.Forms.RightToLeft.No;
                                    break;

                                case "tabindex":
                                    MyObj.TabIndex = Convert.ToInt32(objValue);
                                    break;

                                case "tabstop":
                                    MyObj.TabStop = (bool)objValue;
                                    break;

                                case "text":
                                    MyObj.Text = objValue.ToString() ?? string.Empty;
                                    break;

                                case "tooltiptext":
                                    if (objtk.Element.Type.Equals("N"))
                                    {
                                        ToolTip tt = new()
                                        {
                                            AutoPopDelay = 5000,
                                            InitialDelay = 1000,
                                            ReshowDelay = 500,
                                            ShowAlways = true
                                        };

                                        tt.SetToolTip(MyObj, objtk.AsString().Trim());
                                    }
                                    break;

                                case "top":
                                    MyObj.Top = Convert.ToInt32(objValue);
                                    break;

                                case "visible":
                                    MyObj.Visible = (bool)objValue;
                                    break;

                                case "width":
                                    MyObj.Width = Convert.ToInt32(objValue);
                                    break;

                                default:
                                    result = 1;
                                    break;
                            }
                        }
                        else
                            result = 3;
                    }

                    // We don't save what we don't process
                    if (result == 0)
                    {
                        // We processed it, so save the property to the dictionary
                        // Ignore the CA1854 as it won't put the value into the property
                        UserProperties[propertyName].Element.Value = objValue;
                    }
                }
                else
                    result = 1559;
            }

            return result;
        }

        public override int GetProperty(string propertyName, int idx, out JAXObjects.Token resultToken)
        {
            resultToken = new();
            int result = 0;
            System.Windows.Forms.Control? MyObj = me.visualObject;
            propertyName = propertyName.ToLower();

            // First, we check to make sure that the property exists
            if (UserProperties.ContainsKey(propertyName))
            {
                if (UserProperties[propertyName].SpecialHandling)
                {
                    // Special handling - pass it back
                    result = 2;
                }
                else if (MyObj is null)
                {
                    // MyObj has to have something in it to
                    // process as a visual object.  This is
                    // most likely an error
                    result = 1901;
                }
                else
                {
                    if (UserProperties[propertyName.ToLower()].ClassProperty)
                    {
                        // Get the property and fill in the value
                        resultToken.CopyFrom(UserProperties[propertyName]);

                        switch (propertyName.ToLower())
                        {


                            case "anchor":
                                resultToken.Element.Value = XClass_AuxCode.AnchorToInt(MyObj.Anchor);
                                break;

                            case "autosize":
                                resultToken.Element.Value = MyObj.AutoSize;
                                break;

                            case "backcolor":
                                resultToken.Element.Value = XClass_AuxCode.ColorToInt(MyObj.BackColor);
                                break;

                            case "backstyle":
                                resultToken.Element.Value = MyObj.BackColor == Color.Transparent ? 0 : 1;
                                break;

                            case "caption":
                                resultToken.Element.Value = MyObj.Text;
                                break;

                            case "closable":
                                break;

                            case "controlcount":
                                resultToken.Element.Value = UserProperties["controlcount"].AsInt();
                                break;

                            case "dock":
                                resultToken.Element.Value = MyObj.Dock switch
                                {
                                    DockStyle.Left => 1,
                                    DockStyle.Bottom => 2,
                                    DockStyle.Right => 3,
                                    DockStyle.Fill => 4,
                                    _ => 0
                                };
                                break;

                            case "enabled":
                                resultToken.Element.Value = MyObj.Enabled;
                                break;

                            case "fontname":
                                resultToken.Element.Value = MyObj.Font.Name;
                                break;

                            case "fontsize":
                                resultToken.Element.Value = Convert.ToDouble(MyObj.Font.Size);
                                break;

                            case "forecolor":
                                resultToken.Element.Value = XClass_AuxCode.ColorToInt(MyObj.ForeColor);
                                break;

                            case "height":
                                resultToken.Element.Value = MyObj.Height;
                                break;

                            case "hwnd":
                                resultToken.Element.Value = Convert.ToInt32(MyObj.Handle);
                                break;

                            case "left":
                                resultToken.Element.Value = MyObj.Left;
                                break;

                            case "lockscreen":
                                resultToken.Element.Value = UserProperties["lockscreen"].AsBool();
                                break;

                            case "maxheight":
                                resultToken.Element.Value = MyObj.MaximumSize.Height;
                                break;

                            case "maxwidth":
                                resultToken.Element.Value = MyObj.MaximumSize.Width;
                                break;

                            case "minheight":
                                resultToken.Element.Value = MyObj.MinimumSize.Height;
                                break;

                            case "minwidth":
                                resultToken.Element.Value = MyObj.MinimumSize.Width;
                                break;

                            case "name":
                                resultToken.Element.Value = MyObj.Name; break;

                            case "objects":
                                UserProperties["objects"].ElementNumber = idx;
                                resultToken.Element.Value = UserProperties["objects"].Element.Value;
                                break;

                            case "parent":
                                if (Parent is null)
                                    resultToken.Element.MakeNull();
                                else
                                {
                                    resultToken.Element.Value = Parent;
                                }
                                break;

                            case "parentclass":
                                if (Parent is null)
                                    resultToken.Element.Value = string.Empty;
                                else
                                {
                                    if (Parent.GetProperty("class", out JAXObjects.Token tk) == 0)
                                        resultToken.Element.Value = tk.AsString();
                                    else
                                        resultToken.Element.Value = string.Empty;
                                }
                                break;

                            case "righttoleft":
                                resultToken.Element.Value = MyObj.RightToLeft == System.Windows.Forms.RightToLeft.Yes;
                                break;

                            case "tabindex":
                                resultToken.Element.Value = MyObj.TabIndex;
                                break;

                            case "tabstop":
                                resultToken.Element.Value = MyObj.TabStop;
                                break;

                            case "text":
                                resultToken.Element.Value = MyObj.Text;
                                break;

                            case "top":
                                resultToken.Element.Value = MyObj.Top;
                                break;

                            case "width":
                                resultToken.Element.Value = MyObj.Width;
                                break;

                            case "visible":
                                resultToken.Element.Value = MyObj.Visible;
                                break;

                            default:
                                // Not processed
                                result = 1;
                                break;

                        }
                    }
                    else
                    {
                        // Not processed
                        result = 1;
                    }
                }
            }

            if (result > 99)
            {
                _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);

                result = -1;
            }

            return result;
        }
    }
}
