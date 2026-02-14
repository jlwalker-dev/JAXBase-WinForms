using System.CodeDom.Compiler;
using System.Data;
using System.Windows.Forms;
using static JAXBase.AppClass;

namespace JAXBase
{
    internal class XBase_Class_Visual_Grid : XBase_Class_Visual
    {
        public JAXGridView grid => (JAXGridView)me.visualObject!;

        bool doPostInitSetup = true;

        public XBase_Class_Visual_Grid(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            App.DebugLog("Initializing grid", false);
            SetVisualObject(new JAXGridView(), "Grid", "grid", true, UserObject.URW);
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            // ----------------------------------------
            // Set up events
            // ----------------------------------------
            if (doPostInitSetup)
            {
                grid.CellValueChanged += DgvMain_CellValueChanged;
                grid.DataError += DgvMain_DataError;
                grid.CellValidating += DgvMain_CellValidating;
                //grid.RowValidating += DgvMain_BeforeCellChange;
                grid.CurrentCellChanged += DgvMain_AfterCellChanged;
                grid.KeyDown += DgvMain_KeyPress;

                doPostInitSetup = false;

                grid.Refresh();
            }

            bool result = base.PostInit(callBack, parameterList);
            return result;
        }


        /*
         * ADDCOLUMN(x) 
         *      Add a column where x is a numeric value indicating
         *      the type of column to add.  If columncount is 0
         *      then it becomes Column1.  Setting column count
         *      to a higher number will simply add columns after.
         *              
         */
        public override int _CallMethod(string methodName)
        {
            int results = 0;
            string msg = string.Empty;
            methodName = methodName.ToLower();
            App.ReturnValue.Element.Value = true;

            try
            {
                if (Methods.ContainsKey(methodName))
                {
                    string cCode = Methods[methodName].CompiledCode;

                    // Create a new App.Levels and execute the code
                    if (cCode.Length > 0)
                        results = base._CallMethod(methodName);
                    else
                    {
                        switch (methodName)
                        {
                            case "addcolumn":
                                // Should be just one parameter
                                if (App.ParameterClassList.Count > 1)
                                    results = 94;
                                else
                                {
                                    JAXObjects.Token tk = new();
                                    if (App.ParameterClassList.Count == 1)
                                    {
                                        object? obj = App.GetParameterValue(App.ParameterClassList[0]);

                                        if (obj is null)
                                            tk.Element.MakeNull();
                                        else
                                            tk.Element.Value = obj;
                                    }

                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        // 0:Text, 1:Checkbox, 2:Image, 3:ComboBox, 4:Link, 5: Button
                                        // LATER?-> 6: Numeric, 7:DateTime, 8:Date, 9:Masked, 10:Currency
                                        if (JAXLib.Between(tk.AsInt(), 0, 4))
                                        {
                                            AddColumn(tk.AsInt(), string.Empty, string.Empty);
                                        }
                                        else
                                            results = 11;
                                    }
                                }
                                break;

                            default:
                                results = base._CallMethod(methodName);
                                break;
                        }
                    }
                }
                else
                    results = 1559;

            }
            catch (Exception ex)
            {
                results = 9999;
                msg = ex.Message;
            }

            if (results > 0)
            {
                _AddError(results, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(results, $"{results}|{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                App.ReturnValue.Element.Value = false;
                results = -1;
            }

            return results;
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
            propertyName = propertyName.ToLower();
            JAXObjects.Token tk = new();
            tk.Element.Value = objValue;

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

                            if (propertyName.Equals("visible"))
                            {
                                grid.Update();
                                grid.Refresh();
                            }
                            break;
                    }


                    // Do we need to process this property?
                    if (result == 1 || result == 2)
                    {
                        result = 0;

                        // First, we check to make sure that the property exists
                        if (UserProperties.ContainsKey(propertyName))
                        {
                            // Visual object common property handler
                            switch (propertyName)
                            {
                                case "allowaddnew":
                                    if (tk.Element.Type.Equals("L"))
                                        grid.AllowUserToAddRows = tk.AsBool();
                                    else
                                        result = 11;
                                    break;

                                case "allowautocolumnfit":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        switch (tk.AsInt())
                                        {
                                            case 1:
                                                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                                break;

                                            case 2:
                                                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                                                break;

                                            case 3:
                                                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                                                break;

                                            default:
                                                //grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                                                objValue = 0;
                                                break;
                                        }
                                        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "autocellselection":
                                    if (tk.Element.Type.Equals("L"))
                                    {
                                        if (tk.AsBool())
                                        {
                                            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                                            grid.MultiSelect = false;
                                            grid.EditMode = DataGridViewEditMode.EditOnEnter;
                                            grid.CellEnter += grdView_CellEnter;
                                        }
                                        else
                                        {
                                            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                                            grid.EditMode = DataGridViewEditMode.EditOnKeystroke;
                                            grid.CellEnter -= grdView_CellEnter;
                                        }
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "allowheaderresizing":
                                    if (tk.Element.Type.Equals("L"))
                                        grid.AllowUserToResizeColumns = tk.AsBool();
                                    else
                                        result = 11;
                                    break;

                                case "allowrowresizing":
                                    if (tk.Element.Type.Equals("L"))
                                        grid.AllowUserToResizeRows = tk.AsBool();
                                    else
                                        result = 11;
                                    break;

                                case "columncount":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int newCount = tk.AsInt() < 0 ? 0 : tk.AsInt();

                                        if (newCount < grid.ColumnCount)
                                        {
                                            // Subtract from the end
                                            while (newCount < grid.ColumnCount)
                                            {
                                                // Find the reference JAXObjectWrapper
                                                int pos = grid.Columns.Count - 1;
                                                JAXObjectWrapper? col = GetObject(grid.Columns[pos].Name, out int idx);

                                                if (col is null)
                                                    result = 9999;
                                                else
                                                {
                                                    // Remove them both
                                                    RemoveObject(idx);
                                                    grid.Columns.RemoveAt(pos);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Add more if needed
                                            while (newCount > grid.ColumnCount)
                                            {
                                                AddColumn(0, string.Empty, string.Empty);
                                            }
                                        }
                                    }
                                    else
                                        result = 11;

                                    break;

                                case "deletemark":
                                    if (tk.Element.Type.Equals("L"))
                                    {
                                        if (tk.AsBool())
                                        {
                                            if (UserProperties["recordsource"].AsBool() == false)
                                            {
                                                grid.RowHeadersWidth = 10;
                                                grid.RowHeadersVisible = true;
                                            }
                                        }
                                        else
                                        {
                                            if (UserProperties["recordsource"].AsBool() == false)
                                                grid.RowHeadersVisible = false;
                                        }
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "gridlinecolor":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 16843008 ? 16843008 : clr;
                                        objValue = clr;

                                        grid.GridColor = Color.FromArgb(clr);
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "gridlinewidth":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        grid.CellPainting -= dataGridView1_CellPainting;

                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 16 ? 16 : clr;
                                        grid.GridLineWidth = clr;
                                        objValue = clr;

                                        switch (clr)
                                        {
                                            case 0:
                                                grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
                                                break;

                                            case 1:
                                                grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                                                break;

                                            default:
                                                grid.CellPainting += dataGridView1_CellPainting;
                                                break;
                                        }

                                        grid.Refresh();
                                    }
                                    else
                                        result = 11;
                                    break;


                                case "gridlines":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        grid.CellPainting -= dataGridView1_CellPainting;

                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 3 ? 3 : clr;
                                        objValue = clr;

                                        switch (clr)
                                        {
                                            case 0:
                                                grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
                                                break;

                                            case 1:
                                                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                                                break;

                                            case 2:
                                                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
                                                break;

                                            default:
                                                grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                                                break;
                                        }

                                        grid.Refresh();
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "headerheight":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt();
                                        clr = clr < 0 ? 0 : clr;
                                        clr = clr > 256 ? 256 : clr;
                                        objValue = clr;

                                        grid.ColumnHeadersHeight = clr;
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "highlight":
                                    if (tk.Element.Type.Equals("L"))
                                    {
                                        if (tk.AsBool())
                                        {
                                            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(UserProperties["backcolor"].AsInt());
                                            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(UserProperties["forecolor"].AsInt());

                                        }
                                        else
                                        {
                                            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(UserProperties["highlightbackcolor"].AsInt());
                                            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(UserProperties["highlightforecolor"].AsInt());
                                        }
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "hilightbackcolor":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 16843008 ? 16843008 : clr;
                                        objValue = clr;

                                        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(clr);
                                    }
                                    else
                                        result = 11;

                                    break;

                                case "hilightforecolor":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 16843008 ? 16843008 : clr;
                                        objValue = clr;

                                        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(clr);
                                    }
                                    else
                                        result = 11;

                                    break;

                                case "highlightrow":
                                    if (tk.Element.Type.Equals("L"))
                                    {
                                        if (tk.AsBool())
                                            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                                        else
                                            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "recordmark":
                                    if (tk.Element.Type.Equals("L"))
                                    {
                                        if (tk.AsBool())
                                        {
                                            grid.RowHeadersVisible = true;
                                            grid.RowHeadersWidth = 43;
                                        }
                                        else
                                        {
                                            if (UserProperties["deletemark"].AsBool())
                                            {
                                                grid.RowHeadersVisible = true;
                                                grid.RowHeadersWidth = 10;
                                            }
                                            else
                                                grid.RowHeadersVisible = false;

                                        }
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "recordsourcetype":
                                    // If there is a record source, then bind the data
                                    // using the value provided to bind a
                                    // <0 - No record source
                                    // 0 - Table
                                    // 1 - Alias
                                    // 2 - Prompt
                                    // 3 - Qry File
                                    // 4 - SQL Select
                                    // 5 - Array
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        if (JAXLib.Between(tk.AsInt(), 0, 5))
                                        {
                                            if (string.IsNullOrWhiteSpace(UserProperties["recordsource"].AsString()) == false)
                                            {
                                                switch (tk.AsInt())
                                                {
                                                    case 0:
                                                        // Open a table and bind it
                                                        string tableName = UserProperties["recordsource"].AsString().Trim();
                                                        if (tableName.Contains('&'))
                                                        {
                                                            // Macro expansion needed

                                                        }

                                                        if (result == 0 && tableName.Contains("("))
                                                        {
                                                            // It's definitely a function or variable
                                                            // so math the answer
                                                        }

                                                        if (result == 0)
                                                        {
                                                            // Look for the table in the current folder list

                                                            // If not found, then check for it if there is a current database
                                                        }


                                                        if (result == 0 && GridDBF is not null)
                                                        {
                                                            PrepDataGrid();
                                                            LoadDataIntoGrid();
                                                        }
                                                        break;

                                                    case 1:
                                                        // Look for the alias and bind it
                                                        string alias = UserProperties["recordsource"].AsString();

                                                        if (alias.Contains("&"))
                                                        {

                                                        }

                                                        if (alias.Contains('('))
                                                        {
                                                            // Expression, so math it out
                                                        }

                                                        if (result == 0 && string.IsNullOrWhiteSpace(alias))
                                                            result = 11;

                                                        if (result == 0)
                                                        {
                                                            // We may have an alias so try to open it up
                                                            GridDBF = App.CurrentDS.GetWorkAreaObject(alias);

                                                            if (result == 0 && GridDBF is not null)
                                                            {
                                                                PrepDataGrid();
                                                                LoadDataIntoGrid();
                                                            }
                                                            else
                                                                result = 13;
                                                        }
                                                        break;

                                                    case 2:
                                                        // Prompt for a record source (great for a generic browse window)
                                                        break;

                                                    case 3:
                                                        // Load the Query file and execute it then
                                                        // bind the data to the grid
                                                        break;

                                                    case 4:
                                                        // We should have a SQL statement
                                                        string SQLSelect = UserProperties["recordsource"].AsString().Trim();
                                                        if (SQLSelect.Contains('&'))
                                                        {
                                                            // Handle a macro expansion
                                                        }

                                                        if (result == 0 && SQLSelect.Contains(" ") == false)
                                                        {
                                                            // look for a sql statement in a variable
                                                            JAXObjects.Token sql = App.GetVarFromExpression(SQLSelect, null);
                                                            if (sql.Element.Type.Equals("C"))
                                                                SQLSelect = sql.AsString();
                                                            else
                                                                result = 11;
                                                        }

                                                        if (result == 0 && SQLSelect[..7].Equals("SELECT ", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            // Execute the sql select and bind that data
                                                        }
                                                        else
                                                            result = 11;

                                                        break;

                                                    case 5:
                                                        // Try to bind the record source to an array name
                                                        aGridData = App.GetVarToken(UserProperties["recordsource"].AsString());

                                                        // Is it an array variable name?
                                                        if (aGridData.TType.Equals("A") == false)
                                                        {
                                                            // No, so reset and give error
                                                            result = 234;
                                                            aGridData = new();
                                                        }
                                                        else
                                                        {
                                                            // YES! Bind it to the grid!
                                                            LoadArrayIntoGrid();
                                                        }
                                                        break;
                                                }

                                                grid.Refresh();
                                            }
                                        }
                                        else
                                        {
                                            if (tk.AsInt() < 0)
                                            {
                                                ResetGridToBlank();
                                                UserProperties["recordsource"].Element.Value = string.Empty;
                                            }
                                            else
                                                result = 3003;  // Value or index out of range
                                        }
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "rowheight":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt();
                                        clr = clr < 0 ? 0 : clr;
                                        clr = clr > 256 ? 256 : clr;
                                        grid.RowTemplate.Height = clr;

                                        objValue = clr;
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "scrollbars":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int sb = tk.AsInt();
                                        sb = sb < 0 ? 0 : sb;

                                        switch (sb)
                                        {
                                            case 0:
                                                grid.ScrollBars = ScrollBars.None;
                                                break;

                                            case 1:
                                                grid.ScrollBars = ScrollBars.Horizontal;
                                                break;

                                            case 2:
                                                grid.ScrollBars = ScrollBars.Vertical;
                                                break;

                                            default:
                                                sb = 3;
                                                grid.ScrollBars = ScrollBars.Both;
                                                break;

                                        }

                                        objValue = sb;
                                        grid.Refresh();
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "selecteditembackcolor":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 16843008 ? 16843008 : clr;
                                        objValue = clr;

                                        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(clr);
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "selecteditemforecolor":
                                    if (tk.Element.Type.Equals("N"))
                                    {
                                        int clr = tk.AsInt() < 0 ? 0 : tk.AsInt();
                                        clr = clr > 16843008 ? 16843008 : clr;
                                        objValue = clr;

                                        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(clr);
                                    }
                                    else
                                        result = 11;
                                    break;

                                case "value":
                                    isProgrammaticChange = true;
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

                if (JAXLib.Between(result, 1, 10))
                {
                    // Visual object common property handler
                    switch (propertyName)
                    {

                        case "allowaddnew":
                            returnToken.Element.Value = grid.AllowUserToAddRows;
                            break;

                        case "allowheaderresizing":
                            returnToken.Element.Value = grid.AllowUserToResizeColumns;
                            break;

                        case "allowrowresizing":
                            returnToken.Element.Value = grid.AllowUserToResizeRows;
                            break;

                        case "columncount":
                            returnToken.Element.Value = grid.ColumnCount;
                            break;

                        case "value":
                            break;

                        default:
                            returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
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
                "activatecell","addcolumn","addobject","autofit","addproperty","deletecolumn","doscroll",
                "gridhittest","move", "readexpression", "readmethod","refresh","removeobject","resettodefault",
                "saveasclass","setall","setfocus","writeexpression","writemethod","zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "afterrowcolchange","beforerowcolchange","click","dblclick","deleted","destroy","error","errormessage",
                "init","keypress","load","middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove",
                "mouseup","mousewheel","moved","resize","rightclick","scrolled","uienable","valid","visiblechanged"
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
                "activecolumn,n!,0","activerow,n!,0","allowaddnew,l,","allowautocolumnfit,n,0","allowcellselection,l,.T.","allowheadersizing,l,.T.",
                "allowrowsizing,l,.T.","anchor,n,0",
                "backcolor,R,255|255|255","baseclass,c!,grid",
                "caption,c,","class,c!,grid","classlibrary,c!,","columncount,n,0","controlcount,n,0","comment,c,",
                "deletemark,l,",
                "enabled,l,.T.",
                "FontBold,L,false","FontCharSet,N,1","FontCondense,L,false",
                "FontItalic,L,false","FontName,C,","FontSize,N,0",
                "FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,R,0",
                "gridlinecolor,n,0","gridlinewidth,n,1","gridlines,n,3",
                "headerheight,n,19","Height,N,200","highlight,l,.T.","highlightbackcolor,R,0|120|215","highlightforecolor,R,255|255|255",
                "highlightrow,l,.T.",
                "lastcol,n!,-1","lastrow,n!,-1","left,n,0","leftcolumn,n,1",
                "mousepointer,n,0",
                "name,c,grid",
                "objects,*,",
                "parent,o!,","parentclass,c!,","partition,n,0",
                "readonly,l,","recordmark,l,.T.","recordsource,c,","recordsourcetype,n,1","righttoleft,l,","rowcolchange,n!,0","rowheight,n,18",
                "scrollbars,n,3","selecteditembackcolor,R,0|120|215","selecteditemforecolor,R,255|255|255","splitbar,l,.T.",
                "tabindex,n,1","tabstop,l,.T.","tag,c,","top,n,0","tooltiptext,c,",
                "value,,","view,n,0","visible,l,.T.",
                "width,n,200"
                ];
        }


        /*------------------------------------------------------------------------------------------*
         * SHOW has an override which selects the table in the current
         * work area (if one exists) if the recordsourcetype is set to -1.
         *------------------------------------------------------------------------------------------*/
        public override int DoDefault(string methodName)
        {
            int results = 0;
            string msg = string.Empty;
            methodName = methodName.ToLower();

            try
            {
                if (Methods.ContainsKey(methodName))
                {
                    string cCode = Methods[methodName].CompiledCode;

                    // Create a new App.Levels and execute the code
                    if (cCode.Length > 0)
                        results = base._CallMethod(methodName);
                    else
                    {
                        switch (methodName)
                        {
                            case "autofit":
                                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                                if (grid.Columns.Count > 0)
                                {
                                    // First make all columns size to content
                                    foreach (DataGridViewColumn col in grid.Columns)
                                        col.MinimumWidth = 35;

                                    // Then override the last column (or whichever you prefer) to fill
                                    grid.Columns[grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                }
                                break;

                            default:
                                results = base.DoDefault(methodName);
                                break;
                        }
                    }
                }
                else
                    results = 1559;

            }
            catch (Exception ex)
            {
                results = 9999;
                msg = ex.Message;
            }

            if (results > 0)
            {
                _AddError(results, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(results, $"{results}|{msg}", string.Empty);

                results = -1;
            }

            return results;
        }


        /*------------------------------------------------------------------------------------------*
         * .NET interfacing code for grids
         *------------------------------------------------------------------------------------------*/

        // Add a column to the grid and return the position
        // in the object array.  NOTE: That position is only
        // good as long as nothing is removed from the
        // array, so consider it to be valid immedialy after
        // the AddColumn method executes and any other time
        // use GetObject(string objectname, out int idx) 
        private int AddColumn(int type, string caption, string fieldname)
        {
            // Create and add the new column
            DataGridViewColumn column = type switch
            {
                1 => new DataGridViewCheckBoxColumn(),
                2 => new DataGridViewComboBoxColumn(),
                3 => new DataGridViewButtonColumn(),
                4 => new DataGridViewImageColumn(),
                5 => new DataGridViewLinkColumn(),
                _ => new DataGridViewTextBoxColumn()
            };

            column.Name = $"Column{grid.ColumnCount + 1}";
            column.HeaderText = caption;
            column.Tag = fieldname;
            grid.Columns.Add(column);

            JAXObjectWrapper col = new(App, "column", column.Name, []) { nvObject = grid.Columns[^1] };

            me.GetProperty("controlcount", 0, out JAXObjects.Token tk);
            col.thisObject!.SetObjectIDX(tk.AsInt());
            bool result = base.PostInit(me, []);

            // Non-visual objects may not be able to be assigned
            // a parent, or name during PostInit so we assign
            // them manually, if appropriate
            col.SetName(column.Name);
            col.SetParent(me);


            // Now add it to the grid's objects array
            AddObject(col);

            // And return the current index position
            return tk.AsInt();
        }

        public void grdView_CellEnter(object? sender, DataGridViewCellEventArgs e)
        {
            // Check if the current cell is a TextBox cell
            if (grid.CurrentCell is DataGridViewTextBoxCell cell)
            {
                grid.BeginEdit(false);
                // Select all text in the editing control
                if (grid.EditingControl is System.Windows.Forms.TextBox editingControl)
                {
                    editingControl.SelectAll();
                }
            }
        }

        private void dataGridView1_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e is not null)
            {
                // Let the system draw background, content, selection, etc.
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)   // data cells only
                {
                    int thickness = grid.GridLineWidth;
                    Color lineColor = Color.Black;

                    using (Pen pen = new Pen(lineColor, thickness))
                    {
                        // Bottom horizontal line
                        e.Graphics!.DrawLine(pen,
                            e.CellBounds.Left,
                            e.CellBounds.Bottom - thickness / 2,
                            e.CellBounds.Right,
                            e.CellBounds.Bottom - thickness / 2);

                        // Right vertical line
                        e.Graphics.DrawLine(pen,
                            e.CellBounds.Right - thickness / 2,
                            e.CellBounds.Top,
                            e.CellBounds.Right - thickness / 2,
                            e.CellBounds.Bottom);
                    }
                }

                e.Handled = true;
            }
        }


        // ────────────────────────────────────────────────────────────────
        // Start of Array binding
        // ────────────────────────────────────────────────────────────────

        // Reference the array in UserProperties
        public JAXObjects.Token aGridData = new();

        public void ResetGridToBlank()
        {
            //App.DebugLog("ResetGridToBlank", false);
            grid.Rows.Clear();
        }


        // Load the array into the grid
        private void LoadArrayIntoGrid()
        {
            //App.DebugLog("LoadArrayIntoGrid", false);
            grid.Rows.Clear();

            // Get the number of columns for the grid
            // If less than 1, use the array column count
            int acc = aGridData.Col;
            int arc = aGridData.Row;

            // Fix for 1D array?
            if (arc < 1)
            {
                arc = acc;
                acc = 1;
            }

            // Make sure columncount gets set if < 1
            int col = UserProperties["columncount"].AsInt();
            if (col < 1)
            {
                UserProperties["columncount"].Element.Value = acc;
                col = acc;
            }

            grid.Rows.Add(aGridData.Row); // pre-create rows

            // Row fix in FOR statement for 1D arrays
            for (int r = 1; r <= arc; r++)
            {
                var row = grid.Rows[r - 1];

                // Optional: store the row index in Tag so we know which Token row to update
                row.Tag = r;

                // Fill in the columns of the row
                for (int c = 1; c <= col; c++)
                {
                    if (c <= acc)
                    {
                        // In range of the grid columns
                        aGridData.SetElement(r, c);
                        row.Cells[c - 1].Value = aGridData.Element.Value;
                    }
                    else
                        row.Cells[c - 1].Value = string.Empty;  // Outside array so make blank
                }
            }

            grid.AutoResizeColumns();
        }


        /*
         * Start of event handling
         */
        private void DgvMain_KeyPress(object? sender, KeyEventArgs e)
        {
            //App.DebugLog("Grid Keypress", false);

            // VFP nKeyCode translation
            ParameterClass nKeyCode = new();
            if (App.OS == OSType.Windows)
            {
                nKeyCode.token.Element.Value = JAXLib.FormsVFPKeyPress(e.KeyCode.ToString(), e.KeyValue);
                if (nKeyCode.token.AsInt() > 200) return;   // Don't want modifier keys here
            }
            else
            {
                // TODO - Linux translation?
            }

            // Key modifiers converted for VFP
            int keymods = e.Modifiers == Keys.Shift ? 1 : 0;
            keymods += e.Modifiers == Keys.Control ? 2 : 0;
            keymods += e.Modifiers == Keys.Alt ? 4 : 0;
            ParameterClass nShiftAltCtrl = new();
            nShiftAltCtrl.token.Element.Value = keymods;

            App.ParameterClassList.Add(nKeyCode);
            App.ParameterClassList.Add(nShiftAltCtrl);

            _CallMethod("keypress");
        }

        private void DgvMain_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            //App.DebugLog("CellValidating", false);

            ParameterClass colIndex = new();
            colIndex.token.Element.Value = 0;

            ParameterClass rowIndex = new();
            rowIndex.token.Element.Value = 0;

            ParameterClass cellValue = new();

            if (grid.CurrentCell != null)
            {
                colIndex.token.Element.Value = grid.CurrentCell.ColumnIndex + 1;
                rowIndex.token.Element.Value = grid.CurrentCell.RowIndex + 1;

                if (grid.CurrentCell.Value is not null)
                    cellValue.token.Element.Value = grid.CurrentCell.Value;
                else
                    cellValue.token.Element.MakeNull();
            }

            App.ParameterClassList.Add(rowIndex);
            App.ParameterClassList.Add(colIndex);
            App.ParameterClassList.Add(cellValue);

            _CallMethod("valid");

            // Return value handling
            if (App.ReturnValue.Element.Type.Equals("L"))
            {
                // .T. or .F. handling
                if (App.ReturnValue.AsBool() == false)
                {
                    e.Cancel = true;
                    _CallMethod("ErrorMessage");
                }
            }
            else if (App.ReturnValue.Element.Type.Equals("N"))
            {
                // Forward/backward movement
                int newCol = colIndex.token.AsInt() + App.ReturnValue.AsInt();

                switch (App.ReturnValue.AsInt().CompareTo(0))
                {
                    case -1:
                        if (newCol < 0)
                            grid.CurrentCell = grid.Rows[rowIndex.token.AsInt()].Cells[0];
                        else
                            grid.CurrentCell = grid.Rows[rowIndex.token.AsInt()].Cells[newCol];
                        break;

                    case 0:
                        grid.CurrentCell = grid.Rows[rowIndex.token.AsInt()].Cells[newCol];
                        CallColumnMethod(rowIndex.token.AsInt(), "errormessage");
                        break;

                    case 1:
                        if (newCol < grid.ColumnCount)
                            grid.CurrentCell = grid.Rows[rowIndex.token.AsInt()].Cells[newCol];
                        else
                            grid.CurrentCell = grid.Rows[rowIndex.token.AsInt()].Cells[grid.ColumnCount - 1];
                        break;
                }
            }
        }

        private void CallColumnMethod(int RowIndex, string methodName)
        {
            int colidx = 0;
            for (int i = 0; i < UserProperties["controlcount"].AsInt(); i++)
            {
                JAXObjectWrapper jow = GetObject(i);
                if (jow.Class.Equals("column"))
                {
                    if (colidx == RowIndex)
                    {
                        // Call the errormessage event for the column
                        jow.MethodCall(methodName);
                        break;
                    }
                    colidx++;
                }
            }

        }

        private void DgvMain_BeforeCellChange(object? sender, EventArgs e)
        {
            //App.DebugLog("BeforeCellChanged", false);

            ParameterClass colIndex = new();
            colIndex.token.Element.Value = 0;

            ParameterClass rowIndex = new();
            rowIndex.token.Element.Value = 0;

            ParameterClass cellValue = new();

            if (grid.CurrentCell != null)
            {
                colIndex.token.Element.Value = grid.CurrentCell.ColumnIndex;
                rowIndex.token.Element.Value = grid.CurrentCell.RowIndex;

                if (grid.CurrentCell.Value is not null)
                    cellValue.token.Element.Value = grid.CurrentCell.Value;
                else
                    cellValue.token.Element.MakeNull();
            }

            App.ParameterClassList.Add(rowIndex);
            App.ParameterClassList.Add(colIndex);
            App.ParameterClassList.Add(cellValue);

            _CallMethod("beforerowcolchange");
        }

        // ────────────────────────────────────────────────
        //  Emulate the AfterRowColChange event
        // ────────────────────────────────────────────────
        private void DgvMain_AfterCellChanged(object? sender, EventArgs e)
        {
            //App.DebugLog("AfterCellChanged", false);

            if (grid.CurrentCell is null)
            {
                //App.DebugLog("Null skips AfterRowColChange logic", false);
            }
            else
            {
                App.ParameterClassList.Clear();
                AppHelper.LoadTokenValToParameters(App, new(grid.CurrentCell.ColumnIndex + 1));
                AppHelper.LoadTokenValToParameters(App, new(grid.CurrentCell.RowIndex + 1));
                AppHelper.LoadTokenValToParameters(App, new(grid.CurrentCell.Value));

                //App.DebugLog("Calling AfterRowColChange logic", false);
                _CallMethod("afterrowcolchange");

                UserProperties["lastcol"].Element.Value = grid.CurrentCell.ColumnIndex + 1;
                UserProperties["lastrow"].Element.Value = grid.CurrentCell.RowIndex + 1;
            }
        }

        /*
         // This fires when current record changes, regardless of grid selection quirks
         this.bindingSource1.CurrentChanged += (s, e) =>
         {
            if (bindingSource1.Current is YourClass rec)
            {
                // Perfect VFP-like behavior
                this.Text = $"Record {bindingSource1.Position + 1} of {bindingSource1.Count}";
            }
        };
        */

        // ────────────────────────────────────────────────
        //  When user edits cell and need to write
        //  back to Token
        // ────────────────────────────────────────────────
        private void DgvMain_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            //App.DebugLog("CellValueChanged", false);

            // a little error correction
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            int col = e.ColumnIndex;
            int row = e.RowIndex;

            var gridrow = grid.Rows[e.RowIndex];
            if (gridrow.Tag == null) return;

            if (grid.Columns[col].ReadOnly == false)
            {
                //App.DebugLog("Cell change logic", false);
                int tokenRow = (int)gridrow.Tag;

                string cellString = aGridData.Element.Type switch
                {
                    "L" => (gridrow.Cells[col].Value?.ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase) ? ".T." : ".F.",
                    "N" => gridrow.Cells[col].Value?.ToString() ?? "",
                    _ => gridrow.Cells[col].Value?.ToString() ?? ""
                };

                // ── Write back to your Token array ─────────────
                aGridData.SetElement(tokenRow, col + 1);

                // Attempt to revert back to the correct type
                try
                {
                    JAXObjects.Token newValue = new();
                    switch (aGridData.Element.Type)
                    {
                        case "N":
                            if (double.TryParse(cellString, out double db) == false)
                                throw new Exception("Invalid numeric value");
                            aGridData.Element.Value = db;
                            break;

                        case "C":
                            aGridData.Element.Value = cellString;
                            break;

                        case "L":
                            if ("TYFN".Contains(cellString.Replace(".", "").Replace(" ", "").ToUpper()[0]))
                                aGridData.Element.Value = cellString.Equals(".T.");
                            else
                                throw new Exception("Only .T. or .F. / Y or N allowed");

                            break;
                    }

                    aGridData.Element.Value = newValue;   // let Token do its own type coercion
                }
                catch (Exception ex)
                {
                    //App.DebugLog($"DgvMain_CellValueChanged - error {ex.Message}");

                    // Very defensive — revert + show error
                    if (grid.CurrentCell is null)
                        _CallMethod("errormessage");    // Grid error
                    else
                        CallColumnMethod(grid.CurrentCell.RowIndex, "errormessage");    // Column Error

                    // Revert cell to previous value (simplest recovery)
                    grid.CellValueChanged -= DgvMain_CellValueChanged; // prevent recursion
                    gridrow.Cells[1].Value = aGridData.AsString() ?? "";
                    grid.CellValueChanged += DgvMain_CellValueChanged;
                }
            }
        }


        // Catch any formatting/conversion errors raised by DataGridView
        private void DgvMain_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            //App.DebugLog("DataError", false);
            e.ThrowException = false;

            if (grid.CurrentCell is null)
                _CallMethod("errormessage");    // Grid error
            else
                CallColumnMethod(grid.CurrentCell.RowIndex, "errormessage");    // Column Error
        }

        // Optional: refresh display after external changes to aGridData
        public void RefreshGrid()
        {
            //App.DebugLog("Refresh grid", false);
            LoadArrayIntoGrid();
        }




        /***************************************************************
         * Start of Lazy grid data binding - WOOHOO!
         ***************************************************************/
        JAXDirectDBF? GridDBF = null;

        private void PrepDataGrid()
        {
            App.DebugLog("LoadDataIntoGrid", false);
            grid.Rows.Clear();
            SetProperty("columncount", 0, 0);

            if (GridDBF is not null)
            {
                // We have an active work area
                if (GridDBF.DbfInfo.DBFStream is not null)
                {
                    // We have an active table!
                    grid.VirtualMode = false;
                    grid.ReadOnly = true; // For viewing only
                    grid.AllowUserToAddRows = false; // No new row at the end

                    // Add columns based on DBF schema (from empty DBFRow)
                    // Fetch an empty row to get the structure
                    GridDBF.DBFGotoRecord("TOP", out DataTable JBrow);
                    foreach (DataColumn col in JBrow.Columns)
                    {
                        if (col.ColumnName[..1] != "$") // Skip the deleted flag column if you don't want to show it
                        {
                            // Set the column and return the object location
                            int c = AddColumn(0, col.ColumnName, col.ColumnName);
                        }
                    }

                }
            }
        }


        /*
         * The grid is expected to be set up and configured 
         * before this routine is called.  All rows are then
         * removed and filled back in with current data.
         */
        private int LoadDataIntoGrid()
        {
            int results = 0;

            App.DebugLog("LoadDataIntoGrid", false);

            if (GridDBF is not null && GridDBF.DbfInfo.DBFStream is not null)
            {
                grid.Rows.Clear();

                GridDBF.DBFGotoRecord("top", out DataTable JBrow);

                // Add the rows here because if you do it before the
                // columns, it automatically inserts a blank column
                grid.RowCount = GridDBF.DbfInfo.RecCount; // Total rows to display (grid is 0-based)
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                // Load all of the data
                for (int r = 1; r <= GridDBF.DbfInfo.RecCount; r++)
                {
                    for (int c = 1; c <= grid!.Columns.Count; c++)
                    {
                        if (grid is not null)
                        {
                            // Get the field name in case things don't line up
                            Object? f = grid.Columns[c - 1].Tag;

                            if (f is not null)
                            {
                                string field = (string)f;

                                if (string.IsNullOrWhiteSpace(field) == false)
                                {
                                    if (GridDBF.FieldExists(field))
                                        grid.Rows[r - 1].Cells[c - 1].Value = JBrow.Rows[0][field].ToString();
                                    else
                                        throw new Exception($"4012|{field}");
                                }
                            }
                            else
                                throw new Exception($"9762|{c}");
                        }
                    }

                    GridDBF.DBFSkipRecord(1, out JBrow);
                }


                // refresh the grid
                if (grid is not null)
                {
                    grid.ScrollBars = ScrollBars.Both;
                    grid.Update();
                    grid.Refresh();
                }
            }
            else
                throw new Exception("52|");

            return results;
        }
    }
}
