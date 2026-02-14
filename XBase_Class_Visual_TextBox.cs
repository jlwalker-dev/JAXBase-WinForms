/*------------------------------------------------------------------------------------------*
 * Textbox Visual subclass of XBase_Class_Visual which is subclass of XBase_Class
 * 
 * 2025-11-14 - JLW
 *      Basic property and method support.
 *      
 *------------------------------------------------------------------------------------------*/
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace JAXBase
{
    public class XBase_Class_Visual_TextBox : XBase_Class_Visual
    {
        public int MaxLength = 0;

        public TextBox txt => (TextBox)me.visualObject!;

        public XBase_Class_Visual_TextBox(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new TextBox(), "TextBox", "text", true, UserObject.URW);
        }

        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------

            if (InInit)
            {
                txt.TextChanged += Txt_TextChanged;
            }

            return result;
        }

        private void Txt_TextChanged(object? sender, EventArgs e)
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
         *      >10 - Error Code
         *      
         *------------------------------------------------------------------------------------------*/
        public override int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            int val;
            propertyName = propertyName.ToLower();
            JAXObjects.Token tk = new();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
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
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result, 1, 10))
                    {
                        result = 0;

                        // First, we check to make sure that the property exists
                        if (UserProperties.ContainsKey(propertyName))
                        {
                            tk.Element.Value = objValue;

                            // Intercept property handling
                            switch (propertyName.ToLower())
                            {
                                case "maxlength":
                                    if (tk.Element.Type.Equals("N") == false)
                                        throw new Exception("11|");

                                    val = tk.AsInt();
                                    if (val < 0)
                                        result = 9999;
                                    else
                                    {
                                        // Set the maxlength - 0 = no max
                                        MaxLength = val;
                                    }
                                    break;

                                case "readonly":
                                    if (tk.Element.Type.Equals("L") == false)
                                        throw new Exception("11|");

                                    txt.ReadOnly = tk.AsBool();
                                    break;

                                case "value":
                                    if (tk.Element.Type.Equals("C") == false)
                                        throw new Exception("11|");

                                    isProgrammaticChange = true;
                                    txt.Text = tk.AsString();
                                    isProgrammaticChange = false;
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
                            result = 1559;
                    }
                }
                else
                    result = 1559;
            }

            if (result > 10)
            {
                // log the error
                _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|", string.Empty);

                result = -1;
            }
            else
                result = 0; // No error

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
                result = base.GetProperty(propertyName, idx, out returnToken);

                if (JAXLib.Between(result, 1, 10))
                {
                    result = 0;

                    // Post handling of getproperty
                    switch (propertyName)
                    {
                        case "maxlength":
                            returnToken.Element.Value = txt.MaxLength;
                            break;

                        case "readonly":
                            returnToken.Element.Value = txt.ReadOnly;
                            break;

                        case "value":
                            returnToken.Element.Value = txt.Text;
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
         * Methods for class
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
                "addproperty", "move", "readexpression", "readmethod", "refresh", "resettodefault",
                "saveasclass", "settooriginalvalue", "setfocus", "writeexpression", "writemethod", "zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * Events for class
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","error","gotfocus","init","interactivechange","keypress","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "programmaticchange","rangehigh","rangelow","rightclick","valid","visiblechanged","when"
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
                "alignment,n,3","anchor,n,0",
                "backcolor,n,16777215","backstyle,n,1","bordercolor,n,6579300","borderstyle,n,1",
                "baseclass,C,textbox","class,C,textbox",
                "classlibrary,C,","Comment,C,","controlsource,c,",
                "enablehyperlinks,l,false","Enabled,L,true",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false",
                "format,c,","forecolor,R,0","height,n,21","inputmask,c,",
                "left,N,0",
                "margin,n,2","maxlength,n,0","name,c,text1",
                "originalvalue,,",
                "parent,o,","parentclass,C,",
                "passwordchar,c,",
                "readonly,l,false","righttoleft,L,false",
                "sellength,n,0","selstart,n,0","seltext,n,0","selectonentry,l,f","setoriginalwhen,n,0",
                "tabindex,n,1","tabstop,l,true","tag,C,","text,c,","top,N,0","tooltiptext,c,",
                "value,C,","visible,l,true","width,N,0"
                ];
        }
    }
}
