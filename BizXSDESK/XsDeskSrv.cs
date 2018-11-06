using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DC;
using Nini.Config;
using BizUtils.Data;
using BizUtils.Rest;
using System.Data;
using System.Collections.Generic;

namespace BizXSDESK
{
    internal class XsDeskSrv
    {
        private DCClient clt;
        private string dbConnStr = "server=localhost;user id=root;password=9143;database=nxkj;DataProvider=MySql;";
        DbNetData db = null;

        internal XsDeskSrv()
        { }

        internal void Start()
        {
            IConfigSource cs =
                new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BizXSDESK.ini"));
            IConfig c = cs.Configs["config"];
            dbConnStr = c.Get("DbConnectionString");
            db = new DbNetData(dbConnStr);
            db.CloseConnectionOnError = false;
            db.Open();

            clt = DCClient.Instance("BizXSDESK", "global");
            clt.Srv("/xsdesk/manage/register", manage_register);
            clt.Srv("/xsdesk/manage/list", manage_list);

            clt.Srv("/xsdesk/workdata/kc/syncsave", workdata_kc_syncsave);
            clt.Srv("/xsdesk/workdata/jhd/syncsave", workdata_jhd_syncsave);
            clt.Srv("/xsdesk/workdata/dhd/syncsave", workdata_dhd_syncsave);
            clt.Srv("/xsdesk/workdata/sh/syncsave", workdata_sh_syncsave);
            clt.Srv("/xsdesk/workdata/zf/syncsave", workdata_zf_syncsave);
            clt.Srv("/xsdesk/workdata/xsjl/syncsave", workdata_xsjl_syncsave);
            clt.Srv("/xsdesk/workdata/xsmx/syncsave", workdata_xsmx_syncsave);
            clt.Srv("/xsdesk/workdata/dh/syncsave", workdata_dh_syncsave);

            clt.Srv("/xsdesk/workdata/kc/syncdel", workdata_kc_syncdel);
            clt.Srv("/xsdesk/workdata/jhd/syncdel", workdata_jhd_syncdel);
            clt.Srv("/xsdesk/workdata/dhd/syncdel", workdata_dhd_syncdel);
            clt.Srv("/xsdesk/workdata/sh/syncdel", workdata_sh_syncdel);
            clt.Srv("/xsdesk/workdata/zf/syncdel", workdata_zf_syncdel);
            clt.Srv("/xsdesk/workdata/xsjl/syncdel", workdata_xsjl_syncdel);
            clt.Srv("/xsdesk/workdata/xsmx/syncdel", workdata_xsmx_syncdel);
            clt.Srv("/xsdesk/workdata/dh/syncdel", workdata_dh_syncdel);

            clt.Srv("/xsdesk/workdata/xsjl/query", workdata_xsjl_query);
            clt.Srv("/xsdesk/workdata/xsmx/query", workdata_xsmx_query);
            clt.Srv("/xsdesk/workdata/zf/query", workdata_zf_query);

            clt.Srv("/xsdesk/report/stat1", report_stat1);
            clt.Srv("/xsdesk/report/stat2", report_stat2);
            clt.Srv("/xsdesk/report/stat3", report_stat3);
            clt.Srv("/xsdesk/workdata/zf/stat1", workdata_zf_stat1);
        }

        internal void Stop()
        {
            db.Dispose();
        }

        #region 销售软件管理部分

        private byte[] manage_register(string extInfo, byte[] req)
        {
            RegisterData para;
            byte[] checkResult = PackOrb.CheckRequest<RegisterData>(req, out para,
                new string[]{
                    "xs_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 保存register信息

            CommandConfig CmdConfig = null;
            string tblName = "t_xsdesk_reginfo";

            long ret = 0;
            try
            {
                CmdConfig = DbNetDataUtil.PackCommandConfig(tblName, para.data.Rows[0]);
                CmdConfig.Params["xs_guid"] = para.xs_guid;
                ret -= db.ExecuteInsert(CmdConfig);
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

            return PackOrb.PackRespose(
                            new HttpHeadInfo
                            {
                                StatusCode = HttpStatusCode.Succeed,
                            },
                            new ListDictionary {
                            {"respnum", ret},
                            {"respmsg", "销售注册成功"},
                            }
                        );

            #endregion
        }

        private byte[] manage_list(string extInfo, byte[] req)
        {
            ClientListQuery para;
            byte[] checkResult = PackOrb.CheckRequest<ClientListQuery>(req, out para,
                new string[]{
                    "guid",
                    "client",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询可见销售点信息

            switch (para.client)
            {
                case "xs":
                    try
                    {
                        QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_xsdesk_reginfo where xs_guid = @guid");
                        CmdConfig.Params["guid"] = para.guid;
                        DataTable dt = db.GetDataTable(CmdConfig);

                        return PackOrb.PackRespose(
                                        new HttpHeadInfo
                                        {
                                            StatusCode = HttpStatusCode.Succeed,
                                        },
                                        new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取销售点信息成功"},
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
                case "lk":
                case "mb":
                    try
                    {
                        QueryCommandConfig CmdConfig = new QueryCommandConfig("SELECT t2.* FROM t_client_assosiation t1, t_xsdesk_reginfo t2 where t1.memberid=t2.xs_guid and t1.ownerid=@guid and t1.ownertype=@client;");
                        CmdConfig.Params["guid"] = para.guid;
                        CmdConfig.Params["client"] = para.client;
                        DataTable dt = db.GetDataTable(CmdConfig);

                        return PackOrb.PackRespose(
                                        new HttpHeadInfo
                                        {
                                            StatusCode = HttpStatusCode.Succeed,
                                        },
                                        new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取销售点信息成功"},
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
                default:
                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.ServerSideError,
                                    },
                                    new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", string.Format("无法识别的client值{0}", para.client)},
                                }
                                    );
            }

            #endregion
        }

        #endregion

        #region 销售日常记录同步部分-save

        private byte[] workdata_syncsave(string extInfo, byte[] req, int type)
        {
            WorkData para;
            byte[] checkResult = PackOrb.CheckRequest<WorkData>(req, out para,
                new string[]{
                    "xs_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 保存workdata信息

            CommandConfig CmdConfig = null;
            string tblName = null;
            string[] keyName = null;
            switch (type)
            {
                case 1:
                    //1.1　库存
                    tblName = "t_xsdesk_wdat_kc";
                    keyName = new string[] { "xs_guid", "InventoryID" };
                    break;
                case 2:
                    //1.2　进货单
                    tblName = "t_xsdesk_wdat_jhd";
                    keyName = new string[] { "xs_guid", "RecordID" };
                    break;
                case 3:
                    //1.3　调货单
                    tblName = "t_xsdesk_wdat_dhd";
                    keyName = new string[] { "xs_guid", "RecordID" };
                    break;
                case 4:
                    //1.4　运单
                    tblName = "t_xsdesk_wdat_sh";
                    keyName = new string[] { "xs_guid", "RecordID" };
                    break;
                case 5:
                    //1.5　杂费信息
                    tblName = "t_xsdesk_wdat_zf";
                    keyName = new string[] { "xs_guid", "IncidentalsID" };
                    break;
                case 6:
                    //1.6　销售记录
                    tblName = "t_xsdesk_wdat_xsjl";
                    keyName = new string[] { "xs_guid", "RecordID" };
                    break;
                case 7:
                    //1.7　销售明细
                    tblName = "t_xsdesk_wdat_xsmx";
                    keyName = new string[] { "xs_guid", "RecordID" };
                    break;
                case 8:
                    //1.8　到货
                    tblName = "t_xsdesk_wdat_dh";
                    keyName = new string[] { "xs_guid", "RecordID" };
                    break;
                default:
                    return PackOrb.PackRespose(new HttpHeadInfo(),
                                    new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "不支持的workdata_type类型"},
                                    }
                                );
            }

            long ret = 0;
            try
            {
                foreach (DataRow r in para.data.Rows)
                {
                    CmdConfig = DbNetDataUtil.PackCommandConfig(tblName, r);
                    CmdConfig.Params["xs_guid"] = para.xs_guid;
                    try
                    {
                        ret -= db.ExecuteInsert(CmdConfig);
                    }
                    catch// (Exception ex)
                    {
                        UpdateCommandConfig UpCmdConfig = DbNetDataUtil.PackUpdateCommandConfig(tblName, r, keyName);
                        UpCmdConfig.FilterParams["xs_guid"] = para.xs_guid;
                        ret += db.ExecuteUpdate(UpCmdConfig);
                    }
                }
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

            return PackOrb.PackRespose(
                            new HttpHeadInfo
                            {
                                StatusCode = HttpStatusCode.Succeed,
                            },
                            new ListDictionary {
                            {"respnum", ret},
                            {"respmsg", "日常记录同步保存成功"},
                            }
                        );

            #endregion
        }

        private byte[] workdata_kc_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 1);
        }

        private byte[] workdata_jhd_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 2);
        }

        private byte[] workdata_dhd_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 3);
        }

        private byte[] workdata_sh_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 4);
        }

        private byte[] workdata_zf_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 5);
        }

        private byte[] workdata_xsjl_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 6);
        }

        private byte[] workdata_xsmx_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 7);
        }

        private byte[] workdata_dh_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 8);
        }

        #endregion

        #region 销售日常记录同步部分-del

        private byte[] workdata_syncdel(string extInfo, byte[] req, int type)
        {
            WorkData para;
            byte[] checkResult = PackOrb.CheckRequest<WorkData>(req, out para,
                new string[]{
                    "xs_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 删除workdata信息

            CommandConfig CmdConfig = null;
            string tblName = null;
            string keyName = null;
            switch (type)
            {
                case 1:
                    //1.1　库存
                    tblName = "t_xsdesk_wdat_kc";
                    keyName = "InventoryID";
                    break;
                case 2:
                    //1.2　进货单
                    tblName = "t_xsdesk_wdat_jhd";
                    keyName = "RecordID";
                    break;
                case 3:
                    //1.3　调货单
                    tblName = "t_xsdesk_wdat_dhd";
                    keyName = "RecordID";
                    break;
                case 4:
                    //1.4　损耗
                    tblName = "t_xsdesk_wdat_sh";
                    keyName = "RecordID";
                    break;
                case 5:
                    //1.5　杂费信息
                    tblName = "t_xsdesk_wdat_zf";
                    keyName = "IncidentalsID";
                    break;
                case 6:
                    //1.6　销售记录
                    tblName = "t_xsdesk_wdat_xsjl";
                    keyName = "RecordID";
                    break;
                case 7:
                    //1.7　销售明细
                    tblName = "t_xsdesk_wdat_xsmx";
                    keyName = "RecordID";
                    break;
                case 8:
                    //1.8　到货
                    tblName = "t_xsdesk_wdat_dh";
                    keyName = "RecordID";
                    break;
                default:
                    return PackOrb.PackRespose(new HttpHeadInfo(),
                                    new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "不支持的workdata_type类型"},
                                    }
                                );
            }

            long ret = 0;
            try
            {
                foreach (DataRow r in para.data.Rows)
                {
                    CmdConfig = new CommandConfig(tblName);
                    CmdConfig.Params["xs_guid"] = para.xs_guid;
                    CmdConfig.Params[keyName] = r[keyName];
                    ret += db.ExecuteDelete(CmdConfig);
                }
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

            return PackOrb.PackRespose(
                            new HttpHeadInfo
                            {
                                StatusCode = HttpStatusCode.Succeed,
                            },
                            new ListDictionary {
                            {"respnum", ret},
                            {"respmsg", "日常记录同步删除成功"},
                            }
                        );

            #endregion
        }

        private byte[] workdata_kc_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 1);
        }

        private byte[] workdata_jhd_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 2);
        }

        private byte[] workdata_dhd_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 3);
        }

        private byte[] workdata_sh_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 4);
        }

        private byte[] workdata_zf_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 5);
        }

        private byte[] workdata_xsjl_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 6);
        }

        private byte[] workdata_xsmx_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 7);
        }

        private byte[] workdata_dh_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 8);
        }
        
        #endregion

        #region 销售相关信息查询部分

        private byte[] workdata_xsjl_query(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "xs_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询销售记录信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_xsdesk_wdat_xsjl where xs_guid = @xs_guid and Date >= @startdate and Date <= @enddate order by RecordID");
                CmdConfig.Params["xs_guid"] = para.xs_guid;
                CmdConfig.Params["startdate"] = para.startdate;
                CmdConfig.Params["enddate"] = para.enddate;
                DataTable dt = db.GetDataTable(CmdConfig);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取销售记录信息成功"},
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

        private byte[] workdata_xsmx_query(string extInfo, byte[] req)
        {
            XsmxQuery para1;
            byte[] checkResult1 = PackOrb.CheckRequest<XsmxQuery>(req, out para1,
                new string[]{
                    "xs_guid",
                    "BillsNum",
                });
            if (checkResult1 == null)
            {
                #region 按单号查询销售明细信息

                try
                {
                    QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_xsdesk_wdat_xsmx where xs_guid = @xs_guid and BillsNum = @BillsNum order by RecordID");
                    CmdConfig.Params["xs_guid"] = para1.xs_guid;
                    CmdConfig.Params["BillsNum"] = para1.BillsNum;
                    DataTable dt = db.GetDataTable(CmdConfig);

                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.Succeed,
                                    },
                                    new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取销售明细信息成功"},
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
            else
            {
                DateRangeQuery para2;
                byte[] checkResult2 = PackOrb.CheckRequest<DateRangeQuery>(req, out para2,
                    new string[]{
                    "xs_guid",
                    "startdate",
                    "enddate",
                });

                if (checkResult2 == null)
                {
                    #region 按时间范围查询销售明细信息

                    try
                    {
                        QueryCommandConfig CmdConfig = new QueryCommandConfig("SELECT t1.* FROM t_xsdesk_wdat_xsmx t1 inner join t_xsdesk_wdat_xsjl t2 on t1.BillsNum=t2.BillsNum where t1.xs_guid = @xs_guid and Date >= @startdate and Date <= @enddate order by RecordID");
                        CmdConfig.Params["xs_guid"] = para2.xs_guid;
                        CmdConfig.Params["startdate"] = para2.startdate;
                        CmdConfig.Params["enddate"] = para2.enddate;
                        DataTable dt = db.GetDataTable(CmdConfig);

                        return PackOrb.PackRespose(
                                        new HttpHeadInfo
                                        {
                                            StatusCode = HttpStatusCode.Succeed,
                                        },
                                        new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取销售明细信息成功"},
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
                else
                {
                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                    },
                                    new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", "查询参数错误"},
                                }
                                );
                }
            }
        }

        private byte[] workdata_zf_query(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "xs_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询销售杂费信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_xsdesk_wdat_zf where xs_guid = @xs_guid and Date >= @startdate  and Date <= @enddate order by Date,IncidentalsID");
                CmdConfig.Params["xs_guid"] = para.xs_guid;
                CmdConfig.Params["startdate"] = para.startdate;
                CmdConfig.Params["enddate"] = para.enddate;
                DataTable dt = db.GetDataTable(CmdConfig);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取杂费信息成功"},
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

        private byte[] workdata_tx_query(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "xs_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询销售提现信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_xsdesk_wdat_tx where xs_guid = @xs_guid and Date >= @startdate and Date <= @enddate order by Date,RecordID");
                CmdConfig.Params["xs_guid"] = para.xs_guid;
                CmdConfig.Params["startdate"] = para.startdate;
                CmdConfig.Params["enddate"] = para.enddate;
                DataTable dt = db.GetDataTable(CmdConfig);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取提现信息成功"},
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

        #region 销售数据统计分析部分

        /// <summary>
        ///    输入参数：销售ID，日期；
        ///    返回值：销售采购信息结果集，具体要素如下。
        ///    查询结果：品名、进货公斤数、进货均价、进货总额、出货箱数、箱成本价、出货总额、库存公斤数。
        ///         "date": "",//统计日期
        ///         "MerchandiseName": "",//商品名称
        ///         "KcLastday": 1,//上日库存（KcAmount-DhAmount-JhdAmount+XsmxAmount+ShAmount）
        ///         "KcAmount": 1,//当前库存(库存表)
        ///         "KcAveragePrice": 1,//成本均价（库存表进货均价）
        ///         "DhAmount": 1,//当日到货(到货表)
        ///         "JhdAmount": 1,//本地进货数量(商品数量)
        ///         "DhNextday": 1,//明日到货[根据冷库运单表销售点ID查询运单号，根据运单号查询出库表中出库单]
        ///         "ShAmount": 1//损耗(损耗表求和)
        ///         "XsmxAmount": 1,//当日销量(销售明细表算销售数量求和)
        ///         "XsmxTotalPrice": 1,//销售金额(销售明细表金额的和)
        ///         "XsmxSalesPrice": 1,//当日售价(XsmxTotalPrice/XsmxAmount)
        ///    说明：查询结果根据销售ID从销售库存表、进货单表、出货单表联合查询而得到。
        /// </summary>
        /// <param name="extInfo"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private byte[] report_stat1(string extInfo, byte[] req)
        {
            DateQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateQuery>(req, out para,
                new string[]{
                    "xs_guid",
                    "date",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            DateTime startdate = para.date.Date;
            DateTime enddate = para.date.Date.AddDays(1);
            #region 查询销售点综合报表信息

            List<Stat1Info> stats = new List<Stat1Info>();
            try
            {
                QueryCommandConfig KcQryCmd = new QueryCommandConfig("select * from t_xsdesk_wdat_kc where xs_guid = @xs_guid");
                KcQryCmd.Params["xs_guid"] = para.xs_guid;
                DataTable kcDt = db.GetDataTable(KcQryCmd);


                QueryCommandConfig JhdQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as JhdAmount from t_xsdesk_wdat_jhd where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate group by MerchandiseName");
                JhdQryCmd.Params["xs_guid"] = para.xs_guid;
                JhdQryCmd.Params["startdate"] = startdate;
                JhdQryCmd.Params["enddate"] = enddate;
                DataTable jhdDt = db.GetDataTable(JhdQryCmd);


                QueryCommandConfig DhQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as DhAmount from t_xsdesk_wdat_dh where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate group by MerchandiseName");
                DhQryCmd.Params["xs_guid"] = para.xs_guid;
                DhQryCmd.Params["startdate"] = startdate;
                DhQryCmd.Params["enddate"] = enddate;
                DataTable dhDt = db.GetDataTable(DhQryCmd);


                QueryCommandConfig ShQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as ShAmount from t_xsdesk_wdat_sh where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate group by MerchandiseName");
                ShQryCmd.Params["xs_guid"] = para.xs_guid;
                ShQryCmd.Params["startdate"] = startdate;
                ShQryCmd.Params["enddate"] = enddate;
                DataTable shDt = db.GetDataTable(ShQryCmd);


                QueryCommandConfig XsmxQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as XsmxAmount, ifnull(sum(TotalPrice),0) as XsmxTotalPrice from t_xsdesk_wdat_xsmx where xs_guid = @xs_guid and BillsNum in (select BillsNum from t_xsdesk_wdat_xsjl where Date >= @startdate and Date < @enddate) group by MerchandiseName");
                XsmxQryCmd.Params["xs_guid"] = para.xs_guid;
                XsmxQryCmd.Params["startdate"] = startdate;
                XsmxQryCmd.Params["enddate"] = enddate;
                DataTable xsmxDt = db.GetDataTable(XsmxQryCmd);

                QueryCommandConfig ChdQryCmd = new QueryCommandConfig("select t1.VegetableName,VegetableCfgName,sum(t1.Amount) as DhNextday from t_lkdesk_wdat_chd t1 inner join t_lkdesk_wdat_yd t2 on t1.TransportingNum=t2.BillsNum where xs_guid=@xs_guid and ArriveDate >= @startdate and ArriveDate < @enddate group by VegetableName,VegetableCfgName");
                ChdQryCmd.Params["xs_guid"] = para.xs_guid;
                ChdQryCmd.Params["startdate"] = startdate.AddDays(1);
                ChdQryCmd.Params["enddate"] = enddate.AddDays(1);
                DataTable chdDt = db.GetDataTable(ChdQryCmd);

                //select t1.VegetableName,VegetableCfgName,sum(t1.Amount) as DhNextday from t_xsdesk_wdat_chd t1 inner join t_xsdesk_wdat_yd t2 on t1.TransportingNum=t2.BillsNum where xs_guid='722e447fc64b408d991757eccc7e36b0' and ArriveDate >= @startdate and ArriveDate <= @enddate group by VegetableName,VegetableCfgName

                if (kcDt.Rows.Count > 0)
                {
                    foreach (DataRow r in kcDt.Rows)
                    {
                        Stat1Info stat = new Stat1Info();
                        stat.MerchandiseName = r["MerchandiseName"].ToString();
                        stat.KcAmount = decimal.Parse(r["Amount"].ToString());
                        stat.KcAveragePrice = decimal.Parse(r["AveragePrice"].ToString());

                        DataRow[] jhd_r = jhdDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (jhd_r.Length > 0)
                        {
                            stat.JhdAmount = decimal.Parse(jhd_r[0]["JhdAmount"].ToString());
                        }

                        DataRow[] dh_r = dhDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (dh_r.Length > 0)
                        {
                            stat.DhAmount = decimal.Parse(dh_r[0]["DhAmount"].ToString());
                        }

                        DataRow[] sh_r = shDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (sh_r.Length > 0)
                        {
                            stat.ShAmount = decimal.Parse(sh_r[0]["ShAmount"].ToString());
                        }

                        DataRow[] xsmx_r = xsmxDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (xsmx_r.Length > 0)
                        {
                            stat.XsmxAmount = decimal.Parse(xsmx_r[0]["XsmxAmount"].ToString());
                            stat.XsmxTotalPrice = decimal.Parse(xsmx_r[0]["XsmxTotalPrice"].ToString());
                            if (stat.XsmxAmount > 0)
                            {
                                stat.XsmxSalesPrice = stat.XsmxTotalPrice / stat.XsmxAmount;
                            }
                        }

                        DataRow[] chd_r = chdDt.Select(string.Format("VegetableName='{0}'", stat.MerchandiseName));
                        DataTable chd_dt = chdDt.Clone();
                        foreach (DataRow chd in chd_r)
                        {
                            chd_dt.ImportRow(chd);
                        }
                        stat.DhNextdayCount = chd_dt.Rows.Count;
                        stat.DhNextdayData = chd_dt;

                        stat.KcLastday = stat.KcAmount - stat.DhAmount - stat.JhdAmount + stat.XsmxAmount + stat.ShAmount;

                        stats.Add(stat);
                    }
                }

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", kcDt.Rows.Count},
                                {"respmsg", "获取销售点综合报表信息成功"},
                                {"count", stats.Count},
                                {"data", stats},
                                //{"MerchandiseName", stat.MerchandiseName},
                                //{"KcAmount", stat.KcAmount},
                                //{"JhdSumAmount", stat.JhdSumAmount},
                                //{"JhdSumPrices", stat.JhdSumPrices},
                                //{"JhdAvgPrices", stat.JhdAvgPrices},
                                //{"count", stat.count},
                                //{"data", stat.data},
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

        private byte[] report_stat2(string extInfo, byte[] req)
        {
            DateQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateQuery>(req, out para,
                new string[]{
                    "xs_guid",
                    "date",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            DateTime startdate = para.date.Date;
            DateTime enddate = para.date.Date.AddDays(1);
            #region 查询销售点现金流信息

            //List<Stat1Info> stats = new List<Stat1Info>();
            try
            {
                QueryCommandConfig XsjlQryCmd = new QueryCommandConfig("select ifnull(Imprest+ActualPrice, 0) as ReceivedTotal from t_xsdesk_wdat_xsjl where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate");
                XsjlQryCmd.Params["xs_guid"] = para.xs_guid;
                XsjlQryCmd.Params["startdate"] = startdate;
                XsjlQryCmd.Params["enddate"] = enddate;
                decimal ReceivedTotal = Convert.ToDecimal(db.ExecuteScalar(XsjlQryCmd));

                QueryCommandConfig XsmxQryCmd = new QueryCommandConfig("select ifnull(sum(TotalPrice),0) as ExpectTotal from t_xsdesk_wdat_xsmx where xs_guid = @xs_guid and BillsNum in (select BillsNum from t_xsdesk_wdat_xsjl where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate)");
                XsmxQryCmd.Params["xs_guid"] = para.xs_guid;
                XsmxQryCmd.Params["startdate"] = startdate;
                XsmxQryCmd.Params["enddate"] = enddate;
                decimal ExpectTotal = Convert.ToDecimal(db.ExecuteScalar(XsmxQryCmd));

                decimal Loan = ExpectTotal - ReceivedTotal;


                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", 1},
                                {"respmsg", "获取销售点现金流信息成功"},
                                {"ExpectTotal", ExpectTotal},
                                {"ReceivedTotal", ReceivedTotal},
                                {"Loan", Loan},
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

        private byte[] report_stat3(string extInfo, byte[] req)
        {
            DateQueryWithLk para;
            byte[] checkResult = PackOrb.CheckRequest<DateQueryWithLk>(req, out para,
                new string[]{
                    "lk_guid",
                    "xs_guid",
                    "date",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            DateTime startdate = para.date.Date;
            DateTime enddate = para.date.Date.AddDays(1);
            #region 查询冷库与销售关联报表信息

            List<Stat3Info> stats = new List<Stat3Info>();
            try
            {
                QueryCommandConfig KcQryCmd = new QueryCommandConfig("select * from t_xsdesk_wdat_kc where xs_guid = @xs_guid");
                KcQryCmd.Params["xs_guid"] = para.xs_guid;
                DataTable kcDt = db.GetDataTable(KcQryCmd);


                QueryCommandConfig JhdQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as JhdAmount from t_xsdesk_wdat_jhd where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate group by MerchandiseName");
                JhdQryCmd.Params["xs_guid"] = para.xs_guid;
                JhdQryCmd.Params["startdate"] = startdate;
                JhdQryCmd.Params["enddate"] = enddate;
                DataTable jhdDt = db.GetDataTable(JhdQryCmd);


                QueryCommandConfig DhQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as DhAmount from t_xsdesk_wdat_dh where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate group by MerchandiseName");
                DhQryCmd.Params["xs_guid"] = para.xs_guid;
                DhQryCmd.Params["startdate"] = startdate;
                DhQryCmd.Params["enddate"] = enddate;
                DataTable dhDt = db.GetDataTable(DhQryCmd);


                QueryCommandConfig ShQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as ShAmount from t_xsdesk_wdat_sh where xs_guid = @xs_guid and Date >= @startdate and Date < @enddate group by MerchandiseName");
                ShQryCmd.Params["xs_guid"] = para.xs_guid;
                ShQryCmd.Params["startdate"] = startdate;
                ShQryCmd.Params["enddate"] = enddate;
                DataTable shDt = db.GetDataTable(ShQryCmd);


                QueryCommandConfig XsmxQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as XsmxAmount, ifnull(sum(TotalPrice),0) as XsmxTotalPrice from t_xsdesk_wdat_xsmx where xs_guid = @xs_guid and BillsNum in (select BillsNum from t_xsdesk_wdat_xsjl where Date >= @startdate and Date < @enddate) group by MerchandiseName");
                XsmxQryCmd.Params["xs_guid"] = para.xs_guid;
                XsmxQryCmd.Params["startdate"] = startdate;
                XsmxQryCmd.Params["enddate"] = enddate;
                DataTable xsmxDt = db.GetDataTable(XsmxQryCmd);

                QueryCommandConfig ChdQryCmd = new QueryCommandConfig("select t1.VegetableName,ifnull(sum(t1.Amount),0) as DhNextday from t_lkdesk_wdat_chd t1 inner join t_lkdesk_wdat_yd t2 on t1.TransportingNum=t2.BillsNum where xs_guid=@xs_guid and t1.lk_guid=@lk_guid and ArriveDate >= @startdate and ArriveDate < @enddate group by VegetableName");
                ChdQryCmd.Params["lk_guid"] = para.lk_guid;
                ChdQryCmd.Params["xs_guid"] = para.xs_guid;
                ChdQryCmd.Params["startdate"] = startdate.AddDays(1);
                ChdQryCmd.Params["enddate"] = enddate.AddDays(1);
                DataTable chdDt = db.GetDataTable(ChdQryCmd);

                QueryCommandConfig LkChQryCmd = new QueryCommandConfig("select t1.VegetableName,ifnull(sum(t1.Amount),0) as DhNextday from t_lkdesk_wdat_chd t1 inner join t_lkdesk_wdat_yd t2 on t1.TransportingNum=t2.BillsNum where xs_guid=@xs_guid and t1.lk_guid=@lk_guid and ArriveDate >= @startdate and ArriveDate < @enddate group by VegetableName");
                LkChQryCmd.Params["lk_guid"] = para.lk_guid;
                LkChQryCmd.Params["xs_guid"] = para.xs_guid;
                LkChQryCmd.Params["startdate"] = startdate;
                LkChQryCmd.Params["enddate"] = enddate;
                DataTable lkChDt = db.GetDataTable(LkChQryCmd);

                QueryCommandConfig LkKcQryCmd = new QueryCommandConfig("select * from t_lkdesk_wdat_kc where lk_guid = @lk_guid and MerchandiseTypeID = 1");
                LkKcQryCmd.Params["lk_guid"] = para.lk_guid;
                DataTable lkkcDt = db.GetDataTable(LkKcQryCmd);

                if (kcDt.Rows.Count > 0)
                {
                    foreach (DataRow r in kcDt.Rows)
                    {
                        Stat3Info stat = new Stat3Info();
                        stat.MerchandiseName = r["MerchandiseName"].ToString();
                        stat.KcAmount = decimal.Parse(r["Amount"].ToString());
                        stat.KcAveragePrice = decimal.Parse(r["AveragePrice"].ToString());

                        DataRow[] jhd_r = jhdDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (jhd_r.Length > 0)
                        {
                            stat.JhdAmount = decimal.Parse(jhd_r[0]["JhdAmount"].ToString());
                        }

                        DataRow[] dh_r = dhDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (dh_r.Length > 0)
                        {
                            stat.DhAmount = decimal.Parse(dh_r[0]["DhAmount"].ToString());
                        }

                        DataRow[] sh_r = shDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (sh_r.Length > 0)
                        {
                            stat.ShAmount = decimal.Parse(sh_r[0]["ShAmount"].ToString());
                        }

                        DataRow[] xsmx_r = xsmxDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (xsmx_r.Length > 0)
                        {
                            stat.XsmxAmount = decimal.Parse(xsmx_r[0]["XsmxAmount"].ToString());
                            stat.XsmxTotalPrice = decimal.Parse(xsmx_r[0]["XsmxTotalPrice"].ToString());
                            if (stat.XsmxAmount > 0)
                            {
                                stat.XsmxSalesPrice = stat.XsmxTotalPrice / stat.XsmxAmount;
                            }
                        }

                        DataRow[] chd_r = chdDt.Select(string.Format("VegetableName='{0}'", stat.MerchandiseName));
                        if (chd_r.Length > 0)
                        {
                            stat.DhNextday = decimal.Parse(chd_r[0]["Amount"].ToString());
                        }

                        DataRow[] lkch_r = lkChDt.Select(string.Format("VegetableName='{0}'", stat.MerchandiseName));
                        if (chd_r.Length > 0)
                        {
                            stat.DhNextday = decimal.Parse(lkch_r[0]["Amount"].ToString());
                        }

                        DataRow[] lkkc_r = lkkcDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));
                        if (lkkc_r.Length > 0)
                        {
                            stat.LkKcAmount = decimal.Parse(lkkc_r[0]["Amount"].ToString());
                        }

                        stats.Add(stat);
                    }
                }
                if (lkkcDt.Rows.Count > 0)
                {
                    foreach (DataRow r in lkkcDt.Rows)
                    {
                        if (kcDt.Select(string.Format("MerchandiseName='{0}'", r["MerchandiseName"])).Length == 0)
                        {
                            Stat3Info stat = new Stat3Info();
                            stat.MerchandiseName = r["MerchandiseName"].ToString();
                            stat.LkKcAmount = decimal.Parse(r["Amount"].ToString());

                            DataRow[] jhd_r = jhdDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                            if (jhd_r.Length > 0)
                            {
                                stat.JhdAmount = decimal.Parse(jhd_r[0]["JhdAmount"].ToString());
                            }

                            DataRow[] dh_r = dhDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                            if (dh_r.Length > 0)
                            {
                                stat.DhAmount = decimal.Parse(dh_r[0]["DhAmount"].ToString());
                            }

                            DataRow[] sh_r = shDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                            if (sh_r.Length > 0)
                            {
                                stat.ShAmount = decimal.Parse(sh_r[0]["ShAmount"].ToString());
                            }

                            DataRow[] xsmx_r = xsmxDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                            if (xsmx_r.Length > 0)
                            {
                                stat.XsmxAmount = decimal.Parse(xsmx_r[0]["XsmxAmount"].ToString());
                                stat.XsmxTotalPrice = decimal.Parse(xsmx_r[0]["XsmxTotalPrice"].ToString());
                                if (stat.XsmxAmount > 0)
                                {
                                    stat.XsmxSalesPrice = stat.XsmxTotalPrice / stat.XsmxAmount;
                                }
                            }

                            DataRow[] chd_r = chdDt.Select(string.Format("VegetableName='{0}'", stat.MerchandiseName));
                            if (chd_r.Length > 0)
                            {
                                stat.DhNextday = decimal.Parse(chd_r[0]["Amount"].ToString());
                            }

                            DataRow[] lkch_r = lkChDt.Select(string.Format("VegetableName='{0}'", stat.MerchandiseName));
                            if (chd_r.Length > 0)
                            {
                                stat.DhNextday = decimal.Parse(lkch_r[0]["Amount"].ToString());
                            }

                            stats.Add(stat);
                        }
                    }
                }

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", stats.Count},
                                {"respmsg", "获取冷库与销售关联报表信息成功"},
                                {"count", stats.Count},
                                {"data", stats},
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

        private byte[] workdata_zf_stat1(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "xs_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询冷库杂费汇总信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select IncidentalsName,ifnull(sum(Money),0) as Money from t_xsdesk_wdat_zf where xs_guid = @xs_guid and Date >= @startdate and Date<=@enddate order by IncidentalsID");
                CmdConfig.Params["xs_guid"] = para.xs_guid;
                CmdConfig.Params["startdate"] = para.startdate;
                CmdConfig.Params["enddate"] = para.enddate;
                DataTable dt = db.GetDataTable(CmdConfig);

                decimal Total = 0.0M;
                foreach (DataRow r in dt.Rows)
                {
                    Total += Convert.ToDecimal(r["Money"]);
                }

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取杂费汇总信息成功"},
                                {"Total", Total},
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
