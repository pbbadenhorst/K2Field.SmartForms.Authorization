using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using io = System.IO;

namespace K2Field.SmartForms.Authorization.Helpers
{
    /// <summary>
    /// Represents a log writer to write debugging messages to a provided log file.
    /// </summary>
    public static class Logfile
    {
        #region Log

        /// <summary>
        /// Writes a log message to a specified logfile.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <param name="className">The class name of the object that is writing to the log file.</param>
        /// <param name="methodName">The name of the method that is writing to the log file.</param>
        /// <param name="logLevel">The debugging level of the log message.</param>
        /// <param name="message">The message to be written the log file.</param>
        public static void Log(bool enableLogging, string filePath, ref object logSync, string className, string methodName, string logLevel, string message)
        {
            var text = DateTime.Now.ToString() + "\t" + className + "\t" + methodName + "\t" + logLevel.ToUpper() + "\t" + message;

            if (enableLogging == true)
            {
                lock (logSync)
                {
                    using (var fileStream = new io.FileStream(filePath, io.FileMode.Append, io.FileAccess.Write, io.FileShare.Write))
                    {
                        using (var writer = new io.StreamWriter(fileStream))
                        {
                            writer.WriteLine(text);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
