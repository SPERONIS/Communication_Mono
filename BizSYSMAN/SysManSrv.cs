using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DC;
using Nini.Config;
using BizUtils.Data;
using BizUtils.Rest;
using System.Data;
using System.Text.RegularExpressions;

namespace BizSYSMAN
{
    internal class SysManSrv
    {
        private DCClient clt;
        private string dbConnStr = "server=localhost;user id=root;password=9143;database=nxkj;DataProvider=MySql;";
        DbNetData db = null;

        internal SysManSrv()
        { }

        internal void Start()
        {
            IConfigSource cs =
                new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BizSYSMAN.ini"));
            IConfig c = cs.Configs["config"];
            dbConnStr = c.Get("DbConnectionString");
            db = new DbNetData(dbConnStr);
            db.CloseConnectionOnError = false;
            db.Open();

            clt = DCClient.Instance("BizSYSMAN", "global");
            clt.Srv("/sysman/dic/list", dic_list);

        }

        internal void Stop()
        {
            db.Dispose();
        }

        #region

        private byte[] dic_list(string extInfo, byte[] req)
        {
            DicListQry para;
            byte[] checkResult = PackOrb.CheckRequest<DicListQry>(req, out para,
                new string[]{
                    "dicname",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询字典信息

            try
            {
                string dicTbl = null;
                switch (para.dicname)
                {
                    case "蔬菜名称":
                        dicTbl = "t_dic_vegname";
                        break;
                    case "蔬菜种类":
                        dicTbl = "t_dic_vegtype";
                        break;
                    default:
                        return PackOrb.PackRespose(
                                        new HttpHeadInfo
                                        {
                                            StatusCode = HttpStatusCode.ServerSideError,
                                        },
                                        new ListDictionary {
                                {"respnum", -2},
                                {"respmsg", "不存在此字典"},
                                }
                                    );
                }
                QueryCommandConfig CmdConfig = new QueryCommandConfig(string.Format("select * from {0} order by code", dicTbl));
                DataTable dt = db.GetDataTable(CmdConfig);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取字典成功"},
                                {"count",dt.Rows.Count},
                                {"data", dt},
                                }
                            );
            }
            catch (Exception e)
            {
                DCLogger.LogError(e.Message);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.ServerSideError,
                                },
                                new ListDictionary {
                                {"respnum", -2},
                                {"respmsg", e.Message},
                                }
                            );
            }

            #endregion
        }

        #endregion
    }
}
