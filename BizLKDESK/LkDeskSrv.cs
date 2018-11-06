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

namespace BizLKDESK
{
    internal class LkDeskSrv
    {
        private DCClient clt;
        private string dbConnStr = "server=localhost;user id=root;password=9143;database=nxkj;DataProvider=MySql;";
        DbNetDataProxy db = null;

        internal LkDeskSrv()
        { }

        internal void Start()
        {
            IConfigSource cs =
                new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BizLKDESK.ini"));
            IConfig c = cs.Configs["config"];
            dbConnStr = c.Get("DbConnectionString");
            db = new DbNetDataProxy(dbConnStr);
            //db.CloseConnectionOnError = false;
            //db.Open();

            clt = DCClient.Instance("BizLKDESK", "global");
            clt.Srv("/lkdesk/manage/register", manage_register);
            clt.Srv("/lkdesk/manage/list", manage_list);

            clt.Srv("/lkdesk/dbfile/saveinfo", dbfile_saveinfo);
            clt.Srv("/lkdesk/dbfile/recentbacks", dbfile_recentbacks);

            clt.Srv("/lkdesk/workdata/kc/syncsave", workdata_kc_syncsave);
            clt.Srv("/lkdesk/workdata/jhd/syncsave", workdata_jhd_syncsave);
            clt.Srv("/lkdesk/workdata/chd/syncsave", workdata_chd_syncsave);
            clt.Srv("/lkdesk/workdata/yd/syncsave", workdata_yd_syncsave);
            clt.Srv("/lkdesk/workdata/zf/syncsave", workdata_zf_syncsave);
            clt.Srv("/lkdesk/workdata/tx/syncsave", workdata_tx_syncsave);
            clt.Srv("/lkdesk/workdata/gh/syncsave", workdata_gh_syncsave);

            clt.Srv("/lkdesk/workdata/kc/syncdel", workdata_kc_syncdel);
            clt.Srv("/lkdesk/workdata/jhd/syncdel", workdata_jhd_syncdel);
            clt.Srv("/lkdesk/workdata/chd/syncdel", workdata_chd_syncdel);
            clt.Srv("/lkdesk/workdata/yd/syncdel", workdata_yd_syncdel);
            clt.Srv("/lkdesk/workdata/zf/syncdel", workdata_zf_syncdel);
            clt.Srv("/lkdesk/workdata/tx/syncdel", workdata_tx_syncdel);

            clt.Srv("/lkdesk/pubdata/sg/save", pubdata_sg_save);

            clt.Srv("/lkdesk/workdata/chd/query", workdata_chd_query);
            clt.Srv("/lkdesk/workdata/yd/query", workdata_yd_query);
            clt.Srv("/lkdesk/workdata/zf/query", workdata_zf_query);
            clt.Srv("/lkdesk/workdata/tx/query", workdata_tx_query);

            clt.Srv("/lkdesk/report/stat1", report_stat1);
            clt.Srv("/lkdesk/report/stat2", report_stat2);
            clt.Srv("/lkdesk/report/stat3", report_stat3);
            clt.Srv("/lkdesk/workdata/yd/stat1", workdata_yd_stat1);
            clt.Srv("/lkdesk/workdata/zf/stat1", workdata_zf_stat1);
        }

        internal void Stop()
        {
            db.Dispose();
        }

        #region 冷库软件管理部分

        private byte[] manage_register(string extInfo, byte[] req)
        {
            RegisterData para;
            byte[] checkResult = PackOrb.CheckRequest<RegisterData>(req, out para,
                new string[]{
                    "lk_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 保存register信息

            long ret = 0;
            try
            {
                CommandConfig CmdConfig = new CommandConfig("t_org_reginfo");
                CmdConfig.Params["org_guid"] = para.lk_guid;
                CmdConfig.Params["org_type"] = 2;
                if (para.data.Rows[0]["LkName"] != null)
                {
                    CmdConfig.Params["org_name"] = para.data.Rows[0]["LkName"];
                }
                if (para.data.Rows[0]["Contacts"] != null)
                {
                    CmdConfig.Params["ctr_name"] = para.data.Rows[0]["Contacts"];
                }
                if (para.data.Rows[0]["Tel"] != null)
                {
                    CmdConfig.Params["ctr_tel"] = para.data.Rows[0]["Tel"];
                }
                if (para.data.Rows[0]["Address"] != null)
                {
                    CmdConfig.Params["address"] = para.data.Rows[0]["Address"];
                }
                if (para.data.Rows[0]["CompanyName"] != null)
                {
                    QueryCommandConfig QryCmdConfig = new QueryCommandConfig("select org_guid from t_org_reginfo where org_name = @org_name and org_type = @org_type");
                    QryCmdConfig.Params["org_name"] = para.data.Rows[0]["CompanyName"];
                    QryCmdConfig.Params["org_type"] = 1;
                    object ob = db.ExecuteScalar(QryCmdConfig);

                    CmdConfig.Params["super_guid"] = ob;
                }
                if (para.data.Rows[0]["Township"] != null)
                {
                    QueryCommandConfig QryCmdConfig = new QueryCommandConfig(string.Format("select zoneid from t_dic_zone where zonename like '{0}%'", para.data.Rows[0]["Township"]));
                    object ob = db.ExecuteScalar(QryCmdConfig);
                    if (ob != null)
                        CmdConfig.Params["zone"] = ob;
                }

                ListDictionary detail = new ListDictionary();
                if (para.data.Rows[0]["Fax"] != null)
                {
                    detail.Add("fax", para.data.Rows[0]["Fax"]);
                }
                if (para.data.Rows[0]["CompanyTel"] != null)
                {
                    detail.Add("office_tel", para.data.Rows[0]["CompanyTel"]);
                }
                if (para.data.Rows[0]["Email"] != null)
                {
                    detail.Add("email", para.data.Rows[0]["Email"]);
                }
                if (para.data.Rows[0]["URL"] != null)
                {
                    detail.Add("website", para.data.Rows[0]["URL"]);
                }
                if (detail.Count > 0)
                {
                    CmdConfig.Params["detail"] = detail.ToString();
                }

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
                            {"respmsg", "冷库注册成功"},
                            }
                        );

            #endregion

            #region 保存register信息 t_lkdesk_reginfo

            //CommandConfig CmdConfig = null;
            //string tblName = "t_lkdesk_reginfo";

            //long ret = 0;
            //try
            //{
            //    CmdConfig = DbNetDataUtil.PackCommandConfig(tblName, para.data.Rows[0]);
            //    CmdConfig.Params["lk_guid"] = para.lk_guid;
            //    ret -= db.ExecuteInsert(CmdConfig);
            //}
            //catch (Exception e)
            //{
            //    DCLogger.LogError(e.Message);

            //    return PackOrb.PackRespose(
            //                    new HttpHeadInfo
            //                    {
            //                        StatusCode = HttpStatusCode.ServerSideError,
            //                    },
            //                    new ListDictionary {
            //                    {"respnum", -2},
            //                    {"respmsg", e.Message},
            //                    }
            //                );
            //}

            //return PackOrb.PackRespose(
            //                new HttpHeadInfo
            //                {
            //                    StatusCode = HttpStatusCode.Succeed,
            //                },
            //                new ListDictionary {
            //                {"respnum", ret},
            //                {"respmsg", "冷库注册成功"},
            //                }
            //            );

            #endregion
        }

        public class ResponseData
        {
            public string lk_guid { get; set; }
            public string LkName { get; set; }
            public string CompanyName { get; set; }
            public string Contacts { get; set; }
            public string Tel { get; set; }
            public string Province { get; set; }
            public string City { get; set; }
            public string Township { get; set; }
            public string Address { get; set; }
            public string Fax { get; set; }
            public string CompanyTel { get; set; }
            public string Email { get; set; }
            public string URL { get; set; }
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

            #region 查询可见冷库信息

            QueryCommandConfig CmdConfig = null;
            if (para.client == "lk")
            {
                CmdConfig = new QueryCommandConfig("select * from t_org_reginfo where org_guid = @guid and org_type = @org_type");
                CmdConfig.Params["org_guid"] = para.guid;
            }
            else
            {
                CmdConfig = new QueryCommandConfig("select * from t_org_reginfo where org_type = @org_type and org_guid in (select res_guid from t_usr_auth_res where usr_guid = @usr_guid and res_type = @res_type)");
                CmdConfig.Params["usr_guid"] = para.guid;
                CmdConfig.Params["res_type"] = "org";
            }
            CmdConfig.Params["org_type"] = 2;
            List<ResponseData> data = new List<ResponseData>();
            try
            {
                DataTable cDt = db.GetDataTable(CmdConfig);

                if (cDt != null && cDt.Rows.Count > 0)
                {
                    foreach (DataRow r in cDt.Rows)
                    {
                        ResponseData res = new ResponseData();
                        res.lk_guid = r.Field<string>("org_guid");
                        res.LkName = r.Field<string>("org_name");
                        res.Contacts = r.Field<string>("ctr_name");
                        res.Tel = r.Field<string>("ctr_tel");
                        res.Address = r.Field<string>("address");

                        string zonestr = Biz.CalZonestr(r.Field<int?>("zone") ?? 0, db);
                        if (!string.IsNullOrWhiteSpace(zonestr))
                        {
                            string[] zones = zonestr.Split('-');
                            if (zones.Length > 0)
                                res.Province = zones[0];
                            if (zones.Length > 1)
                                res.City = zones[1];
                            if (zones.Length > 2)
                                res.Township = zones[2];
                        }

                        if (!string.IsNullOrWhiteSpace(r.Field<string>("detail")))
                        {
                            ListDictionary detail = PackOrb.JsonToObj<ListDictionary>(r.Field<string>("detail"));
                            if (detail["fax"] != null)
                                res.Fax = detail["fax"].ToString();
                            if (detail["office_tel"] != null)
                                res.CompanyTel = detail["office_tel"].ToString();
                            if (detail["email"] != null)
                                res.Email = detail["email"].ToString();
                            if (detail["website"] != null)
                                res.URL = detail["website"].ToString();
                        }

                        QueryCommandConfig superQry = new QueryCommandConfig("select org_name as super_name from t_org_reginfo where org_guid = @super_guid");
                        superQry.Params["super_guid"] = r.Field<string>("super_guid");
                        object super_name = db.ExecuteScalar(superQry);
                        if (super_name != null)
                        {
                            res.CompanyName = super_name.ToString();
                        }

                        data.Add(res);
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
                            {"respnum", data.Count},
                            {"respmsg", "冷库列表查询成功"},
                            {"count", data.Count},
                            {"data", data},
                            }
                        );

            #endregion


            #region 查询可见冷库信息 t_lkdesk_reginfo

            //switch (para.client)
            //{
            //    case "lk":
            //        try
            //        {
            //            QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_reginfo where lk_guid = @guid");
            //            CmdConfig.Params["guid"] = para.guid;
            //            DataTable dt = db.GetDataTable(CmdConfig);

            //            return PackOrb.PackRespose(
            //                            new HttpHeadInfo
            //                            {
            //                                StatusCode = HttpStatusCode.Succeed,
            //                            },
            //                            new ListDictionary {
            //                    {"respnum", dt.Rows.Count},
            //                    {"respmsg", "获取冷库信息成功"},
            //                    {"count",dt.Rows.Count},
            //                    {"data", dt},
            //                    }
            //                        );
            //        }
            //        catch (Exception e)
            //        {
            //            DCLogger.LogError(e.Message);

            //            return PackOrb.PackRespose(
            //                            new HttpHeadInfo
            //                            {
            //                                StatusCode = HttpStatusCode.ServerSideError,
            //                            },
            //                            new ListDictionary {
            //                    {"respnum", -2},
            //                    {"respmsg", e.Message},
            //                    }
            //                        );
            //        }
            //    case "xs":
            //    case "mb":
            //        try
            //        {
            //            QueryCommandConfig CmdConfig = new QueryCommandConfig("SELECT t2.* FROM t_client_assosiation t1, t_lkdesk_reginfo t2 where t1.memberid=t2.lk_guid and t1.ownerid=@guid and t1.ownertype=@client;");
            //            CmdConfig.Params["guid"] = para.guid;
            //            CmdConfig.Params["client"] = para.client;
            //            DataTable dt = db.GetDataTable(CmdConfig);

            //            return PackOrb.PackRespose(
            //                            new HttpHeadInfo
            //                            {
            //                                StatusCode = HttpStatusCode.Succeed,
            //                            },
            //                            new ListDictionary {
            //                    {"respnum", dt.Rows.Count},
            //                    {"respmsg", "获取冷库信息成功"},
            //                    {"count",dt.Rows.Count},
            //                    {"data", dt},
            //                    }
            //                        );
            //        }
            //        catch (Exception e)
            //        {
            //            DCLogger.LogError(e.Message);

            //            return PackOrb.PackRespose(
            //                            new HttpHeadInfo
            //                            {
            //                                StatusCode = HttpStatusCode.ServerSideError,
            //                            },
            //                            new ListDictionary {
            //                    {"respnum", -2},
            //                    {"respmsg", e.Message},
            //                    }
            //                        );
            //        }
            //    default:
            //        return PackOrb.PackRespose(
            //                        new HttpHeadInfo
            //                        {
            //                            StatusCode = HttpStatusCode.ServerSideError,
            //                        },
            //                        new ListDictionary {
            //                    {"respnum", -1},
            //                    {"respmsg", string.Format("无法识别的client值{0}", para.client)},
            //                    }
            //                        );
            //}

            #endregion
        }

        #endregion

        #region 冷库数据库备份部分

        private byte[] dbfile_saveinfo(string extInfo, byte[] req)
        {
            DbBackInfo para;
            byte[] checkResult = PackOrb.CheckRequest<DbBackInfo>(req, out para,
                new string[]{
                    "lk_guid",
                    "filename",
                    "filelen",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 保存数据库备份信息

            CommandConfig CmdConfig = null;
            string tblName = "t_lkdesk_dbfileinfo";

            long ret = 0;
            try
            {
                CmdConfig = new CommandConfig(tblName);
                CmdConfig.Params["lk_guid"] = para.lk_guid;
                CmdConfig.Params["filename"] = para.filename;
                CmdConfig.Params["filelen"] = para.filelen;
                CmdConfig.Params["date"] = DateTime.Now;
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
                            {"respmsg", "保存数据库备份信息成功"},
                            }
                        );

            #endregion
        }

        private byte[] dbfile_recentbacks(string extInfo, byte[] req)
        {
            DbBackQry para;
            byte[] checkResult = PackOrb.CheckRequest<DbBackQry>(req, out para,
                new string[]{
                    "lk_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询数据库备份信息

            try
            {
                DataTable dt = db.GetDataTable(
                    string.Format("select * from t_lkdesk_dbfileinfo where lk_guid = '{0}' order by date desc limit {1}",
                    para.lk_guid,
                    para.max)
                    );

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取数据库备份信息成功"},
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

        #region 冷库日常记录同步部分-save

        private byte[] workdata_syncsave(string extInfo, byte[] req, int type)
        {
            WorkData para;
            byte[] checkResult = PackOrb.CheckRequest<WorkData>(req, out para,
                new string[]{
                    "lk_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 保存workdata信息

            CommandConfig CmdConfig = null;
            string tblName = null;
            string keyName = null;
            switch (type)
            {
                case 1:
                    //1.1　库存
                    tblName = "t_lkdesk_wdat_kc";
                    keyName = "InventoryID";
                    break;
                case 2:
                    //1.2　进货单
                    tblName = "t_lkdesk_wdat_jhd";
                    keyName = "PurchaseID";
                    break;
                case 3:
                    //1.3　出货单
                    tblName = "t_lkdesk_wdat_chd";
                    keyName = "StockOutID";
                    break;
                case 4:
                    //1.4　运单
                    tblName = "t_lkdesk_wdat_yd";
                    keyName = "TransportingID";
                    break;
                case 5:
                    //1.5　杂费信息
                    tblName = "t_lkdesk_wdat_zf";
                    keyName = "IncidentalsID";
                    break;
                case 6:
                    //1.6　银行提现记录
                    tblName = "t_lkdesk_wdat_tx";
                    keyName = "RecordID";
                    break;
                case 7:
                    //1.7　供货商记录
                    tblName = "t_lkdesk_wdat_gh";
                    keyName = "SupplierID";
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
                    CmdConfig.Params["lk_guid"] = para.lk_guid;
                    try
                    {
                        ret -= db.ExecuteInsert(CmdConfig);
                    }
                    catch (Exception ex)
                    {
                        UpdateCommandConfig UpCmdConfig = DbNetDataUtil.PackUpdateCommandConfig(tblName, r, new string[] { keyName });
                        UpCmdConfig.FilterParams["lk_guid"] = para.lk_guid;
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

        private byte[] workdata_chd_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 3);
        }

        private byte[] workdata_yd_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 4);
        }

        private byte[] workdata_zf_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 5);
        }

        private byte[] workdata_tx_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 6);
        }

        private byte[] workdata_gh_syncsave(string extInfo, byte[] req)
        {
            return this.workdata_syncsave(extInfo, req, 7);
        }

        #endregion

        #region 冷库日常记录同步部分-del

        private byte[] workdata_syncdel(string extInfo, byte[] req, int type)
        {
            WorkData para;
            byte[] checkResult = PackOrb.CheckRequest<WorkData>(req, out para,
                new string[]{
                    "lk_guid",
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
                    tblName = "t_lkdesk_wdat_kc";
                    keyName = "InventoryID";
                    break;
                case 2:
                    //1.2　进货单
                    tblName = "t_lkdesk_wdat_jhd";
                    keyName = "PurchaseID";
                    break;
                case 3:
                    //1.3　出货单
                    tblName = "t_lkdesk_wdat_chd";
                    keyName = "StockOutID";
                    break;
                case 4:
                    //1.4　运单
                    tblName = "t_lkdesk_wdat_yd";
                    keyName = "TransportingID";
                    break;
                case 5:
                    //1.5　杂费信息
                    tblName = "t_lkdesk_wdat_zf";
                    keyName = "IncidentalsID";
                    break;
                case 6:
                    //1.6　银行提现记录
                    tblName = "t_lkdesk_wdat_tx";
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
                    CmdConfig.Params["lk_guid"] = para.lk_guid;
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

        private byte[] workdata_chd_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 3);
        }

        private byte[] workdata_yd_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 4);
        }

        private byte[] workdata_zf_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 5);
        }

        private byte[] workdata_tx_syncdel(string extInfo, byte[] req)
        {
            return this.workdata_syncdel(extInfo, req, 6);
        }

        #endregion

        #region 冷库业务信息发布部分

        private byte[] pubdata_save(string extInfo, byte[] req, int type)
        {
            PubData para;
            byte[] checkResult = PackOrb.CheckRequest<PubData>(req, out para,
                new string[]{
                    "lk_guid",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 保存pubdata信息

            CommandConfig CmdConfig = null;
            string tblName = null;
            string keyName = null;
            switch (type)
            {
                case 1:
                    //1.发布收购信息
                    tblName = "t_lkdesk_pdat_sg";
                    keyName = "PurchaseID";
                    break;
                default:
                    return PackOrb.PackRespose(new HttpHeadInfo(),
                                    new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "不支持的pubdata_type类型"},
                                    }
                                );
            }

            long ret = 0;
            try
            {
                foreach (DataRow r in para.data.Rows)
                {
                    CmdConfig = DbNetDataUtil.PackCommandConfig(tblName, r);
                    CmdConfig.Params["lk_guid"] = para.lk_guid;
                    try
                    {
                        ret -= db.ExecuteInsert(CmdConfig);
                    }
                    catch// (Exception ex)
                    {
                        UpdateCommandConfig UpCmdConfig = DbNetDataUtil.PackUpdateCommandConfig(tblName, r, new string[] { keyName });
                        UpCmdConfig.FilterParams["lk_guid"] = para.lk_guid;
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
                            {"respmsg", "信息发布保存成功"},
                            }
                        );

            #endregion
        }

        private byte[] pubdata_sg_save(string extInfo, byte[] req)
        {
            return this.pubdata_save(extInfo, req, 1);
        }

        #endregion

        #region 冷库相关信息查询部分

        private byte[] workdata_chd_query(string extInfo, byte[] req)
        {
            TransportingNumQuery para;
            byte[] checkResult = PackOrb.CheckRequest<TransportingNumQuery>(req, out para,
                new string[]{
                    "lk_guid",
                    "TransportingNum",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询冷库出货单信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_wdat_chd where lk_guid = @lk_guid and TransportingNum = @TransportingNum order by TransportingNum,Date,StockOutID");
                CmdConfig.Params["lk_guid"] = para.lk_guid;
                CmdConfig.Params["TransportingNum"] = para.TransportingNum;
                DataTable dt = db.GetDataTable(CmdConfig);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取出货单信息成功"},
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

        private byte[] workdata_yd_query(string extInfo, byte[] req)
        {
            DateRangeQuery para1;
            byte[] checkResult1 = PackOrb.CheckRequest<DateRangeQuery>(req, out para1,
                new string[]{
                    "lk_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult1 == null)
            {
                #region 按时间范围查询冷库运单信息

                try
                {
                    QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_wdat_yd where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate order by Date,BillsNum");
                    CmdConfig.Params["lk_guid"] = para1.lk_guid;
                    CmdConfig.Params["startdate"] = para1.startdate;
                    CmdConfig.Params["enddate"] = para1.enddate;
                    DataTable dt = db.GetDataTable(CmdConfig);

                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.Succeed,
                                    },
                                    new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取运单信息成功"},
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
            else
            {
                BillsNumQuery para2;
                byte[] checkResult2 = PackOrb.CheckRequest<BillsNumQuery>(req, out para2,
                    new string[]{
                    "lk_guid",
                    "BillsNum",
                });
                if (checkResult2 == null)
                {
                    #region 按运单号查询冷库运单信息

                    try
                    {
                        QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_wdat_yd where lk_guid = @lk_guid and BillsNum = @BillsNum order by Date,BillsNum");
                        CmdConfig.Params["lk_guid"] = para2.lk_guid;
                        CmdConfig.Params["BillsNum"] = para2.BillsNum;
                        DataTable dt = db.GetDataTable(CmdConfig);

                        return PackOrb.PackRespose(
                                        new HttpHeadInfo
                                        {
                                            StatusCode = HttpStatusCode.Succeed,
                                        },
                                        new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取运单信息成功"},
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
                else
                {
                    ArriveDateRangeQuery para3;
                    byte[] checkResult3 = PackOrb.CheckRequest<ArriveDateRangeQuery>(req, out para3,
                        new string[]{
                    "lk_guid",
                    "ArriveDateMin",
                    "ArriveDateMax",
                    });
                    if (checkResult3 == null)
                    {
                        #region 按到达时间范围查询冷库运单信息

                        try
                        {
                            QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_wdat_yd where lk_guid = @lk_guid and ArriveDate >= @ArriveDateMin and ArriveDate <= @ArriveDateMax order by Date,BillsNum");
                            CmdConfig.Params["lk_guid"] = para3.lk_guid;
                            CmdConfig.Params["ArriveDateMin"] = para3.ArriveDateMin;
                            CmdConfig.Params["ArriveDateMax"] = para3.ArriveDateMax;
                            DataTable dt = db.GetDataTable(CmdConfig);

                            return PackOrb.PackRespose(
                                            new HttpHeadInfo
                                            {
                                                StatusCode = HttpStatusCode.Succeed,
                                            },
                                            new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取运单信息成功"},
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

        }

        private byte[] workdata_zf_query(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "lk_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询冷库杂费信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_wdat_zf where lk_guid = @lk_guid and Date >= @startdate and Date<=@enddate order by Date,IncidentalsID");
                CmdConfig.Params["lk_guid"] = para.lk_guid;
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

        private byte[] workdata_tx_query(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "lk_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询冷库提现信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from t_lkdesk_wdat_tx where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate order by Date,RecordID");
                CmdConfig.Params["lk_guid"] = para.lk_guid;
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

        #region 冷库数据统计分析部分

        /// <summary>
        ///    输入参数：冷库ID，日期；
        ///    返回值：冷库采购信息结果集，具体要素如下。
        ///    查询结果：品名、进货公斤数、进货均价、进货总额、出货箱数、箱成本价、出货总额、库存公斤数。
        ///    说明：查询结果根据冷库ID从冷库库存表、进货单表、出货单表联合查询而得到。
        /// </summary>
        /// <param name="extInfo"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private byte[] report_stat1(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "lk_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }


            #region 查询冷库进销存信息

            List<Stat1Info> stats = new List<Stat1Info>();
            try
            {
                QueryCommandConfig KcQryCmd = new QueryCommandConfig("select * from t_lkdesk_wdat_kc where lk_guid = @lk_guid and MerchandiseTypeID = 1");
                KcQryCmd.Params["lk_guid"] = para.lk_guid;
                DataTable kcDt = db.GetDataTable(KcQryCmd);


                QueryCommandConfig JhdQryCmd = new QueryCommandConfig("select MerchandiseName, ifnull(sum(Amount),0) as JhdSumAmount, ifnull(sum(TotalPrices),0) as JhdSumPrices from t_lkdesk_wdat_jhd where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate group by MerchandiseName");
                JhdQryCmd.Params["lk_guid"] = para.lk_guid;
                JhdQryCmd.Params["startdate"] = para.startdate;
                JhdQryCmd.Params["enddate"] = para.enddate;
                DataTable jhdDt = db.GetDataTable(JhdQryCmd);


                QueryCommandConfig ChdQryCmd = new QueryCommandConfig("select VegetableName,VegetableCfgName,PointOrginName,ifnull(sum(Amount),0) as Amount,ifnull(sum(TotalPrices),0) as TotalPrices, ifnull(sum(TotalPrices),0)/ifnull(sum(Amount),0.001) as CostUnivalence from t_lkdesk_wdat_chd where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate group by VegetableName,VegetableCfgName,PointOrginName");
                ChdQryCmd.Params["lk_guid"] = para.lk_guid;
                ChdQryCmd.Params["startdate"] = para.startdate;
                ChdQryCmd.Params["enddate"] = para.enddate;
                DataTable chdDt = db.GetDataTable(ChdQryCmd);

                if (kcDt.Rows.Count > 0)
                {
                    foreach (DataRow r in kcDt.Rows)
                    {
                        Stat1Info stat = new Stat1Info();
                        stat.MerchandiseName = r["MerchandiseName"].ToString();
                        stat.KcAmount = decimal.Parse(r["Amount"].ToString());

                        DataRow[] jhd_r = jhdDt.Select(string.Format("MerchandiseName='{0}'", stat.MerchandiseName));

                        if (jhd_r.Length > 0)
                        {
                            stat.JhdSumAmount = decimal.Parse(jhd_r[0]["JhdSumAmount"].ToString());
                            stat.JhdSumPrices = decimal.Parse(jhd_r[0]["JhdSumPrices"].ToString());
                            if (stat.JhdSumAmount > 0)
                            {
                                stat.JhdAvgPrices = stat.JhdSumPrices / stat.JhdSumAmount;
                            }
                        }

                        DataRow[] chd_r = chdDt.Select(string.Format("VegetableName='{0}'", stat.MerchandiseName));
                        DataTable chd_dt = chdDt.Clone();
                        foreach (DataRow chd in chd_r)
                        {
                            chd_dt.ImportRow(chd);
                        }
                        stat.ChdCount = chd_dt.Rows.Count;
                        stat.ChdData = chd_dt;
                        //QueryCommandConfig ChdQryCmd = new QueryCommandConfig("select VegetableCfgName,ifnull(sum(Amount),0) as Amount,ifnull(sum(TotalPrices),0) as TotalPrices, ifnull(sum(TotalPrices),0)/ifnull(sum(Amount),0.001) as CostUnivalence from t_lkdesk_wdat_chd where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate and VegetableName = @VegetableName group by VegetableCfgName");
                        //ChdQryCmd.Params["lk_guid"] = para.lk_guid;
                        //ChdQryCmd.Params["startdate"] = para.startdate;
                        //ChdQryCmd.Params["enddate"] = para.enddate;
                        //ChdQryCmd.Params["VegetableName"] = stat.MerchandiseName;
                        //DataTable chdDt = db.GetDataTable(ChdQryCmd);
                        //stat.count = chdDt.Rows.Count;
                        //stat.data = chdDt;

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
                                {"respmsg", "获取冷库进销存信息成功"},
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
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "lk_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询冷库现金流信息

            try
            {
                QueryCommandConfig TxQryCmd = new QueryCommandConfig("select ifnull(sum(Money),0) as TxTotal from t_lkdesk_wdat_tx where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate");
                TxQryCmd.Params["lk_guid"] = para.lk_guid;
                TxQryCmd.Params["startdate"] = para.startdate;
                TxQryCmd.Params["enddate"] = para.enddate;
                //DataTable dt = db.GetDataTable(TxQryCmd);
                decimal TxTotal = Convert.ToDecimal(db.ExecuteScalar(TxQryCmd));

                QueryCommandConfig ZfQryCmd = new QueryCommandConfig("select ifnull(sum(Money),0) as ZfTotal from t_lkdesk_wdat_zf where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate");
                ZfQryCmd.Params["lk_guid"] = para.lk_guid;
                ZfQryCmd.Params["startdate"] = para.startdate;
                ZfQryCmd.Params["enddate"] = para.enddate;
                //DataTable dt = db.GetDataTable(ZfQryCmd);
                decimal ZfTotal = Convert.ToDecimal(db.ExecuteScalar(ZfQryCmd));

                QueryCommandConfig JhdQryCmd = new QueryCommandConfig("select ifnull(sum(TotalPrices),0) as JhdTotal from t_lkdesk_wdat_jhd where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate");
                JhdQryCmd.Params["lk_guid"] = para.lk_guid;
                JhdQryCmd.Params["startdate"] = para.startdate;
                JhdQryCmd.Params["enddate"] = para.enddate;
                //DataTable dt = db.GetDataTable(JhdQryCmd);
                decimal JhdTotal = Convert.ToDecimal(db.ExecuteScalar(JhdQryCmd));

                decimal Remain = TxTotal - ZfTotal - JhdTotal;

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", 1},
                                {"respmsg", "获取现金流信息成功"},
                                {"TxTotal", TxTotal},
                                {"ZfTotal", ZfTotal},
                                {"JhdTotal", JhdTotal},
                                {"Remain", Remain},
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
            PriceRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<PriceRangeQuery>(req, out para,
                new string[]{
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询市场成交价信息

            try
            {
                string sql = "SELECT MerchandiseName as vegname,date_format(Date, '%Y-%m-%d') as date,max(TotalPrices/Amount) as max,min(TotalPrices/Amount) as min,avg(TotalPrices/Amount) as avg FROM t_lkdesk_wdat_jhd where Date >= @startdate and Date < @enddate {0} {1} group by MerchandiseName,date_format(Date, '%Y-%m-%d') order by MerchandiseName,date_format(Date, '%Y-%m-%d')";
                string vegnameWhere = string.Empty;
                string areaWhere = string.Empty;
                if (!string.IsNullOrEmpty(para.vegname))
                {
                    vegnameWhere = string.Format("and MerchandiseName = '{0}'", para.vegname);
                }
                if (!string.IsNullOrEmpty(para.area))
                {
                    string[] areas = para.area.Split('-');

                    if (areas.Length != 3)
                    {
                        return PackOrb.PackRespose(
                                        new HttpHeadInfo
                                        {
                                            StatusCode = HttpStatusCode.BadRequest,
                                        },
                                        new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", "area参数值格式错误 省-市-区"},
                                }
                                    );
                    }
                    QueryCommandConfig QryCmdConfig = new QueryCommandConfig(string.Format("select zoneid from t_dic_zone where zonename like '{0}%'", areas[2]));
                    object ob = db.ExecuteScalar(QryCmdConfig);
                    if (ob != null)
                        areaWhere = string.Format("and lk_guid in (select org_guid from t_org_reginfo where org_type = 2 and zone = {0})", ob);
                    //if (areas[0] != "null")
                    //{
                    //    string w = string.Format("where Province='{0}'", areas[0]);
                    //    if (areas[1] != "null")
                    //    {
                    //        w += string.Format(" and City='{0}'", areas[1]);
                    //    }
                    //    if (areas[2] != "null")
                    //    {
                    //        w += string.Format(" and Township='{0}'", areas[2]);
                    //    }
                    //    areaWhere = string.Format("and lk_guid in (select lk_guid from t_lkdesk_reginfo {0})", w);
                    //}
                }

                QueryCommandConfig CmdConfig = new QueryCommandConfig(string.Format(sql, vegnameWhere, areaWhere));
                CmdConfig.Params["startdate"] = para.startdate.Date;
                CmdConfig.Params["enddate"] = para.enddate.Date.AddDays(1);
                DataTable dt = db.GetDataTable(CmdConfig);

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", dt.Rows.Count},
                                {"respmsg", "获取市场成交价信息成功"},
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

        private byte[] workdata_zf_stat1(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "lk_guid",
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
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select IncidentalsName,ifnull(sum(Money),0) as Money from t_lkdesk_wdat_zf where lk_guid = @lk_guid and Date >= @startdate and Date<=@enddate order by IncidentalsID");
                CmdConfig.Params["lk_guid"] = para.lk_guid;
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

        private byte[] workdata_yd_stat1(string extInfo, byte[] req)
        {
            DateRangeQuery para;
            byte[] checkResult = PackOrb.CheckRequest<DateRangeQuery>(req, out para,
                new string[]{
                    "lk_guid",
                    "startdate",
                    "enddate",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 查询车次数量信息

            try
            {
                QueryCommandConfig CmdConfig = new QueryCommandConfig("select count(*) as count from t_lkdesk_wdat_yd where lk_guid = @lk_guid and Date >= @startdate and Date <= @enddate");
                CmdConfig.Params["lk_guid"] = para.lk_guid;
                CmdConfig.Params["startdate"] = para.startdate;
                CmdConfig.Params["enddate"] = para.enddate;
                //DataTable dt = db.GetDataTable(CmdConfig);
                int count = Convert.ToInt16(db.ExecuteScalar(CmdConfig));

                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.Succeed,
                                },
                                new ListDictionary {
                                {"respnum", 1},
                                {"respmsg", "获取车次数量信息成功"},
                                {"count",count},
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
