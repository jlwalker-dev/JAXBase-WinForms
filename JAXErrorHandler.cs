/*
 * System Error Handler
 * 
 * History
 *  2025-08-12 - JLW
 *      Start of definition
 *      
 *  
 */
namespace JAXBase
{
    public class JAXErrorHandler
    {
        readonly AppClass App;
        readonly JAXObjectWrapper? Parent = null;
        readonly string Name;

        public JAXErrorHandler(AppClass app, JAXObjectWrapper? parentObject)
        {
            App = app;
            Parent = parentObject;

            if (parentObject is null)
                Name = "*SYSTEM";   // Register the System error handler
            else
            {
                if (parentObject.GetProperty("name", out JAXObjects.Token tk) == 0)
                    Name = tk.AsString();
                else
                    Name = "*EMPTY";
            }
        }

        public void SetErrorCode(string name, string code)
        {

        }

        public void Error(int errorno, int lineNo, string message, string procedure)
        {
        }
    }
}
