/*--------------------------------------------------------------------------------------------------*
 * 2025-05-06 - JLW
 * 
 * Holds common conversion of xBase to .Net, and back, routines for the XClass 
 * properties such as Color, Font related, and Anchor
 * 
 *--------------------------------------------------------------------------------------------------*/
using static JAXBase.JAXObjectsAux;

namespace JAXBase
{
    public class XClass_AuxCode
    {
        public static List<xParameters> ParameterListFromString(AppClass app, string parameterStr)
        {
            List<xParameters> pList = [];
            parameterStr = parameterStr.Replace("\r", "");
            string[] parameterString = parameterStr.Split("\r");

            foreach (string p in parameterString)
            {
                if (p.Length > 1 && p.Contains('='))
                {
                    int f = p.IndexOf('=');
                    xParameters parm = new()
                    {
                        Name = p[..f],
                        Value = JAXBase_Executer_M.RawMath(app, p[(f + 1)..])
                    };

                    pList.Add(parm);
                }
            }

            return pList;
        }


        /* -----------------------------------------------------------------------------------
         *  Font Controls
         * -----------------------------------------------------------------------------------*/
        public static Font SetFont(IJAXClass myObj)
        {

            /* FontCharSet values (Unsupported in Ver 1)
             *   0 Western
             *   1 Default
             *   2 Symbol
             * 128 Japanese
             * 161 Greek
             * 162 Turkish
             * 163 Vietnamese
             * 177 Hebrew
             * 178 Arabic
             * 186 Baltic
             * 204 Cyrillic
             * 238 Central European
             * 
             */

            /* Supported Font Styles
             * 0 Regular
             * 1 Bold
             * 2 Italics
             * 4 Underline
             * 8 Strikethrough
             */


            int style = (myObj.UserProperties["fontbold"].AsBool() ? 1 : 0) + (myObj.UserProperties["FontItalic"].AsBool() ? 2 : 0) + (myObj.UserProperties["FontUnderline"].AsBool() ? 4 : 0) + (myObj.UserProperties["FontStrikeThrough"].AsBool() ? 8 : 0);
            return new(myObj.UserProperties["FontName"].AsString(), myObj.UserProperties["FontSize"].AsFloat(), (FontStyle)style);
        }



        /* -----------------------------------------------------------------------------------
         * Get VFP Anchor values
         * 
         * ANCHOR values - Anchored to parent edge
         * None   0 The control is not anchored to any edges of its container.
         * Top    1 The control is anchored to the top edge of its container.
         * Bottom 2 The control is anchored to the bottom edge of its container.
         * Left   4 The control is anchored to the left edge of its container.
         * Right  8 The control is anchored to the right edge of its container.
         * 
         * VFP ANCHOR values
         * Position 	    Bit Value   Description
         * Top Absolute     1           Anchors control to top border of container and does not change the distance between the top border.
         * Left Absolute    2           Anchors control to left border of container and does not change the distance between the left border.
         * Bottom Absolute  4           Anchors control to bottom border of container and does not change the distance between the bottom border.
         * Right Absolute   8           Anchors control to right border of container and does not change the distance between the right border.
         * 
         * The following are not supported in Ver 1
         * Top Relative     16          Anchors control to top border of container and maintains relative distance between the top border.
         * Left Relative    32          Anchors control to left border of container and maintains relative distance between the left border.
         * Bottom Relative  64          Anchors control to bottom border of container and maintains relative distance between the bottom border.
         * Right Relative   128         Anchors control to right border of container and maintains relative distance between the right border.
         * Horizontal Fixed 256         Anchors center of control relative to left and right borders but remains fixed in size.
         * Vertical Fixed   512         Anchors center of control relative to top and bottom borders but remains fixed in size.
         * 
         * -----------------------------------------------------------------------------------*/
        public static AnchorStyles IntToAnchor(int anchorValue)
        {
            int styleIndex = 0;

            if ((anchorValue & 1) > 0) styleIndex += 1;
            if ((anchorValue & 2) > 0) styleIndex += 2;
            if ((anchorValue & 4) > 0) styleIndex += 4;
            if ((anchorValue & 8) > 0) styleIndex += 8;

            return (AnchorStyles)styleIndex;
        }

        public static int AnchorToInt(AnchorStyles styleIndex)
        {
            int result = 0;

            if (((int)styleIndex & 1) > 0) result += 1;
            if (((int)styleIndex & 2) > 0) result += 2;
            if (((int)styleIndex & 4) > 0) result += 4;
            if (((int)styleIndex & 8) > 0) result += 8;

            return result;
        }



        /* -----------------------------------------------------------------------------------
         * Color Interface
         * -----------------------------------------------------------------------------------*/

        // Alpha Red Green Blue from a property to an int
        public static int ColorToInt(Color argb)
        {
            return argb.R + argb.G * 256 + argb.B * 65536;
        }

        // Convert Alpha Red Green Blue to int and update properties
        public static Color IntToColor(int color)
        {
            int r = color % 256;
            int g = (color % 65536)/256;
            int b = color / 65536;
            return Color.FromArgb(255, r, g, b);
        }

        /* -----------------------------------------------------------------------------------
         * Used for ResetToDefault process which will reset the Value of
         * the control to the registered default value or an empty value
         * if none is registered.
         * 
         * cmd  Action
         * ---  -------------------------------------------
         * C    Clear the default value to an empty value
         * R    Reset value from the defaultvalue
         * S    Set the defaultvalue from current value
         * 
         * -----------------------------------------------------------------------------------*/
        public static void SetDefault(JAXObjectWrapper me, string cmd)
        {
            JAXObjects.Token tk;

            // Doublecheck that this control has set default capabilities
            if (me.IsMember("setdefault").Equals("P"))
            {
                switch (cmd.ToLower())
                {
                    case "CLEAR":
                    case "C":
                        if (me.GetProperty("value", out tk) == 0)
                            me.SetProperty("defaultvalue", tk.Element.ValueAsEmpty()!);
                        break;

                    case "RESET":
                    case "R":
                        if (me.GetProperty("defaultvalue", out tk) == 0)
                            me.SetProperty("value", tk.Element.Value);
                        break;

                    case "SET":
                    case "S":
                        if (me.GetProperty("value", out tk) == 0)
                            me.SetProperty("defaultvalue", tk.Element.Value);
                        break;
                }
            }
        }


        /* -----------------------------------------------------------------------------------*
         * Reset a property to it's default value or empty value.
         * 
         * Properties set up at init may have default values and that will be used.
         * Protected properties won't change and won't be affected.
         * -----------------------------------------------------------------------------------*/
        public static void ResetPropertyToDefault(JAXObjectWrapper me, string property)
        {
            property = property.ToLower().Trim();

            if (me.IsMember(property).Equals("P"))
            {
                if (me.GetProperty(property, out JAXObjects.Token tk) != 0)
                    throw new Exception($"9999||Failed to reset property {property}");

                if (tk.Element.DefaultValue is not null)
                {
                    // If it has a default value, always drop to here
                    // so that protected properties don't get called 
                    // and blow things up with an error
                    if (tk.Element.HasChanged)
                        tk.Element.SetToDefault();
                }
                else
                {
                    // Arrays and objects are ignored
                    if (tk.TType.Equals("A") == false && tk.Element.Type.Equals("O") == false)
                    {
                        // Set user defined properties to an empty value
                        object v = tk.Element.Type switch
                        {
                            "N" => 0,
                            "C" => string.Empty,
                            "D" => DateOnly.MinValue,
                            "T" => DateTime.MinValue,
                            _ => false
                        };

                        me.SetProperty(property, v);
                    }
                }
            }
            else
                throw new Exception("1559|" + property.ToUpper());
        }

        public static void AddLockedProperty(JAXObjectWrapper me, string propertyName, string lockType, string lockValue)
        {
            propertyName = propertyName.ToLower();
            JAXObjects.Token tk;
            if (string.IsNullOrWhiteSpace(lockType))
            {
                // No locktype indicates it can be anything
                // and therefore we start with an empty string
                tk = new();
                tk.Element.Value = string.Empty;
            }
            else
            {
                // Set the default value of the type-locked property
                tk = new(lockValue, lockType);
                tk.Element.SetDefaultValue(tk.Element.Value, true); // When a property type is locked, the default value is automatically set
            }

            // Add the property without calling the ADDPROPERTY method
            me.AddPropertyDirect(propertyName, tk);
        }

        public static void Method_Addobject(AppClass App, JAXObjectWrapper me)
        {
            // TODO - Only certain classes can have objects added to them
            if (App.ParameterClassList.Count == 1 && App.ParameterClassList[0].token.Element.Type.Equals("O"))
            {
                // JAXBase can accept an object in ADDOBJECT()
                me.AddObject((JAXObjectWrapper)App.ParameterClassList[0].token.Element.Value);
            }
            else
            {
                // we're expecting cName, cClass [,aInit1, aInit2...]
                if (App.ParameterClassList.Count > 1)
                {
                    List<JAXObjects.Token> tkList = [];
                    foreach (ParameterClass p in App.ParameterClassList)
                    {
                        JAXObjects.Token tk = new();
                        tk.CopyFrom(p.token);
                        tkList.Add(tk);
                    }
                    me.AddObjectUsingParameters(tkList);
                }
            }
        }

        /*
         * Create an array object using the rowsource and rowsource type information
         * 
         * Row Source Type
         * nValue   Description  
         * 0        None. (Default) 
         * 1        "Value1,Value2,..."
         * 2        Table alias - Version 0.8
         * 3        SQL statement - Version 1.0
         * 4        Query (.qpr) file - Version 1.0
         * 5        Array
         * 6        Fields - After Version 1.0
         * 7        Files - After Version 1.0
         * 8        Field structure of a table - After Version 1.0
         * 9        JSON string - After Version 1.0
         * 10       Collection object - After Version 1.0
         */
        public static JAXObjects.Token GetRowSource(AppClass app, string rowsource, int rowsourcetype)
        {
            JAXObjects.Token rowInfo = new();

            switch (rowsourcetype)
            {
                case 0: break;      // No row source
                case 1:             // Row source is a a comma delimited string
                    string[] sArray = rowsource.Split(',');
                    AppHelper.ASetDimension(rowInfo, sArray.Length, 1);
                    for (int i = 0; i < sArray.Length; i++)
                        rowInfo._avalue[i].Value = sArray[i];
                    break;

                case 2: break;
                case 3: break;
                case 4: break;

                case 5:             // Row source is an array
                    JAXObjects.Token mArray = app.GetVarToken(rowsource);
                    AppHelper.ASetDimension(rowInfo, mArray.Row, mArray.Col);

                    for (int i = 0; i < mArray._avalue.Count; i++)
                        rowInfo._avalue[i].Value = mArray._avalue[i];
                    break;

                case 6: break;
                case 7: break;
                case 8: break;
                case 9: break;
                case 10: break;

                default:
                    throw new Exception("11|");
            }

            return rowInfo;
        }
    }
}
