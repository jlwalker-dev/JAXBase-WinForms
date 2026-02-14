namespace JAXBase
{
    public class JAXBase_Executer_N
    {

        /* 
         * 
         *  NODEFAULT
         *  
         */
        public static string NoDefault(AppClass app, string cmdLine)
        {
            app.ClearErrors();
            string result = string.Empty;

            try
            {
                // Clear off the DoDefaults flag
                app.AppLevels[^1].DoDefault = false;
            }
            catch (Exception ex)
            {
                app.SetError(9999, ex.Message, System.Reflection.MethodBase.GetCurrentMethod()!.Name);
            }

            return result;
        }


    }
}
