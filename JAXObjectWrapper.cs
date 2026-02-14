/*--------------------------------------------------------------------------------------------------*
 * 2025.08.01 - JLW
 *      This class is used to create an object with the events needed to 
 *      make all classes act the same and be able to interface with the
 *      code in the same manner.
 *      
 * 2025.11.05 - JLW
 *      Coming down to the home stretch.  The only event that will be a
 *      problem right now is the destroy event which I expect can be
 *      solved using idispose, but I'll need to read up and test that
 *      out extensively before attempting it.
 *      
 * 2025.11.06 - JLW
 *      Converted over to new classes.  Lots of testing to do but they
 *      should be more bulletproof and will have a lot less code to break;
 *
 * 2025.11.07 - JLW
 *      Needed a few GROK sessions to help me figure things out.  But I've
 *      got a better grip on sub classing. 
 *      
 *--------------------------------------------------------------------------------------------------*/
using Newtonsoft.Json.Linq;

namespace JAXBase
{
    /*----------------------------------------------------------------------------------------------*
     * This wrapper provides the functionality to make all objects appear to work the
     * same, execute code in the events and methods, and otherwise make the classes
     * a unified object type that can be used across the system.  Just like you'd 
     * expect in an xBase language.
     *----------------------------------------------------------------------------------------------*/
    public class JAXObjectWrapper
    {
        public enum Protection { urd, Urd, uRd, URd, URD }

        // Access to the global App class
        public readonly AppClass App;

        // The Visual/Nonvisual class object
        public readonly IJAXClass? thisObject;

        // Quick references so we don't have to do a GetProperty
        // and so we can still handle the EMPTY baseclass correctly
        public JAXObjectWrapper? parent = null;
        public JAXObjectWrapper? Parent
        {
            get { return parent; }
            set
            {
                // Set up the parent related references
                parent = value;

                if (parent is not null)
                {
                    ParentClass = parent.Class;
                    THISFORM = parent.THISFORM;
                    THISFORMSET = parent.THISFORMSET;
                }
                else
                {
                    ParentClass = string.Empty;
                    THISFORM = null; ;
                    THISFORMSET = null;
                }
            }
        }

        public string ParentClass { get; private set; } = string.Empty;


        private string jaxclass = string.Empty;
        public string Class
        {
            get { return jaxclass; }
            set
            {
                // You can't reset the class type
                if (string.IsNullOrWhiteSpace(jaxclass)) jaxclass = value;
            }
        }
        public string Name { get; private set; } = string.Empty;


        private string baseclass = string.Empty;
        public string BaseClass
        {
            get { return baseclass; }
            set
            {
                // You can't reset a base class name
                if (string.IsNullOrWhiteSpace(baseclass)) baseclass = value;
            }
        }

        private string classid = string.Empty;
        public string ClassID
        {
            get { return classid; }
            set
            {
                // You can't reset the classid
                if (string.IsNullOrWhiteSpace(classid)) classid = App.SystemCounter();
            }
        }

        // Visual class flag and object
        public bool VisualClass = false;
        public System.Windows.Forms.Control? visualObject = null;
        public object? nvObject = null;

        public JAXObjectWrapper THIS;
        public JAXObjectWrapper? THISFORM = null;
        public JAXObjectWrapper? THISFORMSET = null;

        // Used to prevent incorrect clearing of AError array by
        // indicating that we are in a multi-method transaction.
        public bool InTransaction = false;

        public Protection Protected = Protection.URD;

        public JAXObjectWrapper(AppClass app, string cClass, string cName, List<ParameterClass>? parameterList)
        {
            App = app;
            Class = cClass;
            THIS = this;
            string lastProp = string.Empty;

            int Err = 0;
            string msg = string.Empty;

            // ClearErrors() must not cleared when in a transaction except for 
            // the transaction that sets the value to true.
            InTransaction = true;

            if (Array.IndexOf(app.lists.JAXObjects, cClass.ToLower()) < 0)
            {
                //-------------------------------------------------------------
                // TODO - DEAL WITH USER DEFINED CLASSES 
                //-------------------------------------------------------------
                Err = 1999; // Not implemented

                // Class name is not a base class so must be defined in app.ClassDefinitions list
                // TODO - Do we add class library name support or just search for the first name match?
                //int f = app.ClassDefinitions.FindIndex(x => x.Name.Equals(cClass, StringComparison.OrdinalIgnoreCase));
                //if (f < 0) Err = 1733; // No class definition

                // TODO - how do we define parent class?
                // create the parent object
                //JAXObjectWrapper jow = new(app, app.ClassDefinitions[f].ParentClass, string.Empty, parameterList);

                // Create a new base class and copy the properties from the parent
                //thisObject = JAXObjectsAux.GetClass(this, jow.BaseClass, cName);

                // Get the properties from the jow object

                // Update the properties in this object

                // Now execute the property code from app.ClassDefinitions

                // Now load the methods from app.ClassDefinitions[f]
            }
            else
            {
                // Class name is a base class
                thisObject = JAXObjectsAux.GetClass(this, cClass, cName);
            }


            // Check initialization progress
            if (Err == 0)
                Err = thisObject is null ? 1901 : 0;

            if (Err == 0)
            {
                string[] JAXProperties = thisObject!.JAXProperties();
                for (int i = 0; i < JAXProperties.Length; i++)
                {

                    string[] prop = JAXProperties[i].Split(',');
                    //app.DebugLog(string.Format("{0} ({1}) to {2} in object {3}", prop[0], prop[1], prop[2], cClass));

                    if (prop.Length == 3)
                    {
                        string p0 = prop[0].ToLower().Trim();
                        string p1 = prop[1].Replace("!", "").ToUpper().Trim();
                        JAXObjects.Token tk = new();

                        if (p0.Contains("list",StringComparison.OrdinalIgnoreCase))
                        {
                            int iii = 0;
                        }

                        try
                        {
                            App.DebugLog($"Adding property {p0}");
                            lastProp = p0;

                            // Some properties are already assigned in some classes
                            // so check first before trying to create it
                            if (thisObject.HasProperty(prop[0]) == false)
                            {
                                switch ((p1 + "*")[..1])
                                {
                                    case "F":   // Numeric float
                                        thisObject.AddProperty(p0, "N", prop[2]);
                                        thisObject.UserProperties[p0].Info = "F";
                                        break;

                                    case "Y":  // Logical using Y/N
                                        thisObject.AddProperty(p0, "L", prop[2]);
                                        thisObject.UserProperties[p0].Info = "Y";
                                        break;

                                    case "N":   // Numeric Integer
                                        thisObject.AddProperty(p0, "N", prop[2]);
                                        thisObject.UserProperties[p0].Info = "I";
                                        break;

                                    case "P":   // Points
                                        JAXObjects.Token pp = new();
                                        AppHelper.ASetDimension(pp, 1, 1);
                                        thisObject.AddProperty(p0, pp);
                                        thisObject.UserProperties[p0].Info = "P";
                                        break;

                                    case "R":   // RGB color value
                                        if (prop[2].Contains("|"))
                                        {
                                            string[] rparts = prop[2].Split('|');
                                            if (rparts.Length == 3)
                                            {
                                                if (int.TryParse(rparts[0], out int rp0) == false) rp0 = 0;
                                                if (int.TryParse(rparts[1], out int rp1) == false) rp1 = 0;
                                                if (int.TryParse(rparts[2], out int rp2) == false) rp2 = 0;

                                                prop[2] = (rp2 + rp1 * 255 + rp0 * 65536).ToString();
                                            }
                                            else
                                                throw new Exception("9999||Color error");
                                        }
                                        else
                                        {
                                            // Expecting a single number value
                                            if (int.TryParse(prop[2], out int test) == false) throw new Exception("1732|");
                                            if (JAXLib.Between(test, 0, 16777215) == false) throw new Exception("41|");
                                            prop[2] = test.ToString();
                                        }

                                        thisObject.AddProperty(p0, "N", prop[2]);
                                        thisObject.UserProperties[p0].Info = "RGB";
                                        break;

                                    case "?":
                                        tk = AppHelper.ProcessExpression(app, prop[2].Trim());
                                        thisObject.AddProperty(p0);
                                        thisObject.SetProperty(p0, tk.Element.Value, 0);
                                        break;

                                    case "#":   // Simple Array with no members
                                        tk.SetDimension(0, 1, true);
                                        thisObject.AddProperty(p0, tk);
                                        break;

                                    default:    // Rest of normal types (C,D,T,L)
                                        if (p1.Length == 0)
                                        {
                                            // No type, so make the property mutable with a string
                                            thisObject.AddProperty(p0);
                                            thisObject.SetProperty(p0, string.Empty, 0);
                                        }
                                        else
                                            thisObject.AddProperty(p0, p1[..1], prop[2]);
                                        break;
                                }
                            }
                            else
                            {
                                // Update the property
                                tk = new();
                                if (p1.Equals("?"))
                                {
                                    // We're expecting an expression that needs to be processed
                                    tk = AppHelper.ProcessExpression(app, prop[2].Trim());
                                }
                                else
                                {
                                    // Converting to type C, D, L, N, O, or T
                                    tk = AppHelper.ReturnStringAsTokenOfType(app, prop[2], p1);
                                }

                                thisObject.SetProperty(p0, tk.Element.Value, 0);
                            }

                            // Update the property attributes
                            thisObject.UserProperties[p0].Protected = prop[1].Contains('!');
                            thisObject.UserProperties[p0].SpecialHandling = prop[1].Contains('$');
                            thisObject.UserProperties[p0].ClassProperty = true;
                            thisObject.UserProperties[p0].Tag = "N";    // Native/User
                        }
                        catch (Exception ex)
                        {
                            msg = ex.Message;
                            Err = 9999;
                        }
                    }
                    else
                        App.DebugLog($"Property error {JAXProperties[i]}");
                }

                // Make sure the name gets set
                if (thisObject is not null && thisObject.UserProperties.ContainsKey("name"))
                    if ((VisualClass && visualObject is not null) || (VisualClass == false && nvObject is not null))
                        thisObject.SetProperty("name", string.IsNullOrWhiteSpace(cName) ? cClass : cName, 0);
            }

            if (Err == 0)
            {
                try
                {
                    // ----------------------------------------------------------------------------------
                    // The following is for all classes except the EMPTY class
                    // ----------------------------------------------------------------------------------
                    if (cClass.Equals("empty", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        // ------------------------------------------------------------------------------
                        // The ID, AError, and Locked properties need to be added
                        // to all properties except the EMPTY property.
                        // ------------------------------------------------------------------------------
                        if (thisObject!.HasProperty("classid") == false)
                        {
                            ClassID = app.SystemCounter();
                            thisObject.AddProperty("classid", "P", ClassID);
                        }

                        if (thisObject!.HasProperty("aerror") == false)
                        {
                            thisObject.AddProperty("aerror");
                            ClearErrors();
                        }

                        if (thisObject!.HasProperty("locked") == false)
                            thisObject.AddProperty("locked", "L", "false");

                        thisObject.UserProperties["classid"].Tag = "N";
                        thisObject.UserProperties["aerror"].Tag = "N";
                        thisObject.UserProperties["locked"].Tag = "N";

                        // Set the AError array to its empty setting
                        ClearErrors();


                        //thisObject._SetMethod("resettodefault", "", "", "M");

                        // Load the methods
                        string[] JAXMethods = thisObject.JAXMethods();
                        for (int i = 0; i < JAXMethods.Length; i++)
                        {
                            App.DebugLog($"Adding method {JAXMethods[i]}");
                            thisObject._SetMethod(JAXMethods[i], string.Empty, string.Empty, "M!");
                            thisObject.Methods[JAXMethods[i]].Tag = "N";
                        }

                        // Load the events
                        string[] JAXEvents = thisObject.JAXEvents();
                        for (int i = 0; i < JAXEvents.Length; i++)
                        {
                            App.DebugLog($"Adding event {JAXEvents[i]}");
                            thisObject._SetMethod(JAXEvents[i], string.Empty, string.Empty, "E!");
                            thisObject.Methods[JAXEvents[i]].Tag = "N";
                        }

                        // ------------------------------------------------------------------------------
                        // The following methods need to be added to all objects
                        // automatically if they have not already been added
                        // ------------------------------------------------------------------------------


                        // ------------------------------------------------------------------------------
                        // Now call the load method, if it exists
                        // ------------------------------------------------------------------------------
                        if (thisObject.IsMember("load").Equals("M"))
                            thisObject._CallMethod("load");
                    }

                    // ------------------------------------------------------------------------------
                    // Perform post cleanup
                    // ------------------------------------------------------------------------------
                    if (parameterList is not null)
                    {
                        if (BaseClass.Equals("column") == false)
                        {
                            // Put passed parameters to App.ParameterList
                            // If we add named parameter support then
                            // it will happen in PostInit and those
                            // parameters will be removed from the
                            // App.ParameterList before moving on to INIT
                            foreach (ParameterClass xPar in parameterList)
                                App.ParameterClassList.Add(xPar);

                            thisObject!.PostInit(parent, parameterList);
                        }
                    }
                    else
                        thisObject!.PostInit(null, []);

                    if (cClass.Equals("empty", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        // Now process the JAX init method
                        if (thisObject!.IsMember("init").Equals("M"))
                        {
                            thisObject._CallMethod("init");
                            if (App.ReturnValue.Element.Type.Equals("L"))
                            {
                                // TODO - If it returns .F. then we kill this class
                            }
                            else
                                throw new Exception("11|");
                        }

                        // Various classes have other methods that need
                        // to be called after their init method completes
                        //thisObject.PostClassInit();
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                    Err = 9999;
                }
            }

            // Did the object initialize correctly?
            // If not, null out thisObject to signal a failure
            if (Err > 0)
            {
                thisObject = null;
                App.SetError(Err, $"{Err}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }
        }


        public void SetParent(JAXObjectWrapper parent)
        {
            if ( Parent is not null)
            {
                if (baseclass.Equals("optionbutton", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("optiongroup") == false)
                    throw new Exception($"3300|{Class}/{Parent.BaseClass}");

                if (baseclass.Equals("menuitem", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("menu") == false)
                    throw new Exception($"3300|{Class}/{Parent.BaseClass}");

                if (baseclass.Equals("toolbutton", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("toolbar") == false)
                    throw new Exception($"3300|{Class}/{Parent.BaseClass}");

                if (baseclass.Equals("form", StringComparison.OrdinalIgnoreCase) && Parent.BaseClass.Equals("formset") == false)
                    throw new Exception($"3300|{Class}/{Parent.BaseClass}");

                if (VisualClass && Parent.VisualClass==false)
                    throw new Exception($"3301|{Class}/{Parent.BaseClass}");
            }

            Parent = parent;
        }

        public void SetName(string name)
        {
            Name = name.Trim().ToLower();

            if (thisObject is not null && thisObject.UserProperties.ContainsKey("name"))
                thisObject.UserProperties["name"].Element.Value = Name;
        }


        /*
         * Clear the aError array by creating it from scratch
         */
        public void ClearErrors()
        {
            if (thisObject is not null && thisObject.HasProperty("aerror"))
            {
                JAXObjects.Token tk = new();
                tk._avalue[0].Value = 0;
                tk._avalue.Add(new());
                tk._avalue.Add(new());
                tk._avalue.Add(new());
                tk.Row = 1;
                tk.Col = 4;
                tk.TType = "A";

                // Have to set the property directly so that the
                // property is the array.  Otherwise SetProperty()
                // will put the array in the element, like an object.
                thisObject.UserProperties["aerror"] = tk;
            }
        }

        public int GetErrorNo()
        {
            int result = 0;
            if (thisObject is null)
                result = 1901;
            else
            {
                thisObject.GetProperty("aerror", 0, out JAXObjects.Token tk);
                result = tk._avalue[0].ValueAsInt;
            }

            return result;
        }

        public int AddError(int errorNo, int lineNo, string message, string procedure)
        {
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (thisObject is null)
                    result = 1901;
                else
                {
                    thisObject._AddError(errorNo, lineNo, message, procedure);

                    if (thisObject.Methods.ContainsKey("error"))
                        result = MethodCall("error");
                }

            }
            catch (Exception ex)
            {
                result = 9999;
                msg = ex.Message;
            }

            if (result > 0)
            {
                App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                result = -1;
            }

            return result;
        }

        public string IsMember(string memTest)
        {
            string result = "X";

            try
            {
                if (thisObject is not null)
                    result = thisObject.IsMember(memTest);
                else
                    App.SetError(1901, "1901|", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }
            catch (Exception ex)
            {
                result = "X";
                App.SetError(9999, $"9999||Class {Class.ToUpper()} error: {ex.Message}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }

        public string DefaultName()
        {
            string result = string.Empty;

            try
            {
                if (thisObject is not null)
                    result = thisObject.DefaultName();
                else
                    App.SetError(1901, "1901|", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }
            catch (Exception ex)
            {
                App.SetError(9999, $"9999||Class {Class.ToUpper()} error: {ex.Message}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }


        // Add a user defined method or update an existing method/event
        public int SetMethod(string methodName, string sourceCode, string compCode)
        {
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (!InTransaction) ClearErrors();

                if (thisObject is null)
                {
                    result = 1901;
                    App.SetError(1901, "1901|", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                }
                else
                    result = thisObject._SetMethod(methodName, sourceCode, compCode, "U");
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                result = 9999;
            }

            if (result > 0)
            {
                App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                result = -1;
            }

            return result;
        }


        /*
         * When a method is called, if there is code in that method, it is executed, otherwise
         * the DoDefault() is executed.  A method with code in it must call DoDefault if it
         * wants the underlying action/code to execute.
         * 
         * Parameters are expected to already be in the stack.
         * 
         */
        public int MethodCall(string methodName)
        {
            //App.DebugLog($"MethodCall for {methodName} in {this.Name}", false);

            int result = 0;
            string msg = string.Empty;
            methodName = methodName.ToLower();

            if (methodName.Contains("errorm", StringComparison.OrdinalIgnoreCase))
            {
                int iii = 0;
            }

            try
            {
                // Clear the aError array before calling a method
                // TODO - this isn't right
                if (!InTransaction) ClearErrors();

                if (thisObject is null)
                    result = 1901;
                else
                {
                    // Check to see if the method exists before trying to call it
                    if (thisObject.IsMember(methodName).Equals("M"))
                    {
                        if (thisObject.Methods[methodName].CompiledCode.Length > 0)
                        {
                            // Execute the coded method
                            result = thisObject._CallMethod(methodName);

                            if (methodName.Equals("error", StringComparison.OrdinalIgnoreCase) && result != 0)
                            {
                                result = 3099;
                                App.SetError(3099, $"Name (BaseClass: {BaseClass}, ID:{ClassID})", string.Empty);
                            }
                        }
                        else
                            result = thisObject.DoDefault(methodName);
                    }
                    else
                        result = 1738;

                    // Everything ok?
                    //if (result == 0 && string.IsNullOrWhiteSpace(methodName) == false)
                    //    result = thisObject._CallMethod(methodName);
                }
            }
            catch (Exception ex)
            {
                result = 9999;
                msg = ex.Message;
            }

            // Clear the parameter list no matter what
            App.ParameterClassList.Clear();

            if (result > 0)
            {
                if (thisObject is not null)
                {
                    thisObject._AddError(result, 0, string.Empty, App.AppLevels[^1].Procedure);

                    if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                        App.SetError(result, $"{result}|", string.Empty);
                }
                else
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                result = -1;
            }

            // Send back the return value
            return result;
        }

        // Standard xBase addobject is stored in a list of tokens
        //      ADDOBJECT(cName, cClass [,OLEClass] [,aInit1 ,aInit2...]
        //
        // TODO - Look this over!  It doesn't do anything, yet!
        public int AddObjectUsingParameters(List<JAXObjects.Token> ObjectParameters)
        {
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (!InTransaction) ClearErrors();

                if (ObjectParameters.Count < 1)
                    result = 11;
                else
                {
                    string cClass = ObjectParameters[0].AsString();
                    string cName = ObjectParameters.Count > 1 ? ObjectParameters[1].AsString() : string.Empty;

                    // Set up app parameters to send
                    //App.ParameterList.Clear();
                    List<ParameterClass> cParams = [];

                    if (ObjectParameters.Count > 2)
                    {
                        for (int i = 2; i < ObjectParameters.Count; i++)
                        {
                            ParameterClass p = new();
                            p.token.Element.Value = ObjectParameters[i].Element.Value;
                            cParams.Add(p);
                        }
                    }

                    // CHECK NAME

                    // Everything is ok, so create the object
                    JAXObjectWrapper jow = new(App, cClass, cName, cParams);

                    if (jow.thisObject is not null)
                    {
                        // Successful! Set the parent and add it
                        // to the objects array
                        jow.SetParent(this);
                        int i = AddObject(jow);

                        if (i >= 0)
                        {
                            jow.thisObject.SetObjectIDX(i);
                        }
                        else
                            result = 1904;
                    }
                    else
                    {
                        // Failed to instantiate
                        result = 1902;
                    }
                }
            }
            catch (Exception ex)
            {
                result = 9999;
                msg = ex.Message;
            }

            if (result > 0)
            {
                AddError(result, -1, msg, string.Empty);
                result = -1;
            }

            return result;
        }


        // Sometimes we have an object to which we want to add to another object
        public int AddObject(JAXObjectWrapper eClass)
        {
            int result = 0;
            int ccount = -1;
            JAXObjects.Token tk;
            int objIdx = -1;
            string msg = string.Empty;

            if (!InTransaction) ClearErrors();

            if (thisObject is null)
                result = 1901;
            else
            {

                // Does this object support the objects array?
                if (thisObject.GetProperty("controlcount", out tk) == 0)
                    ccount = tk.AsInt();

                try
                {
                    string className = string.Empty;
                    if (thisObject.GetProperty("class", out tk) == 0)
                        className = tk.AsString();

                    if (ccount < 0)
                        result = 3016;
                    else
                    {
                        // Can't add an object with no name or class defined as an object
                        // must be defined as a property to be included in this class
                        if (eClass.IsMember("name").Equals("P") && eClass.IsMember("class").Equals("P"))
                        {
                            result = eClass.GetProperty("name", out tk);

                            if (result == 0)
                            {
                                string name = tk.AsString();

                                if (name.Length == 0)
                                {
                                    // Need to create a name so loop through all objects looking for
                                    // the same base class and comparing eClass default name plus a
                                    // counter to existing names until we find the highest matching
                                    // name.  If no matches, then it's number 1.
                                    int nameCount = 1;
                                    string nameTemplate = eClass.DefaultName() + "{0}";

                                    for (int i = 0; i < ccount; i++)
                                    {
                                        string nameTry = string.Format(nameTemplate, i);
                                        result = eClass.GetProperty("baseclass", out tk);

                                        if (result == 0)
                                        {
                                            result = thisObject.GetObject(i).GetProperty("baseclass", out JAXObjects.Token tk2);

                                            if (result == 0)
                                            {
                                                if (tk.AsString().Equals(tk2.AsString(), StringComparison.OrdinalIgnoreCase))
                                                {
                                                    result = thisObject.GetObject(i).GetProperty("name", out tk);
                                                    if (result == 0)
                                                    {
                                                        if (nameTry.Equals(tk.AsString(), StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            nameCount++;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }

                                    if (result == 0)
                                        eClass.SetProperty("name", string.Format(nameTemplate, nameCount));
                                }


                                // We've been given a name - make sure it's not alreay used
                                if (result == 0 && ccount >= 0)
                                {
                                    // Get the object name
                                    result = eClass.GetProperty("name", out tk);

                                    if (result == 0)
                                    {
                                        string cName = tk.AsString();

                                        for (int i = 0; i < ccount; i++)
                                        {
                                            // Check this object name with all others
                                            result = thisObject.GetObject(i).GetProperty("name", out tk);

                                            if (result == 0)
                                            {
                                                if (cName.Equals(tk.AsString(), StringComparison.OrdinalIgnoreCase))
                                                {
                                                    // Same name alreay in use
                                                    result = 3014;
                                                    break;
                                                }
                                            }
                                            else
                                                break;
                                        }
                                    }
                                }
                            }

                            if (result == 0)
                            {
                                // Everything is fine, so set the parent property
                                // and add it to the objects array
                                eClass.SetParent(this);
                                objIdx = thisObject.AddObject(eClass);

                                if (objIdx >= 0)
                                {
                                    result = thisObject.SetObjectIDX(objIdx);

                                    if (result == 0)
                                        result = thisObject._CallMethod("AddObject");
                                }
                            }
                        }
                        else
                            result = 3015;
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                    result = 9999;
                }
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }


        // Other times we want to create a new object and add it to an object
        public int AddObject(string cName, string cClass)
        {
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (!InTransaction) ClearErrors();

                if (thisObject is null)
                    result = 1901;
                else
                {
                    JAXObjectWrapper eClass = new(App, cClass, "", null);
                    JAXObjects.Token tk;

                    int ccount = -1;

                    // Does this object support the objects array?
                    if (thisObject.GetProperty("controlcount", out tk) == 0)
                        ccount = tk.AsInt();

                    string className = string.Empty;
                    if (thisObject.GetProperty("class", out tk) == 0)
                        className = tk.AsString();

                    if (eClass.IsMember("baseclass").Equals("P") && eClass.IsMember("name").Equals("P"))
                    {
                        if (ccount < 0)
                        {
                            // This object doesn't allow objects to be added
                            result = 3016;
                        }
                        else
                        {
                            if (ccount >= 0)
                            {
                                string name = eClass.DefaultName();
                                int HighestVal = 1;

                                if (cName.Length == 0)
                                {
                                    for (int i = 0; i < ccount; i++)
                                    {
                                        result = thisObject.GetObject(i).GetProperty("name", out tk);
                                        if (result == 0)
                                        {
                                            string objName = tk.AsString()[..name.Length];
                                            if (objName.Equals(name[..name.Length]))
                                            {
                                                if (int.TryParse(objName[name.Length..], out int testVal) == false) testVal = 0;
                                                HighestVal = HighestVal < testVal ? testVal : HighestVal;
                                            }
                                        }
                                        else
                                            break;
                                    }

                                    result = eClass.SetProperty("name", string.Format(name + "{0}", HighestVal + 1));
                                }
                            }

                            // If the new object was created, try to add it to this object
                            if (result == 0)
                                result = AddObject(eClass);
                        }
                    }
                    else
                    {
                        // Can't add an empty object
                        result = 3015;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                result = 9999;
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Add a property directly.  Used internally.
        /// </summary>
        /// <param name="cPropertyName"></param>
        /// <param name="eNewValue"></param>
        public int AddPropertyDirect(string cPropertyName, JAXObjects.Token eNewValue)
        {
            int result = 0;
            string msg = string.Empty;

            if (thisObject is null)
                result = 1901;
            else
                result = thisObject.AddProperty(cPropertyName, eNewValue);

            if (result > 0)
            {
                App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                result = -1;
            }

            return result;
        }


        /// <summary>
        /// Add a property directly.  Used internally.
        /// </summary>
        /// <param name="cPropertyName"></param>
        /// <param name="eNewValue"></param>
        public int AddPropertyValue(string cPropertyName, object? eNewValue)
        {
            int result = 0;
            string msg = string.Empty;

            if (thisObject is null)
                result = 1901;
            else
            {
                JAXObjects.Token tk = new();
                if (eNewValue is null)
                    tk.Element.MakeNull();
                else
                    tk.Element.Value = eNewValue;

                result = thisObject.AddProperty(cPropertyName, tk);
            }

            if (result > 0)
            {
                App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Add a proprty and value to the object and trigger the JAX Method
        /// </summary>
        /// <param name="cPropertyName"></param>
        /// <param name="eNewValue"></param>
        /// <param name="nVisiblity"></param>
        /// <param name="cDescription"></param>
        /// <returns></returns>
        /// 
        // TODO - add visibility & description support
        public int AddProperty(string cPropertyName, JAXObjects.Token eNewValue, int nVisiblity, string cDescription)
        {
            // nVisibility & cDescription are not supported at this time
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (thisObject is null)
                    result = 1901;
                else
                {
                    if (!InTransaction) ClearErrors();

                    result = thisObject.AddProperty(cPropertyName, eNewValue);
                    thisObject.UserProperties[cPropertyName].PropType = "U";   // User defined

                    // TODO - set up parameter list
                    if (result == 0)
                        result = MethodCall("AddProperty");
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                result = 9999;
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }


        /// <summary>
        /// Returns the JAXObjectsAux.MethodClass for the specified method.  If not found, Type will be empty.
        /// </summary>
        /// <param name="meth"></param>
        /// <returns></returns>
        public JAXObjectsAux.MethodClass MethodInfo(string meth)
        {
            JAXObjectsAux.MethodClass result = new();
            meth = meth.ToLower();

            if (thisObject is not null && thisObject.Methods.ContainsKey(meth))
                result = thisObject.Methods[meth];

            return result;
        }


        /// <summary>
        /// Uncontrolled access for getting the method/event list. String returned as Name+Type (Ex: CLICKE)
        /// </summary>
        /// <returns></returns>
        public List<string> GetMethodList()
        {
            List<string> props = [];
            int err = 0;
            string msg = string.Empty;

            try
            {
                if (thisObject is null)
                    err = 1901;
                else
                {
                    // Populate the list
                    foreach (KeyValuePair<string, JAXObjectsAux.MethodClass> ky in thisObject.Methods)
                        props.Add(ky.Key.ToUpper());

                    // Sort the list
                    props.Sort();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                err = 9999;
            }

            if (err > 0)
                App.SetError(err, $"{err}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);

            // Return the list
            return props;
        }

        /// <summary>
        /// Uncontrolled access for getting the property list.
        /// </summary>
        /// <returns></returns>
        public List<string> GetPropertyList()
        {
            List<string> props = [];
            int err = 0;
            string msg = string.Empty;

            try
            {
                if (thisObject is null)
                    err = 1901;
                else
                {
                    // Populate the list
                    foreach (KeyValuePair<string, JAXObjects.Token> ky in thisObject.UserProperties)
                        props.Add(ky.Key.ToUpper());

                    // Sort the list
                    props.Sort();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                err = 9999;
            }

            if (err > 0)
                App.SetError(err, $"{err}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);

            // Return the list
            return props;
        }

        /// <summary>
        /// Get a property from this class
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetProperty(string name, out JAXObjects.Token tk)
        {
            tk = new();
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (!InTransaction) ClearErrors();

                if (thisObject is null)
                    result = 1901;
                else
                {
                    result = thisObject.GetProperty(name, out JAXObjects.Token tk1);
                    if (result == 0)
                        tk = tk1;
                    else
                        tk.Element.MakeNull();  // Property is not a member
                }
            }
            catch (Exception ex)
            {
                tk.Element.MakeNull();
                result = 9999;
                msg = ex.Message;
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Get an property from the class with an element index
        /// </summary>
        /// <param name="name"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public int GetProperty(string name, int idx, out JAXObjects.Token tk)
        {
            tk = new();
            JAXObjects.Token tk1 = new();
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (!InTransaction) ClearErrors();

                if (thisObject is null)
                    result = 1901;
                else
                {
                    result = thisObject.GetProperty(name, idx, out tk1);

                    if (result == 0)
                        tk = tk1;
                    else
                        tk.Element.MakeNull();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                tk.Element.MakeNull();
                result = 9999;
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Return the object index for the supplied name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int FindObjectByName(string name)
        {
            int result = -1;
            int err = 0;
            string msg = string.Empty;

            try
            {
                name = name.Trim();
                int objCount = -1;

                if (thisObject is null)
                    err = 1901;
                else
                {

                    if (thisObject.GetProperty("controlcount", out JAXObjects.Token tk) == 0)
                        objCount = tk.AsInt();

                    for (int i = 0; i < objCount; i++)
                    {
                        JAXObjectWrapper obj = thisObject.GetObject(i);
                        string memb = obj.IsMember("name");    // Is there a name property?

                        if (memb.Equals("P"))
                        {
                            JAXObjects.Token tk1 = new();
                            err = obj.GetProperty("name", out tk1);
                            if (err == 0)
                            {
                                if (tk1.AsString().Equals(name, StringComparison.OrdinalIgnoreCase))
                                {
                                    result = i;
                                    break;
                                }
                            }
                            else
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = -1;
                err = 9999;
                msg = ex.Message;
            }

            // Handle any error we find
            if (err > 0)
            {
                if (err == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /*
        /// <summary>
        /// Controlled Access - Get property.  Token is sent out, returns bool if object was found.  If not, token is logical false.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool GetProperty(string name, out JAXObjects.Token token)
        {
            bool result = true;
            token = new();

            try
            {
                if (!InTransaction) ClearErrors();

                if (thisObject is null)
                    throw new Exception("1901|");

                int err = thisObject.GetProperty(name, out JAXObjects.Token tk);
                if (err == 0)
                    token = tk;
                else
                {
                    // Property is not a member
                    result = false;
                    thisObject._AddError(err, App.AppLevels[^1].CurrentLine, JAXErrorList.JAXErrMsg(1559, string.Empty), App.AppLevels[^1].Procedure);
                }
            }
            catch (Exception ex)
            {
                result = false;
                App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }
        */

        /*
        /// <summary>
        /// Controlled Access - Get property with index.  Token is sent out, returns bool if object was found.  If not, token is logical false.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="idx"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool GetProperty(string name, int idx, out JAXObjects.Token token)
        {
            bool result = true;
            token = new();

            try
            {
                if (thisObject is null)
                    throw new Exception("1901|");

                int err = thisObject.GetProperty(name, out JAXObjects.Token tk);
                if (err == 0)
                    token = tk;
                else
                {
                    // Property is not a member
                    result = false;
                    thisObject._AddError(err, App.AppLevels[^1].CurrentLine, JAXErrorList.JAXErrMsg(1559, string.Empty), App.AppLevels[^1].Procedure);
                }
            }
            catch (Exception ex)
            {
                // TODO - Add error support
                result = false;
                App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }
        */

        /// <summary>
        /// Set the property of an element in the Objects array.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public int SetObject(int idx, string property, JAXObjects.Token value)
        {
            int result = 0;
            string msg = string.Empty;

            if (!InTransaction) ClearErrors();
            if (thisObject is null)
                result = 1901;
            else
            {
                // TODO - Check to see if ccount>=0 and check to see ccount>idx
                result = thisObject.SetObjectProperty(idx, property, value);
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Get an element from the Objects array by name.  Sends out index and returns a JOW.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public int GetObject(string name, out JAXObjectWrapper? jow)
        {
            int idx = -1;
            int result = 0;
            string msg = string.Empty;
            jow = null;

            if (!InTransaction) ClearErrors();
            if (thisObject is null)
                result = 1901;
            else
            {
                // TODO - this call needs to change!
                jow = thisObject.GetObject(name, out idx);
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }
            else
                result = idx;

            return result;
        }

        /// <summary>
        /// Get an element from the Objects array by index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public int GetObject(int idx, out JAXObjectWrapper? jow)
        {
            int result = 0;
            string msg = string.Empty;
            jow = null;

            if (!InTransaction) ClearErrors();

            if (thisObject is null)
                result = 1901;
            else
            {
                jow = thisObject.GetObject(idx);
            }

            // TODO - Check to see if ccount>=0 and check to see ccount>idx
            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Set a class property by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int SetProperty(string name, object value)
        {
            int result = 0;
            string msg = string.Empty;

            try
            {
                if (thisObject is null)
                    result = 1901;
                else
                {
                    if (!InTransaction) ClearErrors();

                    if (thisObject.IsMember(name).Equals("P"))
                    {
                        thisObject.SetProperty(name, value, 0);
                        if (name.Equals("name", StringComparison.OrdinalIgnoreCase))
                            Name = value.ToString() ?? string.Empty;

                        App.DebugLog($"Updated {Name}.{name} -> {value}");
                    }
                    else
                    {
                        // Property is not a member
                        result = 1559;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO - Add error support
                result = 9999;
                msg = ex.Message;
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Remove an element from the Objects array by index.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int RemoveObject(int x)
        {
            int result = 0;
            string msg = string.Empty;

            if (!InTransaction) ClearErrors();

            if (thisObject is null)
                result = 1901;
            else
            {
                // TODO - get ccount and compare to x
                result = thisObject.RemoveObject(x);
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Remove an element from the Objects array by name.
        /// </summary>
        /// <param name="cName"></param>
        /// <returns></returns>
        public int RemoveObject(string cName)
        {
            if (!InTransaction) ClearErrors();

            int result = -1;
            int ccount = -1;
            int idx = -1;
            string msg = string.Empty;

            if (thisObject is null)
                result = 1901;
            else
            {
                if (thisObject.GetProperty("controlcount", out JAXObjects.Token tk) == 0)
                    ccount = tk.AsInt();

                for (int i = 0; i < ccount; i++)
                {
                    result = thisObject.GetObject(i).GetProperty("name", out tk);
                    if (cName.Equals(tk.AsString(), StringComparison.OrdinalIgnoreCase))
                    {
                        // Found it, so going to remove it
                        thisObject.RemoveObject(i);
                        idx = i;
                        break;
                    }
                }
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }
            else
                result = idx;

            return result;
        }


        //---------------------------------------------------------
        // I think this needs to go to the base class
        //---------------------------------------------------------
        /// <summary>
        /// Make the form visible.
        /// </summary>
        /// <returns></returns>
        public int Show()
        {
            if (!InTransaction) ClearErrors();

            int result = 0;
            string msg = "SHOW";

            try
            {
                if (thisObject is null)
                    result = 1901;
                else
                {
                    if (JAXLib.InListC(BaseClass, "form", "formset", "browser"))
                    {
                        if (BaseClass.Equals("formset", StringComparison.OrdinalIgnoreCase))
                        {
                            // Formset calls top Form
                            int i = -1;
                            result = thisObject.GetProperty("controlcount", out JAXObjects.Token tk);
                            if (result == 0)
                            {
                                i = tk.AsInt();

                                if (i > 0)
                                {
                                    JAXObjectWrapper obj = thisObject.GetObject(i);
                                    obj.MethodCall("show");
                                }
                                else
                                    result = 1559;
                            }
                        }
                        else
                            visualObject?.Show();
                    }
                    else
                        result = 1559;
                }
            }
            catch (Exception ex)
            {
                result = 9999;
                msg = ex.Message;
            }


            if (result > 0)
            {
                msg = JAXErrorList.JAXErrMsg(result, msg);
                thisObject!._AddError(result, App.AppLevels[^1].CurrentLine, msg, App.AppLevels[^1].Procedure);

                if (string.IsNullOrWhiteSpace(App.AppLevels[^1].Procedure))
                    App.SetError(result, $"{result}|{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Set the ZOrder of the object. JAXBase allows you to order objects explicity.  nOrder>ordercount = bottom of order while nOrder >=0 and < nOrderCount = insert at element.
        /// </summary>
        /// <param name="nOrder"></param>
        public int ZOrder(int nOrder)
        {
            int result = 0;
            string msg = string.Empty;

            if (!InTransaction) ClearErrors();
            if (thisObject is null)
                result = 1901;
            else
            {
                result = thisObject.GetProperty("baseclass", out JAXObjects.Token tk);

                if (result == 0)
                {
                    if (thisObject.IsMember("zorder").Equals("P"))
                    {
                        result = thisObject.GetProperty("parent", out JAXObjects.Token par);
                        if (result == 0)
                        {
                            if (par.TType.Equals("O"))
                            {
                                JAXObjectWrapper parent = (JAXObjectWrapper)par.Element.Value;
                                result = parent.GetProperty("controlcount", out tk);

                                if (result == 0)
                                {
                                    int cCount = tk.AsInt();

                                    int myIDX = thisObject.GetObjectIDX();

                                    // TODO - Change it's order
                                    result = parent.GetObject(myIDX, out JAXObjectWrapper? obj);

                                    if (result == 0)
                                    {
                                        if (nOrder <= 0)
                                        {
                                            // Top of order
                                            parent.RemoveObject(myIDX);
                                        }
                                        else if (nOrder >= cCount)
                                        {
                                            // Bottom of order
                                            parent.RemoveObject(myIDX);
                                            parent.AddObject(obj!);
                                        }
                                        else
                                        {
                                            // Place it here
                                            parent.RemoveObject(myIDX);
                                        }
                                    }
                                }
                            }
                            else
                                result = 9999;
                        }
                    }
                    else
                        result = 3018;  // ZOrder isn't a property
                }
                else
                    result = 3018; // TODO - it's an empty class
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            return result;
        }

        /*
         * Set the number of child objects of a specific class to be in this object
         * Typically used by CommandButtonGroup and OptionGroup
         * 
         * Need to override InTransaction if it's not already set so that errors
         * can be captured.  If an error is discovered, drop out on first sighting
         * so that they don't build up.
         * 
         * In theory, there should never be an error, but things happen, yeah?
         * 
         */
        /// <summary>
        /// Set the number of child objects of the specified cClass.  Naming will start at defaultName1.
        /// </summary>
        /// <param name="nObjectCount"></param>
        /// <param name="cClass"></param>
        /// <returns></returns>
        public int SetObjectCount(int nObjectCount, string cClass)
        {
            int result;
            string msg = string.Empty;
            bool WasInTransaction = InTransaction;
            JAXObjects.Token tk;

            if (!WasInTransaction)
            {
                InTransaction = true;
                ClearErrors();
            }

            if (thisObject is null)
                result = 1901;
            else
            {
                if (nObjectCount >= 0)
                {
                    int sc = -1; // Starting count

                    // Does this object support the objects array?
                    result = thisObject.GetProperty("controlcount", out tk);
                    if (result == 0)
                    {
                        sc = tk.AsInt();

                        int cc = sc;                                                // Current count;

                        // TODO - is SC >=0:

                        if (nObjectCount > sc)
                        {
                            // Adding objects
                            while (cc < nObjectCount)
                            {
                                AddObject(cClass, string.Empty);

                                result = thisObject.GetProperty("aerror", out tk);
                                if (result == 0 && tk._avalue[0].ValueAsInt > 0)
                                    break;

                                // TODO - if command or option, look at the area
                                // of the frame and space each accordingly
                            }
                        }
                        else if (nObjectCount < sc)
                        {
                            // Removing objects
                            while (nObjectCount < cc)
                            {
                                RemoveObject(nObjectCount);

                                result = thisObject.GetProperty("aerror", out tk);
                                if (result == 0 && tk._avalue[0].ValueAsInt > 0)
                                    break;
                            }
                        }
                    }
                }
                else
                    result = 2020; // Sent in a negative count - let them know it's not cool


                if (result == 0)
                {
                    // Now check the results
                    result = -1;
                    if (thisObject.GetProperty("controlcount", out tk) == 0)
                        result = tk.AsInt();

                    if (result != nObjectCount)
                    {
                        // TODO - OOPS!
                        result = 9999;
                        msg = "Failed to create expecte number of objects";
                    }
                }
            }

            if (result > 0)
            {
                if (result == 1901)
                    App.SetError(result, $"{result}||{msg}", System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                else
                    AddError(result, -1, msg, string.Empty);

                result = -1;
            }

            // Reset the global back to original state
            InTransaction = WasInTransaction;

            return result == 0 ? nObjectCount : -1;
        }
    }
}

