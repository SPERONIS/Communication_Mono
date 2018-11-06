using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BizUtils.Rest
{
    public interface IBizSrv
    {
        //void SetDb(BizUtils.Data.DbNetData db);
        //void SetConfig(Nini.Config.IConfig config);
        void SetContext(IBizSrvContext ctx);
        byte[] RequestProc(string extInfo, byte[] req);
    }
}
