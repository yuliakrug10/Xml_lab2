using System;
using System.IO;
using System.Text;

namespace Xml_lab2.Logging
{
    public sealed class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();
        private readonly string _logFilePath;

        private Logger()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xml_lab2");
            Directory.CreateDirectory(folder);
            _logFilePath = Path.Combine(folder, "log.txt");
        }

        public static Logger GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
                return _instance;
            }
        }

        public string LogFilePath
        {
            get { return _logFilePath; }
        }

        public void LogEvent(LogLevel level, string message)
        {
            string line = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " " + LevelToText(level) + ". " + message;

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }

        private static string LevelToText(LogLevel level)
        {
            if (level == LogLevel.Filtering)
            {
                return "Фільтрація";
            }
            if (level == LogLevel.Transformation)
            {
                return "Трансформація";
            }
            return "Збереження";
        }
    }
}
