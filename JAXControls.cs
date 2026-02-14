using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace JAXBase
{
    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    // Create a Panel class that allows you to alter DoubleBuffered
    public class JAXPanel : System.Windows.Forms.Panel
    {
        private Color _borderColor = Color.FromArgb(120, 120, 120);
        private int _borderWidth = 0;
        private int _borderRadius = 0;
        private int _height = 100;
        private int _width = 100;
        private bool _useRoundedCorners = true;
        private DashStyle _borderDashStyle = DashStyle.Solid;

        public JAXPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            AutoScroll = true;
            BackColor = Color.White;
            Padding = new Padding(BorderWidth);
        }

        // ────────────────────────────────────────────────
        // Properties
        // ────────────────────────────────────────────────

        [DefaultValue(true)]
        public new bool DoubleBuffered
        {
            get { return base.DoubleBuffered; }
            set { base.DoubleBuffered = value; }
        }

        [Category("Appearance")]
        [Description("Border color")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Border thickness (pixels)")]
        [DefaultValue(2)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                this.Padding = new Padding(_borderWidth);
                _borderWidth = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Corner radius (0 = rectangle)")]
        [DefaultValue(10)]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Whether to draw rounded corners")]
        [DefaultValue(true)]
        public bool UseRoundedCorners
        {
            get => _useRoundedCorners;
            set { _useRoundedCorners = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Dash style of the border line")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle BorderDashStyle
        {
            get => _borderDashStyle;
            set
            {
                _borderDashStyle = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        public new int Height
        {
            get => _height;
            set
            {
                _height = value;
                this.Size = new Size(_width, _height);
                Invalidate();
            }
        }


        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        public new int Width
        {
            get => _width;
            set
            {
                _width = value;
                this.Size = new Size(_width, _height);
                Invalidate();
            }
        }


        // ────────────────────────────────────────────────
        // Painting
        // ────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);  // background + children

            // Stop painting from happening if border color is <1
            if (_borderWidth < 1 || _borderColor == Color.Transparent)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(_borderColor, _borderWidth)
            {
                DashStyle = _borderDashStyle,
                Alignment = PenAlignment.Inset   // helps with inset feel on rounded borders
            };

            // Adjust for scrollbars
            int sbw = VScroll ? SystemInformation.VerticalScrollBarWidth : 0;
            int sbh = HScroll ? SystemInformation.HorizontalScrollBarHeight : 0;

            var rect = new Rectangle(
                _borderWidth / 2,
                _borderWidth / 2,
                ClientSize.Width - _borderWidth - sbw,
                ClientSize.Height - _borderWidth - sbh);

            if (!_useRoundedCorners || _borderRadius <= 1)
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
            else
            {
                using var path = GetRoundedRect(rect, _borderRadius);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }
    }


    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    public class JAXGridView : DataGridView
    {
        public JAXGridView()
        {
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = true;
            AllowUserToResizeColumns = true;
            AutoGenerateColumns = false;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            BackgroundColor = Color.FromArgb(unchecked((int)0xFFFBFBFB));
            BorderStyle = BorderStyle.None;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersVisible = true;
            DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            DefaultCellStyle.SelectionBackColor = Color.Blue;
            DefaultCellStyle.SelectionForeColor = Color.White;
            MultiSelect = false;
            RowHeadersVisible = true;
            RowHeadersWidth = 43;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            RowTemplate.Height = 28;
            SelectionMode = DataGridViewSelectionMode.CellSelect;
            ScrollBars = ScrollBars.Both;
        }

        public int GridLineWidth = 1;
        public JAXDirectDBF? DBF = null;
        public JAXDirectDBF.DBFInfo? dbfInfo = null;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.ColumnCount > 0)
            {
                // Only extend if there's at least one column and empty space below real rows
                int headerHeight = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
                int totalRowsHeight = headerHeight;
                foreach (DataGridViewRow row in Rows)
                {
                    if (row.Visible) totalRowsHeight += row.Height;
                }

                if (totalRowsHeight >= ClientSize.Height) return; // No empty space

                // Calculate starting Y for empty area
                int startY = totalRowsHeight;

                // Use your grid's actual colors (adjust if you customized them)
                using (Pen gridPen = new Pen(GridColor, GridLineWidth))
                using (Brush cellBrush = new SolidBrush(DefaultCellStyle.BackColor))
                {
                    // Fill the empty area with cell background color first (no gray!)
                    e.Graphics.FillRectangle(cellBrush, 0, startY, ClientSize.Width, ClientSize.Height - startY);

                    //int rowHeight = RowTemplate.Height; // Assume fixed height rows (common & simplest)
                    //                                    // If rows can vary in height, you'd need more logic here

                    //int currentY = startY;

                    //while (currentY < ClientSize.Height)
                    //{
                    //    // Horizontal line across the full width
                    //    //e.Graphics.DrawLine(gridPen, 0, currentY, ClientSize.Width, currentY);

                    //    // Vertical lines: draw at each column boundary
                    //    int x = RowHeadersVisible ? RowHeadersWidth : 0;

                    //    // Vertical line against the left sides
                    //    e.Graphics.DrawLine(gridPen, 0, currentY, 0, Math.Min(currentY + rowHeight, ClientSize.Height));
                    //    if (x > 0) e.Graphics.DrawLine(gridPen, x - 1, currentY, x - 1, Math.Min(currentY + rowHeight, ClientSize.Height));

                    //    foreach (DataGridViewColumn col in Columns)
                    //    {
                    //        if (col.Visible)
                    //        {
                    //            x += col.Width;
                    //            e.Graphics.DrawLine(gridPen, x - 1, currentY, x - 1, Math.Min(currentY + rowHeight, ClientSize.Height));
                    //        }
                    //    }

                    //    // Vertical line against the right side
                    //    e.Graphics.DrawLine(gridPen, ClientSize.Width - 1, currentY, ClientSize.Width - 1, Math.Min(currentY + rowHeight, ClientSize.Height));
                    //    // Horizontal line across the full width
                    //    e.Graphics.DrawLine(gridPen, 0, currentY, x, currentY);

                    //    // Next "fake row"
                    //    currentY += rowHeight;
                    //}

                    //// Final bottom horizontal if needed (to close the last cell)
                    //if (currentY < ClientSize.Height)
                    //{
                    //    e.Graphics.DrawLine(gridPen, 0, currentY, ClientSize.Width, currentY);
                    //}
                }
            }
        }
    }


    // Another Helper class - this is used by the debugger
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this System.Windows.Forms.Control control, Action action)
        {
            if (control.IsDisposed)
                return;

            if (control.InvokeRequired)
            {
                // Use BeginInvoke — fire and forget (non-blocking)
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }



    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    /// <summary>
    /// A Panel-based label that automatically resizes to fit its text content
    /// with configurable inner padding / border-like spacing.
    /// </summary>
    [DesignerCategory("Code")]
    public class JAXLabel : JAXPanel
    {
        private readonly System.Windows.Forms.Label innerLabel;

        public JAXLabel()
        {
            // Prevent flicker and improve painting
            DoubleBuffered = true;
            ResizeRedraw = true;
            Visible = true;

            // Create inner label
            innerLabel = new System.Windows.Forms.Label
            {
                Text = "Label",
                AutoSize = true,                    // crucial for correct sizing
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,      // usually want label bg transparent
                Dock = DockStyle.None,              // we control position manually
                Visible = true
            };

            Controls.Add(innerLabel);

            // Subscribe to changes that should trigger resize
            innerLabel.TextChanged += (s, e) => UpdateSize();
            innerLabel.FontChanged += (s, e) => UpdateSize();
            innerLabel.PaddingChanged += (s, e) => UpdateSize();
            innerLabel.AutoSizeChanged += (s, e) => UpdateSize();

            // Initial layout
            PerformLayout();
            UpdateSize();
        }

        // ───────────────────────────────────────────────
        //  Public properties - forward most important ones
        // ───────────────────────────────────────────────

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        public override string Text
        {
            get => innerLabel.Text;
            set // <--- How do I get this fixed?  Setting string! just moves the problem to the get
            {
                if (value is not null)
                {
                    // TODO - we should problably fix up the caption value
                    innerLabel.Text = value.ToString();
                    UpdateSize();
                    Invalidate();
                }
                else
                    throw new Exception("11|");
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        public new Font Font
        {
            get => innerLabel.Font;
            set
            {
                innerLabel.Font = value;
                UpdateSize();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ContentAlignment TextAlign
        {
            get => innerLabel.TextAlign;
            set => innerLabel.TextAlign = value;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color TextColor
        {
            get => innerLabel.ForeColor;
            set => innerLabel.ForeColor = value;
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Padding Padding
        {
            get => base.Padding;
            set
            {
                base.Padding = value;
                UpdateSize();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new BorderStyle BorderStyle
        {
            get => base.BorderStyle;
            set => base.BorderStyle = value;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => BackColor;
            set => BackColor = value;
        }

        // Optional: expose more label properties if needed
        [Browsable(false)]
        public System.Windows.Forms.Label InnerLabel => innerLabel;

        // ───────────────────────────────────────────────
        //  Core sizing logic
        // ───────────────────────────────────────────────

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            PositionLabel();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            PositionLabel();
        }

        private void UpdateSize()
        {
            if (innerLabel == null) return;

            // Calculate desired size based on text + padding
            System.Drawing.Size textSize = innerLabel.GetPreferredSize(System.Drawing.Size.Empty);

            int totalWidth = textSize.Width + Padding.Left + Padding.Right;
            int totalHeight = textSize.Height + Padding.Top + Padding.Bottom;

            // Only auto-size if not manually sized by user/designer
            if (!Size.IsEmpty && !AutoSize)
            {
                // If control has fixed size → just reposition label inside
                PositionLabel();
            }
            else
            {
                // Auto-size the panel to fit content
                Size = new System.Drawing.Size(totalWidth, totalHeight);
            }

            PositionLabel();
            Invalidate();
        }

        private void PositionLabel()
        {
            if (innerLabel == null) return;

            // Center or align the label inside the padded area
            int x = Padding.Left;
            int y = Padding.Top;

            // If label is not autosize → make it fill available space
            if (!innerLabel.AutoSize)
            {
                innerLabel.Size = new System.Drawing.Size(
                    ClientSize.Width - Padding.Left - Padding.Right,
                    ClientSize.Height - Padding.Top - Padding.Bottom);
            }
            else
            {
                // For AutoSize labels → just position according to alignment
                switch (innerLabel.TextAlign)
                {
                    case ContentAlignment.MiddleCenter:
                    case ContentAlignment.MiddleLeft:
                    case ContentAlignment.MiddleRight:
                        y = (ClientSize.Height - innerLabel.Height) / 2;
                        break;
                    case ContentAlignment.BottomCenter:
                    case ContentAlignment.BottomLeft:
                    case ContentAlignment.BottomRight:
                        y = ClientSize.Height - innerLabel.Height - Padding.Bottom;
                        break;
                }

                switch (innerLabel.TextAlign)
                {
                    case ContentAlignment.MiddleCenter:
                    case ContentAlignment.TopCenter:
                    case ContentAlignment.BottomCenter:
                        x = (ClientSize.Width - innerLabel.Width) / 2;
                        break;
                    case ContentAlignment.MiddleRight:
                    case ContentAlignment.TopRight:
                    case ContentAlignment.BottomRight:
                        x = ClientSize.Width - innerLabel.Width - Padding.Right;
                        break;
                }
            }

            innerLabel.Location = new System.Drawing.Point(x, y);
        }

        // Optional: make sure label resizes with panel when needed
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            UpdateSize();
        }

        // Optional: support AutoSize on the JAXLabel itself
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        public new bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                base.AutoSize = value;
                UpdateSize();
            }
        }
    }

    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    /// <summary>
    /// A Panel-based label that automatically resizes to fit its text content
    /// with configurable inner padding / border-like spacing.
    /// </summary>
    [DesignerCategory("Code")]
    public class JAXCheckBox : System.Windows.Forms.CheckBox
    {
        private Color _borderColor = Color.Gray;
        private int _borderWidth = 1;

        public JAXCheckBox()
        {
            // Important: keep standard checkbox appearance
            Appearance = Appearance.Normal;

            // Optional but recommended in most cases
            FlatStyle = FlatStyle.Standard;
            AutoSize = true;   // usually what you want
        }

        [Category("Appearance")]
        [Description("Color of the border drawn around the entire checkbox control")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();   // redraw when changed
            }
        }

        [Category("Appearance")]
        [Description("Width of the border in pixels (0 = no border)")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                if (value < 0) value = 0;
                _borderWidth = value;
                Invalidate();
                // Optional: adjust padding if border is thick
                Padding = new Padding(value);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Let the base class draw the standard checkbox + text + check mark
            base.OnPaint(e);

            //BorderColor = BorderWidth == 0 ? Color.Transparent : BorderColor;
            if (BorderWidth > 0 && BorderColor != Color.Transparent)
            {
                using (var pen = new Pen(BorderColor, BorderWidth))
                {
                    // Draw border around the entire client area
                    // Adjust rectangle inward by half the pen width to avoid clipping
                    float halfWidth = BorderWidth / 2f;

                    var rect = new RectangleF(halfWidth, halfWidth, ClientSize.Width - BorderWidth, ClientSize.Height - BorderWidth);

                    e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        // Optional: redraw when size changes (especially useful if AutoSize=false)
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        // Optional: better visual when focused (you can customize or remove)
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
    }


    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    public class JAXPictureBox : PictureBox
    {
        private Color _borderColor = Color.Gray;
        private int _borderWidth = 2;

        public JAXPictureBox()
        {
            // Default settings - feel free to change
            BorderStyle = BorderStyle.None;     // We draw our own → disable built-in border
            BackColor = Color.Transparent;      // Helps when image has transparency
        }

        [Category("Appearance")]
        [Description("Color of the custom border")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();  // Redraw control
            }
        }

        [Category("Appearance")]
        [Description("Thickness of the border in pixels (0 = no border)")]
        [DefaultValue(2)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Math.Max(0, value);
                Invalidate();
                // Optional: adjust padding so content doesn't touch thick borders
                Padding = new Padding(_borderWidth);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Let base class draw the image (and background if any)
            base.OnPaint(e);

            if (_borderWidth > 0 && _borderColor != Color.Transparent)
            {
                using (var pen = new Pen(_borderColor, _borderWidth))
                {
                    // Center the pen stroke so it doesn't get clipped
                    float halfWidth = _borderWidth / 2f;

                    var rect = new RectangleF(
                        halfWidth,
                        halfWidth,
                        Width - _borderWidth,
                        Height - _borderWidth);

                    // Optional: anti-aliased lines look better
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        // Redraw when size changes (important for thick borders or docked/anchored controls)
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        // Optional: redraw when padding changes
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            Invalidate();
        }
    }


    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    public class JAXNumericUpDown : JAXPanel
    {
        public SmartNumericUpDown InnerNumericUpDown { get; private set; }

        private int _borderWidth = 0;
        private Color _borderColor = Color.Transparent;
        private int _currentTop = 0;
        private int _currentLeft = 0;
        private int _currentBorderWidth = 0;
        private bool _lastChangeFromUserTyping = false;

        public JAXNumericUpDown()
        {
            this.BorderStyle = BorderStyle.None;
            this.BackColor = Color.DarkGray;           // ← default border color
            //this.Padding = new Padding(1);             // ← border thickness (2px all sides)
            //this.BorderWidth = 0;

            InnerNumericUpDown = new SmartNumericUpDown
            {
                BorderStyle = BorderStyle.Fixed3D,
                Dock = DockStyle.None,
                Top = _borderWidth,
                Left = _borderWidth,
                BackColor = System.Drawing.SystemColors.Window
            };

            this.Controls.Add(InnerNumericUpDown);
            this.Size = new System.Drawing.Size(InnerNumericUpDown.Width, InnerNumericUpDown.Height);

            // Extrnal access to events
            //JAXNumericUpDown.InnerNumericUpDown.Enter += (s, e) =>
        }

        // Expose important NumericUpDown properties directly
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public decimal Value
        {
            get => InnerNumericUpDown.Value;
            set => InnerNumericUpDown.Value = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public decimal Minimum
        {
            get => InnerNumericUpDown.Minimum;
            set => InnerNumericUpDown.Minimum = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public decimal Maximum
        {
            get => InnerNumericUpDown.Maximum;
            set => InnerNumericUpDown.Maximum = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool InterceptArrowKeys
        {
            get => InnerNumericUpDown.InterceptArrowKeys;
            set => InnerNumericUpDown.InterceptArrowKeys = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public System.Windows.Forms.LeftRightAlignment UpDownAlign
        {
            get => InnerNumericUpDown.UpDownAlign;
            set => InnerNumericUpDown.UpDownAlign = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public System.Windows.Forms.HorizontalAlignment TextAlign
        {
            get => InnerNumericUpDown.TextAlign;
            set => InnerNumericUpDown.TextAlign = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Hexadecimal
        {
            get => InnerNumericUpDown.Hexadecimal;
            set => InnerNumericUpDown.Hexadecimal = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public decimal Increment
        {
            get => InnerNumericUpDown.Increment;
            set => InnerNumericUpDown.Increment = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int DecimalPlaces
        {
            get => InnerNumericUpDown.DecimalPlaces;
            set => InnerNumericUpDown.DecimalPlaces = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ThousandsSeparator
        {
            get => InnerNumericUpDown.ThousandsSeparator;
            set => InnerNumericUpDown.ThousandsSeparator = value;
        }


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => this.BackColor;
            set
            {
                this.BackColor = value;
                _borderColor = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int BorderWidth
        {
            get => this.Padding.Left; // assuming uniform padding
            set
            {
                // Only do something if the value changes
                if (InnerNumericUpDown is null)
                {
                    _borderWidth = Math.Max(0, value);
                    _borderWidth = value;
                }
                else
                {
                    _borderWidth = Math.Max(0, value);

                    // Adjust the size of the panel
                    this.Size = new System.Drawing.Size(InnerNumericUpDown.Width + _borderWidth * 2, InnerNumericUpDown.Height + _borderWidth * 2);

                    // Adjust the position of the inner control
                    InnerNumericUpDown.Top = _borderWidth;
                    InnerNumericUpDown.Left = _borderWidth;

                    // TODO?? - Adjust panel position so top/left
                    // moves and the spinner does not visualy move

                    if (value < 1)
                        BackColor = Color.Transparent;
                    else
                        BackColor = _borderColor;

                    Invalidate();
                }
            }
        }
    }

    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    public class SmartNumericUpDown : NumericUpDown
    {
        private bool _lastChangeFromUserTyping = false;

        protected override void OnValueChanged(EventArgs e)
        {
            // This is the only reliable place to read UserEdit during a value change
            _lastChangeFromUserTyping = this.UserEdit;

            // Optional: reset our internal flag if needed (base often resets UserEdit soon after)
            base.OnValueChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            // This is the only reliable place to read UserEdit during a value change
            _lastChangeFromUserTyping = this.UserEdit;

            base.OnTextChanged(e);
        }
        // Public read-only property → use this in your ValueChanged handler
        public bool LastChangeWasFromTyping
        {
            get { return _lastChangeFromUserTyping; }
        }
    }


    /* -----------------------------------------------------------------------------------------*
     * -----------------------------------------------------------------------------------------*/
    public class JAXComboBox : System.Windows.Forms.Panel
    {
        private System.Windows.Forms.ComboBox _comboBox;
        private int _borderWidth = 0;
        private Color _borderColor = Color.Black;

        public JAXComboBox()
        {
            this.BorderStyle = BorderStyle.None;
            this.BackColor = _borderColor;
            this.Padding = new Padding(_borderWidth);

            _comboBox = new System.Windows.Forms.ComboBox
            {
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.ControlText,
                DropDownStyle = ComboBoxStyle.DropDownList, // default; change as needed
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Standard
            };

            this.Controls.Add(_comboBox);

            // Auto-adjust size to inner + border
            UpdateSize();
            this.SizeChanged += (s, e) => UpdateInnerBounds();
        }

        // Public properties
        [Category("Appearance")]
        [Description("Thickness of the outer border in pixels")]
        [DefaultValue(0)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Math.Max(0, value);
                this.Padding = new Padding(_borderWidth);
                UpdateSize();
                UpdateInnerBounds();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color of the outer border when not focused")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                BackColor = _borderWidth == 0 ? Color.Transparent : value;
            }
        }

        // Forward most common ComboBox properties/events
        [Browsable(false)]
        public System.Windows.Forms.ComboBox InnerComboBox => _comboBox;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object SelectedItem
        {
            get => _comboBox.SelectedItem ?? string.Empty;
            set => _comboBox.SelectedItem = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int SelectedIndex
        {
            get => _comboBox.SelectedIndex;
            set => _comboBox.SelectedIndex = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => _comboBox.Text;
            set => _comboBox.Text = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Sorted
        {
            get => _comboBox.Sorted;
            set => _comboBox.Sorted = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new int Width
        {
            get => _comboBox.Width;
            set
            {
                _comboBox.Width = value;
                Invalidate();
            }

        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new int Height
        {
            get => _comboBox.Height;
            set
            {
                _comboBox.Height = value;
                Invalidate();
            }

        }


        public System.Windows.Forms.ComboBox.ObjectCollection Items => _comboBox.Items;

        public event EventHandler SelectedIndexChanged
        {
            add => _comboBox.SelectedIndexChanged += value;
            remove => _comboBox.SelectedIndexChanged -= value;
        }

        public event EventHandler SelectedValueChanged
        {
            add => _comboBox.SelectedValueChanged += value;
            remove => _comboBox.SelectedValueChanged -= value;
        }

        private void UpdateSize()
        {
            // Optional: if you want auto-size based on default inner + thickness
            // For now we let user set Size, and inner fills it
            this.Size = new System.Drawing.Size(_comboBox.Width + _borderWidth * 2, _comboBox.Height + _borderWidth * 2);
        }

        private void UpdateInnerBounds()
        {
            if (_comboBox == null) return;
            // Dock.Fill already handles this perfectly with padding
            // But call PerformLayout if needed after thickness change
            this.PerformLayout();
        }
    }


    /* -----------------------------------------------------------------------------------------*
     * Listbox with sizeable borders
     * -----------------------------------------------------------------------------------------*/
    public class JAXListBox : ListBox
    {
        private int _borderWidth = 0;
        private Color _borderColor = Color.Black;

        public JAXListBox()
        {
            BorderStyle = BorderStyle.None;

            // This line was missing → enables your OnPaint to run
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint,  // reduces flicker on redraw
                true
            );
        }

        [Category("Appearance")]
        [Description("Thickness of the outer border in pixels")]
        [DefaultValue(0)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Math.Max(0, value);
                Invalidate();      // repaint the control
                                   // PerformLayout(); // optional: helps if scrollbar/layout glitches appear
            }
        }

        [Category("Appearance")]
        [Description("Color of the outer border when not focused")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor   // removed unnecessary DesignerSerializationVisibility (default is fine)
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw background + items + scrollbar first (critical!)
            base.OnPaint(e);

            if (_borderWidth > 0 && _borderColor != Color.Transparent)
            {
                using var pen = new Pen(_borderColor, _borderWidth);

                // Use half-width inset to keep thick borders fully visible inside bounds
                float half = _borderWidth / 2f;

                e.Graphics.DrawRectangle(pen,
                    half,
                    half,
                    ClientSize.Width - _borderWidth,
                    ClientSize.Height - _borderWidth);
            }
        }
    }
}
