using DataGate.Com.Logs;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.Logs
{
    public class NLogManager : ILogManager
    {
        public NLogManager()
        {

        }

        public ILog GetLogger(string name)
        {
            return new NLogImpl(name);
        }
    }
}
