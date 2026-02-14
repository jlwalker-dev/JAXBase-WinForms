/*------------------------------------------------------------------------------------------*
 * Create an array that holds a png image of a shape.
 * 
 * The shape can be a line, any poly of 3 or more sides, any elipse, or
 * a complex poly created using the polypoints array.
 * 
 * Include: SixLabors.ImageSharp
 *          SixLabors.Drawing
 *          
 *------------------------------------------------------------------------------------------*/
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using System.Windows.Media;

namespace JAXBase
{
    public class XBase_Class_Visual_ShapeBase : XBase_Class_Visual
    {
        SixLabors.ImageSharp.PointF[] polygonPoints = [];

        public PictureBox shape => (PictureBox)me.visualObject!;

        public XBase_Class_Visual_ShapeBase(JAXObjectWrapper jow, string name) : base(jow, name)
        {
            SetVisualObject(new PictureBox(), name, name, true, UserObject.urw);
        }

        public override bool PostInit(JAXObjectWrapper? callBack, List<ParameterClass> parameterList)
        {
            bool result = false;

            // ----------------------------------------
            // Final setup of properties
            // ----------------------------------------
            if (InInit)
            {
                // Make sure the defaults are as we need them
                shape.BorderStyle = BorderStyle.None;

                if (me.BaseClass.Equals("line", StringComparison.OrdinalIgnoreCase))
                {
                    // Make sure the defaults are as we need them
                    shape.BorderStyle = BorderStyle.None;
                    SetProperty("height", 1, 0);
                    SetProperty("width", 100, 0);
                    SetProperty("rotation", 90, 0);
                    SetProperty("points", 2, 0);
                    UserProperties["points"].Protected = true;
                }

                result = base.PostInit(callBack, parameterList);
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
         *      9   - Success, do nothing more
         *      10  - <same as 9 for now>
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
            JAXObjects.Token objtk = new();
            objtk.Element.Value = objValue;
            string objType = objtk.Element.Type;
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
                        case "polypoints":
                            // TODO - Convert the array
                            if (objtk.TType.Equals("A") && objtk.Row > 0 && objtk.Col > 1)
                            {
                                polygonPoints = new SixLabors.ImageSharp.PointF[objtk.Col];
                                for (int i = 0; i < objtk.Row; i++)
                                {
                                    objtk.SetElement(i, 1);
                                    int x = objtk.AsInt();
                                    objtk.SetElement(i, 2);
                                    int y = objtk.AsInt();
                                    polygonPoints[i] = new(x, y);
                                }

                                result = 9;
                                //FixPicture();
                            }

                            break;

                        case "points":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                int x = objtk.AsInt();
                                x = x > 1 ? x : throw new Exception("11|");
                                UserProperties[propertyName].Element.Value = x;
                                polygonPoints = [];
                                result = 9;
                                FixShape();
                            }
                            break;

                        case "fillstyle":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                int x = objtk.AsInt();
                                if (JAXLib.Between(x, 0, 5))
                                {
                                    UserProperties[propertyName].Element.Value = objtk.AsInt();
                                    result = 9;
                                    FixShape();
                                }
                            }
                            break;

                        case "fillcolor":
                            UserProperties[propertyName].Element.Value = JAXUtilities.ReturnColorInt(objtk.Element.Value);
                            result = 9;
                            break;

                        case "bordercolor":
                            UserProperties[propertyName].Element.Value = JAXUtilities.ReturnColorInt(objtk.Element.Value);
                            result = 9;
                            break;

                        case "borderstyle":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                int x = objtk.AsInt();
                                x = JAXLib.Between(x, 0, 6) ? x : throw new Exception("11|");
                                UserProperties[propertyName].Element.Value = x;
                                result = 9;
                                FixShape();
                            }
                            break;

                        case "borderwidth":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                int x = objtk.AsInt();
                                x = JAXLib.Between(x, 0, 255) ? x : throw new Exception("11|");
                                UserProperties[propertyName].Element.Value = x;
                                result = 9;
                                FixShape();
                            }
                            break;

                        case "rotation":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                double x = objtk.AsDouble();
                                if (x < 0)
                                {
                                    x = Math.Abs(x);
                                    x = x % 360.00D;
                                    x = -x;
                                }
                                else
                                    x = x % 360.00D;

                                UserProperties[propertyName].Element.Value = x;
                                result = 9;
                                FixShape();
                            }
                            break;

                        case "scale":
                            if (objtk.Element.Type.Equals("N"))
                            {
                                double x = objtk.AsDouble();
                                if (JAXLib.Between(x, 0D, 100D))
                                {
                                    UserProperties[propertyName].Element.Value = x;
                                    result = 9;
                                    FixShape();
                                }
                            }
                            break;



                        case "height":
                            // TODO - I may have this wrong for height/width
                            if (objtk.Element.Type.Equals("N"))
                                shape.Height = objtk.AsInt() + 2;
                            else
                                result = 11;
                            break;

                        case "width":
                            if (objtk.Element.Type.Equals("N"))
                                shape.Width = objtk.AsInt() + 2;
                            else
                                result = 11;
                            break;

                        default:
                            // Process standard properties
                            result = base.SetProperty(propertyName, objValue, objIdx);
                            break;
                    }

                    // Do we need to process this property?
                    if (JAXLib.Between(result, 0, 10))
                    {
                        if (result < 9)
                        {
                            result = 0;

                            // Visual object common property handler
                            switch (propertyName.ToLower())
                            {
                                case "***":
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

            return result;
        }


        // ------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------
        // 2026-02-04 - JLW
        //      This was one of the more error prone attempts to get GROK to help me out.
        //      We worked on getting something set up for over two hours.  Very unusual now
        //      that I've got a better handle prompting.
        //
        // 2026-02-05 - JLW
        //      Created this class as a common class for line/shape classes and added
        //      borderstyle support.  Took a stab at saving real estate for vertical
        //      and horizontal lines.
        // 
        /// <summary>
        /// Creates an image with a lie, polygon, rounded polygon (or circle/ellipse when curvature = 99)
        /// </summary>
        /// <param name="width">Final image width in pixels</param>
        /// <param name="height">Final image height in pixels</param>
        /// <param name="points">Number of polygon sides (3 = triangle, 4 = rect, 5+, etc.)</param>
        /// <param name="rotationDeg">Rotation in degrees (0 = one vertex at top by convention)</param>
        /// <param name="curvature">0 = sharp corners, 1–98 = increasing roundness, 99 = full circle/ellipse</param>
        /// <param name="borderColor">Color of the outline</param>
        /// <param name="borderWidth">Thickness of the border (in pixels)</param>
        /// <param name="fillColor">Interior fill color (use Color.Transparent for no fill)</param>
        /// <param name="fillStyle>">How the fill color is to be laid out</param>
        /// <returns>Image<Rgba32> you can save / assign to PictureBox.Image</returns>
        /// 
        /*
         * FillStyle                                    Border Style
         *      0 - Solid                                   0 - Transparent
         *      1 - Transparent                             1 - Solid
         *      2 - Horizontal line                         2 - Dash
         *      3 - Vertical line                           3 - Dot
         *      4 - Upward diagonal                         4 - Dash Dot
         *      5 - Downward diagonal                       5 - Dash Dot Dot
         *      6 - Cross <not supported>                   6 - Inside solid <not supported>
         *      7 - Diagonal Cross <not supported>
         */
        private void FixShape()
        {
            int width = UserProperties["height"].AsInt() + 2;
            int height = UserProperties["width"].AsInt() + 2;
            int points = polygonPoints.Length > 0 ? polygonPoints.Length : UserProperties["points"].AsInt();
            float rotationDeg = UserProperties["rotation"].AsFloat();
            int curvature = UserProperties["curvature"].AsInt();
            SixLabors.ImageSharp.Color borderColor = default;
            float borderWidth = UserProperties["borderwidth"].AsInt();
            int borderStyle = UserProperties["borderstyle"].AsInt();
            int rgb = UserProperties["fillcolor"].AsInt();
            int fillStyle = UserProperties["fillstyle"].AsInt();
            float scaleFactor = UserProperties["scale"].AsFloat();
            scaleFactor = MathF.Round(scaleFactor / 100.0000F, 3); // Ends up 0.000 -> 1.000

            //SixLabors.ImageSharp.Color fillColor = new(new Rgb24((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF)));

            // Set up opaque coloring (0xFF at end)
            SixLabors.ImageSharp.Color fillColor = SixLabors.ImageSharp.Color.FromRgba((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 0xFF);

            // Defaults / guards
            if (points < 2) points = 2;
            if (curvature < 0) curvature = 0;
            if (curvature > 99) curvature = 99;
            if (borderWidth < 0) borderWidth = 0;
            if (points == 4) rotationDeg += 45; // Make the rectangle horizontal/vertical

            // If it's a line
            if (points == 2)
            {
                // Special processing for lines - some properties
                // are ignored, such as fillcolor and fillstyle

                // Line rotation correction - so that makes
                // the rotation correction below as -60 which
                // really makes no sense to me
                rotationDeg += 30;

                // Set scale factor to 100% if close
                if (scaleFactor > 0.998F)
                {
                    scaleFactor = 1.000F;

                    // A scale factor of 100 means that we will
                    // try to use minimal real estate for lines


                    // Attempt to save real estate if it's a line class
                    if (me.BaseClass.Equals("line", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO - ONLY IF IT'S A LINE CLASS!
                        // Figure out the formulas to create a picture
                        // area of the exact height/width needed to allow
                        // the requested length/rotation/width to show up.
                        //
                        // The line should start at either the upper/lower
                        // left corner and go to the opposite lower/upper
                        // corner on the right side, using as little real
                        // estate as possible.

                        // First stab at using minimal real estate by minimizing
                        // size/width for horizontal and vertical lines
                        if (JAXLib.InList(rotationDeg, 0F, 180F))
                        {
                            // Adjust the image size
                            width = Convert.ToInt32(borderWidth * 2) + 1;
                            shape.Width = width;
                            shape.Left = UserProperties["left"].AsInt();
                        }

                        if (JAXLib.InList(rotationDeg, 90F, 270F))
                        {
                            // Adjust the image size
                            height = Convert.ToInt32(borderWidth * 2) + 1;
                            shape.Height = height;
                            shape.Top = UserProperties["top"].AsInt();
                        }
                    }
                }
            }

            if (scaleFactor * width > 1 && scaleFactor * height > 1)
            {
                // Calculate and draw the object
                borderColor = borderColor == default ? SixLabors.ImageSharp.Color.Black : borderColor;
                fillColor = fillColor == default ? SixLabors.ImageSharp.Color.White : fillColor;

                var image = new Image<Rgba32>(width, height, SixLabors.ImageSharp.Color.Transparent);

                float centerX = width / 2f;
                float centerY = height / 2f;

                // Base radius, inset for border
                // TODO - this gets it close, but...
                float inset = borderWidth;
                float radiusX = (width - inset * 2) / 2f;
                float radiusY = (height - inset * 2) / 2f;
                float baseRadius = Math.Min(radiusX, radiusY);

                float scaleX = radiusX / baseRadius;
                float scaleY = radiusY / baseRadius;

                // Generate points (works for 2 or more points)
                SixLabors.ImageSharp.PointF[] polyPoints = new SixLabors.ImageSharp.PointF[points];
                float angleStep = 360f / Math.Max(points, 3); // avoid div-by-2 issue

                // Rotation correction for all objects
                float startAngle = -90f + rotationDeg;         // top = 0° rotation

                for (int i = 0; i < points; i++)
                {
                    float angle = startAngle + i * angleStep;
                    float rad = angle * MathF.PI / 180f;

                    float x = centerX + MathF.Cos(rad) * baseRadius * scaleX;
                    float y = centerY + MathF.Sin(rad) * baseRadius * scaleY;

                    polyPoints[i] = new SixLabors.ImageSharp.PointF(MathF.Round(x, 0), MathF.Round(y, 0));
                }

                image.Mutate(ctx =>
                {
                    var fillBrush = GetFillBrush(fillColor, fillStyle);

                    if (points == 2)
                    {
                        // Special case: draw a simple line segment
                        if (borderWidth > 0.1f && borderColor != SixLabors.ImageSharp.Color.Transparent)
                        {
                            var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(borderColor, borderWidth);
                            ctx.DrawLine(pen, polyPoints[0], polyPoints[1]);
                        }
                    }
                    else
                    {
                        // Polygon / ellipse case
                        IPath shapePath;

                        if (curvature == 99)
                        {
                            shapePath = new EllipsePolygon(centerX, centerY, radiusX, radiusY);
                            if (Math.Abs(scaleFactor - 1f) > 0.001f)  // skip if ≈100%
                                shapePath = shapePath.Scale(scaleFactor);  // uniform X/Y scale
                        }
                        else
                        {
                            float cornerRadius = baseRadius * (curvature / 120f);
                            shapePath = CreateRoundedPolygonPath(polyPoints, cornerRadius);

                            if (Math.Abs(scaleFactor - 1f) > 0.001f)  // skip if ≈100%
                                shapePath = shapePath.Scale(scaleFactor);  // uniform X/Y scale
                        }

                        // Fill
                        if (fillColor != SixLabors.ImageSharp.Color.Transparent)
                        {
                            if (fillBrush != null)
                                ctx.Fill(fillBrush, shapePath);  // ← accepts Brush here
                        }

                        // Border
                        if (borderWidth > 0.1f && borderColor != SixLabors.ImageSharp.Color.Transparent && borderStyle != 0)
                        {
                            // SWITCH statement won't work!
                            if (borderStyle == 2)
                            {
                                var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.Dash(borderColor, borderWidth);
                                ctx.Draw(pen, shapePath);
                            }
                            else if (borderStyle == 3)
                            {
                                var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.Dot(borderColor, borderWidth);
                                ctx.Draw(pen, shapePath);
                            }
                            else if (borderStyle == 4)
                            {
                                var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.DashDot(borderColor, borderWidth);
                                ctx.Draw(pen, shapePath);
                            }
                            else if (borderStyle == 5)
                            {
                                var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.DashDotDot(borderColor, borderWidth);
                                ctx.Draw(pen, shapePath);
                            }
                            else
                            {
                                // Default is solid pen (6-Inside solid is not supported)
                                var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(borderColor, borderWidth);
                                ctx.Draw(pen, shapePath);
                            }
                        }
                    }
                });

                // Now fill the picturebox image in a way that
                // is supposed to prevent memory leaks
                using var ms = new MemoryStream();
                image.SaveAsPng(ms);
                ms.Position = 0;
                using var old = shape.Image;
                shape.Image = System.Drawing.Image.FromStream(ms);
            }
        }


        // Helper routine to set up fill style ------------------------------------------------------
        SixLabors.ImageSharp.Drawing.Processing.Brush? GetFillBrush(SixLabors.ImageSharp.Color fillColor, int fillStyle)
        {
            // fillStyle 1 = Transparent → no fill at all
            if (fillStyle == 1)
            {
                return null;  // or Brushes.Transparent, but null is cleaner for conditional Fill
            }

            // Default to solid for unknown values or 0
            if (fillStyle == 0 || fillStyle < 0 || fillStyle > 7)
            {
                return new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(fillColor);
            }

            // Hatch patterns (foreground = fillColor, background = transparent)
            return fillStyle switch
            {
                2 => SixLabors.ImageSharp.Drawing.Processing.Brushes.Horizontal(fillColor),               // Horizontal lines
                3 => SixLabors.ImageSharp.Drawing.Processing.Brushes.Vertical(fillColor),                 // Vertical lines
                4 => SixLabors.ImageSharp.Drawing.Processing.Brushes.BackwardDiagonal(fillColor),         // Upward diagonal (↗)  – note naming
                5 => SixLabors.ImageSharp.Drawing.Processing.Brushes.ForwardDiagonal(fillColor),          // Downward diagonal (↘)
                6 => SixLabors.ImageSharp.Drawing.Processing.Brushes.Percent20(fillColor),                // Cross / large grid (closest common match)
                _ => new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(fillColor)                    // fallback
            };
        }

        // Helper routine if they play around with curvature
        private IPath CreateRoundedPolygonPath(SixLabors.ImageSharp.PointF[] points, float cornerRadius)
        {
            if (cornerRadius <= 0.1f || points == null || points.Length < 3)
            {
                return new Polygon(points ?? Array.Empty<SixLabors.ImageSharp.PointF>());
            }

            var builder = new PathBuilder();
            int n = points.Length;

            // Approximation constant for quadratic bezier ~ circle quadrant
            const float kappa = 0.5522848f;

            SixLabors.ImageSharp.PointF? previousEnd = null;  // We'll track the end of the previous segment

            for (int i = 0; i < n; i++)
            {
                SixLabors.ImageSharp.PointF prev = points[(i - 1 + n) % n];
                SixLabors.ImageSharp.PointF curr = points[i];
                SixLabors.ImageSharp.PointF next = points[(i + 1) % n];

                Vector2 currVec = curr;
                Vector2 incoming = new Vector2(curr.X - prev.X, curr.Y - prev.Y);
                Vector2 outgoing = new Vector2(next.X - curr.X, next.Y - curr.Y);

                float lenIn = incoming.Length();
                float lenOut = outgoing.Length();

                Vector2 incomingNorm = lenIn > 1e-4f ? Vector2.Normalize(incoming) : Vector2.Zero;
                Vector2 outgoingNorm = lenOut > 1e-4f ? Vector2.Normalize(outgoing) : Vector2.Zero;

                float pull = Math.Min(cornerRadius, Math.Min(lenIn * 0.45f, lenOut * 0.45f));

                Vector2 startVec = currVec - incomingNorm * pull;
                Vector2 endVec = currVec + outgoingNorm * pull;

                Vector2 ctrl1Vec = startVec + incomingNorm * (pull * kappa);
                Vector2 ctrl2Vec = endVec - outgoingNorm * (pull * kappa);

                SixLabors.ImageSharp.PointF arcStart = startVec;
                SixLabors.ImageSharp.PointF arcEnd = endVec;
                SixLabors.ImageSharp.PointF ctrl1 = ctrl1Vec;
                SixLabors.ImageSharp.PointF ctrl2 = ctrl2Vec;

                if (i == 0)
                {
                    builder.MoveTo(arcStart);
                }
                else
                {
                    // Connect from previous arcEnd → current arcStart
                    // (previousEnd is guaranteed non-null after first iteration)
                    builder.AddLine(previousEnd!.Value, arcStart);
                }

                builder.AddQuadraticBezier(ctrl1, ctrl2, arcEnd);

                previousEnd = arcEnd;  // Remember for next iteration

                // If degenerate → we still added the bezier (or skipped math), but connection is handled above
            }

            return builder.Build();  // auto-closes the path
        }
        // 2026-02-04 - End Grok build --------------------------------------------------------------
        // ------------------------------------------------------------------------------------------

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
                // Get the property and fill in the value
                returnToken.CopyFrom(UserProperties[propertyName]);


                switch (propertyName)
                {
                    // Intercept special handling of properties
                    case "height":
                        returnToken.Element.Value = shape.Height - 2;
                        break;

                    case "width":
                        returnToken.Element.Value = shape.Width - 2;
                        break;

                    default:
                        // Process standard properties
                        result = base.GetProperty(propertyName, idx, out returnToken);
                        break;
                }

                if (JAXLib.Between(result, 1, 10))
                {
                    result = 0;

                    // Visual object common property handler
                    switch (propertyName.ToLower())
                    {
                        case "***":
                            result = 1559;
                            break;

                        default:
                            returnToken.CopyFrom(UserProperties[propertyName]); //returnToken.Element.Value = UserProperties[propertyName].Element.Value;
                            result = 0;
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
                "addproperty","move","readexpression","readmethod","refresh","resettodefault",
                "saveasclass","settooriginalvalue","setfocus","writeexpression","writemethod","zorder"
                ];
        }

        /*------------------------------------------------------------------------------------------*
         * 
         *------------------------------------------------------------------------------------------*/
        public override string[] JAXEvents()
        {
            return
                [
                "click","dblclick","destroy","error",
                "init","keypress","load",
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
            return
                 [
                "anchor,n,0",
                "backcolor,R,240|240|240","backstyle,n,1","BaseClass,C,shape","bordercolor,R,100|100|100","borderstyle,n,0","borderwidth,n,1",
                "Class,C,shape","ClassLibrary,C!,","Comment,C,","curvature,n,0",
                "drawmode,n,13",
                "Enabled,L,true",
                "fillcolor,R,0|0|0","fillstyle,n,1",
                "Height,N,50",
                "left,N,0",
                "name,c,command",
                "parent,o!,","parentclass,C!,","points,n,3","polypoints,,",
                "rotation,n,0",
                "scale,n,100","sides,N,4",
                "tag,C,","transparency,n,100","top,N,0","tooltiptext,c,",
                "visible,l,true",
                "width,N,50"
                ];

        }


        // Modified from GROK solution - create a png and put it into an array
        public byte[] CreatePolygonPngBytes()
        {
            int width = UserProperties["width"].AsInt();
            int height = UserProperties["height"].AsInt();
            int forecolor = UserProperties["forecolor"].AsInt();
            int backcolor = UserProperties["backcolor"].AsInt();
            int degrees = UserProperties["rotation"].AsInt() % 360;

            int border = UserProperties["borderwidth"].AsInt();
            int borderstyle = UserProperties["borderstyle"].AsInt();
            int margin = borderstyle > 0 ? UserProperties["bordermargin"].AsInt() : 0;

            int finalWidth = width + 2 * (margin + border);
            int finalHeight = height + 2 * (margin + border);

            using var img = new Image<Rgba32>(finalWidth, finalHeight);

            // Fully transparent background
            img.Mutate(x => x.BackgroundColor(SixLabors.ImageSharp.Color.Transparent));

            // --- Draw your rotated polygon in the center (original size) ---
            var originalPoints = new SixLabors.ImageSharp.PointF[]
            {
                new(400, 100), new(600, 200), new(550, 400),
                new(250, 400), new(200, 200)
            };

            var center = new SixLabors.ImageSharp.PointF(width / 2f, height / 2f);
            var rotatedPoints = originalPoints
                .Select(p => RotatePoint(p, center, degrees))
                .Select(p => new SixLabors.ImageSharp.PointF(
                    p.X + margin + border,   // offset into the padded area
                    p.Y + margin + border))
                .ToArray();

            var polygon = new Polygon(new LinearLineSegment(rotatedPoints));

            img.Mutate(x => x
                .Fill(SixLabors.ImageSharp.Color.FromRgba(70, 130, 255, 180), polygon)
                .Draw(SixLabors.ImageSharp.Color.FromRgba(0, 0, 139, 255), 7f, polygon));

            // --- Draw the 2-pixel black border (inside the margin) ---
            var borderRect = new SixLabors.ImageSharp.Rectangle(
                margin, margin,
                width + 2 * border,
                height + 2 * border);

            img.Mutate(x => x.Draw(
                SixLabors.ImageSharp.Color.Black,
                border * 2,  // thickness = 2px → total width 4px, but we draw centered
                new RectangularPolygon(borderRect)));

            // Save to PNG with transparency
            using var ms = new MemoryStream();
            img.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            return ms.ToArray();
        }

        // Helper: rotate a point around a center
        private static SixLabors.ImageSharp.PointF RotatePoint(SixLabors.ImageSharp.PointF point, SixLabors.ImageSharp.PointF center, float degrees)
        {
            float radians = degrees * MathF.PI / 180f;
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            float x = dx * cos - dy * sin + center.X;
            float y = dx * sin + dy * cos + center.Y;

            return new SixLabors.ImageSharp.PointF(x, y);
        }

        // GROK rocks out loud - test them out and save the best
        public class PngOverlayHelper
        {
            /// <summary>
            /// Overlays topPngBytes on top of basePngBytes.
            /// Both must be valid PNG byte arrays.
            /// Supports full alpha blending and different sizes.
            /// </summary>
            /// <param name="basePngBytes">Bottom layer (background)</param>
            /// <param name="topPngBytes">Top layer (foreground, can be transparent)</param>
            /// <param name="x">X position to place top image (default: centered)</param>
            /// <param name="y">Y position to place top image (default: centered)</param>
            /// <returns>New PNG byte array with overlay</returns>
            public byte[] Overlay(
                byte[] basePngBytes,
                byte[] topPngBytes,
                int? x = null,
                int? y = null)
            {
                using var baseImg = SixLabors.ImageSharp.Image.Load<Rgba32>(basePngBytes);
                using var topImg = SixLabors.ImageSharp.Image.Load<Rgba32>(topPngBytes);

                // Auto-center if not specified
                int posX = x ?? (baseImg.Width - topImg.Width) / 2;
                int posY = y ?? (baseImg.Height - topImg.Height) / 2;

                // Ensure position is within bounds (clamp)
                posX = Math.Clamp(posX, 0, baseImg.Width - topImg.Width);
                posY = Math.Clamp(posY, 0, baseImg.Height - topImg.Height);

                // Perform the overlay with alpha blending
                baseImg.Mutate(ctx => ctx.DrawImage(topImg, new SixLabors.ImageSharp.Point(posX, posY), 1f)); // 1f = full opacity

                using var ms = new MemoryStream();
                baseImg.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                return ms.ToArray();
            }
        }

        public class PngMasterComposer
        {
            /// <summary>
            /// Overlay multiple PNG byte arrays in order (bottom to top)
            /// Perfect for badges, frames, watermarks, stickers, etc.
            /// </summary>
            public static byte[] OverlayMany(params (byte[] png, int? x, int? y)[] layers)
            {
                if (layers.Length == 0) throw new ArgumentException("Need at least one layer");

                using var baseImg = SixLabors.ImageSharp.Image.Load<Rgba32>(layers[0].png);

                for (int i = 1; i < layers.Length; i++)
                {
                    var (png, x, y) = layers[i];
                    using var topImg = SixLabors.ImageSharp.Image.Load<Rgba32>(png);

                    int posX = x ?? (baseImg.Width - topImg.Width) / 2;
                    int posY = y ?? (baseImg.Height - topImg.Height) / 2;

                    posX = Math.Clamp(posX, 0, baseImg.Width - topImg.Width);
                    posY = Math.Clamp(posY, 0, baseImg.Height - topImg.Height);

                    baseImg.Mutate(ctx => ctx.DrawImage(topImg, new SixLabors.ImageSharp.Point(posX, posY), 1f));
                }

                using var ms = new MemoryStream();
                baseImg.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                return ms.ToArray();
            }

            /* Add to a form
             * pic.Dock = DockStyle.Fill;
             * pic.SizeMode = PictureBoxSizeMode.Zoom;
             * Load += (s, e) => pic.Image = Image.FromStream(new MemoryStream(pngarray));
             */
        }
    }
}
