using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace JAXBase
{
    public partial class JAXDebuggerForm : Form
    {
        private readonly AppClass App;
        private DebugAction _pendingAction = DebugAction.None;
        private const int MaxLines = 210;  // When exceeded, remove top 10 → keeps 200 visible
        private string lastprg = string.Empty;
        private int lastlevel = 0;

        // Stores the actual live objects being watched, keyed by name (case-insensitive)
        private readonly Dictionary<string, string> watchedObjects = new(StringComparer.OrdinalIgnoreCase);

        public event Action<DebugAction>? OnActionChosen;

        public enum DebugAction
        {
            None = 0,
            Step = 1,      // F6
            StepInto = 2,  // F8
            Cancel = 3,    // Esc
            Resume = 4,    // F5
            Continue = 5,
            Pause = 6
        }

        public JAXDebuggerForm(AppClass app)
        {
            InitializeComponent();
            SetupTreeView();

            App = app;

            this.Left = 0;
            this.Top = 0;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = true;
            this.KeyPreview = true; // Allows the form to receive key events before controls

            // Handle function keys and Esc at form level
            this.KeyDown += DebugControlForm_KeyDown;

            // Treat closing the window (X button) as Cancel
            //this.FormClosing += (s, e) => { OnActionChosen?.Invoke(DebugAction.Cancel); };

            btnStep.Click += (s, e) => { OnActionChosen?.Invoke(DebugAction.Step); };
            btnStepInto.Click += (s, e) => { OnActionChosen?.Invoke(DebugAction.StepInto); };
            btnResume.Click += (s, e) => { OnActionChosen?.Invoke(DebugAction.Resume); };
            btnCancel.Click += (s, e) => { OnActionChosen?.Invoke(DebugAction.Cancel); };
        }

        private void SetupTreeView()
        {
            treeView1.ShowLines = true;
            treeView1.ShowPlusMinus = true;
            treeView1.ShowRootLines = true;
            treeView1.FullRowSelect = true;
            treeView1.LabelEdit = false; // Optional: set true if you want rename support later

            // Optional: nice font
            treeView1.Font = new Font("Segoe UI", 9F);

            // Initial message
            treeView1.Nodes.Add("Enter a variable name in the textbox and press Enter to watch it.");

            treeView1.ContextMenuStrip = contextMenuStripTree;

            // Event for the menu item (create a ToolStripMenuItem named "removeWatchToolStripMenuItem")
            removeWatchToolStripMenuItem.Click += (s, e) => DeleteSelectedRootNode();
        }

        private void DebugControlForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F6:
                    OnActionChosen?.Invoke(DebugAction.Step);
                    e.Handled = true;
                    break;
                case Keys.F8:
                    OnActionChosen?.Invoke(DebugAction.StepInto);
                    e.Handled = true;
                    break;
                case Keys.F5:
                    OnActionChosen?.Invoke(DebugAction.Resume);
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    OnActionChosen?.Invoke(DebugAction.Cancel);
                    e.Handled = true;
                    break;
            }
        }


        // General Update of all controls
        public void UpdateAll()
        {
            this.SuspendLayout();
            try
            {
                // update labels, grids, etc.
                if (lastprg.Equals(App.AppLevels[^1].PrgName, StringComparison.OrdinalIgnoreCase) == false || lastlevel != App.AppLevels.Count)
                {
                    lastprg = App.AppLevels[^1].PrgName;
                    AppendLog($"---- {lastprg.ToUpper()} Level: {App.AppLevels.Count} ----" + Environment.NewLine, Color.Blue.ToArgb(), -1, true);

                    // Upon load, we don't want to display the line of code as we'll be
                    // coming through here again with the same line once load is complete
                    if (lastlevel != 0)
                        AppendLog(App.AppLevels[^1].CurrentLineOfCode + Environment.NewLine);

                    lastlevel = App.AppLevels.Count;
                    lblLevel.Text = lastlevel.ToString();
                    lblPrg.Text = lastprg;
                }
                else
                    AppendLog(App.AppLevels[^1].CurrentLineOfCode + Environment.NewLine);

                lblLine.Text = App.AppLevels[^1].FileLine.ToString();
            }
            finally
            {
                this.ResumeLayout(true);
                this.PerformLayout();
            }
        }


        public void UpdateDisplay()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(UpdateDisplay);
                return;
            }

            UpdateAll();
            this.BringToFront(); // optional
        }


        // Call this from your main loop after each command to refresh display
        public void RefreshDisplay()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(RefreshDisplay));
                return;
            }

            UpdateAll(); // your existing method that refreshes labels, grids, etc.
            this.BringToFront();
            this.WindowState = FormWindowState.Normal; // un-minimize if needed
        }


        // Optional: allow closing to abort debugging
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            OnActionChosen?.Invoke(DebugAction.Cancel);
            base.OnFormClosing(e);
        }


        private void JAXDebuggerForm_Load(object sender, EventArgs e)
        {
            // Optional: initial empty message in TreeView
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add("Add a watched variable to inspect its contents.");
            UpdateAll();
        }

        private void AppendLog(string text, int foreColor = -1, int backColor = -1, bool bold = false)
        {
            // Temporarily allow editing if the control is read-only
            bool wasReadOnly = codebox.ReadOnly;
            codebox.ReadOnly = false;

            // Save current selection state (in case something else had a selection)
            int oldSelectionStart = codebox.SelectionStart;
            int oldSelectionLength = codebox.SelectionLength;

            // Move caret to the end for appending
            codebox.SelectionStart = codebox.Text.Length;
            codebox.SelectionLength = 0;

            // Apply foreground color if specified
            if (foreColor != -1)
                codebox.SelectionColor = Color.FromArgb(foreColor);
            else
                codebox.SelectionColor = codebox.ForeColor;

            // Apply background color if specified
            if (backColor != -1)
                codebox.SelectionBackColor = Color.FromArgb(backColor);
            else
                codebox.SelectionBackColor = codebox.BackColor;

            // Apply bold styling
            Font currentFont = codebox.SelectionFont ?? codebox.Font; // fallback to control font
            FontStyle newStyle = currentFont.Style;
            if (bold)
                newStyle |= FontStyle.Bold;
            else
                newStyle &= ~FontStyle.Bold;

            Font newFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            codebox.SelectionFont = newFont;

            // Append the text with the applied formatting
            codebox.AppendText(text);

            // Reset selection properties to defaults for next appends (recommended)
            codebox.SelectionColor = codebox.ForeColor;
            codebox.SelectionBackColor = codebox.BackColor;
            codebox.SelectionFont = new Font(codebox.Font.FontFamily, codebox.Font.Size, FontStyle.Regular);

            // Trim old lines if we exceed the limit
            if (codebox.Lines.Length > MaxLines)
            {
                int linesToRemove = 10;
                int removeUpToIndex = codebox.GetFirstCharIndexFromLine(linesToRemove);

                codebox.Select(0, removeUpToIndex);
                codebox.SelectedText = string.Empty;
            }

            // Scroll to the bottom to show the latest text
            codebox.SelectionStart = codebox.Text.Length;
            codebox.ScrollToCaret();

            // Restore original read-only state
            codebox.ReadOnly = wasReadOnly;
        }


        private void txtWatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            string name = txtWatch.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                txtWatch.Focus();
                return;
            }

            // Prevent duplicates (case-insensitive)
            if (watchedObjects.ContainsKey(name))
            {
                MessageBox.Show($"You're already watching '{name}'.", "Already Watched",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtWatch.Clear();
                txtWatch.Focus();
                return;
            }

            // Add to our watch dictionary
            watchedObjects.Add(name, "Level: {App.AppLevels.Count-1}   Prg:{App.AppLevels[^1].PrgName}, Pri/Pub,Loc");

            // Add or refresh the tree
            RefreshTreeView();

            // Prepare for next entry
            txtWatch.Clear();
            txtWatch.Focus();
        }

        private void RefreshTreeView()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            if (watchedObjects.Count == 0)
            {
                treeView1.Nodes.Add("No variables being watched. Type a name and press Enter.");
            }
            else
            {
                foreach (var kvp in watchedObjects.OrderBy(k => k.Key)) // Alphabetical order
                {
                    string name = kvp.Key;
                    string obj = kvp.Value;

                    TreeNode rootNode = new TreeNode(BuildRootNodeText(name, obj))
                    {
                        Tag = name // Store the object for potential future use
                    };

                    PopulateTreeNode(rootNode, name);
                    treeView1.Nodes.Add(rootNode);
                    rootNode.Expand(); // Auto-expand first level for convenience
                }
            }

            treeView1.EndUpdate();
        }

        private string BuildRootNodeText(string name, string objInfo)
        {
            JAXObjects.Token obj = (name.Contains('[') || name.Contains('(')) ? App.GetVarFromExpression(name, null) : App.GetVarToken(name);

            string typeName = string.Empty;

            if (obj.TType.Equals("A"))
                typeName = "Array";
            else if (obj.Element.Type.Equals("O"))
                typeName = "Object";

            if (typeName.Length == 1)
                typeName = $"{name} {obj.Element.Type} = {obj.AsString()}";
            else
                typeName = $"{name} {typeName}";

            return typeName;
        }

        private void PopulateTreeNode(TreeNode parent, string name)
        {
            JAXObjects.Token obj = (name.Contains('[') || name.Contains('(')) ? App.GetVarFromExpression(name, null) : App.GetVarToken(name);

            if (obj.Element.Type.Equals("X"))
            {
                parent.Nodes.Add("null");
                return;
            }

            if (obj.TType.Equals("U"))
            {
                parent.Nodes.Add("unknown expression");
                return;
            }


            // Simple variables that are not objects are display and done
            if (obj.TType.Equals("S") && obj.Element.Type.Equals("O") == false)
            {
                parent.Text += " = " + obj.AsString();
                return;
            }

            if (obj.TType.Equals("A"))
            {
                // Handle Arrays
                parent.Nodes.Add("(empty)");
            }
            else
            {
                // Handle Ojbects
                parent.Nodes.Add("(no public members)");
            }


            // Arrays and collections
            //if (obj is IEnumerable enumerable && !(obj is string))
            //{
            //    int index = 0;
            //    foreach (object item in enumerable)
            //    {
            //        string itemType = item?.GetType().Name ?? "null";
            //        TreeNode node = new TreeNode($"[{index}] {{{itemType}}}");
            //        parent.Nodes.Add(node);
            //        PopulateTreeNode(node, item);
            //       index++;
            //   }

            //    if (index == 0)
            //        parent.Nodes.Add("(empty)");
            //
            //    return;
            //}

            //// Regular objects — show public properties and fields
            //bool hasMembers = false;

            //// Properties
            //foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
            //{
            //    hasMembers = true;
            //    object value = null;
            //    try { value = prop.GetValue(obj); }
            //    catch { value = "<exception>"; }

            //    TreeNode node = new TreeNode($"{prop.Name} {{{prop.PropertyType.Name}}}");
            //    parent.Nodes.Add(node);
            //    PopulateTreeNode(node, value);
            //}

            //// Fields
            //foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            //{
            //    hasMembers = true;
            //    object value = null;
            //    try { value = field.GetValue(obj); }
            //    catch { value = "<exception>"; }

            //    TreeNode node = new TreeNode($"{field.Name} {{{field.FieldType.Name}}}");
            //    parent.Nodes.Add(node);
            //    PopulateTreeNode(node, value);
            //}

            //if (!hasMembers)
            //    parent.Nodes.Add("(no public members)");
        }


        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRootNode();
                e.Handled = true;
            }
        }

        private void DeleteSelectedRootNode()
        {
            if (treeView1.SelectedNode == null)
                return;

            // Only allow deletion of root nodes (top-level watched variables)
            if (treeView1.SelectedNode.Parent != null)
            {
                MessageBox.Show("You can only remove top-level watched variables.", "Remove Watch",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string nodeText = treeView1.SelectedNode.Text;
            string name = ExtractVariableName(nodeText);

            if (string.IsNullOrEmpty(name))
                return;

            // Confirm deletion (optional but recommended)
            var result = MessageBox.Show($"Remove watch for '{name}'?", "Confirm Remove",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Remove from dictionary
            watchedObjects.Remove(name);

            // Refresh the entire tree
            RefreshTreeView();

            // Put focus back
            txtWatch.Focus();
        }

        private string ExtractVariableName(string nodeText)
        {
            // Example: "user {Object} = ..." → take everything before the first space
            int index = nodeText.IndexOf(' ');
            return index > 0 ? nodeText.Substring(0, index) : nodeText.Trim();
        }

        private void treeView1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRootNode();
                e.Handled = true;
            }
        }
    }
}
