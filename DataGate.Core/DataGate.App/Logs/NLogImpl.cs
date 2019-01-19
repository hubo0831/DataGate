using DataGate.Com.Logs;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataGate.App.Logs
{
    public class NLogImpl : ILog
    {
        readonly Logger logger;

        public NLogImpl(string name)
        {
            logger = LogManager.GetLogger(name);
        }

        public void Write(LogInfo logInfo, Exception ex = null)
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "", logInfo.Message);
            if (ex != null)
            {
                logInfo.Exception += ex.GetType().Name + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace;
                logInfo.LogLevel = LogType.Error;
            }

            foreach (PropertyInfo pi in logInfo.GetType().GetProperties())
            {
                var val = pi.GetValue(logInfo);
                logEvent.Properties[pi.Name] = val;
            }
            logEvent.TimeStamp = logInfo.OpTime;
            logEvent.Exception = ex;
            logEvent.Level = ConvertLevel(logInfo.LogLevel);
            logger.Log(logEvent);
        }

        private LogLevel ConvertLevel(LogType logType)
        {
            switch (logType)
            {
                case LogType.Debug: return NLog.LogLevel.Debug;
                case LogType.Error: return NLog.LogLevel.Error;
                case LogType.Fatal: return NLog.LogLevel.Fatal;
                case LogType.Info: return NLog.LogLevel.Info;
                case LogType.Warning: return NLog.LogLevel.Warn;
                default:
                    return NLog.LogLevel.Trace;
            }
        }
    }
}
