using System;
using System.IO;

namespace MufflonoSoft.CodeNinjaSpy.Logging
{
    internal class SimpleLogger : ILogger
    {
        public bool Debug { get; set; }

        private static string _logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeNinjaSpyLog.txt");

        public void Log(string message)
        {
            if (Debug)
                File.AppendAllText(_logFile, message);
        }
    }
}