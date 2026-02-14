/*
 * This class is the key to the system as it holds everything
 * that is system related, such as variables, settings, ect.
 * 
 * The only way to pass information in a global/public manner 
 * is through this class.
 *          
 */
using DeftSharp.Windows.Input.Mouse;
using NodaTime;
using NodaTime.TimeZones;
using System.Net;
using System.Runtime.InteropServices;
using static JAXBase.JAXObjects;

namespace JAXBase
{
    public class AppClass
    {
        //-------------------------------------------------------------
        // COMMAND BYTE DECLARATIONS
        //-------------------------------------------------------------

        // Literals - a literal expression is typically a string
        // like a file name or constant
        public static readonly char literalStart = (char)2;
        public static readonly char literalEnd = (char)3;

        // Header
        public static readonly char headerStartByte = (char)6;
        public static readonly char headerEndByte = (char)7;
        public static readonly char headerMapStartByte = (char)8;
        public static readonly char headerMapEndByte = (char)9;

        // Expressions
        public static readonly char expByte = (char)14;         // Exp start
        public static readonly char expParam = (char)15;        // Delimits RPN parts
        public static readonly char expEnd = (char)16;          // End of expression
        public static readonly char expDelimiter = (char)17;    // Delimits RPN expressions
        public static readonly char parameterEnd = (char)18;    // Delimits parameters

        public static readonly char stmtDelimiter = (char)20;// Statement delimiter

        // Command start/stop bytes
        public static readonly char appByte = (char)27;      // App module declaration
        public static readonly char appEnd = (char)28;       // End module declaration
        public static readonly char cmdByte = (char)29;      // Start of command
        public static readonly char cmdEnd = (char)30;       // End of a command

        public static readonly int CurrentMajorVersion = 0;  // Version Info
        public static readonly int CurrentMinorVersion = 2;

        public readonly Utilities utl;
        public readonly JAXLanguageLists lists = new();

        // Used for var assignments
        public readonly Token NullToken;

        public Dictionary<string, string> OnKeyLabel = [];
        public Dictionary<string, string> MiscInfo = [];
        public Dictionary<string, string> SysObjects = [];
        //public List<JAXObjects.Token> ParameterList = [];
        public List<ParameterClass> ParameterClassList = [];
        public List<string> CmdList = [];

        public JAXObjects.Token ReturnValue = new();
        public string RTFileName = string.Empty;

        public string OnErrorCommand = string.Empty;
        public string InDefine = string.Empty;
        public JAXObjectWrapper? InDefineObject = null;

        public readonly string MyInstance;

        public readonly JAXBase_Executer JaxExecuter;
        public readonly JAXBase_Compiler JaxCompiler;
        public JAXMath JaxMath;

        public string ActiveConsole { get; private set; } = "default";
        public Dictionary<string, JAXConsole> JAXConsoles = []; // Console windows

        public Dictionary<char, string> XRef4Runtime = new(); // Convert compler byte to runtime codes
        public Dictionary<string, string> RunTimeCodes = new(); // Runtime codes - Human readable runtime statement elements

        //public readonly JAXSettings JaxSettings = new();
        public readonly JAXVariables JaxVariables = new();

        public JAXImages JaxImages;

        public JAXDebugger? JaxDebugger = null;
        public JAXDebuggerForm.DebugAction DebugAction = JAXDebuggerForm.DebugAction.None;
        public bool DebugActionConsumed = false;

        public DateTimeZone TimeZone { get { return BclDateTimeZone.FromTimeZoneInfo(TimeZoneInfo.Local); } }

        //-------------------------------------------------------------
        // INITIALIZATION
        //-------------------------------------------------------------
        public AppClass()
        {
            utl = new(this);
            JaxImages = new(this);

            // Perform the App Startup
            JAXStartup.AppStartup(this);

            JAXConsole JaxConsole = new JAXConsole();
            JAXConsoles.Add("default", JaxConsole);
            JAXConsoles["default"].SetPosition(0, 0);
            JAXConsoles["default"].Visible(true); // Make default visible

            AppWorkFolder = JaxVariables._WorkPath;

            AppLogFile = string.Format(AppWorkFolder + "Logs\\System_{0}.log", DateTime.Now.ToString("yyyyMMddHHmmssff"));

            // Make sure the app work folder is there
            if (string.IsNullOrWhiteSpace(AppWorkFolder) == false && Directory.Exists(AppWorkFolder) == false)
                Directory.CreateDirectory(AppWorkFolder);

            // Make sure the log folder is there
            string path = JAXLib.JustFullPath(AppLogFile);
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS = OSType.Windows;
                string[] uName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split("\\");

                if (uName.Length > 1)
                    UserName = uName[1].ToUpper();
                else
                    UserName = uName[0].ToUpper();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) OS = OSType.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) OS = OSType.Mac;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) OS = OSType.FreeBSD;

            // Set up the system and default data session
            jaxDataSession.Add(0, new(this, "*system"));
            jaxDataSession.Add(1, new(this, "*default"));
            CurrentDataSession = 1;

            CurrentDS.JaxSettings.Default = JAXLib.Addbs(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            // ----------------------------------------------------------------
            // Perform some platform specific startup chores
            // ----------------------------------------------------------------
            switch (OS)
            {
                case OSType.Windows:
                    break;

                case OSType.Linux:
                    break;

                case OSType.Mac:
                    break;

                case OSType.FreeBSD:
                    break;

                default: // UNKNOWN
                    break;
            }

            // Set up the null token
            NullToken = new();
            NullToken.Element.MakeNull();

            if (File.Exists(ExeFolder + "jaxbase.ini"))
            {
                bool inError = false;
                string iData = JAXLib.FileToStr(ExeFolder + "jaxbase.ini").Replace("\n", "");
                string[] iniData = iData.Split('\r');

                for (int i = 0; i < iniData.Length; i++)
                {
                    string iniLine = iniData[i].Trim();
                    if (iniLine.Length > 0)
                    {
                        if (iniLine[0].Equals(';') == false)
                        {
                            if (iniLine.Contains('='))
                            {
                                string[] iLine = iniLine.Split('=');
                                switch (iLine[0])
                                {
                                    case "logfolder":
                                        iLine[1] = JAXLib.Addbs(iLine[1].Trim());
                                        if (Directory.Exists(iLine[1]) == false)
                                        {
                                            try
                                            {
                                                Directory.CreateDirectory(iLine[1]);
                                                AppLogFile = iLine[1] + string.Format("System_{0}.log", DateTime.Now.ToString("yyyyMMddHHmmssff"));
                                            }
                                            catch (Exception e)
                                            {
                                                DebugLog(string.Format("INI Error with {0} - {1}", iniLine, e.Message));
                                                inError = true;
                                            }
                                        }
                                        break;

                                    case "loglife":
                                        if (int.TryParse(iLine[1], out LogLife) == false)
                                        {
                                            LogLife = 0;
                                            DebugLog(string.Format("INI Error with {0} - could not parse int value", iniLine));
                                            inError = true;
                                        }
                                        break;

                                    case "workfolder":
                                        iLine[1] = JAXLib.Addbs(iLine[1].Trim());
                                        if (Directory.Exists(iLine[1]) == false)
                                        {
                                            try
                                            {
                                                Directory.CreateDirectory(iLine[1]);
                                                AppWorkFolder = iLine[1];
                                            }
                                            catch (Exception e)
                                            {
                                                DebugLog(string.Format("INI Error with {0} - {1}", iniLine, e.Message));
                                                inError = true;
                                            }
                                        }
                                        break;

                                    case "tempfolder":
                                        iLine[1] = JAXLib.Addbs(iLine[1].Trim());
                                        if (Directory.Exists(iLine[1]) == false)
                                        {
                                            try
                                            {
                                                Directory.CreateDirectory(iLine[1]);
                                                AppTempFolder = iLine[1];
                                            }
                                            catch (Exception e)
                                            {
                                                DebugLog(string.Format("INI Error with {0} - {1}", iniLine, e.Message));
                                                inError = true;
                                            }
                                        }
                                        break;

                                    case "console":         // console window flag
                                        break;

                                    case "default":         // Set the default directory
                                        iLine[1] = JAXLib.Addbs(iLine[1].Trim());
                                        if (Directory.Exists(iLine[1]) == false)
                                        {
                                            try
                                            {
                                                Directory.CreateDirectory(iLine[1]);
                                                CurrentDS.JaxSettings.Default = JAXLib.Addbs(iLine[1].Trim());
                                            }
                                            catch (Exception e)
                                            {
                                                DebugLog(string.Format("INI Error with {0} - {1}", iniLine, e.Message));
                                                inError = true;
                                            }
                                        }
                                        break;

                                    default:
                                        DebugLog(string.Format("Unknown INI command - {0}", iniLine));
                                        inError = true;
                                        break;
                                }
                            }
                        }
                    }
                }

                if (inError)
                    MessageBox.Show("Errors were detected in the jaxbase.ini file - check the log for details", "JAXBase INI Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Delete log files older than x days
            try
            {
                string logPath = JAXLib.JustFullPath(AppLogFile);


                FilerLib.GetFiles(logPath, out string[] logFiles);
                for (int i = 0; i < logFiles.Length; i++)
                {
                    FilerLib.GetFileInfo(logFiles[i], out string[] fileInfo);
                    if (fileInfo.Length > 3)
                    {
                        if (DateTime.TryParse(fileInfo[2], out DateTime dt))
                        {
                            if ((LogLife > 0 && (DateTime.Now - dt).TotalDays > LogLife) || LogLife == -1)
                                FilerLib.DeleteFile(logPath + logFiles[i]);
                        }
                        else
                            DebugLog(string.Format("Could not parse file date of {0} for {1}", fileInfo[2], logPath + logFiles[i]));
                    }
                    else
                        DebugLog(string.Format("Could not get file info for {0}", fileInfo[2]));

                }
            }
            catch (Exception ex)
            {
                DebugLog(string.Format("Error in Log clean up - {0}", ex.Message));
            }

            AppLevels.Add(new AppLevel());

            JaxExecuter = new(this);
            JaxCompiler = new(this);
            JaxMath = new(this);

            MyInstance = SystemCounter();
        }


        //-------------------------------------------------------------
        // GLOBALS
        //-------------------------------------------------------------

        /*-----------------------------------------------------------*
         * System variables
         *-----------------------------------------------------------*/
        public enum OSType { Windows, Linux, Mac, FreeBSD, Unknown };
        public readonly bool UsesTables = true;
        public readonly string UserName = string.Empty;
        public readonly string MachineName = Environment.MachineName;
        public readonly string HostName = Dns.GetHostName();
        public readonly string ComputerName = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.MachineName;
        public readonly OSType OS = OSType.Unknown;
        public readonly string ExeFolder = string.Empty;

        public Form? WaitWindow = null;

        /*-----------------------------------------------------------*
         * Language settings - set commands
         * Hidden System and object settings
         * Printer and Tool settings - system _variables
         *-----------------------------------------------------------*/
        //public readonly JAXObjects JAXSettings = new(); // Settings
        public JAXObjects JAXSysObj = new();            // System vars
        public JAXObjects JAXPrtObj = new();            // Printer/tools



        /*-----------------------------------------------------------*
         * DataSession handling
         *-----------------------------------------------------------*/
        public Dictionary<int, JAXDataSession> jaxDataSession = [];
        public int CurrentDataSession { get; private set; } = 0;
        public JAXDataSession CurrentDS { get { return jaxDataSession[CurrentDataSession]; } set { } }

        public int DestroyDataSession(int datasession)
        {
            return jaxDataSession.Remove(datasession) ? 0 : 1;
        }

        public int CreateNewDataSession(string name)
        {
            int ds = 2;

            // Look for the lowest open data session number
            for (int i = 0; i < jaxDataSession.Count; i++)
            {
                if (jaxDataSession.ContainsKey(ds))
                    ds++;
                else
                    break;
            }

            // Set up the new session name
            if (string.IsNullOrEmpty(name))
                name = "*Session" + ds.ToString("000");

            // Set up the new session
            JAXDataSession dsession = new(this, name);

            // Copy the JAXSettings info from session
            // 1 and add it to the dictionary
            if (jaxDataSession.ContainsKey(1))
                dsession.JaxSettings = JAXUtilities.CloneJson(jaxDataSession[1].JaxSettings) ?? new();

            jaxDataSession.Add(ds, dsession);

            return ds;
        }


        public void SetDataSession(int ds)
        {
            if (jaxDataSession.ContainsKey(ds))
                CurrentDataSession = ds;
            else
                throw new Exception(string.Format("4014|{0}", ds));
        }

        /*-----------------------------------------------------------*
         * Logging
         *-----------------------------------------------------------*/
        public string AppLogFile = string.Empty;
        public string AppWorkFolder = string.Empty;
        public string AppBaseFolder = string.Empty;
        public string AppTempFolder = string.Empty;

        public int ConsoleWidth = -1;       // No console requested
        public bool Overwrite = false;      // Insert/Overwrite status
        public bool InRead = true;          // false=no, true=in read

        private int LogLife = 0; // 0=keep, -1=kill all, 1+=age in days


        public List<FileHandle> FileHandles = [];





        public List<CCodeCache> CodeCache = [];
        public Dictionary<string, CCodeCache> ClassLibs = [];
        public List<string> PRGCache = [];

        public List<ClassDef> ClassDefinitions = [];
        public string CurrentClassMethod = string.Empty;


        public List<AppLevel> AppLevels = [];

        public bool SuspendFlag = false;      // True= suspend execution
        public bool CancelFlag = false;       // True= cancel execution
        public bool RuntimeFlag = false;      // True= running a prg
        public bool InCompile = false;        // True= in compile cmd

        // Used by the Continue command
        public LoopClass LastLocate = new();

        public List<string> WithHold = [];

        // ------------------------------------------------------------
        // Mouse and Keyboard handling
        // ------------------------------------------------------------
        public MouseListener mouseListener = new();
        //public KeyboardListener keyboardListener = new();

        // ------------------------------------------------------------
        // Create a unique 10-character session key representing
        // the time since Jan 1, 0001 in 1/10,000 seconds
        // ------------------------------------------------------------
        public long systemCounter { get; private set; } = 0;

        /// <summary>
        /// Creates a unique 10 character session key.
        /// </summary>
        /// <param name="MaxChars"></param>
        /// <returns></returns>
        public string SystemCounter()
        {
            long t = DateTime.Now.Ticks;

            // C# code will never need this loop but C++ code
            // on some future system just might need it.
            while (t / 1000 == systemCounter)
                t = DateTime.Now.Ticks;

            systemCounter = t / 1000;

            utl.Conv36(systemCounter, 10, out string p1);
            return p1;
        }


        // Return the current flow control object
        public string GetLoopStack()
        {
            return AppLevels[^1].LoopStack.Count > 0 ? AppLevels[^1].LoopStack[^1] : string.Empty;
        }

        // Add a new flow control object to the stack
        public string AddLoop(string ltype)
        {
            if (AppLevels[^1].LoopStack.Count > 999) throw new Exception("Control object stack overflow");
            utl.Conv64(AppLevels[^1].LoopCounter++, 2, out string lvl);
            string lp = string.Format("{0}{1}", ltype, lvl);
            AppLevels[^1].LoopStack.Add(lp);
            return lp;
        }

        // Add an existing flow control object to the stack
        public void PushLoop(string lcontrol)
        {
            if (AppLevels[^1].LoopStack.Count > 999) throw new Exception("Control object stack overflow");
            AppLevels[^1].LoopStack.Add(lcontrol);

            if (lcontrol[0] == 'F')
            {
                // Add the for loop
                AppLevels[^1].ForLoops.Add(lcontrol, new());
            }
        }


        // Remove the most recent loop stack object
        public string PopLoopStack()
        {
            string pop = string.Empty;
            if (AppLevels[^1].LoopStack.Count == 0) throw new Exception("Control object stack underflow");
            pop = AppLevels[^1].LoopStack[^1];
            AppLevels[^1].LoopStack.RemoveAt(AppLevels[^1].LoopStack.Count - 1);

            // Toss the dictionary entry for the FOR loop
            if (pop[0] == 'F')
                AppLevels[^1].ForLoops.Remove(pop);

            return pop;
        }


        public void PushAppLevel(string prgName, JAXObjectWrapper? wrapper, string wrapperMethod)
        {
            AppLevel appLevel = new()

            {
                PrgName = prgName,
                ThisObject = wrapper,
                ThisObjectMethod = wrapperMethod
            };
            AppLevels.Add(appLevel);
        }


        public void Cancel()
        {
            // Cancel running application
            if (RuntimeFlag)
            {
                // Cancel everything
            }

            CancelFlag = true;
            RuntimeFlag = false;

            // IDE cancel will release all app levels greater than 0 and
            // reset the vars to blank
        }

        public void PopAppLevel()
        {
            AppLevels.RemoveAt(AppLevels.Count - 1);
        }


        // Set debug ON/OFF will control when the debug works
        // Send to the default console
        public void DebugLog(string text) { DebugLog(text, true); }
        public void DebugLog(string text, bool writeToFileOnly)
        {
            if (CurrentDS.JaxSettings.Debug)
            {
                string debugText = DateTime.Now.ToString("MM/dd HH:mm:ss.ffff").PadRight(20) + text;
                JAXLib.StrToFile(debugText + "\r\n", AppLogFile, 3);

                if (writeToFileOnly == false && JAXConsoles.ContainsKey("default"))
                {
                    if (JAXConsoles["default"].active)
                        JAXConsoles["default"].WriteLine(text);
                }
            }
        }


        // Set talk ON/OFF will control when the TALK works
        // Send to the default console
        public void Talk(string text)
        {
            if (CurrentDS.JaxSettings.Talk && JAXConsoles["default"].active)
            {
                if (JAXConsoles.ContainsKey("default"))
                    Console.WriteLine(text);
            }

            DebugLog("TALK: " + text, true); // Always write to the log file
        }


        /*
         * When building a parameter list, we just need to know where 
         * the varaible is located so we can locate it again when 
         * it's pulled from the list.
         */
        public bool FindVar(string varName, out int level)
        {
            bool result = false;
            level = 0;

            try
            {
                // First check the currently running AppLevel becuase
                // a local var will have precidence over a public or
                // private var of the same name defined earlier in
                // the app levels
                JAXObjects.Token tk = AppLevels[^1].LocalVars.GetToken(varName);

                if (tk.TType.Equals("U"))
                    tk = AppLevels[^1].PrivateVars.GetToken(varName);

                if (tk.TType.Equals("U"))
                {
                    // Look for private var of this name (level 0 vars are public)
                    for (int i = AppLevels.Count - 1; i >= 0; i--)
                    {
                        tk = AppLevels[^1].LocalVars.GetToken(varName);
                        level = 0;

                        if (tk.TType.Equals("U") == false)
                        {
                            result = true;
                            level = i;
                            break;
                        }
                    }
                }
                else
                {
                    // Found in current level
                    level = AppLevels.Count - 1;
                    result = true;
                }
            }
            catch (Exception ex)
            {
                SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                level = 0;
                result = false;
            }


            return result;
        }


        /*-------------------------------------------------------------*
         * 
         * Sets up the var as local if it's not already a local var
         * 
         *-------------------------------------------------------------*/
        public void MakeLocalVar(string varName, int row, int col, bool alterArray)
        {
            Token tk;
            varName = varName.ToLower();

            if (JAXLib.InList(varName, "this", "thisform", "thisformset"))
            {
                // Can only set if it's not already in existance
                tk = AppLevels[^1].LocalVars.GetToken(varName);
                if (tk.TType.Equals("U") == false)
                    throw new Exception("2405|" + varName.ToUpper());
            }

            if (row < 0 || col < 0)
            {
                // It's a simple var
                row = 1;
                col = 1;
            }

            if (row > 0 && col < 1)
            {
                // Set up 1D array settings
                col = row;
                row = 0;
            }

            // make sure col is at least 1
            if (col < 1) col = 1;

            // Is it a memory Var reference?  Strip m. if it is
            if (varName.Length > 2 && varName[..2].Equals("m.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            // Is it a legal var name?  First non underscore char must be a letter
            if (JAXLib.Between(varName.Replace("_", "")[..1].ToLower(), "a", "z") == false)
                throw new Exception(string.Format("225|{0}", varName));

            // Vars names may only contain letters, numbers, or underscores
            if (JAXLib.ChrTran(varName.ToLower(), "abcdefghijklmnopqrstuvwxyz0123456789_", "").Length > 0)
                throw new Exception(string.Format("225|{0}", varName));

            // Check local variables
            tk = AppLevels[^1].PrivateVars.GetToken(varName);

            // Check private variables
            if (tk.TType.Equals("U"))
            {
                // Not found anywhere so add it to the private sector
                // of the current level and if it's an array, tell it
                // that it's ok to set the dimensions
                AppLevels[^1].LocalVars.SetDimension(varName, row, col, true);
            }
            else
                throw new Exception("1208|" + varName);
        }

        /*-------------------------------------------------------------*
         * 
         * Sets up the var as public if it's not already a local var
         * 
         *-------------------------------------------------------------*/
        public void MakePublicVar(string varName, int row, int col, bool alterArray)
        {
            Token tk;
            varName = varName.ToLower();

            if (JAXLib.InList(varName, "this", "thisform", "thisformset"))
                throw new Exception("2403|" + varName.ToUpper());

            if (row > 0 && col < 1)
            {
                // Set up 1D array settings
                col = row;
                row = 0;
            }

            if (row < 0 && col < 0)
            {
                row = 1;
                col = 1;
            }

            // Is it a memory Var reference?  Strip m. if it is
            if (varName.Length > 2 && varName[..2].Equals("m.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            // Is it a legal var name?  First non underscore char must be a letter
            if (JAXLib.Between(varName.Replace("_", "")[..1].ToLower(), "a", "z") == false)
                throw new Exception(string.Format("225|{0}", varName));

            // Vars names may only contain letters, numbers, or underscores
            if (JAXLib.ChrTran(varName.ToLower(), "abcdefghijklmnopqrstuvwxyz0123456789_", "").Length > 0)
                throw new Exception(string.Format("225|{0}", varName));

            // Does this var exist?
            tk = GetVarToken(varName);
            //GetVar(varName, out tk);

            // Check private variables (Globals are AppLevel[0] private vars
            if (tk.TType.Equals("U"))
            {
                // Not found so add it to the private sector of level 0
                // and if it's an array, set the dimensions if allowed
                AppLevels[0].PrivateVars.SetDimension(varName, row, col, alterArray);
            }
            else
                throw new Exception("1208|" + varName);
        }


        /*-------------------------------------------------------------*
         * Sets up the var as private if it's not already a public var 
         * and copies the entire contents of the JAXObjects.Token 
         * into the var.
         *-------------------------------------------------------------*/
        public void SetVarOrMakePrivate(string varName, JAXObjects.Token tk)
        {
            SetVarOrMakePrivate(varName, 1, 1, false);
            SetVar(varName, tk);
        }

        /*-------------------------------------------------------------*
         * Sets up the var as private if it's not already available
         * Returns -1 =Local, 0=Public, >1 =Private level
         * indicating where the variable is located
         *
         * If makeArray is true, it will force the variable to be an 
         * array if element count > 1 and if false, it will be an 
         * array only if the element count > 1
         *-------------------------------------------------------------*/
        public int SetVarOrMakePrivate(string varName, int row, int col, bool alterArray)
        {
            int iResult = 0;
            Token tk;
            varName = varName.ToLower();

            if (JAXLib.InList(varName, "this", "thisform", "thisformset"))
                throw new Exception("2404|" + varName.ToUpper());

            // 1D arrays may come in as col < 1
            if (row > 0 && col < 1)
            {
                // Set up 1D array settings
                col = row;
                row = 0;
            }

            // make sure row & col are at least set to 1
            row = row < 0 ? 0 : row;
            col = col < 1 ? 1 : col;

            // Is it a memory Var reference?  Strip m. if it is
            if (varName.Length > 2 && varName[..2].Equals("m.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            // Legal var name?  First non underscore char must be a letter
            if (JAXLib.Between(varName.Replace("_", "")[..1].ToLower(), "a", "z") == false)
                throw new Exception(string.Format("225|{0}", varName));

            // Vars names only contain letters, numbers, or underscores
            if (JAXLib.ChrTran(varName.ToLower(), "abcdefghijklmnopqrstuvwxyz0123456789_", "").Length > 0)
                throw new Exception(string.Format("225|{0}", varName));

            // Check local variables
            tk = AppLevels[^1].LocalVars.GetToken(varName);

            // Check private variables
            if (tk.TType.Equals("U"))
            {
                // Check all the private vars
                for (int i = AppLevels.Count - 1; i >= 0; i--)
                {
                    tk = AppLevels[i].PrivateVars.GetToken(varName);
                    if (tk.TType.Equals("U") == false)
                    {
                        // Found it in the private vars of an app level
                        AppLevels[i].PrivateVars.SetDimension(varName, row, col, alterArray);
                        DebugLog($"Set dimensions of {varName} to {row},{col} in AppLevel {i}");
                        iResult = i;
                        break;
                    }
                }

                if (tk.TType.Equals("U"))
                {
                    // Not found anywhere so add it to the private
                    // sector of the current level
                    AppLevels[^1].PrivateVars.SetDimension(varName, row, col, alterArray);
                    DebugLog($"Set dimensions of {varName} to {row},{col} in AppLevel {AppLevels.Count - 1}");
                    iResult = AppLevels.Count - 1;
                }
            }
            else
            {
                // Found it in the local variables of the current
                // app level
                AppLevels[^1].LocalVars.SetDimension(varName, row, col, alterArray);
                DebugLog($"Set dimensions of {varName} to {row},{col} in AppLevel {AppLevels.Count - 1}");
                iResult = -1;
            }

            return iResult;
        }





        /*-------------------------------------------------------------*
         * Get a variable from the App local/private, and global
         * stacks along with fields from the current workarea
         *
         * Precedence is:
         *      "table.", "a." - ".j", or "m." memory variables
         *      object.property
         *      current workarea fields
         *      local
         *      private
         *      global
         *
         *-------------------------------------------------------------*/
        public Token GetVarToken(string varName) { return GetVarToken(varName, false); }

        public Token GetVarToken(string varName, bool fillAbsentData)
        {
            Token tk;
            varName = varName.ToLower();

            // If it's a "M." variable, skip checking the local work area
            if (varName.Length > 2 && varName[..2].Equals("M.", StringComparison.OrdinalIgnoreCase))
            {
                // Strip the m. from the name
                varName = varName[2..];

                // Check local variables
                tk = AppLevels[^1].LocalVars.GetToken(varName);
            }
            else
            {
                // Is it a Table.Field reference?
                if (varName.Contains('.'))
                {
                    // TODO - Get the table and field name
                    string[] varParts = varName.Split('.');

                    if (CurrentDS.IsWorkArea(varParts[0]))
                    {

                        // if tbl is a -> j then it's a work area reference
                        int wa = "abcdefghij".IndexOf(varParts[0], StringComparison.CurrentCultureIgnoreCase);

                        if (wa < 0)
                        {
                            // It's a table reference
                            tk = CurrentDS.GetFieldToken(varParts[0], varParts[1], fillAbsentData);
                        }
                        else
                        {
                            // It's a work area refreence
                            tk = CurrentDS.GetFieldToken(wa, varParts[1], fillAbsentData);
                        }
                    }
                    else
                    {
                        // Assume it's an object and we're using the wrong tool
                        tk = new();
                        tk.TType = "U";
                    }
                }
                else
                {
                    // is there a table open in this workarea?
                    if (CurrentDS.FieldExists(varName))
                    {
                        // Check the current open table
                        tk = CurrentDS.GetFieldToken(-1, varName, fillAbsentData);
                    }
                    else
                    {
                        tk = new();
                        tk.TType = "U";
                    }
                }

                // Check local variables
                if (tk.TType.Equals("U")) tk = AppLevels[^1].LocalVars.GetToken(varName);
            }

            // Check private variables (Globals are AppLevel[0] private vars)
            if (tk.TType.Equals("U"))
            {
                // Check all the private vars
                for (int i = AppLevels.Count - 1; i >= 0; i--)
                {
                    tk = AppLevels[i].PrivateVars.GetToken(varName);
                    if (tk.TType.Equals("U") == false)
                        break;   // Found it!
                }
            }

            // Make sure this, thisform, and thisformset are used correctly
            if (JAXLib.InListC(varName, "this", "thisform", "thisformset"))
            {
                switch (varName)
                {
                    case "thisformset":
                        if ("X".Contains(tk.TType))
                            throw new Exception("2402|");
                        break;

                    case "this":
                        if ("X".Contains(tk.TType))
                            throw new Exception("2400|");
                        break;

                    default:
                        if ("X".Contains(tk.TType))
                            throw new Exception("2401|");
                        break;
                }
            }

            return tk;
        }

        /*-------------------------------------------------------------*
         * 
         *-------------------------------------------------------------*/
        public void SetVarWithSimpleToken(string varName, SimpleToken stoken, int row, int col)
        {
            Token tk;

            varName = varName.Trim().ToLower();

            // Is it a memory Var reference?  Strip m. if it is
            if (varName.Length > 2 && varName[..2].Equals("m.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            if (JAXLib.InList(varName, "this", "thisform", "thisformset"))
            {
                // Can only set if it's not already in existance
                tk = AppLevels[^1].LocalVars.GetToken(varName);
                if (tk.TType.Equals("U") == false)
                    throw new Exception("2405|" + varName.ToUpper());
            }

            // Check local variables
            tk = AppLevels[^1].LocalVars.GetToken(varName);

            // Check private variables (Globals are AppLevel[0] private vars)
            if (tk.TType.Equals("U"))
            {
                // Check all the private vars
                for (int i = AppLevels.Count - 1; i >= 0; i--)
                {
                    tk = AppLevels[i].PrivateVars.GetToken(varName);
                    if (tk.TType.Equals("U") == false)
                    {
                        AppLevels[i].PrivateVars.SetValueWithSimpleToken(varName, stoken, row, col);
                        break;   // Found it!
                    }
                }
            }
            else
            {
                // Set the local variable
                AppLevels[^1].LocalVars.SetValueWithSimpleToken(varName, stoken, row, col);
            }
        }


        /*-------------------------------------------------------------*
         * Specifically override with a new token
         *-------------------------------------------------------------*/
        public void SetVar(string varName, Token newTK)
        {
            Token tk;

            varName = varName.Trim().ToLower();

            // Is it a memory Var reference?  Strip m. if it is
            if (varName.Length > 2 && varName[..2].Equals("M.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            if (JAXLib.InList(varName, "this", "thisform", "thisformset"))
            {
                // Can only set if it's not already in existance
                tk = AppLevels[^1].LocalVars.GetToken(varName);
                if (tk.TType.Equals("U") == false)
                    throw new Exception("2405|" + varName.ToUpper());
            }

            // Check local variables
            tk = AppLevels[^1].LocalVars.GetToken(varName);

            // Check private variables (Globals are AppLevel[0] private vars]
            if (tk.TType.Equals("U"))
            {
                // Check all the private vars
                for (int i = AppLevels.Count - 1; i >= 0; i--)
                {
                    tk = AppLevels[i].PrivateVars.GetToken(varName);
                    if (tk.TType.Equals("U") == false)
                    {
                        DebugLog($"Set private var {varName} in AppLevel {i} to {newTK.Element.Value}");
                        AppLevels[i].PrivateVars.SetToken(varName, newTK);
                        break;   // Found it!
                    }
                }
            }
            else
            {
                // Set the local variable
                DebugLog($"Set local var {varName} in AppLevel {AppLevels.Count - 1} to {newTK.Element.Value}");
                AppLevels[^1].LocalVars.SetToken(varName, newTK);
            }
        }


        /*-------------------------------------------------------------*
         * This routine is not to be used except for setting local 
         * vars for the system
         * 
         * Such as THIS, THISFORM, THISFORMSET, etc
         *-------------------------------------------------------------*/
        public void SetLocalSystemVar(string varName, object? obj, int row, int col, bool alterArray)
        {
            // Check local variables
            if (varName.Length > 3 && varName[..2].Equals("m.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            Token tk = AppLevels[^1].LocalVars.GetToken(varName);

            if (tk.TType.Equals("U"))
            {
                // Not found so add it to the local current level
                AppLevels[^1].LocalVars.SetDimension(varName, row, col, alterArray);
            }

            SetVar(varName, obj, row, col);
        }




        /**************************************************************
         * This routine takes a string which is expected to hold a 
         * variable reference and if it contains a ( or [ then assumes 
         * it's an array variable and solves for what's in the brackets
         * and returns a string in the format of
         *      var[x] or var[x,y]
         * where x and y are expected to be valid numeric values for 
         * that array
         * 
         * If ( or [ are not found, it just returns the string 
         * expecting it to be just a simple variable reference.
         * 
         * The OUT is a class that contains the variable name, row, 
         * and column values. The "col" value is zero if only one 
         * dimension is given
         **************************************************************/
        public string SolveVariableReference(string varString, out VarRef vref)
        {
            vref = new();
            varString = varString.ToLower();
            string sResult = string.Empty;

            if (varString.Length > 3 && varString[..2].Equals("m.", StringComparison.OrdinalIgnoreCase))
                varString = varString[2..];

            if (varString[^1] == '.')
                throw new Exception("10|");

            if (varString.Contains('[') || varString.Contains('('))
            {
                // Indications are that we have an array variable
                while (true)
                {
                    if ("[(".Contains(varString[0]))
                        break;
                    else
                    {
                        sResult += varString[..1];
                        varString = varString[1..];
                    }
                }

                vref.varName = sResult;

                // We have the array variable, now solve for what's
                // inside the dimension brackets and strip the
                // outside brackets
                varString = varString[1..];
                varString = varString[..^1];

                // look for the comma
                string sRow = string.Empty;
                string sCol = string.Empty;
                string quote = string.Empty;

                while (string.IsNullOrEmpty(varString) == false)
                {
                    if (string.IsNullOrEmpty(quote) && "([\"'".Contains(varString[0]))
                    {
                        sRow += varString[0].ToString();
                        quote = varString[0] switch
                        {
                            '[' => "]",
                            '(' => ")",
                            _ => varString[0].ToString(),
                        };
                    }
                    else if (quote.Equals(varString[0]))
                    {
                        // found the other side of the quoted material
                        sRow += varString[0].ToString();
                        quote = string.Empty;
                    }
                    else if (string.IsNullOrEmpty(varString) || varString[0].Equals(','))
                    {
                        // Found the comma!
                        if (varString[0].Equals(','))
                        {
                            if (varString.Length > 1)
                                sCol = varString[1..].Trim();
                            else
                                throw new Exception("10|");
                        }
                        break;
                    }
                    else
                        sRow += varString[0];

                    // strip the leading character
                    varString = varString[1..];
                }

                // Solve for sRow & sCol
                if (string.IsNullOrWhiteSpace(sRow) == false)
                {
                    JaxMath.SolveMath(sRow, out JAXObjects.Token st);
                    if (st.AsInt() < 1)
                        throw new Exception("31|");
                    else
                    {
                        sResult += "[" + st.AsString();
                        vref.row = st.AsInt();
                    }
                }

                if (string.IsNullOrWhiteSpace(sCol) == false)
                {
                    JaxMath.SolveMath(sCol, out JAXObjects.Token st);  // skip the comma
                    if (st.AsInt() < 0)
                        throw new Exception("31|");
                    else
                    {
                        vref.col = st.AsInt();
                        sResult += "," + st.AsString();
                    }
                }

                sResult += "]";
            }
            else
            {
                // Just a simple variable
                sResult = varString;
                vref.varName = varString;
            }

            return sResult;
        }



        public string SetVarFromExpression(string expr, object? obj, bool createVar)
        {
            string result = string.Empty;
            VarRef var = new();
            JAXObjects.Token currentVar = new();

            // Macro expansion
            if (expr.Contains('&'))
                expr = JAXMacroHandler.Expand(this, expr);

            if (JAXLib.InListC(expr, ".null.", "null"))
                throw new Exception("10||.NULL.");

            // Get the var parts
            List<string> objList = BreakVar(expr);

            if (objList.Count > 0)
            {
                int withStack = AppLevels[^1].WithStack.Count;

                // Get the current with stack expression
                if (string.IsNullOrWhiteSpace(objList[0]))
                {
                    if (withStack > 0)
                    {
                        // This may be a layered with so we'll be inserting
                        // into objList[0] as long as the we keep finding
                        // variables starting with a period.
                        while (withStack > 0)
                        {
                            // if objList[0] is not empty then the previous with
                            // stack item started with a period, so that means we
                            // have to add another to the front of the list
                            if (string.IsNullOrWhiteSpace(objList[0]) == false)
                                objList.Insert(0, string.Empty);

                            // Get the stacked item and break it up if it's
                            // a multipart variable.  Then add all parts to 
                            // the front of the objList
                            string wsitem = AppLevels[^1].WithStack[withStack - 1];
                            string[] wsitems = wsitem.Split('.');

                            // Add last to first to keep it in the right order
                            for (int i = wsitems.Length; i > 0; i--)
                            {
                                if (string.IsNullOrWhiteSpace(objList[0]))
                                    objList[0] = wsitems[i - 1];            // Always blank on first iteration
                                else
                                    objList.Insert(0, wsitems[i - 1]);
                            }

                            withStack--;

                            if (objList[0][0] != '.')
                                break;

                            if (withStack < 0)
                                throw new Exception($"2300||Top of with stack is {AppLevels[^1].WithStack[0]}");
                        }
                    }
                    else
                        throw new Exception($"2301||There is nothing on the with stack for .{objList[1]}");
                }

                // Now resolve it
                result = SolveVariableReference(objList[0], out var);

                if (objList.Count > 1)
                {
                    currentVar = GetVarFromExpression(objList[0], null);

                    // Is it an object?
                    if (currentVar.Element.Type.Equals("O") == false)
                        throw new Exception("1924|" + var.varName);

                    // Object Ref
                    JAXObjectWrapper? thisObject = (JAXObjectWrapper)currentVar.Element.Value;

                    for (int i = 1; i < objList.Count - 1; i++)
                    {

                        // Is the next list item an array or method call?
                        if (objList[i].Contains('(') || objList[i].Contains('['))
                        {
                            // We expect to be going after an array/object
                            currentVar = GetVarFromExpression(objList[i], (JAXObjectWrapper)currentVar.Element.Value);

                            // TODO - Take a bite later - UDF or Array?
                        }
                        else
                        {
                            // Get the next item in the list - must be an object
                            string member = thisObject.IsMember(objList[i]);
                            if (member.Equals("O") == false)
                                throw new Exception("1924|" + var.varName);

                            if (objList[i].Contains("grd", StringComparison.OrdinalIgnoreCase))
                            {
                                int ii = 0;
                            }
                            int f = thisObject.FindObjectByName(objList[i]);
                            if (f < 0)
                                throw new Exception("9999|Faild to find a known object");

                            thisObject.GetProperty("objects", f, out currentVar);
                        }

                        // It must be an object
                        if (currentVar.Element.Type.Equals("O") == false)
                            throw new Exception("1924|" + var.varName);

                        // Store the object
                        thisObject = (JAXObjectWrapper)currentVar.Element.Value;
                    }

                    // Create a token to for the sent value
                    JAXObjects.Token val = new();

                    if (obj is null)
                        val.Element.MakeNull();
                    else
                        val.Element.Value = obj!;

                    // Attempt to assign it to the property
                    string memberType = thisObject.IsMember(objList[^1]).ToUpper();
                    switch (memberType)
                    {
                        case "U":   // Unknown
                            throw new Exception("1924|" + objList[^1]); // not found    

                        case "E":   // Event
                        case "M":   // Method
                        case "O":   // Object
                            throw new Exception("1737|"); // not a property

                        case "P":   // Property
                            thisObject.SetProperty(objList[^1], obj!);  // What about arrays and nulls???
                                                                        //thisObject.SetProperty(objList[^1], val);  // What about arrays???
                            break;

                        default:
                            throw new Exception($"1559|{objList[^1].ToUpper()}");
                    }
                }
                else
                {
                    // It's a var or array reference
                    if (var.col < 1)
                    {
                        var.col = var.row;
                        var.row = 1;
                    }

                    var.col = var.col < 1 ? 1 : var.col;
                    var.row = var.row < 1 ? 1 : var.row;

                    // Is it a non-array variable?  First, make sure it exists
                    // when the createVar flag is set to true.
                    if (createVar && var.row == 1 && var.col == 1)
                        SetVarOrMakePrivate(var.varName, var.row, var.col, false);

                    // Attempt to set the object element and if
                    // something goes wrong, an error is raised.
                    DebugLog($"Set {var.varName} to '{obj}'");
                    SetVar(var.varName, obj, var.row, var.col);
                }
            }

            return result;
        }


        /*
         * Get the var token from an expression from a simple "i" to 
         * something as complex as Form1.object[3].value
         * 
         * If the var/property does not exist, then a JAXBase error 
         * is raised.
         * 
         * Objects cause this routine to call itself recursively 
         * until the desired property is located.
         * 
         * Examples of valid variable expressions:
         * 
         *  i
         *  ii[1,3+a]
         *  form1.object[3].aInfo[b,3+val(strInfo)]
         * 
         * If in format a.b, the checks a to see if it's an alias
         * and if it is, checks b to see if it's a field.
         * If it's an alias field, returns the value.
         * If it's not an alias field, checks to see if a is an
         * object variable,  and tosses an error if it's not.
         * 
         */
        public Token GetVarFromExpression(string expr, JAXObjectWrapper? parent)
        {
            Token? result = new();
            Token answer = new();
            VarRef var;

            if (expr.Contains("gaPropCount[gnPropCount,2]", StringComparison.OrdinalIgnoreCase))
            {
                int iii = 0;
            }

            string thisVar = string.Empty;

            try
            {
                // Get the top of the list
                string varRemains = string.Empty;
                bool NotATableRef = true;
                int wa = 0;

                // Macro expansion
                if (expr.Contains('&'))
                    expr = JAXMacroHandler.Expand(this, expr);

                if (JAXLib.InListC(expr, ".null.", "null"))
                    throw new Exception("10||.NULL.");

                if (expr.Contains('.'))
                {
                    // Expecting object.property or alias.field
                    List<string> objList = BreakVar(expr);
                    int WithCount = AppLevels[^1].WithStack.Count - 1;


                    // Is this an alias.field?
                    if (objList.Count == 2)
                    {
                        // Is this referencing a work area?
                        if (CurrentDS.IsWorkArea(objList[0]))
                        {
                            // Does the field exist in this work area?
                            wa = CurrentDS.GetWorkArea(objList[0]);
                            NotATableRef = CurrentDS.FieldExists(objList[1], wa) == false;

                            if (NotATableRef)
                            {
                                // Not a field, is it a var?
                                answer = GetVarToken(objList[0]);
                                if (answer.TType.Equals("U") || answer.Element.Type.Equals("O") == false)
                                {
                                    // Not an object variable, so toss a a field error
                                    throw new Exception("4012|");
                                }
                            }
                        }
                    }

                    if (NotATableRef)
                    {
                        // Not an alias - so step throug it after grabbing
                        // the last one to pass on once we resolve this list
                        thisVar = objList[^1];

                        // Process all but the last object of the list
                        for (int i = 0; i < objList.Count - 1; i++)
                        {
                            if (objList[i].Length == 0)
                            {
                                if (i != 0) throw new Exception("REALLY?");
                                if (WithCount < 0) throw new Exception("NO WITH!");

                                // Grab the most recent WITH
                                string[] withVar = AppLevels[^1].WithStack[WithCount].Split('.');
                                for (int j = withVar.Length - 1; j >= 0; j--)
                                {
                                    if (string.IsNullOrWhiteSpace(objList[0]))
                                        objList[0] = withVar[j];
                                    else
                                        objList.Insert(0, withVar[j]);
                                }

                                WithCount--;
                            }

                            if (i == 0)
                            {
                                // Get the parent - TODO Arrays? UDF?
                                JAXObjects.Token ptk = GetVarToken(objList[i].TrimStart('.'));
                                if (ptk.Element.Type.Equals("O"))
                                    parent = (JAXObjectWrapper)ptk.Element.Value;
                                else
                                    throw new Exception($"1924|{objList[i]}");
                            }
                            else
                            {
                                if (i < objList.Count - 1)
                                {
                                    if (parent is not null)
                                    {
                                        // Get the object
                                        SolveVariableReference(objList[i].TrimStart('.'), out var);
                                        string memb = parent.IsMember(var.varName);

                                        if (memb.Equals("O"))
                                        {
                                            int f = parent.FindObjectByName(var.varName);
                                            if (parent.GetObject(f, out JAXObjectWrapper? jow) >= 0)
                                                parent = jow!;
                                        }
                                        else
                                            throw new Exception("NOT AN OBJECT");
                                    }
                                }
                                else
                                    thisVar = objList[i].TrimStart('.');
                            }
                        }
                    }
                    else
                    {
                        // It's a field in a work area, so get that value
                        // and mark it with it's Alias.Field 
                        result.Element.Value = CurrentDS.CurrentWA.DbfInfo.CurrentRow.Rows[0][objList[1]];
                        result.Alias = CurrentDS.WorkAreas[wa].DbfInfo.Alias + "." + objList[0];
                    }
                }
                else
                {
                    thisVar = expr.TrimStart(literalStart).TrimEnd(literalEnd);
                }

                if (NotATableRef)
                {
                    // Try to get the variable reference
                    SolveVariableReference(thisVar, out var);

                    if (parent is null)
                    {
                        // ARRAY
                        answer = GetVarToken(var.varName);

                        if (var.row > 0)
                        {
                            // Get an array element
                            if (var.col < 1)
                            {
                                var.col = var.row;
                                var.row = 1;
                            }

                            // Return the element
                            result.Element.Value = answer._avalue[(var.row - 1) * var.col + var.col - 1];
                        }
                        else
                            result = answer;    // Return the entire array
                    }
                    else
                    {
                        // TODO - Array? UDF?
                        string memb = parent.IsMember(var.varName);

                        if (memb.Equals("O"))
                        {
                            int f = parent.FindObjectByName(var.varName);
                            if (parent.GetObject(f, out JAXObjectWrapper? jow) >= 0)
                                result.Element.Value = jow!;
                        }
                        else if (memb.Equals("M"))
                        {
                            parent.MethodCall(var.varName);
                            result.Element.Value = ReturnValue.Element.Value;
                        }
                        else
                        {
                            parent.GetProperty(var.varName, 0, out result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetError(9999, "System error retrieving " + expr, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                SetError(9999, "System Error Message: " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                result.TType = "X";
            }

            return result!;
        }



        /*-------------------------------------------------------------*
         * Set an object value
         *-------------------------------------------------------------*/
        public void SetVar(string varName, object? obj, int r, int c)
        {
            Token tk;

            varName = varName.Trim().ToLower();

            // Is it a memory Var reference?  Strip m. if it is
            if (varName.Length > 2 && varName[..2].Equals("M.", StringComparison.OrdinalIgnoreCase))
                varName = varName[2..];

            if (JAXLib.InList(varName, "this", "thisform", "thisformset"))
            {
                // Can only set if it's not already in existance
                // or it's a bool equal to false
                tk = AppLevels[^1].LocalVars.GetToken(varName);
                if (tk.TType.Equals("U") == false && (tk.Element.Type.Equals("L") == false || tk.AsBool()))
                    throw new Exception("2405|" + varName.ToUpper());

                if (obj is not null)
                    DebugLog($"Set {varName} to {((JAXObjectWrapper)obj).Name} in AppLevel {AppLevels.Count - 1}", false);
            }

            // Check local variables
            tk = AppLevels[^1].LocalVars.GetToken(varName);

            // Check private variables
            if (tk.TType.Equals("U"))
            {
                // Check all the private vars
                for (int i = AppLevels.Count - 1; i >= 0; i--)
                {
                    tk = AppLevels[i].PrivateVars.GetToken(varName);
                    if (tk.TType.Equals("U") == false)
                    {
                        DebugLog($"Set private var {varName} in AppLevel {i} to {obj ?? ".NULL."}");
                        AppLevels[i].PrivateVars.SetValue(varName, obj, r, c);
                        break;   // Found it!
                    }
                }
            }
            else
            {
                // Set the local variable
                DebugLog($"Set local var {varName} in AppLevel {AppLevels.Count - 1} to {obj ?? ".NULL."}");
                AppLevels[^1].LocalVars.SetValue(varName, obj, r, c);
            }
        }

        /*
         * This class is used for passing parameters by reference or value
         * such as DO prg WITH var.  Typically, only certain routines
         * will send the ByRef flags P/L
         * 
         * Type:    P - PrivateVars (Public if Level=0) - ByRef
         *          L - LocalVars - ByRef
         *          M - Math string (such as "(4+3)*7")
         *          R - RPN Expression - By Value
         *          V - Var Expression to search - By Value
         *          T - Token value
         *          
         * Level:   0 = These PrivateVars are the Public vars
         *         >0 = P = PrivateVars, L=LocalVars
         *          Ignore if Type R or V
         *          
         */
        public JAXObjects.Token GetParameterToken(ParameterClass? p)
        {
            JAXObjects.Token result = new();

            if (p is null)
            {
                if (ParameterClassList.Count > 0)
                {
                    p = ParameterClassList[0];
                    ParameterClassList.RemoveAt(0);
                }
            }


            if (p is null)
            {
                // Return a null
                result.Element.MakeNull();
            }
            else
            {
                switch (p.Type)
                {
                    case "P":   // Private variable
                        result = AppLevels[p.Level].PrivateVars.jaxObject[p.RefVal.ToLower()];
                        break;

                    case "L":   // Local variable
                        result = AppLevels[p.Level].LocalVars.jaxObject[p.RefVal.ToLower()];
                        break;

                    case "T":   // Token value
                        result = p.token;
                        break;

                    case "R":   // RPN
                        result = SolveFromRPNString(p.RefVal);
                        break;

                    case "M":   // Math String
                        JaxMath.SolveMath(p.RefVal, out result);
                        break;

                    default:
                        throw new Exception($"1999||GetParametervalue Type ={p.Type}");
                }
            }

            return result;
        }

        /*-------------------------------------------------------------*
         * Just return the value of a parameter
         * 
         * An array will return just the first element
         *-------------------------------------------------------------*/
        public object? GetParameterValue(ParameterClass p)
        {
            JAXObjects.Token result = new();
            JAXObjects.Token answer = GetParameterToken(p);
            if (answer.TType.Equals("A"))
                result.Element.Value = answer._avalue[0].Value;
            else
                result.CopyFrom(answer);

            return result.Element.Value;
        }


        public bool CreateDebugLog = true;
        public bool ClearActiveWindow = false;


        /*-------------------------------------------------------------*
         * ERROR RECORDING/REPORTING
         *-------------------------------------------------------------*/
        private readonly List<JAXErrors> Errors = [];
        public readonly string Name = string.Empty;

        /* ------------------------------------------------------------*
         * Records all errors and reports errors in an xBase manner.  
         * Current error handler will be called from here.
         * 
         * ErrMessage Parameter handling
         *      If just a message, record to system log
         *      If pipe delimited, expected format is:
         *          JAXErr|MsgParameter|System message
         *          JaxErr will expand and use the MsgParameter
         *          and report to System log along with the 
         *          System message if present
         *          
         * TODO - tie in error handling
         * ------------------------------------------------------------*/
        public void SetError(int ErrNo, string ErrMessage, string ErrProcedure)
        {
            if (ErrNo == 9999)
            {
                int ii = 0;
            }

            int jaxErr = ErrNo;
            string jaxErrMsg = ErrMessage;

            if (ErrMessage.Contains('|'))
            {
                string[] msg = ErrMessage.Split("|");
                if (int.TryParse(msg[0], out int err)) jaxErr = err;

                jaxErrMsg = JAXErrorList.JAXErrMsg(jaxErr, msg.Length > 1 ? msg[1] : string.Empty);
            }

            JAXErrors e = new()
            {
                ErrorNo = jaxErr,
                ErrorMessage = jaxErrMsg,
                ErrorProcedure = AppLevels.Count > 0 ? AppLevels[^1].PrgName + (AppLevels[^1].Procedure.Length > 0 ? "." + AppLevels[^1].Procedure : string.Empty) : ErrProcedure,
                ErrorSource = AppLevels.Count > 0 && AppLevels[^1].CurrentLine > 0 ? AppLevels[^1].CurrentLineOfCode : string.Empty,
                ErrorLine = AppLevels.Count > 0 ? AppLevels[^1].CurrentLine : 0
            };

            string errTextMsg = (AppLevels.Count > 0 && AppLevels[^1].CurrentLine > 0) ?
                string.Format("     Error {0} @ Line {1} in {2} - {3}", e.ErrorNo, e.ErrorLine, e.ErrorProcedure, e.ErrorMessage) :
                string.Format("     Error {0} in {1} - {2}", e.ErrorNo, e.ErrorProcedure, e.ErrorMessage);

            if (CurrentDS.JaxSettings.Alternate && string.IsNullOrWhiteSpace(CurrentDS.JaxSettings.Alternate_Name) == false)
                JAXLib.StrToFile(errTextMsg, CurrentDS.JaxSettings.Alternate_Name, 1);

            DebugLog($"{errTextMsg} - {ErrMessage}");

            Errors.Add(e);

            if (jaxErr > 0)
            {

                if (AppLevels.Count > 0 && AppLevels[^1].CurrentLine > 0)
                {
                    jaxErrMsg += string.Format("\rProgram: {0}\rProcedure:{1}\rLine: {2}", AppLevels[^1].PrgName, ErrProcedure, AppLevels[^1].CurrentLine);

                    if (CodeCache.Count > AppLevels[^1].PRGCacheIdx)
                    {
                        string sFile = CodeCache[AppLevels[^1].PRGCacheIdx].SourceFile;
                        if (File.Exists(sFile))
                        {
                            // Source is available, so get the line of code in error
                            // TODO - check to see we need to concatinate lines to
                            // display the entire line of source code
                            string pCode = JAXLib.FileToStr(sFile);
                            pCode = pCode.Replace("\n", "");
                            string[] pcd = pCode.Split("\r");

                            if (AppLevels[^1].CurrentLine - 1 < pcd.Length)
                                AppLevels[^1].CurrentLineOfCode = pcd[AppLevels[^1].CurrentLine - 1];
                        }
                    }
                    else
                    {
                        DebugLog(string.Format("CodeCache.Count={0}, AppLevels[{1}].PRGCacheIdx={2} in {3}", CodeCache.Count, AppLevels.Count - 1, AppLevels[^1].PRGCacheIdx, System.Reflection.MethodBase.GetCurrentMethod()!.Name));
                    }

                    jaxErrMsg += string.Format("\rCommand: {0}", AppLevels[^1].CurrentLineOfCode);
                }

                // Command box based error
                //if (InCompile == false)
                //    MessageBox.Show(jaxErrMsg, string.Format("Error {0}", jaxErr), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ClearErrors() { Errors.Clear(); }
        public int ErrorCount() { return Errors.Count; }
        public int LastErrorNo() { return Errors.Count > 0 ? Errors[^1].ErrorNo : 0; }
        public JAXErrors GetLastError() { return Errors[^1]; }

        public void SetAsType(string varName, string rpn)
        {
            JAXObjects.Token answer = GetVarToken(varName);
            string asType = SolveFromRPNString(rpn).AsString().ToUpper().Trim();

            if (asType.Length > 0)
            {
                asType = asType switch
                {
                    "B" => "N",
                    "C" => "C",
                    "D" => "D",
                    "F" => "N",
                    "I" => "N",
                    "L" => "L",
                    "N" => "N",
                    "O" => "O",
                    "T" => "T",
                    "V" => "C",
                    "Y" => "N",
                    "INT" => "I",
                    "INTEGER" => "I",
                    "NUM" => "N",
                    "NUMERIC" => "N",
                    "NUMBER" => "N",
                    "DATE" => "D",
                    "DT" => "D",
                    "DATETIME" => "T",
                    "DTTM" => "T",
                    "LOGICAL" => "L",
                    "BOOL" => "L",
                    "BOOLEAN" => "L",
                    "LOG" => "L",
                    "CHAR" => "C",
                    "CHARACTER" => "C",
                    "STRING" => "C",
                    "STR" => "C",
                    "OBJ" => "O",
                    "OBJECT" => "O",
                    "DOUBLE" => "N",
                    "DOUB" => "N",
                    "FLOAT" => "N",
                    "VARCHAR" => "C",
                    "VARC" => "C",
                    "CURRENCY" => "N",
                    "CURR" => "N",
                    _ => throw new Exception("11|")
                };
            }
            if (asType.Length == 1 && "NDITLCO".Contains(asType))
                answer.SetAsType(asType);
        }


        /*-------------------------------------------------------------*
         * Break an array or UDF into a list of parts
         *-------------------------------------------------------------*/
        public List<string> BreakArrayOrUDF(string varName)
        {
            List<string> objList = [];

            int cpos = 0;

            if (")]".Contains(varName[^1]) == false) throw new Exception("10||End of varname is not ) or ]");

            char startQuote = varName[^1] == ')' ? '(' : '[';
            varName = varName[..^1];

            // Break the var name from the array or UDF
            while (varName.Length > 0 && cpos < varName.Length)
            {
                if (varName[cpos] == startQuote)
                {
                    // Found the left ( or [
                    objList.Add(varName[..cpos]);

                    // Is there anything left to parse?
                    if (cpos < varName.Length - 1)
                    {
                        cpos++;
                        varName = varName[cpos..];
                    }
                    else
                        varName = string.Empty;

                    break;  // we've got the name part
                }
                else
                {
                    if ("([".Contains(varName[cpos]))
                        throw new Exception("10||Mismatched start and end bracket/parens");
                    else
                        cpos++;
                }
            }

            // If it's a UDF, we're going to want to break up the rest of this
            cpos = 0;
            char endQuote = '\0';

            while (varName.Length > 0 && cpos < varName.Length)
            {
                char c = varName[cpos];

                if (endQuote == '\0')
                {
                    if ("(['\"".Contains(c))
                    {
                        if (c == '(')
                            endQuote = ')';
                        else if (c == '[')
                            endQuote = ']';
                        else
                            endQuote = c;
                    }
                    else if (c == ',')
                    {
                        // Found a comma for split
                        objList.Add(varName[..cpos].Trim(','));
                        if (cpos >= varName.Length) throw new Exception("10|");
                        varName = varName[cpos..].Trim(',');
                        cpos = 0;

                        if (varName.Contains(',') == false)
                        {
                            // Should be something left after the comma
                            if (string.IsNullOrWhiteSpace(varName)) throw new Exception("10||Expression missing after commad");

                            // Can't find a comma so add it to the list and we're done!
                            objList.Add(varName);
                            varName = string.Empty;
                            break;
                        }

                        // Skip the cpos++ so we're
                        // starting at pos 0
                        continue;
                    }
                }
                else if (endQuote == c)
                {
                    // Found the end quote
                    endQuote = '\0';
                }

                cpos++;
            }

            // Add anything left over into the list
            if (string.IsNullOrWhiteSpace(varName) == false)
                objList.Add(varName);

            return objList;
        }


        /*
         * Break a var into 
        */
        public List<string> BreakVar(string varName)
        {
            List<string> objList = [];

            try
            {
                int cpos = 0;
                char endQuote = '\0';

                if (varName.Contains('.'))
                {
                    while (varName.Length > 0 && cpos < varName.Length)
                    {
                        char c = varName[cpos++];

                        if (endQuote == '\0')
                        {
                            if ("(['\"".Contains(c))
                            {
                                if (c == '(')
                                    endQuote = ')';
                                else if (c == '[')
                                    endQuote = ']';
                                else
                                    endQuote = c;
                            }
                            else if (c == '.')
                            {
                                // Found a period to split
                                objList.Add(varName[..cpos].TrimEnd('.'));
                                if (cpos >= varName.Length) throw new Exception("10|");
                                varName = varName[cpos..];
                                cpos = 0;

                                if (IsCompoundVar(varName) == false)
                                {
                                    // Can't find a period, so we're done!
                                    objList.Add(varName);
                                    varName = string.Empty;
                                    break;
                                }
                            }
                        }
                        else if (endQuote == c)
                        {
                            // Found the end quote
                            endQuote = '\0';
                        }
                    }

                    // Shouldn't ever end up here with varName still holding a value
                    if (string.IsNullOrWhiteSpace(varName) == false) throw new Exception("10|");
                }
                else
                {
                    // Not an object name
                    objList.Add(varName.Trim());
                }
            }
            catch (Exception ex)
            {
                SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
                objList = [];
            }

            return objList;
        }


        public bool IsCompoundVar(string varName)
        {
            bool result = false;
            int cpos = 0;
            char endQuote = '\0';

            while (varName.Length > 0 && cpos < varName.Length)
            {
                char c = varName[cpos++];

                if (endQuote == '\0')
                {
                    if ("(['\"".Contains(c))
                    {
                        if (c == '(')
                            endQuote = ')';
                        else if (c == '[')
                            endQuote = ']';
                        else
                            endQuote = c;
                    }
                    else if (c == '.')
                    {
                        // Found a period outside of a quoted area
                        // so this is a compound variable (object.property)
                        result = true;
                        break;
                    }
                }
                else if (endQuote == c)
                {
                    // Found the end quote
                    endQuote = '\0';
                }
            }

            return result;
        }


        /*-------------------------------------------------------------------------------------------*
         * 
         * OBJECT CALL
         * 
         * Call an object.method or return a object.property
         * 
         * It can be something simple like FORM1.SHOW, FORM1.REFRESH(), 
         * FORM1.Caption, or something complex like 
         * FORM1.PGFRAME1.PAGE1.CONTAINER1.OBJECT[4].Value
         * 
         * The eCodes.Expressions[0] holds the entire string for the
         * call and is broken down by element.
         * 
         * Parameter ExpectingValue = false means it has to be an
         * event or method call, which may still return a value.
         * 
         * Out parameter objResult will hold the property/object
         * returned at the end.
         * 
         *-------------------------------------------------------------------------------------------*/
        public string ObjectCall(ExecuterCodes eCodes, bool expectingValue, out JAXObjects.Token objResult)
        {
            string result = "U";
            objResult = new();
            //DebugLog($"ObjectCall in App Level {AppLevels.Count - 1}", false);

            if (eCodes.Expressions.Count > 0 && eCodes.Expressions[0].RNPExpr.Contains("gopropbox.gr", StringComparison.OrdinalIgnoreCase))
            {
                int iii = 0;
            }

            try
            {
                // Make sure thier is only one expression
                if (eCodes.Expressions.Count != 1) throw new Exception("10||Must have only one expression");

                // Resolve the expression
                JAXObjects.Token answer = SolveFromRPNString(eCodes.Expressions[0].RNPExpr);

                // The expression must be a character string
                if (answer.Element.Type.Equals("C") == false) throw new Exception("11|");

                string expr = answer.AsString().Trim();

                // Macro expansion
                if (expr.Contains('&'))
                    expr = JAXMacroHandler.Expand(this, expr);

                if (JAXLib.InListC(expr, ".null.", "null"))
                    throw new Exception("10||.NULL.");

                // Set up the object list
                List<string> objList = BreakVar(expr);

                // If anything was sent, process it
                if (objList.Count > 0)
                {
                    int withStack = AppLevels[^1].WithStack.Count;

                    // Get the current with stack expression
                    if (string.IsNullOrWhiteSpace(objList[0]))
                    {
                        if (withStack > 0)
                        {
                            // This may be a layered with so we'll be inserting
                            // into objList[0] as long as the we keep finding
                            // variables starting with a period.
                            while (withStack > 0)
                            {
                                if (string.IsNullOrWhiteSpace(objList[0]) == false)
                                    objList.Insert(0, string.Empty);

                                objList[0] = AppLevels[^1].WithStack[withStack - 1];
                                withStack--;

                                if (objList[0][0] != '.')
                                    break;

                                if (withStack < 0)
                                    throw new Exception($"2300||Top of with stack is {AppLevels[^1].WithStack[0]}");
                            }
                        }
                        else
                            throw new Exception($"2301||There is nothing on the with stack for .{objList[1]}");
                    }

                    // We have the object broken down by list so let's start processing it
                    // by getting the base object.
                    JAXObjects.Token currentVar = GetVarFromExpression(objList[0], null);

                    // If the current var is not an object, then check to see if it's a UDF
                    // TODO

                    // If we don't come up with an object, toss an error
                    if (currentVar.Element.Type.Equals("O") == false)
                        throw new Exception("11|");

                    if (objList.Count == 1)
                    {
                        // There is only one item in the list
                        if (expectingValue)
                        {
                            // If we're expecting a value, send back the object
                            result = "O";
                            objResult.Element.Value = currentVar.Element.Value;
                        }
                        else
                            throw new Exception("10||Expecting a method/event call for " + objList[0]);
                    }
                    else
                    {
                        // We have 2 or more list items
                        for (int i = 1; i < objList.Count; i++)
                        {
                            // Save the current object
                            JAXObjectWrapper thisObject = (JAXObjectWrapper)currentVar.Element.Value;

                            if (objList[i].Contains('(') || objList[i].Contains('['))
                            {
                                // TODO - It's an array or UDF
                                List<string> varInfo = BreakArrayOrUDF(objList[i]);
                                string memberType = thisObject.IsMember(varInfo[0]).ToUpper();

                                if ("ME".Contains(memberType) == false) throw new Exception("1738|" + varInfo[0]);

                                // TODO - what about objects?

                                ParameterClassList.Clear();
                                List<ParameterClass> cParams = [];

                                // set up the parameter list
                                for (int j = 1; j < varInfo.Count; j++)
                                {
                                    // solve the math string for this parameter
                                    JaxMath.SolveMath(varInfo[j], out answer);
                                    ParameterClass c = new ParameterClass();
                                    c.token.CopyFrom(answer);
                                    c.Type = "T";
                                    ParameterClassList.Add(c);
                                }

                                // Call the method/event
                                if (thisObject.MethodCall(varInfo[0]) < 0)
                                    throw new Exception($"{thisObject.GetErrorNo()}|");
                            }
                            else
                            {
                                // If we're not on the last element of the list, we have a problem
                                //if (i + 1 != objList.Count) throw new Exception("1575|" + objList[i]);

                                // Is it part of the current object?
                                string memberType = thisObject.IsMember(objList[i]).ToUpper();

                                // Use the return code to decide what to do
                                switch (memberType)
                                {
                                    case "E":   // Event
                                    case "M":   // Method
                                                // TODO - break out any parameters

                                        // Call the method
                                        thisObject.MethodCall(objList[i]);

                                        // Return what was sent back as a token
                                        objResult.Element.Value = ReturnValue.Element.Value;
                                        result = memberType;
                                        break;

                                    case "O":   // We're looking for an OBJECT[]
                                                // If we're not looking for a value, we have a problem
                                                //if (i+1>=objList.Count && expectingValue == false)
                                                //    throw new Exception("1738|" + objList[i].ToUpper());

                                        // TODO - break down the object var call

                                        // Get the object index by name since we know it exists
                                        if (thisObject.GetObject(objList[i], out JAXObjectWrapper? jow) >= 0)
                                            objResult.Element.Value = jow!;
                                        else
                                            throw new Exception("9999|");

                                        result = memberType;
                                        currentVar = new();
                                        currentVar = objResult;
                                        break;

                                    case "P":   // Property - array properties are handled above
                                                // If we're not expecting a value, then we have a
                                                // problem being here and will toss an exception.
                                        if (i + 1 >= objList.Count && expectingValue == false)
                                            throw new Exception("1738|" + objList[i].ToUpper());

                                        // Get the property token and return it
                                        thisObject.GetProperty(objList[i], 0, out objResult);
                                        result = memberType;
                                        break;

                                    default:
                                        throw new Exception("1999|Object member type " + memberType);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Sure don't want to be here, but if we are, get it logged
                SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            // Return the token
            return result;
        }


        /*-------------------------------------------------------------------------------------------*
         * PURPOSE:
         *      This routine is used to grab an expression from the command string and return the 
         *      remaining command string along with the expression value as a token.  This command 
         *      is expected to be used in cases where a literal is expected but may be replaced by 
         *      an expression in parenthisis.
         * 
         *      Source examples:
         * 
         *          USE (tablename)
         *      
         *          AVERAGE (exprString) ALL TO ARRAY (arrayName)
         *      
         *      This allows us to extend the XBase language by putting in (experession) instead of
         *      having to perform marco substituion all the time, which will be faster since we you
         *      need to compile macro supstitution results during execution.
         * 
         * 
         * 
         * PROCESS DESCRIPTION:
         *      Get the next expression value from the command and send out the
         *      value found as an object token and return the rest of the string
         * 
         *      Literals are in the form of:
         *          <literalStart>literalstring<literalEnd>
         *      
         *      Expressions are in the form:
         *          <expByte>expstring1<expParam>exprstring2<exprParam>exprstring3...<expEnd>
         * 
         *      Grab the string between the start and end then process accordingly.  A literal
         *      is passed back as a string, while an expression is broken into a list by <expParam> 
         *      byte and returned, typically, as a string.
         * 
         *-------------------------------------------------------------------------------------------*/
        public JAXObjects.Token SolveFromRPNString(string Command)
        {
            JAXObjects.Token answer = new();
            List<string> rpnList = [];
            string cmdRest = string.Empty;

            if (Command.Contains("CPEMInfo.dbf"))
            {
                int iii = 0;
            }

            try
            {
                if (Command[0] == AppClass.literalStart)
                {
                    // Process a literal, returning as a string
                    if (Command[^1] != AppClass.literalEnd)
                        throw new Exception("10|SyntaxError|Mismatched literal expression");

                    answer.Element.Value = Command.TrimStart(AppClass.literalStart).TrimEnd(AppClass.literalEnd);
                }
                else if (Command[0] == AppClass.expByte)
                {
                    // Process an expression
                    if (Command[^1] != AppClass.expEnd)
                        throw new Exception("10||Invalid expression string");

                    // Break out the expressions to a list
                    rpnList = Command.TrimStart(AppClass.expByte).TrimEnd(AppClass.expEnd).Split(AppClass.expParam).ToList();

                    if (rpnList.Count == 3 && rpnList[0][0] == '_')
                    {
                        int iii = 0;
                    }
                    // Process the RPNList
                    answer = JaxMath.MathSolve(rpnList);
                }
                else
                    throw new Exception(string.Format("10||Unknown command byte {0}", Command[0]));
            }
            catch (Exception ex)
            {
                SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            // If there is more to the expression, then there better be
            // an expression delimiter in the next byte
            if (cmdRest.Length > 0)
            {
                if (cmdRest[0] == AppClass.expDelimiter)
                    cmdRest = cmdRest[1..];
                else
                    throw new Exception(string.Format("10||Unexpected byte '{0}'", cmdRest[0]));
            }

            // Return the token
            return answer;
        }

    }
}
