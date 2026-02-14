using static JAXBase.JAXDebuggerForm;

namespace JAXBase
{
    public class JAXDebugger
    {
        private readonly JAXDebuggerForm _form;
        private volatile DebugAction _currentAction = DebugAction.None;
        private bool _actionReady = false;

        public JAXDebugger(AppClass app)
        {
            _form = new(app);

            // Wire up all your buttons — this is the ONLY place actions are set
            _form.OnActionChosen += action =>
            {
                _currentAction = action;
                _actionReady = true;  // This wakes up the waiting loop below
            };
        }


        // Call this once at the start of debugging
        public void BeginDebugging(IWin32Window? owner = null)
        {
            _form.Show(owner);  // Modeless — does NOT block
            _form.BringToFront();
        }

        // Call this every time you want to pause and get user input
        public DebugAction GetResponse()
        {
            // Always update the display first
            _form.UpdateAll();
            _form.BringToFront();
            _form.WindowState = FormWindowState.Normal;
            _form.Activate();

            // Reset for this pause
            _actionReady = false;
            _currentAction = DebugAction.None;

            // Pump messages until the user clicks something
            while (!_actionReady && _form.Visible && !_form.IsDisposed)
            {
                Application.DoEvents();   // Keeps form fully responsive
                Thread.Sleep(10);         // Don't burn 100% CPU
            }

            // If user closed the form, treat as abort
            if (!_form.Visible || _form.IsDisposed)
                return DebugAction.Cancel;

            return _currentAction;
        }

        // Optional: close when done
        public void EndDebugging()
        {
            _form.Close();
        }
    }
}
