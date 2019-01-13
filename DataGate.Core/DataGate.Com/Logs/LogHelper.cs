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
        /// 输出日志到Log4Net
        /// </summary>
        /// <param name="logInfo"></param>
        /// <param name="ex"></param>
        public static void Write(LogInfo logInfo, Exception ex = null)
        {
            if (log == null) return;
            //修改人：卢英杰
            //修改于: 2015.8.26。
            //原因：发现写入日志文件的时候有许多空日志，所以加入一些判断来限制空日志信息的产生。
            //if (log != null)   原方法
            //if (log != null && logInfo != null && !string.IsNullOrWhiteSpace(logInfo.ActionName) 
            //    && !string.IsNullOrWhiteSpace(logInfo.ModuleName))
            log.Write(logInfo, ex);
        }
    }
}
