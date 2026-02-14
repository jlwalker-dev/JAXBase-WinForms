namespace JAXBase
{
    public class JAXBase_Executer_G
    {

        /* TODO NOW
         * 
         * GATHER
         * 
         */
        public static string Gather(AppClass app, string cmdRest)
        {
            string result = string.Empty;

            try
            {
            }
            catch (Exception ex)
            {
                app.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }


        /* TODO NOW
         * 
         * GETEXPR()
         * 
         */
        public static string GetExpr(AppClass app, string cmdRest)
        {
            string result = string.Empty;

            try
            {
            }
            catch (Exception ex)
            {
                app.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }

        /* 
         * GOTO
         * 
         * Move Record Pointer to record potision
         * 
         */
        public static string Goto(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;

            // Where are we?
            int ds = jbe.App.CurrentDataSession;
            int wa = jbe.App.CurrentDS.CurrentWorkArea();

            try
            {

                if (eCodes.SESSION > 0)
                    jbe.App.SetDataSession(eCodes.SESSION);

                int cwa = jbe.App.CurrentDS.CurrentWorkArea();

                // Go to the desired workarea
                JAXObjects.Token workarea = new();
                workarea.Element.Value = string.IsNullOrWhiteSpace(eCodes.InExpr) ? wa : jbe.App.SolveFromRPNString(eCodes.InExpr);
                if (workarea.Element.Type.Equals("N"))
                    jbe.App.CurrentDS.SelectWorkArea(workarea.AsInt());
                else if (workarea.Element.Type.Equals("C"))
                    jbe.App.CurrentDS.SelectWorkArea(workarea.Element.ValueAsString);
                else
                    throw new Exception("11|");

                if (eCodes.Expressions.Count != 1) throw new Exception("10|");

                JAXObjects.Token tk = jbe.App.SolveFromRPNString(eCodes.Expressions[0].RNPExpr);
                if (tk.Element.Type.Equals("C"))
                {
                    if (tk.AsString().Equals("TOP"))
                        jbe.App.CurrentDS.CurrentWA.DBFGotoRecord("TOP", out _);
                    else if (tk.AsString().Equals("BOTTOM"))
                        jbe.App.CurrentDS.CurrentWA.DBFGotoRecord("BOTTOM", out _);
                    else
                        throw new Exception($"12|{tk.AsString()}");
                }
                else if (tk.Element.Type.Equals("N"))
                    jbe.App.CurrentDS.CurrentWA.DBFGotoRecord(tk.AsInt(), out _);
                else 
                    throw new Exception("11|");

                // Back to where we were
                jbe.App.CurrentDS.SelectWorkArea(cwa);

                jbe.App.SetDataSession(ds);
                jbe.App.CurrentDS.SelectWorkArea(wa); // Restore workarea
            }
            catch (Exception ex)
            {
                jbe.App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }
    }
}
