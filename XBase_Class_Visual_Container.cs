using System.Drawing.Drawing2D;

namespace JAXBase
{
    internal class XBase_Class_Visual_Container : XBase_Class_Visual
    {

        public JAXPanel container => (JAXPanel)me.visualObject!;

        public XBase_Class_Visual_Container(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new JAXPanel(), "Container", "container", true, UserObject.URW);
            container.DoubleBuffered = true;
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
         * Handle the commmon properties by calling the base and then handle the special cases.
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
         *     >0   - Error Code
         *      
         *------------------------------------------------------------------------------------------*/
        public override int SetProperty(string propertyName, object objValue, int objIdx)
        {
            int result = 0;
            JAXObjects.Token tk = new();
            tk.Element.Value = objValue;

            propertyName = propertyName.ToLower();

            if (UserProperties.ContainsKey(propertyName) && UserProperties[propertyName].Protected)
                result = 3026;
            else
            {
                if (UserProperties.ContainsKey(propertyName))
                {
                    switch (propertyName)
                    {
                        case "borderstyle":
                            if (tk.Element.Type.Equals("N"))
                            {
                                // Make sure it's a number between 0 and 4
                                if (JAXLib.Between(tk.AsInt(), 0, 4))
                                {
                                    objValue = tk.AsInt();

                                    container.BorderDashStyle = tk.AsInt() switch
                                    {
                                        1 => DashStyle.Dash,
                                        2 => DashStyle.Dot,
                                        3 => DashStyle.DashDot,
                                        4 => DashStyle.DashDotDot,
                                        _ => DashStyle.Solid
                                    };
                                }
                                else
                                    result = 41;
                            }
                            else
                                result = 11;
                            break;

                        case "dock":
                            if (tk.Element.Type.Equals("N"))
                            {
                                // Make sure it's an integer
                                objValue = tk.AsInt();

                                switch (tk.AsInt())
                                {
                                    case 0: container.Dock = DockStyle.None; break;
                                    case 1: container.Dock = DockStyle.Left; break;
                                    case 2: container.Dock = DockStyle.Right; break;
                                    case 3: container.Dock = DockStyle.Top; break;
                                    case 4: container.Dock = DockStyle.Bottom; break;
                                    case 5: container.Dock = DockStyle.Fill; break;
                                    default:
                                        objValue = UserProperties["dock"];
                                        break;
                                }

                                container.Invalidate(); // in case there's a grid
                                container.Refresh();
                            }
                            else
                                result = 11;
                            break;

                        case "dotspacing":
                            if (tk.Element.Type.Equals("N"))
                            {
                                int s = tk.AsInt();

                                // ignore invalid values
                                s = s < 0 ? 0 : s;
                                s = s > 128 ? 128 : s;

                                // Set it up and force it to show up
                                UserProperties["dotspacing"].Element.Value = s;  // Need to to this before calling contaner_Paint()
                                container.Invalidate();
                                container.Refresh();
                            }
                            else
                                result = 11;
                            break;

                        case "height":
                            if (tk.Element.Type.Equals("N"))
                            {
                                int s = tk.AsInt();
                                if (s < 0)
                                    result = 41;
                                else
                                    container.Height = s;
                            }
                            else
                                result = 11;
                            break;

                        case "width":
                            if (tk.Element.Type.Equals("N"))
                            {
                                int s = tk.AsInt();
                                if (s < 0)
                                    result = 41;
                                else
                                    container.Width = s;
                            }
                            else
                                result = 11;
                            break;

                        default:
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            result = result == 0 ? 9 : result;
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result, 0, 10))
                    {
                        if (result < 9)
                        {
                            // We processed it or just need to save the property
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
            else
                result = 0;

            return result;
        }


        /*------------------------------------------------------------------------------------------*
             * GetProperty method returns 
             *      0 = Successfully returning value
             *      1 = Not processed, returning .F.
             *      
             *    >10 = Error code
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
                    case "borderstyle":
                        returnToken.Element.Value = container.BorderStyle switch
                        {
                            BorderStyle.FixedSingle => 1,
                            BorderStyle.Fixed3D => 2,
                            _ => 0
                        };
                        break;

                    case "height":
                        returnToken.Element.Value = container.Height;
                        break;

                    case "width":
                        returnToken.Element.Value = container.Width;
                        break;


                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (result > 0 && result < 10)
                {
                    if (result < 9)
                        returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;

                    result = 0;
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
                "activecontrol,N!,0","anchor,n,0",
                "backcolor,R,240|240|240","backstyle,n,1","bordercolor,R,100|100|100","borderstyle,N,0","borderwidth,N,1",
                "baseclass,C!,container",
                "class,C!,container","classlibrary,C!,","comment,C,","controlcount,N!,0",
                "dock,n,0","dotspacing,n,0",
                "Enabled,L,true",
                "forecolor,R,0",
                "Height,N,200",
                "keypreview,L,false",
                "left,N,0",
                "name,C,container",
                "objects,*,",
                "parent,o!,","parentclass,C!,","picture,C,",
                "tag,C,","tabindex,N,1","tabstop,L,true","top,N,0","tooltiptext,c,",
                "visible,L,true",
                "width,N,200"
                ];
        }


        private void contaner_Paint(object sender, PaintEventArgs e)
        {
            int spacing = UserProperties["dotspacing"].AsInt();

            if (spacing > 0)  // Spacing <1 turns off the dots as it has to repaint them each time!
            {
                /* backup in case I need to play around with it
                Graphics g = e.Graphics;
                Brush dotBrush = Brushes.Black;  // Dot color
                int dotSize = 2;  // Diameter of each dot

                for (int x = 0; x < container.Width; x += spacing)
                {
                    for (int y = 0; y < container.Height; y += spacing)
                    {
                        g.FillEllipse(dotBrush, x - dotSize / 2, y - dotSize / 2, dotSize, dotSize);
                    }
                }
                */

                Size dotSpacing = new(spacing, spacing);
                ControlPaint.DrawGrid(e.Graphics, container.ClientRectangle, dotSpacing, container.BackColor);
            }
        }
    }
}