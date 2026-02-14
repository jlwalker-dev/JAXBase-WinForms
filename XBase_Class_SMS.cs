namespace JAXBase
{
    public class XBase_Class_SMS : XBase_Class
    {
        // === SMS SETTINGS ===

        public XBase_Class_SMS(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            name = string.IsNullOrEmpty(name) ? "sms" : name;
            SetVisualObject(null, "SMS", name, false, UserObject.URW);
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = base.PostInit(callBack, parameterList);

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------

            return result;
        }
    }
}
