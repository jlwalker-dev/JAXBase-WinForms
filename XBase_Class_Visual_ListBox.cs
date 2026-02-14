/*
 * Listbox - Winforms is being replaced so I'm not worrying about making a special class
 * 
 */
using System.ComponentModel;
using System.Windows.Controls;

namespace JAXBase
{
    public class XBase_Class_Visual_ListBox : XBase_Class_Visual
    {
        // Listbox item that holds DisplayValue & id
        private class ListBoxItem
        {
            public int ID = 0;
            private string _displayText = string.Empty;
            public string DisplayText
            {
                get => _displayText;
                set => _displayText = value;
            }

            // Sort-friendly version (used internally by comparer or BindingSource)
            public string SortKey => DisplayText?.ToUpperInvariant() ?? string.Empty;
        }

        // Binding list to tie into listbox
        private BindingList<ListBoxItem> listBoxItems = [];
        private int listID = 1;

        // Used for sorting list
        private BindingSource bindingSource = new();

        private string searchBuffer = "";
        private DateTime lastKeyTime = DateTime.MinValue;


        public System.Windows.Forms.ListBox lstBox => (System.Windows.Forms.ListBox)me.visualObject!;

        public XBase_Class_Visual_ListBox(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new System.Windows.Forms.ListBox(), "ListBox", "list", true, UserObject.URW);
        }

        public new bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------
            if (InInit)
            {
                bindingSource.DataSource = listBoxItems;

                lstBox.DisplayMember = "Name";
                lstBox.ValueMember = "ID";
                lstBox.DataSource = bindingSource;
                //lstBox.DataSource = listBoxItems;

                listBoxItems.AllowNew = false;
                listBoxItems.AllowEdit = false;
                listBoxItems.AllowRemove = false;

                lstBox.KeyPress -= listBox_KeyPress;
                lstBox.KeyPress += listBox_KeyPress;
            }

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
         *      9   - Success, do no further processing
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
            JAXObjects.Token tk = new(objValue);

            if (UserProperties.ContainsKey(propertyName))
            {
                if (UserProperties[propertyName].Protected)
                    result = 3026;
                else
                {
                    switch (propertyName)
                    {
                        case "autohidescrollbar":
                            break;

                        case "bordercolor":
                            break;

                        case "borderwidth":
                            break;

                        case "disabledbackcolor":
                            break;

                        case "disabledforecolor":
                            break;

                        case "disableditembackcolor":
                            break;

                        case "disableditemforecolor":
                            break;

                        case "displayvalue":
                            break;

                        case "firstelement":
                            break;

                        case "incrementsearch":
                            break;

                        case "itemdata":
                            break;

                        case "itemforecolor":
                            break;

                        case "itembackcolor":
                            break;

                        case "itemiddata":
                            break;

                        case "listcount":
                            break;

                        case "listindex":
                            break;

                        case "listitem":
                            break;

                        case "listitemid":
                            break;

                        case "moverbars":
                            break;

                        case "multiselect":
                            if (tk.Element.Type.Equals("L"))
                                lstBox.SelectionMode = tk.AsBool() ? System.Windows.Forms.SelectionMode.MultiExtended : System.Windows.Forms.SelectionMode.One;
                            else
                                result = 11;
                            break;

                        case "newindex":
                            break;

                        case "newitemid":
                            break;

                        case "numberofelements":
                            break;

                        case "rowsource":
                            break;

                        case "rowsourcetype":
                            break;

                        case "selected":
                            break;

                        case "selectedid":
                            break;

                        case "selecteditembackcolor":
                            break;

                        case "selecteditemforecolor":
                            break;

                        case "sorted":
                            if (tk.Element.Type.Equals("L"))
                            {
                                // 0 - no sort
                                // 1 - Ascending case sensitive
                                // 2 - Descending case sensitive
                                // 3 - Ascending case insensitive
                                // 4 - Descending case insensitive
                                int sortType = UserProperties["sorttype"].AsInt();

                                // Sort the list via binding source (wild!)
                                string sortString = sortType == 0 ? string.Empty : sortType < 3 ? "DisplayText " : "SortKey ";
                                sortString += sortType == 0 ? string.Empty : JAXLib.InList(sortType, 1, 3) ? "ASC" : "DES";
                                bindingSource.Sort = sortString;
                            }
                            else
                                result = 11;
                            break;


                        case "sorttype":
                            if (tk.Element.Type.Equals("N"))
                            {
                                if (JAXLib.Between(tk.AsInt(), 0, 4))
                                    objValue = tk.AsInt();
                                else
                                    result = 41;
                            }
                            else
                                result = 11;
                            break;

                        case "terminateread":
                            break;

                        case "topindex":
                            if (tk.Element.Type.Equals("N"))
                                lstBox.TopIndex = tk.AsInt();
                            else
                                result = 11;
                            break;

                        case "topitemid":
                            break;

                        case "value":
                            if (tk.Element.Type.Equals("C"))
                                lstBox.SelectedValue = tk.AsString();
                            else
                                result = 11;
                            break;

                        default:
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            result = result == 0 ? 9 : result;
                            break;
                    }

                    // Was the property retrieved?
                    if (JAXLib.Between(result, 0, 10))
                    {
                        if (result < 9)
                            UserProperties[propertyName].Element.Value = objValue;

                        result = 0;
                    }
                }
            }
            else
                result = 1559;

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

                switch (propertyName)
                {
                    case "autohidescrollbar":
                        break;

                    case "bordercolor":
                        break;

                    case "borderwidth":
                        break;

                    case "disabledbackcolor":
                        break;

                    case "disabledforecolor":
                        break;

                    case "disableditembackcolor":
                        break;

                    case "disableditemforecolor":
                        break;

                    case "displayvalue":
                        break;

                    case "firstelement":
                        break;

                    case "incrementsearch":
                        break;

                    case "itemdata":
                        break;

                    case "itemforecolor":
                        break;

                    case "itembackcolor":
                        break;

                    case "itemiddata":
                        break;

                    case "listcount":
                        break;

                    case "listindex":
                        break;

                    case "listitem":
                        break;

                    case "listitemid":
                        break;

                    case "moverbars":
                        break;

                    case "multiselect":
                        returnToken.Element.Value = lstBox.SelectionMode == System.Windows.Forms.SelectionMode.MultiExtended;
                        break;

                    case "newindex":
                        break;

                    case "newitemid":
                        break;

                    case "numberofelements":
                        break;

                    case "selected":
                        if (lstBox.SelectionMode == System.Windows.Forms.SelectionMode.One)
                        {
                            // Return the selected object
                            returnToken.Element.Value = (string)(lstBox.SelectedItem ?? string.Empty);
                        }
                        else
                        {
                            // Return comma delimited string
                            string temp = string.Empty;
                            foreach (object selectedItem in lstBox.SelectedItems)
                            {
                                string t = (string)selectedItem;
                                temp += t.Trim() + ",";
                            }

                            returnToken.Element.Value = temp.TrimEnd(',');
                        }
                        break;

                    case "selectedid":
                        break;

                    case "selecteditembackcolor":
                        break;

                    case "selecteditemforecolor":
                        break;

                    case "sorted":
                        returnToken.Element.Value = string.IsNullOrWhiteSpace(bindingSource.Sort) == false;
                        break;

                    case "topindex":
                        break;

                    case "topitemid":
                        break;

                    case "value":
                        break;

                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        result = result == 0 ? 9 : result;
                        break;
                }

                if (JAXLib.Between(result, 1, 10))
                {
                    if (result < 9)
                        returnToken.CopyFrom(UserProperties[propertyName]);

                    result = 0;
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
                "additem","addlistitem","addproperty","clear","indextoitemid","move","readexpression","readmethod",
                "refresh","removeitem","removelistitem","requery","resettodefault","saveasclass","setfocus",
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
                "click","dblclick","destroy","error","gotfocus","init","keypress","load","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "rightclick","valid","visiblechanged","when"
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
                "anchor,N,0","autohidescrollbar,n,0",
                "BaseClass,C!,listbox","bordercolor,n,6579300","borderwidth,n,0",
                "Class,C,ComboBox","ClassLibrary,C,","Comment,C,","controlsource,c,",
                "disabledbackcolor,R,255|255|255","disabledforecolor,R,109|109|109","disableditembackcolor,R,255|255|255",
                "disableditemforecolor,R,109|109|109","displayvalue,c,",
                "Enabled,L,true",
                "firstelement,n,1","FontBold,L,false","FontCharSet,N,1","FontCondense,L,false","FontItalic,L,false",
                "FontName,C,","FontSize,N,0","FontStrikeThrough,L,false","FontUnderline,L,false","forecolor,R,0",
                "Height,N,0",
                "incrementsearch,l,true","itemdata,n,0","itemforecolor,R,0","itemiddata,n,0",
                "left,N,0","list,#,","listcount,n,0","listindex,n,0","listitem,c,","listitemid,n,0",
                "moverbars,L,.F.","multiselect,L,.F.",
                "name,c,","newindex,n,0","newitemid,n,0","numberofelements,n,0",
                "parent,o,","parentclass,C,",
                "righttoleft,L,false","rowsource,c,","rowsourcetype,n,0",
                "selected,l,false","selectedid,l,false","selecteditembackcolor,R,0|120|215","selecteditemforecolor,R,255|255|255",
                "sorted,l,false","sorttype,c,",
                "tabindex,n,0","tabstop,l,true","tag,c,","terminateread,l,.F.","top,N,0","topindex,n,1","topitemid,n,-1","tooltiptext,c,",
                "value,,","visible,l,true",
                "width,N,100"
                ];
        }


        /*
         * Support for listbox specific methods and events
         * 
         * additem/addlistitem - add an entry or entries to the listbox
         * removeitem/removelistitem - remove an entry or entries from the listbox
         * 
         */
        public override int DoDefault(string methodName)
        {
            int result = 0;

            try
            {
                switch (methodName.ToLower())
                {
                    case "additem":
                    case "addlistitem":
                        if (App.ParameterClassList.Count != 1)
                        {
                            _AddError(6500, 0, methodName, methodName);
                            result = 6999;
                        }
                        else
                        {
                            JAXObjects.Token tk = App.GetParameterToken(null);
                            if (tk.TType.Equals("A"))
                            {
                                listBoxItems.RaiseListChangedEvents = false;

                                if (tk.Row == 0)
                                {
                                    // 1D array
                                    for (int i = 0; i < tk.Col; i++)
                                    {
                                        ListBoxItem lbi = new();
                                        lbi.DisplayText = tk._avalue[i].ValueAsString;
                                        lbi.ID = listID++;
                                        listBoxItems.Add(lbi);
                                        //lstBox.Items.Add(tk._avalue[i].ValueAsString);
                                    }
                                }
                                else
                                {
                                    // 2D array
                                    for (int i = 0; i < tk.Row; i++)
                                    {
                                        tk.SetElement(i + 1, 1);
                                        ListBoxItem lbi = new();
                                        lbi.DisplayText = tk._avalue[i].ValueAsString;
                                        lbi.ID = listID++;
                                        listBoxItems.Add(lbi);
                                        //lstBox.Items.Add(tk.AsString());
                                    }
                                }

                                listBoxItems.RaiseListChangedEvents = false;
                                listBoxItems.ResetBindings();
                            }
                            else
                                lstBox.Items.Remove(tk.AsString());
                        }
                        break;

                    case "removeitem":
                    case "removelistitem":
                        if (App.ParameterClassList.Count != 1)
                        {
                            _AddError(6500, 0, methodName, methodName);
                            result = 6999;
                        }
                        else
                        {
                            JAXObjects.Token tk = App.GetParameterToken(null);
                            if (tk.TType.Equals("A"))
                            {
                                if (tk.Row == 0)
                                {
                                    // 1D array
                                    for (int i = 0; i < tk.Col; i++)
                                    {
                                        if (tk.Element.Type.Equals("N") && JAXLib.Between(tk.AsInt(), 1, lstBox.Items.Count))
                                            listBoxItems.RemoveAt(tk.AsInt() - 1); //lstBox.Items.RemoveAt(tk.AsInt() - 1);
                                        else
                                        {
                                            // Remove by name - Case Sensitive?
                                            for (int j = 0; j < listBoxItems.Count; j++)
                                            {
                                                if (listBoxItems[j].DisplayText.Equals(tk.AsString()))
                                                {
                                                    listBoxItems.RemoveAt(j);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // 2D array
                                    for (int i = 0; i < tk.Row; i++)
                                    {
                                        tk.SetElement(i + 1, 1);
                                        if (tk.Element.Type.Equals("N") && JAXLib.Between(tk.AsInt(), 1, lstBox.Items.Count))
                                            listBoxItems.RemoveAt(tk.AsInt() - 1);  //lstBox.Items.RemoveAt(tk.AsInt() - 1);
                                        else
                                        {
                                            // Remove by name - Case Sensitive?
                                            for (int j = 0; j < listBoxItems.Count; j++)
                                            {
                                                if (listBoxItems[j].DisplayText.Equals(tk.AsString()))
                                                {
                                                    listBoxItems.RemoveAt(j);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // If a number is sent and it's in the range of the item count
                                // then remove the object at that item count location (1 based)
                                if (tk.Element.Type.Equals("N") && JAXLib.Between(tk.AsInt(), 1, lstBox.Items.Count))
                                    lstBox.Items.RemoveAt(tk.AsInt() - 1);
                                else
                                    lstBox.Items.Remove(tk.AsString());
                            }
                        }
                        break;

                    default:
                        result = base.DoDefault(methodName);
                        break;
                }
            }
            catch (Exception ex)
            {
                App.DebugLog($"Error in ListBox DoDefault {methodName} - {ex.Message}");
                result = 9999;
            }

            return result;
        }


        // Default keypress event
        private void listBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Is there JAXCode to execute first?
            if (string.IsNullOrWhiteSpace(Methods["keypress"].CompiledCode)==false)
            {
                // set up parameters and call the method
            }

            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                return;  // optional: ignore non-alphanum

            e.Handled = true;  // suppress default prefix-jump

            if ((DateTime.Now - lastKeyTime).TotalSeconds > 1.5)
                searchBuffer = "";  // timeout → reset

            if (e.KeyChar == (char)Keys.Back)
            {
                if (searchBuffer.Length > 0)
                    searchBuffer = searchBuffer.Substring(0, searchBuffer.Length - 1);
            }
            else
            {
                searchBuffer += char.ToUpper(e.KeyChar);  // or .ToLower for case-insensitive
            }

            lastKeyTime = DateTime.Now;

            // Find first match (prefix or contains – your choice)
            int foundIndex = lstBox.FindString(searchBuffer);

            if (foundIndex >= 0)
            {
                lstBox.SelectedIndex = foundIndex;
                lstBox.TopIndex = foundIndex; // scroll to make visible
            }
        }
    }
}
