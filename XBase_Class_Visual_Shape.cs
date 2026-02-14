/*
 * Shape class can also draw a line, however the height/width is
 * always the same and can take up a lot of real estate.
 * 
 * Testing shows the curvature is largely unneeded to create a
 * nice looking circle or ellipse if you put in 1000 or a higher
 * number for Points.  Speed does not seem to suffer.
 * 
 */
namespace JAXBase
{
    public class XBase_Class_Visual_Shape : XBase_Class_Visual_ShapeBase
    {
        public XBase_Class_Visual_Shape(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new PictureBox(), "Shape", "shape", true, UserObject.urw);
        }
    }
}
