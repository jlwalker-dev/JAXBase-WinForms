namespace JAXBase
{
    public class XBase_Class_Collection :XBase_Class
    {
        public XBase_Class_Collection(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            name = string.IsNullOrEmpty(name) ? "collection" : name;
            SetVisualObject(null, "Collection", name, false, UserObject.URW);
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
