using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace JAXBase
{
    public class JAXBase_Executer_O
    {
        public static JAXObjectWrapper ObjectResolve(AppClass app, string varObject)
        {
            string[] objParts = varObject.Split('.');

            // This is the base object
            //app.GetVar(objParts[0], out JAXObjects.Token obj);
            JAXObjects.Token obj = app.GetVarFromExpression(objParts[0], null);
            JAXObjectWrapper? thisObject = (JAXObjectWrapper)obj.Element.Value;

            if (thisObject is not null)
            {
                // The following are all nested objects
                for (int i = 1; i < objParts.Length - 2; i++)
                {
                    int j = thisObject.FindObjectByName(objParts[i]);
                    thisObject.GetObject(j, out thisObject);

                    if (thisObject is null) throw new Exception("1901|");
                }
            }
            else
                throw new Exception("1901|");

            return thisObject;
        }

        /* TODO NOW
         * 
         * ON KEY LABEL
         * 
         */
        public static string On(AppClass app, string CmdString)
        {
            return string.Empty;
        }


        /* TODO 
         * 
         * OPEN DATABASE
         * 
         */
        public static string Open(AppClass app, string CmdString)
        {
            return string.Empty;
        }


        /* 
         * 
         * OTHERWISE
         * If we stumble onto this, we will look for an endcase because we should only be loading 
         * this command in a DO CASE statement. The DO CASE statement jumps through the related 
         * case statements until if finds a case expression that is true, otherwise, or endcase.
         * 
         * When an expression is true, it starts with the next command record and continues until
         * it finds another case statement, otherwise or an end case.
         * 
         */
        public static string Otherwise(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;

            try
            {
                string cEndCase = AppClass.cmdByte.ToString() + jbe.App.MiscInfo["endcasecmd"] + eCodes.SUBCMD;

                // Find the endcase
                int pos = jbe.App.PRGCache[jbe.App.AppLevels[^1].PRGCacheIdx].IndexOf(cEndCase);

                if (pos < 0)
                    throw new Exception("1211|");   // If/Else/Endif stmt is missing
                else
                {
                    jbe.App.utl.Conv64(pos, 3, out string pos2);
                    result = "Y" + pos2; // Return the position of the endcase
                }
            }
            catch (Exception ex)
            {
                jbe.App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }
    }
}
