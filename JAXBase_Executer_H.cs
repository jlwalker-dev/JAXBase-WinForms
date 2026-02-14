using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAXBase
{
    internal class JAXBase_Executer_H
    {
        /* TODO
         * 
         * HELP
         * 
         */
        public static string Help(JAXBase_Executer jbe, ExecuterCodes eCodes)
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
