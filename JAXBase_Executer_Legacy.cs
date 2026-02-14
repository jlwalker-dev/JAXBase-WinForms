namespace JAXBase
{
    public class JAXBase_Executer_Legacy
    {
        /*
         * ? statement
         */
        public static string QPrint(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            jbe.App.ClearErrors();
            string result = SolveQPrint(jbe, eCodes);

            if (jbe.App.ErrorCount() == 0)
            {
                jbe.App.JAXConsoles[jbe.App.ActiveConsole].WriteLine(string.Empty);
                jbe.App.JAXConsoles[jbe.App.ActiveConsole].Write(result);
                jbe.App.DebugLog("Console: " + result,jbe.App.CurrentDS.JaxSettings.Talk == false);

                if (jbe.App.CurrentDS.JaxSettings.Alternate && string.IsNullOrWhiteSpace(jbe.App.CurrentDS.JaxSettings.Alternate_Name) == false)
                    JAXLib.StrToFile(result, jbe.App.CurrentDS.JaxSettings.Alternate_Name, 1);
            }

            return string.Empty;
        }


        /*
         * ?? statement
         */
        public static string QQPrint(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            jbe.App.ClearErrors();
            string result = SolveQPrint(jbe, eCodes);

            if (jbe.App.ErrorCount() == 0)
            {
                jbe.App.JAXConsoles[jbe.App.ActiveConsole].Write(result);
                jbe.App.DebugLog("Console: "+result,jbe.App.CurrentDS.JaxSettings.Talk == false);

                if (jbe.App.CurrentDS.JaxSettings.Alternate && string.IsNullOrWhiteSpace(jbe.App.CurrentDS.JaxSettings.Alternate_Name) == false)
                    JAXLib.StrToFile(result, jbe.App.CurrentDS.JaxSettings.Alternate_Name, 1);
            }

            return string.Empty;
        }


        /*
         * Resolve the ? and ?? statement body
         */
        public static string SolveQPrint(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;

            foreach (ExCodeRPN rpn in eCodes.Expressions)
            {
                if (string.IsNullOrEmpty(rpn.RNPExpr))
                    continue;

                if (rpn.RNPExpr.Contains("propertyinfo",StringComparison.OrdinalIgnoreCase))
                {
                    int iii = 0;
                }

                JAXObjects.Token answer = jbe.App.SolveFromRPNString(rpn.RNPExpr);
                result += answer.AsString() + " ";
            }

            return result.Length > 0 ? result[..^1] : string.Empty;
        }


        /*
         * Place source code into the current AppLevel
         */
        public static string SourceCode(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            jbe.App.AppLevels[^1].CurrentLineOfCode = eCodes.COMMAND;
            return string.Empty;
        }
    }
}
