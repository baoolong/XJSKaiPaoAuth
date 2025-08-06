using System;
using System.IO;
using System.Text;

namespace XJSKaiPaoAuth
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class Logger
    {
        private static readonly object lockObj = new object();
        private static string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        
        static Logger()
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public static void Debug(string message)
        {
            WriteLog(LogLevel.Debug, message);
        }

        public static void Info(string message)
        {
            WriteLog(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            WriteLog(LogLevel.Warning, message);
        }

        public static void Error(string message)
        {
            WriteLog(LogLevel.Error, message);
        }

        public static void Error(string message, Exception ex)
        {
            WriteLog(LogLevel.Error, $"{message}\n异常信息: {ex.Message}\n堆栈跟踪: {ex.StackTrace}");
        }

        public static void Fatal(string message)
        {
            WriteLog(LogLevel.Fatal, message);
        }

        public static void Fatal(string message, Exception ex)
        {
            WriteLog(LogLevel.Fatal, $"{message}\n异常信息: {ex.Message}\n堆栈跟踪: {ex.StackTrace}");
        }

        private static void WriteLog(LogLevel level, string message)
        {
            lock (lockObj)
            {
                try
                {
                    string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                    string logFile = Path.Combine(logDirectory, $"{dateStr}.log");
                    
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                    
                    File.AppendAllText(logFile, logEntry + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // 日志写入失败时的处理，可以考虑输出到控制台
                    Console.WriteLine($"日志写入失败: {ex.Message}");
                }
            }
        }

        public static string GetLogDirectory()
        {
            return logDirectory;
        }

        public static string[] GetLogFiles()
        {
            if (Directory.Exists(logDirectory))
            {
                return Directory.GetFiles(logDirectory, "*.log");
            }
            return new string[0];
        }

        public static string ReadLogFile(string dateStr)
        {
            string logFile = Path.Combine(logDirectory, $"{dateStr}.log");
            if (File.Exists(logFile))
            {
                return File.ReadAllText(logFile, Encoding.UTF8);
            }
            return string.Empty;
        }

        public static void CleanOldLogs(int daysToKeep = 30)
        {
            lock (lockObj)
            {
                try
                {
                    if (Directory.Exists(logDirectory))
                    {
                        var files = Directory.GetFiles(logDirectory, "*.log");
                        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);
                            if (fileInfo.CreationTime < cutoffDate)
                            {
                                try
                                {
                                    fileInfo.Delete();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error($"删除旧日志文件失败: {file}", ex);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("清理旧日志时发生错误", ex);
                }
            }
        }
    }
}