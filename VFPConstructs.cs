using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JAXBase
{
    public static class VFPConstructs
    {
    }

    public static class GlobalKeyboardHook
    {
        // Delegate required for the low-level hook
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const uint WM_HOTKEY_MSG = 0x0312;   // WM_HOTKEY is safe and built-in

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static int _globalHotkeyId = 0xA000;   // arbitrary unique base

        // This is the ONLY thing the hook thread is allowed to touch directly
        private static readonly ManualResetEventSlim _initComplete = new ManualResetEventSlim(false);

        public static void Install()
        {
            // Register a few global hotkeys the Windows way (zero P/Invoke hell, 100% safe)
            RegisterGlobalHotkey(Keys.F12, 0);                     // F12 alone
            RegisterGlobalHotkey(Keys.F1, 0);                      // F1 alone
            RegisterGlobalHotkey(Keys.PrintScreen, 0);
            RegisterGlobalHotkey(Keys.Pause, 0);
            RegisterGlobalHotkey(Keys.LWin, 0);
            RegisterGlobalHotkey(Keys.RWin, 0);
            RegisterGlobalHotkey(Keys.L, Keys.Control | Keys.Alt); // Ctrl+Alt+L example

            // Low-level fallback for keys Windows won't let us register (very few)
            _hookID = SetHook(_proc);
            _initComplete.Set();
        }

        public static void Uninstall()
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;

            // Unregister the WM_HOTKEY ones
            for (int id = _globalHotkeyId; id < _globalHotkeyId + 100; id++)
                UnregisterHotKey(IntPtr.Zero, id);
        }

        // ------------------------------------------------------------------
        // 1. Preferred: Use real Windows global hotkeys (WM_HOTKEY) – 100% safe
        // ------------------------------------------------------------------
        private static void RegisterGlobalHotkey(Keys key, Keys modifiers)
        {
            int id = _globalHotkeyId++;
            uint mod = 0;
            if ((modifiers & Keys.Control) != 0) mod |= 0x0002; // MOD_CONTROL
            if ((modifiers & Keys.Alt) != 0) mod |= 0x0001; // MOD_ALT
            if ((modifiers & Keys.Shift) != 0) mod |= 0x0004; // MOD_SHIFT
            if ((modifiers & Keys.LWin) != 0 || (modifiers & Keys.RWin) != 0) mod |= 0x0008; // MOD_WIN

            if (!RegisterHotKey(IntPtr.Zero, id, mod, (uint)key))
                return; // some keys can't be registered globally (rare)

            // Store mapping so we know what to run later
            HotkeyMap[id] = (key, modifiers);
        }

        private static readonly Dictionary<int, (Keys key, Keys mods)> HotkeyMap = new();

        // This hidden window receives WM_HOTKEY messages on the MAIN UI thread
        private static readonly MessageOnlyWindow MessageWindow = new();

        private sealed class MessageOnlyWindow : NativeWindow, IDisposable
        {
            public MessageOnlyWindow()
            {
                CreateHandle(new CreateParams { ExStyle = 0 });
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY_MSG)
                {
                    int id = m.WParam.ToInt32();
                    if (HotkeyMap.TryGetValue(id, out var hotkey))
                    {
                        // 100% safe – we are on the UI thread
                        var (key, mods) = hotkey;
                        OnGlobalHotkeyPressed(key, mods);
                    }
                }
                base.WndProc(ref m);
            }

            public void Dispose() => DestroyHandle();
        }

        private static void OnGlobalHotkeyPressed(Keys key, Keys modifiers)
        {
            // ←←← HERE YOU CAN SAFELY ACCESS ANYTHING ←←←
            // Interpreter, forms, config, database, etc.
            //_ = Interpreter.Instance.ExecuteEventProcedureAsync($"GLOBALHOTKEY.{key}");
            // Example: block the default PrintScreen behavior
            if (key == Keys.PrintScreen)
                return; // we swallowed it

            
        }

        // ------------------------------------------------------------------
        // 2. Fallback low-level hook for the very few keys Windows blocks
        // ------------------------------------------------------------------
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)0x0104)) // WM_KEYDOWN or WM_SYSKEYDOWN
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Force-block these no matter who has focus
                if (key == Keys.F12 || key == Keys.PrintScreen || key == Keys.Pause)
                {
                    OnGlobalHotkeyPressed(key, Control.ModifierKeys);
                    return (IntPtr)1;   // swallowed
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule!;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(module.ModuleName), 0);
        }

        // ------------------------------------------------------------------
        // P/Invokes
        // ------------------------------------------------------------------
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}

