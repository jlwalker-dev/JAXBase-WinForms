/*
 * Dedicated line class - will only hold two (2) in the points property.
 * Ignores fillstyle, fillcolor, and polypoints properties.
 * 
 * Line class will also attempt to use as little real estate as possible
 * by adjusting HEIGHT/WIDTH on the fly.
 * 
 */
namespace JAXBase
{
    public class XBase_Class_Visual_Line : XBase_Class_Visual_ShapeBase
    {
        public XBase_Class_Visual_Line(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new PictureBox(), "Line", "line", true, UserObject.urw);
        }
    }
}
