using System.CodeDom;

namespace JAXBase
{
    public class XBase_Class_Visual_Column : XBase_Class_Visual
    {
        public DataGridViewColumn Column => (DataGridViewColumn)me.nvObject!;

        public new string MyDefaultName { get; set; } = "column";

        public XBase_Class_Visual_Column(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(null, "column", string.IsNullOrWhiteSpace(name) ? MyDefaultName : name, false, UserObject.urw);
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = true;

            // ----------------------------------------
            // Final setup of properties and events
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
            int style;

            propertyName = propertyName.ToLower();
            JAXObjects.Token tk = new();
            tk.Element.Value = objValue;


            result = 0;

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                // First, we check to make sure that the property exists
                if (UserProperties.ContainsKey(propertyName))
                {
                    // Visual object common property handler
                    switch (propertyName)
                    {

                        case "caption":
                            if (tk.Element.Type.Equals("C"))
                                Column.HeaderText = tk.AsString().Trim();
                            else
                                result = 11;
                            break;

                        case "backcolor":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0) tk.Element.Value = 0;
                                if (tk.AsInt() > 16777215) tk.Element.Value = 16777215;
                                objValue = tk.AsInt();

                                Column.DefaultCellStyle.BackColor = XClass_AuxCode.IntToColor(tk.AsInt());
                                break;
                            }
                            else
                                result = 11;
                            break;

                        case "fontbold":
                        case "fontcharset":
                        case "fontitalic":
                        case "fontoutline":
                        case "fontshadow":
                        case "fontstrikethrough":
                        case "fontunderline":
                            if (tk.Element.Type.Equals("L"))
                            {
                                UserProperties[propertyName].Element.Value = tk.AsBool();
                                style = (UserProperties["fontbold"].AsBool() ? 1 : 0) + (UserProperties["fontitalic"].AsBool() ? 2 : 0) + (UserProperties["fontunderline"].AsBool() ? 4 : 0) + (UserProperties["fontstrikethrough"].AsBool() ? 8 : 0);
                                Column.DefaultCellStyle.Font = new Font(UserProperties["fontname"].AsString(), UserProperties["fontsize"].AsFloat(), (FontStyle)style);

                            }
                            else
                                result = 11;
                            break;

                        case "fontname":
                            if (tk.Element.Type.Equals("C"))
                            {
                                UserProperties[propertyName].Element.Value = tk.AsString().Trim();
                                objValue = tk.AsString().Trim();

                                style = (UserProperties["fontbold"].AsBool() ? 1 : 0) + (UserProperties["fontitalic"].AsBool() ? 2 : 0) + (UserProperties["fontunderline"].AsBool() ? 4 : 0) + (UserProperties["fontstrikethrough"].AsBool() ? 8 : 0);
                                Column.DefaultCellStyle.Font = new Font(UserProperties["fontname"].AsString(), UserProperties["fontsize"].AsFloat(), (FontStyle)style);
                            }
                            else
                                result = 11;
                            break;

                        case "fontsize":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0) tk.Element.Value = 0;
                                if (tk.AsInt() > 16777215) tk.Element.Value = 16777215;
                                objValue = tk.AsInt();

                                UserProperties[propertyName].Element.Value = tk.AsInt();
                                style = (UserProperties["fontbold"].AsBool() ? 1 : 0) + (UserProperties["fontitalic"].AsBool() ? 2 : 0) + (UserProperties["fontunderline"].AsBool() ? 4 : 0) + (UserProperties["fontstrikethrough"].AsBool() ? 8 : 0);
                                Column.DefaultCellStyle.Font = new Font(UserProperties["fontname"].AsString(), UserProperties["fontsize"].AsFloat(), (FontStyle)style);
                            }
                            else
                                result = 11;
                            break;

                        case "forecolor":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (tk.AsInt() < 0) tk.Element.Value = 0;
                                if (tk.AsInt() > 16777215) tk.Element.Value = 16777215;
                                objValue = tk.AsInt();

                                Column.DefaultCellStyle.ForeColor = XClass_AuxCode.IntToColor(tk.AsInt());
                                break;
                            }
                            else
                                result = 11;
                            break;

                        case "name":
                            if (tk.Element.Type.Equals("C"))
                            {
                                Column.Name = tk.AsString().Trim();
                                objValue = tk.AsString().Trim();
                                me.SetName(tk.AsString().Trim());
                            }
                            else
                                result = 11;
                            break;

                        case "readonly":
                            if (tk.Element.Type.Equals("L"))
                                Column.ReadOnly = tk.AsBool();
                            else
                                result = 11;
                            break;

                        case "recordsource":
                            if (tk.Element.Type.Equals("C"))
                            {
                                Column.DataPropertyName = tk.AsString().Trim();
                                objValue = tk.AsString().Trim();
                            }
                            else
                                result = 11;
                            break;

                        case "value":
                            isProgrammaticChange = true;
                            // TODO
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

            // Column is a special case and won't call Base.GetProperty()

            // First, we double check to make sure that the property exists
            if (UserProperties.ContainsKey(propertyName))
            {
                // Get the property and fill in the value
                returnToken.CopyFrom(UserProperties[propertyName]);

                // Visual object common property handler
                switch (propertyName)
                {

                    case "caption":
                        returnToken.Element.Value = Column.HeaderText;
                        break;

                    case "name":
                        returnToken.Element.Value = Column.Name;
                        break;

                    case "recordsource":
                        returnToken.Element.Value = Column.DataPropertyName;
                        break;

                    case "value":
                        DataGridView grd = (DataGridView)me.Parent!.visualObject!;
                        returnToken.Element.Value = grd.CurrentRow!.Cells[me.Name].Value ?? string.Empty;
                        break;

                    default:
                        returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
                        result = 0;
                        break;
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
         * Methods list
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
                ];
        }


        /*------------------------------------------------------------------------------------------*
         * Events list
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","doubleclick","error","errormessage",
                "mouseenter","mousehover","mouseleave","visiblechanged","when"
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
                "baseclass,C,column","backcolor,r,255|255|255",
                "caption,c,","class,C,column","classlibrary,C,","comment,c,",
                "enabled,l,.t.",
                "FontBold,L,","FontCharSet,N,1","FontCondense,L,","FontItalic,L,false","FontName,C,",
                "FontSize,N,0","FontStrikeThrough,L,","FontUnderline,L,","forecolor,r,0",
                "height,n,32",
                "name,c,",
                "parent,o,","parentclass,c,",
                "readonly,L,","righttoleft,L,", "recordsource,c,",
                "tag,c,","tooltip,c,",
                "value,,","visible,L,.t.",
                "width,n,32"
                ];
        }
    }
}
