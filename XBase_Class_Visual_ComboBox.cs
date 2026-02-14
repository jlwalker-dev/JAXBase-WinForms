/*
 * ComboBox
 * 
 */
namespace JAXBase
{
    public class XBase_Class_Visual_ComboBox : XBase_Class_Visual
    {

        // This list holds the row source array followed by important related values
        private List<ListObjectCollection> ListItemArray = [];
        private int ListCounter = 0;
        private int ListColumns = 1;
        private int BoundColumn = 1;

        List<string> ItemList = [];

        public JAXComboBox CboBox => (JAXComboBox)me.visualObject!;

        public XBase_Class_Visual_ComboBox(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new JAXComboBox(), "Combobox", "combobox", true, UserObject.URW);
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
         * Handle the commmon properties by calling the base and then
         * handle the special cases.
         * 
         * Return result from XBase_Visual_Class
         *      0   - Successfully proccessed
         *      1   - Did not process
         *      2   - Requires special processing
         *      9   - Processed and saved, do not do anything else
         *      10  - Processed and saved
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
            int val, rows, cols;
            JAXObjects.Token tk = new();

            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;

            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                if (UserProperties.ContainsKey(propertyName))
                {
                    switch (propertyName)
                    {
                        case "bordercolor":
                            objValue = JAXUtilities.ReturnColorInt(objValue);
                            CboBox.BorderColor = XClass_AuxCode.IntToColor((int)objValue);
                            break;

                        case "borderwidth":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                if (JAXLib.Between(objtk.AsInt(), 0, 255))
                                {
                                    CboBox.BorderWidth = objtk.AsInt();
                                    objValue = objtk.AsInt();
                                }
                                else
                                    result = 41;
                            }
                            else
                                result = 11;
                            break;

                        // Intercept special handling of properties
                        case "boundcolumn":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                val = objtk.AsInt();
                                if (val < 1 || val > ListColumns) throw new Exception("31|");

                                // Are there rows in the ListItemArray?
                                if (BoundColumn != val && val > 0 && ListItemArray[0].ItemRow.Count >= val)
                                {
                                    // Clear out the items
                                    CboBox.Items.Clear();

                                    // Add the new bound column
                                    for (int r = 0; r < ListItemArray.Count; r++)
                                        CboBox.Items.Add(ListItemArray[r].ItemRow._avalue[val - 1].ValueAsString);

                                    // Set to the bound column to the first list item
                                    CboBox.SelectedIndex = 1;

                                    BoundColumn = val;
                                }
                            }
                            else
                                result = 11;

                            break;

                        case "columncount":
                            try
                            {
                                if (objtk.Element.Type.Equals("N"))
                                {

                                    // Resize the array by copying to a new one
                                    cols = objtk.AsInt();
                                    if (cols < 1)
                                    {
                                        if (cols < 0)
                                            result = 9999;
                                        else
                                        {
                                            ListColumns = 0;
                                            ListItemArray = [];
                                        }
                                    }
                                    else
                                    {
                                        for (int r = 0; r < ListItemArray.Count; r++)
                                        {
                                            tk = ListItemArray[r].ItemRow;
                                            ListItemArray[r].ItemRow = AppHelper.ACopyToNew(tk, 1, cols);
                                        }
                                        ListColumns = cols;
                                    }
                                }
                                else
                                    result = 11;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message + "LISTITEM");
                            }
                            break;

                        case "list":        // by Item index
                        case "listitem":    // by ItemID
                                            // Update the item index TODO - 1 or 2D (row,col)
                            if (objIdx > ItemList.Count + 1) throw new Exception("1231|");
                            if (objIdx < 0) throw new Exception("31|");

                            if (objIdx == 0 || objIdx == ItemList.Count)
                                CboBox.Items.Add(objValue.ToString() ?? string.Empty);              // Add to end
                            else
                                CboBox.Items[objIdx - 1] = objValue.ToString() ?? string.Empty;     // Replace existing

                            // TODO - Fix up the CboBox.Item if it's the bound column

                            break;

                        case "listindex":   // Move to the item index
                            rows = Convert.ToInt32(objValue);
                            if (ListItemArray.Count > 0)
                            {
                                rows = rows > 0 && rows <= ListItemArray.Count ? rows : 1;
                                GetObjectArrayRow(rows);
                                CboBox.SelectedIndex = rows - 1;
                            }
                            break;

                        case "listcount":
                            throw new Exception(string.Format("Property {0} is read only.", propertyName.ToUpper()));

                        case "rowsource":
                            // When assigned, loads the array, if the data source exists
                            string rowsource = objtk.AsString();

                            // Load an array with the load source
                            JAXObjects.Token LoadArray = XClass_AuxCode.GetRowSource(App, rowsource, UserProperties["rowsourcetype"].AsInt());

                            // If we got something back, load it!
                            if (LoadArray.TType.Equals("A"))
                            {
                                // Clear out the ListItemArray object & counter
                                ListItemArray = [];
                                CboBox.Items.Clear();
                                ListCounter = 0;
                                AddObjectArrayRow(LoadArray);
                            }
                            break;

                        default:
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result, 0, 10))
                    {
                        if (result < 9)
                        {
                            // We processed it or just need to save the property (perhaps again)
                            // Ignore the CA1854 as it won't put the value into the property
                            UserProperties[propertyName].Element.Value = objValue;
                        }

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
                    case "boundcolumn":
                        returnToken.Element.Value = BoundColumn;
                        break;

                    case "columncount":
                        returnToken.Element.Value = ListColumns;
                        break;

                    case "list":
                        if (idx > ItemList.Count + 1) throw new Exception("1231|");
                        if (idx < 0) throw new Exception("31|");
                        returnToken.Element.Value = CboBox.Items[idx - 1] ?? string.Empty;
                        break;

                    case "listcount":
                        returnToken.Element.Value = CboBox.Items.Count;
                        break;

                    case "listitem":
                        if (idx < 0) throw new Exception("31|");

                        if (JAXLib.Between(idx, 1, ListItemArray.Count))
                            returnToken.Element.Value = ItemList[idx - 1];   // Return the list item string
                        else
                            returnToken.Element.Value = string.Empty;        // we're very forgiving
                        break;

                    case "listindex":
                        returnToken.Element.Value = CboBox.SelectedIndex + 1;
                        break;

                    case "sorted":
                        returnToken.Element.Value = CboBox.Sorted;
                        break;

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
                    result = 0;

                    // Visual object common property handler
                    switch (propertyName)
                    {
                        default:
                            returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
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


        public override int DoDefault(string methodName)
        {
            int result = 0;

            // Process the object's native method if appropriate
            switch (methodName.ToLower())
            {
                case "additem":
                    if (App.ParameterClassList.Count > 0)
                    {
                        for (int i = 0; i < App.ParameterClassList.Count; i++)
                        {
                            // Can add more than one element to the row
                            // at a time using multiple parameters
                            JAXObjects.Token tk = AppHelper.GetParameterClassToken(App, App.ParameterClassList[i]);
                            AddObjectArrayRow(tk);
                        }
                    }
                    else
                        result = 9999;
                    break;

                default:
                    result = base.DoDefault(methodName);
                    break;
            }

            return result;
        }

        /*------------------------------------------------------------------------------------------*
         * Methods for class
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXMethods()
        {
            return
                [
                "additem","addproperty","clear", "indextoitemid","itemidtoindex","move", "readexpression", "readmethod",
                "refresh", "resettodefault","removeitem","removelistitem","requery","resettodefault","saveasclass", "settooriginalvalue",
                "setfocus", "setviewport", "writeexpression", "writemethod", "zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * Events for class
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","downclick","dropdown","error","gotfocus",
                "init","interactivechange","keypress","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "programmaticchange","rangehigh","rangelow","rightclick","upclick","valid","when"
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
                "alignment,N,0","anchor,N,0",
                "backcolor,R,255|255|255","BaseClass,C!,Combobox","borderwidth,n,0","bordercolor,R,0",
                "Class,C,ComboBox","ClassLibrary,C,","ColumnCount,N,0","columnlines,l,true","columnwidths,c,","Comment,C,","controlsource,c,",
                "displaycount,n,0","displayvalue,c,Combo1",
                "Enabled,L,true",
                "firstelement,n,1","FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false",
                "FontName,C,","FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false",
                "forecolor,R,0","format,c,",
                "Height,N,0",
                "incrementsearch,l,true","inputmask,c,","itemdata,n,0","itemforecolor,R,0",
                "itemiddata,n,0",
                "left,N,0","list,#,","listcount,n,0","listindex,n,0","listitemid,n,0",
                "margin,n,2","maxlength,n,0",
                "name,c,","newindex,n,0","newitemid,n,0","numberofelements,n,0",
                "parent,o,","parentclass,C,","picture,c,","pictureselectiondisplay,n,0",
                "readonly,l,false","righttoleft,L,false","rowsource,c,","rowsourcetype,n,0",
                "selected,l,false","selectedid,l,false","sorted,l,false","style,n,0",
                "tabindex,n,0","tabstop,l,true","tag,c,","text,c,","top,N,0","topindex,n,1","topitemid,n,-1","tooltiptext,c,",
                "value,,","visible,l,true","width,N,100"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * Accept a new item which may be an array.  If it is, fill
         * up the row, if possible.  Extra elements in the newItem
         * array are ignored.
         * 
         * First element is always converted to string since the item
         * displayed and returned is to be a string.
         * 
         * Multiple rows are respected and added
         * 
         *------------------------------------------------------------------------------------------*/
        private int AddObjectArrayRow(JAXObjects.Token newItem)
        {
            // Ignore value, just add a new ListListItemArray to ListItemArray
            int addingRows = newItem.Row;
            int addingCols = newItem.Col;

            // Add what we have to the current list
            for (int r = 0; r < addingRows; r++)
            {
                ListObjectCollection loc = new(ListColumns, ++ListCounter);

                for (int c = 0; c < addingCols; c++)
                {
                    if (c < ListColumns)
                    {
                        int element = r * addingCols + c;
                        loc.ItemRow._avalue[c] = newItem._avalue[element];
                    }
                }

                ListItemArray.Add(loc);
                CboBox.Items.Add(loc.ItemRow._avalue[BoundColumn - 1].ValueAsString);
            }

            UserProperties["newindex"].Element.Value = ListItemArray.Count;
            UserProperties["newitemid"].Element.Value = ListCounter;
            UserProperties["itemiddata"].Element.Value = ListCounter;
            return ListItemArray.Count - 1;
        }

        /*------------------------------------------------------------------------------------------*
         * Insert an item or item array into the ListItemArray at the index.
         * Only the first row is inserted, others are ignored.
         *------------------------------------------------------------------------------------------*/
        private void InsertObjectArrayRowAt(JAXObjects.Token newItem, int moveIDX)
        {
            // Ignore value, just add a new ListListItemArray to ListItemArray
            int addingRows = newItem.Row;
            int addingCols = newItem.Col;

            // Clear out the ListItemArray object & counter
            ListItemArray = [];
            ListCounter = 0;

            // Add what we have
            ListObjectCollection loc = new(ListColumns, ++ListCounter);

            for (int c = 0; c < addingCols; c++)
            {
                if (addingCols < ListColumns)
                    loc.ItemRow._avalue[c] = newItem._avalue[c];
            }

            if (moveIDX >= 0)
            {
                if (moveIDX < ListItemArray.Count)
                    ListItemArray.Insert(moveIDX, loc);
                else
                    ListItemArray.Add(loc);
            }
            else
                throw new Exception("3003|");
        }

        /*------------------------------------------------------------------------------------------*
         * Remove an item from the ListItemArray.
         *------------------------------------------------------------------------------------------*/
        private void RemoveObjectArrayRow(int idx) { ListItemArray.RemoveAt(idx); }

        /*------------------------------------------------------------------------------------------*
         * Move the selected ListItemArray element into the related UserProperties
         * 
         * The idx parameter must be the JAXBase value (1+)
         *------------------------------------------------------------------------------------------*/
        private void GetObjectArrayRow(int idx)
        {
            JAXObjects.Token objectRow = new();
            AppHelper.ASetDimension(objectRow, 1, ListColumns);

            if (ListItemArray.Count > 0)
            {
                if (idx < 1 || idx > ListItemArray.Count)
                    throw new Exception($"1234||Index received was {idx}");

                idx--;

                for (int i = 0; i < ListColumns; i++)
                    objectRow._avalue[i] = ListItemArray[idx].ItemRow._avalue[i];

                UserProperties["list"] = objectRow;
                UserProperties["value"].Element.Value = objectRow._avalue[BoundColumn - 1];
            }
            else
            {
                // Nothing in the item array
                UserProperties["list"] = objectRow;
                UserProperties["value"].Element.Value = string.Empty;
            }
        }
    }
}
