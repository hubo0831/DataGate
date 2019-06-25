using System;

namespace DataGate.Com.Logs
{
    /// <summary>
    /// 系统日志帮助类
    /// </summary>
    public static class LogHelper
    {
        static ILog log;
        static ILogManager mLogManager;

        public static void Init(ILogManager logManager, string logName)
        {
            mLogManager = logManager;
            log = mLogManager.GetLogger(logName);
        }

        /// <summary>
        /// 输出日志到Log
        /// </summary>
        /// <param name="logInfo"></param>
        /// <param name="ex"></param>
        public static void Write(LogInfo logInfo, Exception ex = null)
        {
            log?.Write(logInfo, ex);
        }

        /// <summary>
        /// 输出日志到Log,立即写
        /// </summary>
        /// <param name="logInfo"></param>
        /// <param name="ex"></param>
        public static void WriteFast(LogInfo logInfo, Exception ex = null)
        {
            log?.Write(logInfo, ex);
            log.Flush();
        }
    }
}
