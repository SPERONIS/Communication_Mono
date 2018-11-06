using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DC;
using Nini.Config;

namespace BizUtils.Rest
{
    public class BizSrvContext : IBizSrvContext
    {
        IConfigSource cs = null;
        IConfig c = null;
        string bizSrvName;

        public BizSrvContext(string bizSrvName, string cfgFilePath)
        {
            cs = new IniConfigSource(cfgFilePath);
            c = cs.Configs["config"];
            this.bizSrvName = bizSrvName;
        }

        public string GetConfig(string cfgItem)
        {
            return c.Get(cfgItem);
        }

        public void ReportStatus(ListDictionary statusObj)
        {
            this.LogInfo(string.Format("from [{0}]:{1}", bizSrvName, "I'm alive."));
        }

        #region Logging

        public void LogTrace(string message)
        {
            DCLogger.LogTrace(string.Format("from [{0}]:{1}", bizSrvName, message));
        }

        public void LogTrace(string message, params object[] args)
        {
            DCLogger.LogTrace(string.Format("from [{0}]:{1}", bizSrvName, message), args);
        }

        public void LogInfo(string message)
        {
            DCLogger.LogInfo(string.Format("from [{0}]:{1}", bizSrvName, message));
        }

        public void LogInfo(string message, params object[] args)
        {
            DCLogger.LogInfo(string.Format("from [{0}]:{1}", bizSrvName, message), args);
        }

        public void LogError(string message)
        {
            DCLogger.LogError(string.Format("from [{0}]:{1}", bizSrvName, message));
        }

        public void LogError(string message, params object[] args)
        {
            DCLogger.LogError(string.Format("from [{0}]:{1}", bizSrvName, message), args);
        }

        public void LogFatal(string message)
        {
            DCLogger.LogFatal(string.Format("from [{0}]:{1}", bizSrvName, message));
        }

        public void LogFatal(string message, params object[] args)
        {
            DCLogger.LogFatal(string.Format("from [{0}]:{1}", bizSrvName, message), args);
        }

        #endregion
    }
}
