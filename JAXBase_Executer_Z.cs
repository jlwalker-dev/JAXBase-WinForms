namespace JAXBase
{
    public class JAXBase_Executer_Z
    {
        /*
         * 
         * ZAP IN nWorkArea | cAlias
         * 
         */
        public static string Zap(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;
            int wa = jbe.App.CurrentDS.CurrentWorkArea();

            try
            {
                // Go to the desired workarea
                JAXObjects.Token workarea = new();
                workarea.Element.Value = string.IsNullOrWhiteSpace(eCodes.InExpr) ? wa : jbe.App.SolveFromRPNString(eCodes.InExpr);
                if (workarea.Element.Type.Equals("N"))
                    jbe.App.CurrentDS.SelectWorkArea(workarea.AsInt());
                else if (workarea.Element.Type.Equals("C"))
                    jbe.App.CurrentDS.SelectWorkArea(workarea.Element.ValueAsString);
                else
                    throw new Exception("11|");

                jbe.App.CurrentDS.CurrentWA.DBFZap();
            }
            catch (Exception ex)
            {
                jbe.App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }
            finally
            {
                jbe.App.CurrentDS.SelectWorkArea(wa);
            }

            return result;
        }

    }
}
