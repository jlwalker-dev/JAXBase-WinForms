namespace JAXBase
{
    public class XBase_Class_Visual_PageFrame : XBase_Class_Visual
    {
        public TabControl pgFrame => (TabControl)me.visualObject!;

        public XBase_Class_Visual_PageFrame(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new TabControl(), "PageFrame", "pgFrame", true, UserObject.URW);
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
         *      >10 - Error code
         * 
         * 
         * Return from here
         *      0   - Successfully processed
         *      >0  - Error Code
         *      
         *------------------------------------------------------------------------------------------*/
        public new virtual int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            propertyName = propertyName.ToLower();

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
                    if (result == 1 || result == 2)
                    {
                        result = 0;

                        // First, we check to make sure that the property exists
                        if (UserProperties.ContainsKey(propertyName))
                        {
                            // Visual object common property handler
                            switch (propertyName)
                            {
                                case "test":
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

                    // Visual object common property handler
                    switch (propertyName.ToLower())
                    {
                        case "activepage":
                        case "pagecount":
                        case "taborientation":
                        case "tabstretch":
                        case "tabstyle":
                        case "tabs":
                            result = 1559;
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
                "addproperty", "addobject", "move", "readexpression", "readmethod", "refresh", "resettodefault",
                "saveasclass", "settooriginalvalue", "setfocus", "writeexpression", "writemethod", "zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","dragdrop","dragover","error","gotfocus",
                "init","keypress","load","lostfocus",
                "middleclick","mousedown","mouseenter","mousehover","mouseleave","mousemove","mouseup","mousewheel",
                "rightclick","visiblechanged","when"
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
            return [
                "activepage,N!,0","anchor,n,0",
                "backcolor,R,240|240|240","bordercolor,R,100|100|100","borderwidth,N,1",
                "baseclass,C!,pageframe",
                "class,C!,pageframe","classlibrary,C!,","comment,C,","controlcount,N!,0",
                "Enabled,L,true",
                "forecolor,R,0",
                "Height,N,200",
                "keypreview,L,false",
                "left,N,0",
                "name,C,pageframe",
                "objects,*,",
                "pagecount,n,2","parent,o!,","parentclass,C!,","picture,C,",
                "righttoleft,L,",
                "tag,C,","tabindex,N,1","taborientation,N,0","tabstop,L,true","tabstretch,N,1","tabstyle,n,0","tabs,L,.T.","tooltiptext,c,","top,N,0",
                "visible,L,true",
                "width,N,200"
                ];
        }
    }
}
