/*
 * The Form is the basic IDE - C# does not provide the capabilities that will
 * make a true xBase form, so I've got this one for the basic IDE and the
 * xClass_Form will suffice for the user code needs.
 * 
 */
using ZXing;
using static JAXBase.AppClass;

namespace JAXBase
{
    public partial class FrmJAXBase : Form
    {
        readonly AppClass App;

        public FrmJAXBase(AppClass app)
        {
            App = app;

            // ON KEY LABEL only works on Windows until we move to
            // a different UI layer, like Avalonia for C#
            // Won't be an issue in Version 2 with Qt
            if (App.OS == OSType.Windows)
                Application.AddMessageFilter(new F1Swallower(app));

            InitializeComponent();

            // Set the StartPosition to Manual so we can control the location
            this.StartPosition = FormStartPosition.Manual;

            // Get the working area of the primary screen (excluding taskbar)
            Rectangle workingArea = Screen.PrimaryScreen!.WorkingArea;

            // Calculate the new location for the form
            // X-coordinate: Right edge of working area - form width
            // Y-coordinate: Top edge of working area
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Top);
        }

        private void FrmJAXBase_Load(object sender, EventArgs e)
        {
            txtBox.Font = new Font("Courier New", 10);
            //txtBox.PreviewKeyDown += (s, e) => e.IsInputKey = true;
            //txtBox.KeyDown += (s, e) => { e.Handled = false; e.SuppressKeyPress = false; };
            txtBox.KeyPress += TxtBox_KeyPress;
            txtBox.TabStop = true;
            txtBox.TabIndex = 1;
            txtBox.Multiline = true;
            FrmJAXBase_SizeChanged(null, null);
            txtBox.Focus();
        }

        private void FrmJAXBase_SizeChanged(object? sender, EventArgs? e)
        {
            // Just update the panel size
            pnlScreen.Location = new() { X = 0, Y = 0 };
            pnlScreen.Size = new() { Height = this.Height - 41, Width = this.Width - 18 };
            txtBox.Size = new() { Height = pnlScreen.Height, Width = pnlScreen.Width };
        }

        private void TxtBox_KeyPress(object? sender, KeyPressEventArgs? e)
        {
            int x = 0;
            int y = 0;
            int k = 0;

            // Can't type during runtime unless suspended
            if (App.RuntimeFlag == false || App.SuspendFlag)
            {
                if (sender is not null)
                {
                    TextBox Sndr = (TextBox)sender;
                    x = Sndr.SelectionStart;
                    y = Sndr.SelectionLength;

                    if (e is not null)
                    {
                        k = e.KeyChar;

                        if (k == 13)
                        {
                            int f = Sndr.Text.IndexOf(Environment.NewLine, x);
                            if (f >= 0)
                                x = f;
                            else
                                x = Sndr.TextLength;

                            string a = y == 0 ? Sndr.Text[..x] : Sndr.Text.Substring(y, x);

                            if (string.IsNullOrWhiteSpace(a) == false)
                            {
                                string[] b = a.Split('\r');
                                string c = b[^1].Trim('\n');        // This is the command

                                if (x < Sndr.TextLength || y > 0)
                                {
                                    Sndr.Text += c;
                                    Sndr.SelectionStart = Sndr.TextLength;
                                    Sndr.SelectionLength = 0;
                                }

                                App.ClearErrors();  // Reset errors befor executing user command

                                // Compile the command
                                if (App.CurrentDS.JaxSettings.Alternate && string.IsNullOrWhiteSpace(App.CurrentDS.JaxSettings.Alternate_Name) == false)
                                    JAXLib.StrToFile(c, App.CurrentDS.JaxSettings.Alternate_Name, 1);

                                string r = App.JaxCompiler.CompileLine(c, false);

                                if (r.Length > 1)
                                {
                                    string outText = App.JaxExecuter.ExecuteCommand(r) ?? string.Empty;

                                    if (outText.Length > 0)
                                    {
                                        App.JAXConsoles[App.ActiveConsole].Write(Environment.NewLine + outText);
                                        App.DebugLog("Console: " + outText);
                                    }
                                    if (outText.Length > 0 && App.CurrentDS.JaxSettings.Alternate && string.IsNullOrWhiteSpace(App.CurrentDS.JaxSettings.Alternate_Name) == false)
                                        JAXLib.StrToFile(outText, App.CurrentDS.JaxSettings.Alternate_Name, 3);
                                }

                                if (App.ErrorCount() > 0)
                                {
                                    JAXErrors err = App.GetLastError();
                                    MessageBox.Show(err.ErrorMessage, string.Format("Error {0}", err.ErrorNo), MessageBoxButtons.OK, MessageBoxIcon.Error);

                                }
                            }
                        }
                    }
                }
            }
            else
                e!.Handled = true;   // Eat the keystroke
        }


        /*
         * Perform an orderly shutdown of the application
         */
        private void FrmJAXBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            JAXBase_Executer_Q.Quit(App, null);
        }

        private void txtBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

