using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace DC
{
    /// <summary>
    /// 1 Trace - 最常见的记录信息，一般用于普通输出
    /// 2 Debug - 同样是记录信息，不过出现的频率要比Trace少一些，一般用来调试程序
    /// 3 Info - 信息类型的消息
    /// 4 Warn - 警告信息，一般用于比较重要的场合
    /// 5 Error - 错误信息
    /// 6 Fatal - 致命异常信息。一般来讲，发生致命异常之后程序将无法继续执行。
    /// </summary>
    public static class DCLogger
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void LogTrace(string message)
        {
            logger.Trace(message);
        }

        public static void LogTrace(string message, params object[] args)
        {
            logger.Trace(message, args);
        }

        public static void LogInfo(string message)
        {
            logger.Info(message);
        }

        public static void LogInfo(string message, params object[] args)
        {
            logger.Info(message, args);
        }

        public static void LogWarn(string message)
        {
            logger.Warn(message);
        }

        public static void LogWarn(string message, params object[] args)
        {
            logger.Warn(message, args);
        }

        public static void LogError(string message)
        {
            logger.Error(message);
        }

        public static void LogError(string message, params object[] args)
        {
            logger.Error(message, args);
        }

        public static void LogFatal(string message)
        {
            logger.Fatal(message);
        }

        public static void LogFatal(string message, params object[] args)
        {
            logger.Fatal(message, args);
        }
    }
}
