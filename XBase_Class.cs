/*------------------------------------------------------------------------------------------*
 * BASE CLASS FOR ALL JAX CLASSES
 * 
 * 2025-11-02 - JLW
 *      OK, I'm late to the game.  Remember, I'm a VFP developer and not
 *      a C#.Net developer.  I tried to figure out how to propely subclass
 *      in C# and got frustrated.  There are some basic differences between
 *      the two that caused me problems.  I got frustrated because I was
 *      in a rapid development mindset, so I just wrote an interface and
 *      coded out each class as a full implementation. By the time I got
 *      through most of the visual classes, I realized I had to stop
 *      and see if I could figure this out.
 * 
 *      I finially "got it" when I asked for subclassing information from 
 *      GROK and spent some time asking questions.  I hate most AIs but
 *      I'll be damned if GROK isn't helpful.  So now I'm rewriting the
 *      whole class structure upon a single _BASE class and when I'm done,
 *      each class will have pretty much just anything that deviate from 
 *      the norm.
 * 
 *      I'm not going to call Version 0.5 done until I've gotten this down
 *      cold.  Hopefully by January 2026 I will be able to start work
 *      on delivering Version 0.6.
 * 
 * 2025-11-19 - JLW
 *      One last "major" change is that originally, most classes would 
 *      return zero upon success a positive number if there was an error.
 *      
 *      Of course, that runs counter to what the SQL class would do which
 *      is -1 if an error and 0 or positive if everything is ok.  I've
 *      decided that it's best to make things consistent across the board
 *      so -1 is the flag for an error, and you just need to look at
 *      the error array to figure out what is wrong.  Zero or positive
 *      number means everything successfully executed.
 * 
 * 
 * Version 0.8 is going to introduce Avalonia so that I can move it to Linux
 * 
 * Minimal / Recommended Starting Set (Desktop-only apps)
 * These cover most common WinForms controls (Button, Label/TextBlock, 
 *      TextBox, CheckBox, RadioButton, ComboBox, ListBox, DataGrid, 
 *      Panel/Grid/StackPanel, Menu, etc.):
 *  
 *  Package ID              Purpose / What it Replaces      When to install
 *  ----------------------- ------------------------------- ------------------------
 *  Avalonia                Core framework                  Always required
 *  Avalonia.DesktopDesktop platforms (Win/Mac/Linux)       Required
 *  Avalonia.Themes.Fluent  Modern Fluent Design look       Almost always       
 *  Avalonia.Fonts.Inter    Clean, modern look              Recommended
 *  
 *  Extra / Specialized Packages (for specific WinForms controls or features)
 *  
 *  WinForms                Avalonia Equivalent NuGet Package
 *  DataGridView            DataGrid            Avalonia.Controls.DataGrid 
 *  TreeView                Built-in TreeView   Included in Avalonia
 *  ListView / Columns      ListBox + custom    Included or Datagrid
 *                          columns or DataGrid 
 *  TabControl              TabControl
 *  MenuStrip/              MenuFlyout, 
 *    ContextMenuStrip      NativeMenu, or Menu
 *    
 *  ToolStrip/ToolBar       Menu + Button       FluentAvalonia
 *                          / ToggleButton or 
 *                          third-party or 
 *                          look at Actipro
 *                          
 *  MessageBox              MessageBox          MessageBox.Avalonia 
 *  
 *  
 *  -------------------------
 *  Other future needs
 *  -------------------------
 *  Advanced docking        Third-party     Actipro Docking, DockPanel Suite for Avalonia
 *  Charts / Gauges         Third-party     LiveCharts2.Avalonia or ScottPlot.Avalonia
 *------------------------------------------------------------------------------------------*/
using System.ComponentModel;
using static JAXBase.JAXObjectsAux;

namespace JAXBase
{
    public class XBase_Class : IJAXClass
    {
        // Control flag for the object array indicating if
        // the JAXCode can work with the array.  Caps mean
        // the code is allowed to Read, Write, or Update.
        public enum UserObject { urw, Urw, uRw, URw, UrW, URW }

        public AppClass App;
        public JAXObjectWrapper me;
        public JAXObjectWrapper? Parent = null;
        public Dictionary<string, JAXObjects.Token> UserProperties { get; private set; } = [];
        public Dictionary<string, MethodClass> Methods { get; private set; } = [];

        public int MyIDX = -1;
        readonly private int nextMove = 0;
        public int GetNextMove() { return nextMove; }

        public int SetObjectIDX(int idx)
        {
            MyIDX = idx;
            return 0;
        }

        public int GetObjectIDX() { return MyIDX; }

        public bool CanUseObjects = false;          // Can external code use the objects array
        public bool CanReadObjects = false;         // Can external code read the objects array
        public bool CanWriteObjects = false;        // Can external code write the objects array

        public bool InInit = true;
        public bool isProgrammaticChange = false;

        public virtual bool VisualClass { get; set; } = false;
        public virtual string MyDefaultName { get; set; } = string.Empty;
        public virtual string MyBaseClass { get; set; } = string.Empty;

        public XBase_Class(JAXObjectWrapper jow, string name)
        {
            App = jow.App;
            me = jow;
            me.BaseClass = string.Empty;
            MyDefaultName = string.Empty;
            MyBaseClass = string.Empty;
        }

        /*------------------------------------------------------------------------------------------*
         * Set up if a visual object and the user object access settings
         *------------------------------------------------------------------------------------------*/
        protected virtual void SetVisualObject(Control? MyObj = null, string myBaseClass = "", string MyDefaultName = "", bool VisualClass = false, UserObject uobj = UserObject.urw)
        {
            me.VisualClass = VisualClass;
            me.BaseClass = myBaseClass;

            if (VisualClass && MyObj is not null)
                me.visualObject = MyObj;

            int userObject = (int)uobj;

            CanUseObjects = userObject > 0;
            CanReadObjects = JAXLib.InList(userObject, 2, 3, 5);
            CanWriteObjects = userObject > 3;

            // Visual class common interfaces
            if (VisualClass && MyObj is not null && MyBaseClass.Equals("menu") == false)
            {
                string[] jaxevents = JAXEvents();

                // Keyboard/mouse events
                MyObj.Click += MyObj_Click;
                MyObj.DoubleClick += MyObj_DoubleClick;
                MyObj.KeyPress += MyObj_KeyPress;

                // Mouse events
                MyObj.MouseDown += MyObj_MouseDown;
                MyObj.MouseEnter += MyObj_MouseEnter;
                MyObj.MouseHover += MyObj_MouseHover;
                MyObj.MouseLeave += MyObj_MouseLeave;
                //MyObj.MouseMove += MyObj_MouseMove;
                MyObj.MouseUp += MyObj_MouseUp;
                MyObj.MouseWheel += MyObj_MouseWheel;

                // Move related events
                MyObj.Enter += MyObj_GotFocus;
                MyObj.Leave += MyObj_LostFocus;
                //MyObj.Move += MyObj_Move;

                // Data events
                if (Array.IndexOf(jaxevents, "valid") >= 0)
                    MyObj.Validating += MyObj_Validating;
            }
        }



        /*------------------------------------------------------------------------------------------*
         * Post init setting up the Parent object and parent class
         *------------------------------------------------------------------------------------------*/
        public virtual bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            // We don't need this code
            Parent = callBack;
            if (Parent is not null)
            {
                // TODO? - switch to AddError and result=false
                if (me.BaseClass.Equals("optionbutton", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("optiongroup") == false)
                    throw new Exception($"3300|{me.Class}/{Parent.BaseClass}");

                if (me.BaseClass.Equals("menuitem", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("menu") == false)
                    throw new Exception($"3300|{me.Class}/{Parent.BaseClass}");

                if (me.BaseClass.Equals("toolbutton", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("toolbar") == false)
                    throw new Exception($"3300|{me.Class}/{Parent.BaseClass}");

                if (me.BaseClass.Equals("form", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("formset") == false)
                    throw new Exception($"3300|{me.Class}/{Parent.BaseClass}");

                if (VisualClass && Parent.VisualClass == false)
                    throw new Exception($"3301|{me.Class}/{Parent.BaseClass}");

                if (Parent.GetProperty("name", out JAXObjects.Token tk) >= 0)
                {
                    UserProperties["parentclass"].Element.Value = tk.AsString();
                    App.DebugLog($"Setting parent of {me.Name} to {Parent.Name}", false);
                }
            }

            // Update the properties of this object
            // Remember to watch out for triggered properties that
            // may need to be run after all of the other properties
            // have been processed
            foreach (ParameterClass p in parameterList)
            {
                object? obj = App.GetParameterValue(p);
                string Name = (obj is null) ? string.Empty : obj.ToString() ?? string.Empty;

                if (UserProperties.ContainsKey(Name) == false)
                    AddProperty(Name);

                SetProperty(Name, p.token.Element.Value, 0);
            }

            InInit = false;
            return true;
        }

        /*------------------------------------------------------------------------------------------*
         * Some classes have things that need to happen after their init has executed
         *------------------------------------------------------------------------------------------*/
        public virtual bool PostClassInit() { return true; }

        /*------------------------------------------------------------------------------------------*
         * Update the AError array
         *------------------------------------------------------------------------------------------*/
        public virtual void _AddError(int errorNo, int lineNo, string message, string procedure)
        {
            if (UserProperties.ContainsKey("aerror"))
            {
                JAXObjects.Token aerr = UserProperties["aerror"];

                // Check to make sure we really have an array with 4 columns
                if (aerr.TType.Equals("A") && aerr.Col == 4)
                {
                    int i = 0;

                    // If _avalue[0] is not zero then we have to add another
                    // row to the array and position the strting point on
                    // the new row before saving the error
                    if (aerr._avalue[0].ValueAsInt != 0)
                    {
                        i = aerr._avalue.Count;
                        for (int j = 0; j < 4; j++)
                            aerr._avalue.Add(new());
                    }

                    // Add the error to the array
                    aerr._avalue[i + 0].Value = errorNo;
                    aerr._avalue[i + 1].Value = lineNo;
                    aerr._avalue[i + 2].Value = errorNo < 9999 ? JAXErrorList.JAXErrMsg(errorNo, message) : message;
                    aerr._avalue[i + 3].Value = procedure;
                }
            }
        }

        /*------------------------------------------------------------------------------------------*
         * Get a property
         *------------------------------------------------------------------------------------------*/
        public virtual int GetProperty(string propertyName, out JAXObjects.Token token)
        {
            return GetProperty(propertyName, 0, out token);
        }


        /*------------------------------------------------------------------------------------------*
         * Add an object to the end of the objects array
         *------------------------------------------------------------------------------------------*/
        public virtual int AddObject(JAXObjectWrapper value)
        {
            int err = 0;
            if (CanUseObjects == false) throw new Exception("3019|");

            if (CanWriteObjects)
            {
                if (value.VisualClass)
                {
                    if (me.visualObject is not null)
                    {
                        if (value.visualObject is not null)
                        {
                            me.visualObject.Controls.Add(value.visualObject);
                        }
                        else
                            err = 1901;
                    }
                    else
                        err = 9999;   // I'm not inialized!
                }

                if (err == 0)
                {
                    UserProperties["objects"].Add(value);
                    UserProperties["controlcount"].Element.Value = UserProperties["controlcount"].AsInt() + 1;

                    if (value.thisObject is not null)
                        value.thisObject.PostInit(me, []);
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
         * Insert an object in the objects array at a specific location
         * moving the rest of the objects down and expanding the array
         * by one element
         *------------------------------------------------------------------------------------------*/
        public virtual int InsertObjectAt(JAXObjectWrapper obj, int moveIDX)
        {
            int result = 0;

            if (CanUseObjects == false)
                result = 3019;
            else if (CanWriteObjects)
            {

            }
            else
                result = 3109;

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
         * Remove an object from the specified index in the objects array
         *------------------------------------------------------------------------------------------*/
        public int RemoveObject(int idx)
        {
            int result = 0;

            if (CanUseObjects == false)
                result = 3019;
            else if (CanWriteObjects)
            {
                if (idx >= UserProperties["objects"].Col)
                    throw new Exception("3003|");

                JAXObjectWrapper jow = (JAXObjectWrapper)UserProperties["objects"].Element.Value;

                if (jow is not null && jow.thisObject is not null)
                {
                    if (jow.Protected == JAXObjectWrapper.Protection.URD)
                        UserProperties["objects"].RemoveAt(idx);
                    else
                        throw new Exception($"3042|{jow.Name}");
                }
                else
                    UserProperties["objects"].RemoveAt(idx);  // Remove nulled obejct

            }
            else
                result = 3019;

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
         * Get an object by index from the objects array
         *------------------------------------------------------------------------------------------*/
        public virtual JAXObjectWrapper GetObject(int idx)
        {
            JAXObjectWrapper? jaxClass = null;

            if (CanUseObjects == false) throw new Exception("3019|");
            if (CanReadObjects)
            {
                if (idx >= 0 && UserProperties["objects"].Count > idx)
                {
                    UserProperties["objects"].ElementNumber = idx;
                    jaxClass = (JAXObjectWrapper)UserProperties["objects"].Element.Value;
                }
            }

            if (jaxClass is null)
                throw new Exception(string.Format("Object index ({0}) is out of bounds", idx));

            return jaxClass;
        }

        /*------------------------------------------------------------------------------------------*
         * Get an object by name from the objects array, returning the object and index
         * in the objects array
         *------------------------------------------------------------------------------------------*/
        public virtual JAXObjectWrapper? GetObject(string objectname, out int idx)
        {
            idx = -1;
            JAXObjectWrapper? jaxClass = null;

            if (CanReadObjects)
            {
                int count = UserProperties["objects"].Count;

                for (int i = 0; i < count; i++)
                {
                    UserProperties["objects"].ElementNumber = i;
                    JAXObjectWrapper o = (JAXObjectWrapper)UserProperties["objects"].Element.Value;
                    o.GetProperty("name", out JAXObjects.Token tk);
                    string name = tk.Element.Type.Equals("C") ? tk.AsString().ToUpper() : string.Empty;
                    if (name.Equals(objectname.ToUpper()))
                    {
                        jaxClass = o;
                        idx = i;
                        break;
                    }
                }
            }

            return jaxClass;
        }


        /*------------------------------------------------------------------------------------------*
         * Get a list trio (Name,Type,Tag) of all Properties, Methods, and Events in the class
         * along with Tag = (U)ser or (S)ystem
         *------------------------------------------------------------------------------------------*/
        public virtual List<GenericClass> GetPEMList()
        {
            List<GenericClass> results = [];
            foreach (KeyValuePair<string, JAXObjects.Token> tk in UserProperties)
            {
                GenericClass listItem = new()
                {
                    Name = tk.Key,
                    Type = "P",
                    Tag = tk.Value.Tag
                };

                results.Add(listItem);
            }

            foreach (KeyValuePair<string, MethodClass> tk in Methods)
            {
                GenericClass listItem = new()
                {
                    Name = tk.Key,
                    Type = tk.Value.Type,
                    Tag = tk.Value.Tag
                };

                results.Add(listItem);
            }

            return results;
        }


        /*- Virtual method -------------------------------------------------------------------------*
         * 
         * Non visual classes will typically call here to get the value of the 
         * property from the UserProperties dictionary.
         * 
         * Return INT result
         *      0   - Successfully proccessed
         *      1   - Just saved to UserProperties
         *      2   - Requires special handling, did not process
         *      >10 - Error code
         *      
         *------------------------------------------------------------------------------------------*/
        public virtual int GetProperty(string propertyName, int idx, out JAXObjects.Token returnToken)
        {
            returnToken = new();
            int result = 0;
            propertyName = propertyName.ToLower();

            if (CanReadObjects || propertyName.Equals("objects", StringComparison.OrdinalIgnoreCase) == false)
            {
                if (UserProperties.ContainsKey(propertyName))
                    returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
                else
                    result = 1559;
            }
            else
            {
                if (CanUseObjects)
                    result = 3023;
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
         * Get an object from the OBJECTS array
         *------------------------------------------------------------------------------------------*/
        public virtual int GetObjectProperty(int idx, string propertyName, out JAXObjects.Token objToken)
        {
            int result = 0;
            objToken = new();
            propertyName = propertyName.ToLower();

            if (CanReadObjects && UserProperties.TryGetValue("objects", out JAXObjects.Token? value))
            {
                if (idx < 0 || idx >= value.Row)
                {
                    // Out of the bounds of the array's index
                    result = 3003;
                }
                else
                {
                    // Found the object array, return the correct index
                    // Objects are always JAXObjectWrappers types
                    value.SetElement(idx, 1);
                    JAXObjectWrapper o = (JAXObjectWrapper)value.Element.Value;
                    o.GetProperty(propertyName, out objToken);
                }
            }
            else
            {
                // Object not found or not available
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
         * Set the default value for this control
         *------------------------------------------------------------------------------------------*/
        public virtual int SetDefault(string cmd)
        {
            int result = 0;
            XClass_AuxCode.SetDefault(me, cmd);
            return result;
        }

        /*------------------------------------------------------------------------------------------*
         * Set an object[] property
         *------------------------------------------------------------------------------------------*/
        public virtual int SetObjectProperty(int idx, string propertyName, JAXObjects.Token value)
        {
            int result = 0;
            propertyName = propertyName.ToLower();

            if (CanWriteObjects)
            {

            }
            else
            {
                if (IsMember("objects").Equals("P"))
                    result = 3025;
                else
                    result = 3019;
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

        /*- Virtual Method -------------------------------------------------------------------------*
         * 
         * Non visual classes will typically call here for basic storing of the 
         * property to the UserProperties dictionary.
         * 
         * Return INT result
         *      0   - Successfully proccessed
         *      1   - Just saved to UserProperties
         *      2   - Requires special handling, did not process
         *      >10 - Error code
         *      
         *------------------------------------------------------------------------------------------*/
        public virtual int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;

            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;
            App.DebugLog($"MyObj={((me.visualObject is null) ? "null" : me.visualObject.Name)} BASE.{propertyName}={objtk.AsString()}");

            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                switch (propertyName)
                {
                    // These are protected properties
                    case "baseclass":
                    case "class":
                    case "classlibrary":
                    case "parent":
                    case "parentclass":
                        if (MyBaseClass.Equals("empty", StringComparison.OrdinalIgnoreCase))
                            // Ignore the CA1854 as it won't put the value into the property
                            if (UserProperties.ContainsKey(propertyName))
                                UserProperties[propertyName].Element.Value = objValue;
                            else
                                result = 1559;
                        else
                            result = 3024;                          // Protected property
                        break;

                    default:
                        // We processed it or just need to save the property
                        // Ignore the CA1854 as it won't put the value into the property
                        if (UserProperties.ContainsKey(propertyName))
                            UserProperties[propertyName].Element.Value = objValue;
                        else
                            result = 1559;
                        break;
                }
            }

            return result;
        }

        /*------------------------------------------------------------------------------------------*
         * Add a property to the class with a default .F. value
         *------------------------------------------------------------------------------------------*/
        public virtual int AddProperty(string propertyName)
        {
            JAXObjects.Token token = new();
            return AddProperty(propertyName, token);
        }

        /*------------------------------------------------------------------------------------------*
         * Add a property object to the class passed as a JAX Object Wrapper
         *------------------------------------------------------------------------------------------*/
        public virtual int AddProperty(string propertyName, JAXObjectWrapper token)
        {
            JAXObjects.Token tk = new();
            tk.Element.Value = token;
            return AddProperty(propertyName, tk);
        }

        /*------------------------------------------------------------------------------------------*
         * Add a property to the class with a value passed as a var token
         *------------------------------------------------------------------------------------------*/
        public virtual int AddProperty(string propertyName, JAXObjects.Token token)
        {
            int result = 0;

            propertyName = propertyName.ToLower();

            // Not a form object, so try to add as a user property
            if (UserProperties.ContainsKey(propertyName) || Methods.ContainsKey(propertyName))
                result = 1771;
            else
                UserProperties.Add(propertyName, token);

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
         * Set the method's code.  If sent source and not compiled code, then compile
         * the source.  If sent compiled code, save it and ignore sourece if sent.
         * If sent neither, clear out the compiled code.
         * 
         * First time through?  Need to set the type to (M)ethod, (E)vent, or (U)ser
         * defined method.
         *------------------------------------------------------------------------------------------*/
        public virtual int _SetMethod(string methodName, string SourceCode, string CompCode, string Type)
        {
            MethodClass? mc = null;
            methodName = methodName.ToLower();
            int result = 0;

            if (methodName.Equals("writemethod"))
            {
                int iii = 0;
                //App.DebugLog($"Method {methodName} -> {SourceCode}", false);
            }

            if (me.BaseClass.Equals("empty", StringComparison.OrdinalIgnoreCase))
                result = 9999;
            else if (UserProperties.ContainsKey(methodName))    // Is this trying to overwrite a property?
                result = 1738;
            else
            {
                // Does the method already exist?
                if (Methods.TryGetValue(methodName, out MethodClass? value))
                {
                    // Get the current method definition and update the source code
                    mc = value;
                    mc.PrgCall = SourceCode;
                }
                else
                {
                    // Create a new method definition
                    mc = GetMethod(methodName);
                    mc.PrgCall = SourceCode;

                    if (string.IsNullOrWhiteSpace(Type) == false)
                    {
                        mc.Type = Type[..1].ToUpper();
                        //mc.Tag = Type.Contains('!') ? "N" : "U";    // Finding a ! means it's a native method
                        mc.Inherited = Type.Contains("#"); // Inherited 
                        if ("MEU".Contains(mc.Type) == false) throw new Exception("Invalid method type: " + mc.Type);
                    }
                }
            }

            // Is there some source code to compile?
            if (result == 0 && CompCode.Length == 0 && SourceCode.Length > 0)
            {
                CompCode = App.JaxCompiler.CompileBlock(SourceCode, true, out int errorCount);

                if (errorCount > 0)
                    result = 9997;
            }

            if (result == 0)
            {
                // Update the compiled code
                mc!.CompiledCode = CompCode;

                // Store the method definition
                if (!Methods.TryAdd(methodName, mc))
                    Methods[methodName] = mc;
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
         * Call the JAXCode for a method
         *------------------------------------------------------------------------------------------*/
        public virtual int _CallMethod(string methodName)
        {
            int results = 0;
            //App.AppLevels[^1].DoDefault = true;

            try
            {
                if (Methods.ContainsKey(methodName.ToLower()))
                {
                    string cCode = Methods[methodName.ToLower()].CompiledCode;

                    // Create a new App.Levels and execute the code
                    if (cCode.Length > 0)
                    {
                        //App.DebugLog($"_CallMethod for {methodName} start ─ this: {this.GetHashCode()}  me: {me?.GetHashCode() ?? -1}  me.Name: {me?.Name ?? "?"}", false);

                        //// Call the routine to compile and execute a block of code
                        _ = App.JaxExecuter.ExecuteCodeBlock(me!, methodName, cCode);

                    }
                    else
                        results = DoDefault(methodName);
                }
                else
                    results = 1559;

            }
            catch (Exception ex)
            {
                results = 9999;
            }

            if (results > 0)
            {
                _AddError(results, 0, string.Empty, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(results, $"{results}|", string.Empty);

                results = -1;
            }

            return results;
        }

        public virtual int DoDefault(string methodName)
        {
            int result = 0;
            methodName = methodName.ToLower();
            JAXObjects.Token tk = new();


            if (Methods.ContainsKey(methodName))
            {
                //string mcall = Methods[methodName].PrgCall;
                //App.DebugLog($"DODEFAULT - Method {methodName} for class {UserProperties["class"].AsString()} - {me.Name} - {UserProperties["classid"].AsString()} - Source {mcall}");

                switch (methodName)
                {
                    case "addobject":
                        // TODO - Only certain classes can have objects added to them
                        if (App.ParameterClassList.Count == 1 && App.ParameterClassList[0].token.Element.Type.Equals("O"))
                        {
                            // JAXBase can accept an object in ADDOBJECT()
                            AddObject((JAXObjectWrapper)App.ParameterClassList[0].token.Element.Value);
                        }
                        else
                        {
                            // we're expecting cName, cClass [,aInit1, aInit2...]
                            if (App.ParameterClassList.Count == 0)
                                result = 1229;
                            else
                            {
                                List<JAXObjects.Token> ParameterList = [];
                                foreach (ParameterClass p in App.ParameterClassList)
                                {
                                    JAXObjects.Token t = new();
                                    object? obj = App.GetParameterValue(p);
                                    if (obj is null)
                                        t.Element.MakeNull();
                                    else
                                        t.Element.Value = obj;

                                    ParameterList.Add(t);
                                }

                                me.AddObjectUsingParameters(ParameterList);
                            }
                        }
                        break;

                    case "error":
                        if (App.CurrentDS.JaxSettings.ErrorClassReporting)
                        {
                            // We are supposed to report the error
                            if (UserProperties.ContainsKey("aerror"))
                            {
                                tk = UserProperties["aerror"];
                                if (tk._avalue[0].Type.Equals("N") && tk._avalue[0].ValueAsInt > 0)
                                {
                                    // we have an actual error to report!
                                    App.SetError(tk._avalue[0].ValueAsInt, tk._avalue[2].ValueAsString, string.Empty);
                                }
                            }
                        }
                        break;

                    case "errormessage":
                        // Show an Invalid Input message in upper right
                        App.WaitWindow = JAXLib.WaitWindow(App, "Invalid Input", -1, -1, false, false, 3, out _);
                        break;

                    case "refresh":
                        me.visualObject?.Refresh();
                        App.DebugLog("---- FORM REFRESH ----");
                        break;

                    case "resettodefault":
                        // Only certain objects can reset to default
                        if (App.ParameterClassList.Count == 1 && App.ParameterClassList[0].token.Element.Type.Equals("C"))
                            result = ResetPropertyToDefault(App.ParameterClassList[0].token.AsString());
                        else
                            result = 1559;

                        break;

                    case "setfocus":
                        //App.DebugLog($"Setfocus default code start ─ this: {this.GetHashCode()}  me: {me?.GetHashCode() ?? -1}  me.Name: {me?.Name ?? "?"}", false);
                        if (me.VisualClass && me.visualObject is not null)
                            me.visualObject.Focus();
                        break;

                    case "show":
                        if (JAXLib.InListC(me.BaseClass, "form", "formset", "browser"))
                            result = me.Show();
                        break;

                    case "writemethod":
                        // Only some classes allow method code to be written at runtime
                        string cMethodName = (App.ParameterClassList.Count > 0) ? App.ParameterClassList[0].token.AsString() : string.Empty;
                        string cSourceCode = (App.ParameterClassList.Count > 1) ? App.ParameterClassList[1].token.AsString() : string.Empty;
                        string cCompCode = (App.ParameterClassList.Count > 2) ? App.ParameterClassList[2].token.AsString() : string.Empty;

                        //App.DebugLog($"WriteMethod {cMethodName} -> {cSourceCode}", false);
                        if (cMethodName.Contains("after"))
                        {
                            int iii = 0;
                        }

                        result = me.SetMethod(cMethodName, cSourceCode, cCompCode);
                        break;

                    default:
                        break;
                }

                if (result > 0)
                {
                    _AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                    if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                        App.SetError(result, $"{result}|", string.Empty);

                    result = -1;
                }
            }

            return result;
        }

        public virtual void MakeNextDefaultName(JAXObjectWrapper value)
        {
            if (value.GetProperty("name", out JAXObjects.Token tk) == 0)
            {
                string name = tk.AsString();
                if (name.Equals(value.DefaultName(), StringComparison.OrdinalIgnoreCase))
                {
                    JAXObjects.Token objects = UserProperties["objects"];
                    int icount = objects.Row * objects.Col;
                    int ncount = 1;

                    if (icount == 0)
                        name = name + "1";
                    else
                    {
                        // Find the highest default name in the objects list
                        for (int i = 0; i < icount; i++)
                        {
                            JAXObjectWrapper jow = (JAXObjectWrapper)objects._avalue[i].Value;
                            if (jow.GetProperty("name", out tk) == 0)
                            {
                                string tname = tk.AsString();
                                if (tname.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                                {
                                    // found a default name match so look for the highest
                                    // default name number out of the list
                                    while (tname.CompareTo($"{name}{ncount}") >= 0)
                                        ncount++;
                                }
                            }
                        }

                        // finalize the name
                        name += $"{ncount}";
                    }

                    value.SetProperty("name", name);
                }
            }
        }
        /*------------------------------------------------------------------------------------------*
         *------------------------------------------------------------------------------------------*/
        public virtual string DefaultName() { return MyDefaultName; }

        /*------------------------------------------------------------------------------------------*
         * List of properties registered to the class
         *------------------------------------------------------------------------------------------*/
        public virtual string[] JAXProperties() { return []; }

        /*------------------------------------------------------------------------------------------*
         * List of methods registered to the class
         *------------------------------------------------------------------------------------------*/
        public virtual string[] JAXMethods() { return []; }

        /*------------------------------------------------------------------------------------------*
         * List of events registered to the class
         *------------------------------------------------------------------------------------------*/
        public virtual string[] JAXEvents() { return []; }


        /*------------------------------------------------------------------------------------------*
         * Resets a property to it's default value
         *------------------------------------------------------------------------------------------*/
        public virtual int ResetPropertyToDefault(string property)
        {
            int result = 0;
            XClass_AuxCode.ResetPropertyToDefault(me, property);
            return result;
        }


        /*
         * Set a property of all like classes
         * Deep dive through all child objects
         */
        public virtual void SetAllOfClass(string Class, string propertyName, JAXObjects.Token objtk)
        {
            if (UserProperties.ContainsKey("objects"))
            {
                JAXObjects.Token otk = UserProperties["objects"];
                for (int i = 0; i < otk._avalue.Count; i++)
                {
                    // Protection, but should always be true
                    if (otk._avalue[i].Value is JAXObjectWrapper)
                    {
                        // If it's a button then adjust the property
                        JAXObjectWrapper itk = (JAXObjectWrapper)otk._avalue[i].Value;
                        if (itk.Class.Equals(Class, StringComparison.OrdinalIgnoreCase))
                            if (UserProperties.ContainsKey(propertyName))
                                SetObjectProperty(i, propertyName, objtk);

                        // Deep dive
                        itk.thisObject!.SetAllOfClass(Class, propertyName, objtk);
                    }
                }

            }
        }


        /*
         * Set a property of all like baseclasses
         * Deep dive through all child objects
         */
        public virtual void SetAllOfBaseClass(string BaseClass, string propertyName, JAXObjects.Token objtk)
        {
            if (UserProperties.ContainsKey("objects"))
            {
                // Deep dive first
                SetAllOfClass(BaseClass, propertyName, objtk);

                JAXObjects.Token otk = UserProperties["objects"];
                for (int i = 0; i < otk._avalue.Count; i++)
                {
                    // Protection, but should always be true
                    if (otk._avalue[i].Value is JAXObjectWrapper)
                    {
                        // If it's a button then adjust the property
                        JAXObjectWrapper itk = (JAXObjectWrapper)otk._avalue[i].Value;
                        if (itk.Class.Equals(BaseClass, StringComparison.OrdinalIgnoreCase))
                            if (UserProperties.ContainsKey(propertyName))
                                SetObjectProperty(i, propertyName, objtk);

                        // Deep dive
                        itk.thisObject!.SetAllOfClass(BaseClass, propertyName, objtk);
                    }
                }
            }
        }


        /*------------------------------------------------------------------------------------------*
         * Add a property, with value, and locked to a specific var type
         *------------------------------------------------------------------------------------------*/
        public virtual int AddProperty(string propertyName, string lockType, string lockValue)
        {
            int result = 0;
            XClass_AuxCode.AddLockedProperty(me, propertyName, lockType, lockValue);
            return result;
        }

        /*------------------------------------------------------------------------------------------*
         * Returns a bool on whether the provided name is a property in the object
         *------------------------------------------------------------------------------------------*/
        public virtual bool HasProperty(string propertyName) { return UserProperties.ContainsKey(propertyName.ToLower().Trim()); }

        /*------------------------------------------------------------------------------------------*
         * Returns a string determining if the name is a member of the control
         * M - Method/Event
         * P - Property
         * O - Object
         * U - Unknown
         *------------------------------------------------------------------------------------------*/
        public virtual string IsMember(string name)
        {
            string isMember = "U";

            if (UserProperties.ContainsKey(name.ToLower())) // Is it a property?
                isMember = "P";
            else if (Methods.ContainsKey(name.ToLower()))   // Is it a method/event?
                isMember = "M";
            else
            {
                if (UserProperties.TryGetValue("objects", out JAXObjects.Token? Objs))
                {
                    // Is it an object?
                    for (int i = 0; i < UserProperties["controlcount"].AsInt(); i++)
                    {
                        Objs.ElementNumber = i;
                        JAXObjectWrapper oname = (JAXObjectWrapper)Objs.Element.Value;

                        if (oname.GetProperty("name", 0, out JAXObjects.Token tk) >= 0)
                        {
                            string nam = tk.Element.Type.Equals("C") ? tk.AsString() : string.Empty;
                            if (nam.Equals(name, StringComparison.OrdinalIgnoreCase))
                            {
                                isMember = "O";
                                break;
                            }
                        }
                    }
                }
            }

            return isMember;
        }


        /*------------------------------------------------------------------------------------------*
         *------------------------------------------------------------------------------------------*
         * JAXBase Visual Class events
         * 
         * Look at C:\Users\jlw61\OneDrive\Desktop\Grok\CSharp\FormGetLostFocus for more info
         * on how to track movement between controls.  Will need to create a link to THISFORM,
         * and it's about time I did that anyway.  Add in THISFORMSET!
         *------------------------------------------------------------------------------------------*
         *------------------------------------------------------------------------------------------*/
        public virtual void MyObj_LostFocus(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("lostfocus"))
                _CallMethod("lostfocus");

            // If by tab/shift+tab then set direction of travel
        }

        public virtual void MyObj_GotFocus(object? sender, EventArgs e)
        {
            bool OK2Enter = true;

            if (Control.MouseButtons == MouseButtons.Left)
            {
                // Got focus via mouse click
            }
            else
            {

                // Got focus via keyboard
                if (Control.ModifierKeys == Keys.Shift)
                {
                    // SHIFT+Tab
                }
                else
                {
                    // Tab
                }
            }

            // If there is a when event
            if (Methods.ContainsKey("when"))
            {
                _CallMethod("when");

                // OK to enter?
                if (App.ReturnValue.Element.Type.Equals("L"))
                    OK2Enter = App.ReturnValue.AsBool();
                else
                    throw new Exception("11|");
            }

            if (OK2Enter)
            {
                // Entering this control
                if (Methods.ContainsKey("gotfocus"))
                    _CallMethod("gotfocus");

                // Update what control we're in
            }
            else
            {
                // If move was based on TAB/SHIFT+TAB then go to the
                // next control in direction of travel.  Otherwise go
                // back to the control we were in.
            }
        }

        public virtual void MyObj_MouseWheel(object? sender, MouseEventArgs? e)
        {
            if (Methods.ContainsKey("mousewheel"))
            {
                int WheelDir = 1;   // Get wheel direction  pos or neg
                MouseButtonAction("mousewheel", WheelDir, e);
            }
        }

        public virtual void MyObj_MouseMove(object? sender, MouseEventArgs e)
        {
            if (Methods.ContainsKey("mousemove"))
                MouseButtonAction("mousemove", e);
        }

        public virtual void MyObj_MouseUp(object? sender, MouseEventArgs e)
        {
            if (Methods.ContainsKey("mouseup"))
                MouseButtonAction("mouseup", e);
        }

        public virtual void MyObj_MouseDown(object? sender, MouseEventArgs e)
        {
            if (Methods.ContainsKey("mousedown"))
                MouseButtonAction("mousedown", e);
        }

        public virtual void MyObj_MouseHover(object? sender, EventArgs e)
        {
            // Set parameters nXCoord, nYCoord
            if (Methods.ContainsKey("mousehover"))
                MouseButtonAction("mousehover", -1, null);
        }

        public virtual void MyObj_MouseEnter(object? sender, EventArgs e)
        {
            //App.DebugLog($"MouseEnter start ─ this: {this.GetHashCode()}  me: {me?.GetHashCode() ?? -1}  me.Name: {me?.Name ?? "?"}", false);
            if (Methods.ContainsKey("mouseenter"))
                MouseButtonAction("mouseenter", -1, null);
        }

        private void MouseButtonAction(string cMethod, MouseEventArgs e)
        {
            int nButton = e.Button switch
            {
                MouseButtons.Left => 1,
                MouseButtons.Right => 2,
                _ => 4
            };

            MouseButtonAction(cMethod, nButton, e);
            //App.DebugLog($"MouseEnter finished ─ this: {this.GetHashCode()}  me: {me?.GetHashCode() ?? -1}  me.Name: {me?.Name ?? "?"}", false);
        }

        private void MouseButtonAction(string cMethod, int nButton, MouseEventArgs? e)
        {
            int nShift = 0; // ToDo - get SHIFT=1, CTRL=2, ALT=4

            int xCoord = 0;
            int yCoord = 0;

            if (e is not null)
            {
                xCoord = e.X;
                yCoord = e.Y;
            }

            ParameterClass tk = new();

            App.ParameterClassList.Clear();

            if (nButton >= 0)
            {
                // Only add if nButton>=0
                tk.token.Element.Value = nButton;
                App.ParameterClassList.Add(tk);
            }

            tk = new();
            tk.token.Element.Value = nShift;
            App.ParameterClassList.Add(tk);

            tk = new();
            tk.token.Element.Value = xCoord;
            App.ParameterClassList.Add(tk);

            tk = new();
            tk.token.Element.Value = yCoord;
            App.ParameterClassList.Add(tk);

            _CallMethod(cMethod);
        }

        public virtual void MyObj_MouseLeave(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("mouseleave"))
                MouseButtonAction("mouseleave", -1, null);
        }

        public virtual void MyObj_DoubleClick(object? sender, EventArgs e)
        {
            if (Methods.ContainsKey("doubleclick"))
                _CallMethod("doubleclick");
        }

        public virtual void MyObj_Click(object? sender, EventArgs e)
        {
            //App.DebugLog($"XBASE - {me.Name}.click - {UserProperties["name"].AsString()} - {UserProperties["classid"].AsString()}");
            if (Methods.ContainsKey("click"))
                _CallMethod("click");
        }

        public virtual void MyObj_Move(object? sender, EventArgs? e)
        {
            if (Methods.ContainsKey("moved"))
                _CallMethod("moved");
        }

        public virtual void MyObj_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Set parameters nKeyCode, nShiftAltCtrl
            if (Methods.ContainsKey("keypress"))
                _CallMethod("keypress");
        }

        public virtual void MyObj_Validating(object? sender, CancelEventArgs e)
        {
            if (Methods.ContainsKey("valid"))
            {
                _CallMethod("valid");

                // If validation is 0 or false, cancel
                if (App.ReturnValue.Element.Type.Equals("L"))
                    e.Cancel = App.ReturnValue.AsBool() == false;
                else if (App.ReturnValue.Element.Type.Equals("N"))
                    e.Cancel = App.ReturnValue.AsInt() == 0;
                else
                    throw new Exception("11|");
            }
        }
    }
}
