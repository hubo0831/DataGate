using System;
namespace DataGate.Com.Logs
{
    public interface ILog
    {
        void Write(LogInfo logInfo, LogType logType, Exception ex);
    }
}
