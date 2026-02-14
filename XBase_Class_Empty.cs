/*
 * The Empty Class.  It's a thing of beauty!
 */
namespace JAXBase
{
    public class XBase_Class_Empty : XBase_Class
    {
        public XBase_Class_Empty(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(null, "Empty", string.Empty, false, UserObject.urw);
            me.BaseClass = "EMPTY";
            me.Class = string.IsNullOrWhiteSpace(name) ? "EMPTY" : name;
            me.ClassID = jow.App.SystemCounter();
        }
    }
}
