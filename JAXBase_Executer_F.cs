namespace JAXBase
{
    public class JAXBase_Executer_F
    {

        /* TODO
         * 
         * FLUSH
         * 
         */
        public static string Flush(AppClass app, string cmdRest)
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
         *
         * FOR VarName = nInitialValue TO nFinalValue [STEP nIncrement] 
         *
         *      LoopLabel/forVarExpr/startExpr/endExpr/stepExpr
         *
         */
        public static string For(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;

            try
            {
                jbe.App.DebugLog("Entering FOR");

                if (jbe.App.AppLevels.Count < 2) throw new Exception("2|");
                string PrgCode = jbe.App.PRGCache[jbe.App.AppLevels[^1].PRGCacheIdx];

                // Check to see if we're in this FOR loop
                string thisLoop = eCodes.SUBCMD.Length > 0 ? eCodes.SUBCMD : throw new Exception("9999|Missing FOR ID");
                string currentLoop = jbe.App.GetLoopStack();

                if (currentLoop.Equals(thisLoop))
                {
                    // YES! Continuing the FOR loop
                    LoopClass thisFor = jbe.App.AppLevels[^1].ForLoops[thisLoop];

                    // Next step value
                    JAXObjects.Token tk = jbe.App.GetVarFromExpression(thisFor.VarName, null);

                    if (tk.Element.Type.Equals("N"))
                    {
                        double nextStep = tk.AsDouble() + thisFor.StepValue;

                        if ((thisFor.StepValue > 0D && nextStep > thisFor.EndValue) || (thisFor.StepValue < 0D && nextStep < thisFor.EndValue))
                        {
                            // Done!
                            string look4 = jbe.App.MiscInfo["endforcmd"] + thisLoop;
                            int f = PrgCode.IndexOf(look4);
                            if (f >= 0)
                            {
                                // Go to the next command after the endfor
                                jbe.App.utl.Conv64(f, 3, out string pos);
                                result = "Y" + pos;
                            }
                            else
                                throw new Exception("1213|");
                        }
                        else
                            jbe.App.SetVarFromExpression(thisFor.VarName, nextStep, true);
                    }
                    else
                        throw new Exception("27|");
                }
                else
                {
                    // Start of a FOR loop
                    string vExpr = eCodes.Expressions.Count > 0 ? eCodes.Expressions[0].RNPExpr : throw new Exception("10");
                    string fStart = eCodes.ForExpr.Length > 0 ? eCodes.ForExpr : throw new Exception("10|");
                    string fEnd = eCodes.To.Count > 0 ? eCodes.To[0].Name : throw new Exception("10|");
                    double forStep = eCodes.STEP > 0 ? eCodes.STEP : 1.00;

                    double forStart = 0;
                    double forEnd = 0;

                    JAXObjects.Token answer = new();
                    answer = jbe.App.SolveFromRPNString(vExpr);
                    if (answer.Element.Type.Equals("C"))
                        vExpr = answer.AsString();
                    else
                        throw new Exception("10|");

                    answer = jbe.App.SolveFromRPNString(fStart);
                    if (answer.Element.Type.Equals("N"))
                        forStart = answer.AsDouble();
                    else
                        throw new Exception("11|");

                    answer = jbe.App.SolveFromRPNString(fEnd);
                    if (answer.Element.Type.Equals("N"))
                        forEnd = answer.AsDouble();
                    else
                        throw new Exception("11|");


                    jbe.App.PushLoop(thisLoop);
                    jbe.App.AppLevels[^1].ForLoops[thisLoop].VarName = jbe.App.SetVarFromExpression(vExpr, forStart, true);
                    jbe.App.AppLevels[^1].ForLoops[thisLoop].StepValue = forStep;
                    jbe.App.AppLevels[^1].ForLoops[thisLoop].EndValue = forEnd;
                }

            }
            catch (Exception ex)
            {
                jbe.App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }


        /* TODO NOW
         * 
         * FOREACH
         * 
         */
        public static string ForEach(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;

            try
            {
                if (jbe.App.AppLevels.Count < 2) throw new Exception("2|");
            }
            catch (Exception ex)
            {
                jbe.App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }

        /* TODO NOW
         * 
         * FREE
         * 
         */
        public static string Free(JAXBase_Executer jbe, ExecuterCodes eCodes)
        {
            string result = string.Empty;

            try
            {
            }
            catch (Exception ex)
            {
                jbe.App.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }

    }
}
