using System;

namespace XJSKaiPaoAuth
{
    public static class LoggerUsageExample
    {
        public static void DemonstrateLogger()
        {
            // 记录不同级别的日志
            Logger.Info("应用程序启动");
            Logger.Debug("这是调试信息");
            Logger.Warning("这是一个警告信息");
            Logger.Error("这是一个错误信息");
            
            try
            {
                int a = 10;
                int b = 0;
                int result = a / b;
            }
            catch (Exception ex)
            {
                Logger.Error("除零错误发生", ex);
            }
            
            Logger.Info("应用程序关闭");
            
            // 获取日志信息
            string logDir = Logger.GetLogDirectory();
            Console.WriteLine($"日志目录: {logDir}");
            
            string[] logFiles = Logger.GetLogFiles();
            Console.WriteLine($"日志文件数量: {logFiles.Length}");
            
            // 清理30天前的日志
            Logger.CleanOldLogs(30);
        }
    }
}