using System.Data;
using static JAXBase.AppHelper;

namespace JAXBase
{
    public class VFPForm
    {
        /*
         * Convert a VFP SCX form definition to a JAXBase form definition to the designated path
         * with the same stem but jax extension
         * 
         * return -1 if a conversion failure
         *         0 if sucess
         */
        public static int ConvertToJAX(AppClass app, string fName, string outPath)
        {
            int result = -1;

            if (File.Exists(fName))
            {
                ExtensionTypes Extension = AppHelper.GetCodeFileExtensions("F");
                string FormFile = string.Empty;
                string objName = JAXLib.JustStem(fName);

                // Open up the scx
                if (app.CurrentDS.OpenTable(fName, "thisscx") == 0)
                {
                    // find record where platform="COMMENT" and UniqueID="RESERVED" and load Properties for font information
                    JAXDirectDBF jdbf = app.CurrentDS.CurrentWA;
                }
                else
                    throw new Exception("52|");

                // Create the JAXBase Form definition table structure

                // Create the JaxBase Form definition table

                // Copy from the VFP table to the JAXBase definition table

                // Lose them both up
                app.CurrentDS.CloseDBF("thisscx");
                app.CurrentDS.CloseDBF("JAXForm");
            }
            else
                throw new Exception("1|" + fName);

            return result;
        }

        /* ------------------------------------------------------------------*
         * Create a Class Definition from VFP SCX table using just the
         * platform='WINDOWS' records.
         * 
         * VFP always creates an single form SCX so that there is a header 
         * record followed by the dataenvironment, followed by cursor 
         * definitions, followed by the form definitions, followed by 
         * everything else, and ending with a trailer record.
         * 
         * 
         *  2025-06-22
         *      Formsets are not supported, yet, and I am waiting until
         *      I get into c++ before I consider supporting them.
         *      
         * ------------------------------------------------------------------*/
        /// <summary>
        /// Convert an SCX file to a source code file
        /// </summary>
        /// <param name="app"></param>
        /// <param name="fName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ConvertForm(AppClass app, string fName)
        {
            ExtensionTypes Extension = AppHelper.GetCodeFileExtensions("F");

            string crlf = ((char)13).ToString() + ((char)10).ToString();
            string cr = ((char)13).ToString();

            string FormFile = string.Empty;
            string objName = JAXLib.JustStem(fName);

            // Force it to end with an scx extension - no screwing around with other extensions
            if (JAXLib.JustExt(fName).Equals(Extension.SourceTable, StringComparison.OrdinalIgnoreCase) == false)
                fName = JAXLib.JustFullPath(fName) + JAXLib.JustStem(fName) + "." + Extension.SourceTable;

            // If it's not here, look for it in the path list
            if (File.Exists(fName) == false)
                fName = FindPathForFile(app, fName) + JAXLib.JustFName(fName);

            // Is it available?
            if (File.Exists(fName))
            {
                // Create a table instance

                // Open up the scx
                if (app.CurrentDS.OpenTable(fName, "thisscx") == 0)
                {
                    // find record where platform="COMMENT" and UniqueID="RESERVED" and load Properties for font information
                    JAXDirectDBF jdbf = app.CurrentDS.CurrentWA;

                    // 2025-07-07 - Added ability to autoload memo info when reading record(s)
                    jdbf.DBFSelect("properties", "top 1", "platform='COMMENT' and uniqueid='RESERVED'", true, out DataTable dt);
                    if (dt.Rows.Count == 0) throw new Exception("8000|Missing 'RESERVED' record");
                    string FormFontInfo = GetFieldToken(app, dt.Rows[0], "properties").AsString().Replace(((char)10).ToString(), "");
                    string[] FormFonts = FormFontInfo.Split((char)13);
                    FormFile = JAXLib.JustFullPath(fName) + JAXLib.JustStem(fName) + "." + Extension.SourceCode; // Intermediate prg for form
                    FilerLib.DeleteFile(FormFile);

                    JAXLib.StrToFile("* Form Name: " + fName + crlf, FormFile, 1);
                    JAXLib.StrToFile("* Created  : " + DateTime.Now.ToString() + crlf, FormFile, 1);
                    JAXLib.StrToFile("* JAXBase  : " + AppClass.CurrentMajorVersion.ToString() + "." + AppClass.CurrentMinorVersion.ToString() + crlf, FormFile, 1);
                    JAXLib.StrToFile("* ================================================================" + crlf, FormFile, 1);


                    // ---------------------------------------------------------------------------------------------------------
                    // TODO - DO FORM FormName [NAME VarName [LINKED]] [WITH cParameterList] [TO VarName][NOREAD][NOSHOW]
                    // ---------------------------------------------------------------------------------------------------------
                    JAXLib.StrToFile(string.Format("release {0}", objName) + crlf, FormFile, 1);
                    JAXLib.StrToFile(string.Format("public {0}", objName) + crlf, FormFile, 1);
                    JAXLib.StrToFile(string.Format("{0}=createobject('{1}')", objName, objName) + crlf, FormFile, 1);
                    JAXLib.StrToFile(string.Format("{0}.show()", objName) + crlf, FormFile, 1);
                    JAXLib.StrToFile("return" + crlf, FormFile, 1);
                    JAXLib.StrToFile(crlf, FormFile, 1);
                    // ---------------------------------------------------------------------------------------------------------

                    // Class=form - get class, baseclass, classloc, objname, properties, reserved3, and methods
                    Dictionary<string, string> ParentChild = new();
                    jdbf.DBFSelect("*", "all", "platform='WINDOWS' and not deleted()", true, out dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string parent = GetFieldToken(app, dt.Rows[i], "parent").AsString().Trim();
                        string objname = GetFieldToken(app, dt.Rows[i], "objname").AsString().Trim();

                        if (ParentChild.ContainsKey(parent.ToLower()) == false)
                            ParentChild.Add(parent.ToLower(), objname);
                        else
                            ParentChild[parent.ToLower()] += cr + objName;
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ProcessSCXClassEntry(app, dt.Rows[i], ParentChild, FormFile, FormFonts);
                    }
                }
                else
                    throw new Exception("52|");
            }
            else
                throw new Exception("1|" + fName);

            return FormFile;
        }

    }
}
