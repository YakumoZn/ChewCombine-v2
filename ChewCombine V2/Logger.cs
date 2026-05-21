using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChewCombine_V2
{
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object _lock = new object();

        static Logger()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string logsDir = Path.Combine(baseDir, "logs");

            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            LogFilePath = Path.Combine(logsDir, $"log_{timestamp}.txt");
        }

        public static void Info(string message) => WriteLog("INFO", message);

        public static void Warn(string message) => WriteLog("WARN", message);

        public static void Error(string message, Exception ex = null)
        {
            WriteLog("ERROR", message);
            if (ex != null)
            {
                WriteLog("TRACE", ex.ToString());
            }
        }

        private static void WriteLog(string level, string message)
        {
            lock (_lock)
            {
                try
                {
                    string logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, logEntry);
                }
                catch
                {
                    // Ignore logger exceptions
                }
            }
        }
    }
}
