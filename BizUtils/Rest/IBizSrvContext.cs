using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BizUtils.Rest
{
    public interface IBizSrvContext
    {
        string GetConfig(string cfgItem);

        void ReportStatus(ListDictionary statusObj);

        #region Logging

        void LogTrace(string message);

        void LogTrace(string message, params object[] args);

        void LogInfo(string message);

        void LogInfo(string message, params object[] args);

        void LogError(string message);

        void LogError(string message, params object[] args);

        void LogFatal(string message);

        void LogFatal(string message, params object[] args);

        #endregion
    }
}
