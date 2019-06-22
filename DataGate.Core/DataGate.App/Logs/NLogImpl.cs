using DataGate.Com.Logs;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataGate.App.Logs
{
    /// <summary>
    /// NLog的日志实现
    /// </summary>
    public class NLogImpl : ILog
    {
        readonly Logger _nlogger;

        public NLogImpl(string name)
        {
            _nlogger = LogManager.GetLogger(name);
        }

        public void Write(LogInfo logInfo, Exception ex = null)
        {
            logInfo.OpTime = DateTime.Now;
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
            _nlogger.Log(logEvent);
        }

        private LogLevel ConvertLevel(LogType logType)
        {
            switch (logType)
            {
                case LogType.Info: return NLog.LogLevel.Info;
                case LogType.Debug: return NLog.LogLevel.Debug;
                case LogType.Warn: return NLog.LogLevel.Warn;
                case LogType.Error: return NLog.LogLevel.Error;
                case LogType.Fatal: return NLog.LogLevel.Fatal;
                default:
                    return NLog.LogLevel.Trace;
            }
        }
    }
}
