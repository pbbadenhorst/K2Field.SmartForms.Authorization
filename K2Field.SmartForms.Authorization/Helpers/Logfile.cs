using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using io = System.IO;

namespace K2Field.SmartForms.Authorization.Helpers
{
    public static class Logfile
    {
        #region Write To Log

        public static void WriteToLog(bool enabled, string logfilePath, string logLevel, string method, string message)
        {
            io.StreamWriter logWriter = null;

            try
            {
                if (enabled == true)
                {
                    logWriter = new io.StreamWriter(logfilePath, true);
                    logWriter.WriteLine(DateTime.Now.ToString() + "\t" + method.Trim() + "\t" + logLevel.ToUpper().Trim() + "\t" + message);
                    logWriter.Flush();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (logWriter != null)
                {
                    logWriter.Close();
                    logWriter.Dispose();
                    logWriter = null;
                }
            }
        }

        #endregion
    }
}
