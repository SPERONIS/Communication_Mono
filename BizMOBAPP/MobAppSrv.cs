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

namespace BizMOBAPP
{
    internal class MobAppSrv
    {
        private DCClient clt;
        private string dbConnStr = "server=localhost;user id=root;password=9143;database=nxkj;DataProvider=MySql;";
        DbNetData db = null;

        internal MobAppSrv()
        { }

        internal void Start()
        {
            IConfigSource cs =
                new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BizMOBAPP.ini"));
            IConfig c = cs.Configs["config"];
            dbConnStr = c.Get("DbConnectionString");
            db = new DbNetData(dbConnStr);
            db.CloseConnectionOnError = false;
            db.Open();

            clt = DCClient.Instance("BizMOBAPP", "global");
            clt.Srv("/mobapp/manage/register", manage_register);
            clt.Srv("/mobapp/manage/login", manage_login);

        }

        internal void Stop()
        {
            db.Dispose();
        }

        #region 移动端管理部分

        private byte[] manage_register(string extInfo, byte[] req)
        {
            RegisterData para;
            byte[] checkResult = PackOrb.CheckRequest<RegisterData>(req, out para,
                new string[]{
                    "MobileNumber",
                    "Account",
                    "Password",
                });
            if (checkResult != null)
            {
                return checkResult;
            }
            if (!Regex.IsMatch(para.Account, "^[A-Za-z][A-Za-z0-9_]*$"))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                },
                                new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", "帐号必须以英文字母开始，且只能由英文、数字和下划线组成"},
                                }
                            );
            }
            if (!Regex.IsMatch(para.MobileNumber, "^[0-9]*$"))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                },
                                new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", "手机号必须以数字组成"},
                                }
                            );
            }

            #region 保存register信息

            long ret = 0;
            string mb_guid = Guid.NewGuid().ToString("N");
            try
            {
                QueryCommandConfig QryCmdConfig = new QueryCommandConfig("select count(*) from t_mobapp_reginfo where MobileNumber = @MobileNumber or Account = @Account");
                QryCmdConfig.Params["MobileNumber"] = para.MobileNumber;
                QryCmdConfig.Params["Account"] = para.Account;
                object ob = db.ExecuteScalar(QryCmdConfig);
                if (ob != null && Int32.Parse(ob.ToString()) > 0)
                {
                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                    },
                                    new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", "该手机号或帐号已注册"},
                                }
                                );
                }
                CommandConfig CmdConfig = new CommandConfig("t_mobapp_reginfo");
                CmdConfig.Params["mb_guid"] = mb_guid;
                CmdConfig.Params["MobileNumber"] = para.MobileNumber;
                CmdConfig.Params["Account"] = para.Account;
                CmdConfig.Params["Password"] = para.Password;
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
                            {"respmsg", "用户注册成功"},
                            {"mb_guid", mb_guid},
                            }
                        );

            #endregion
        }

        private byte[] manage_login(string extInfo, byte[] req)
        {
            ClientLogin para;
            byte[] checkResult = PackOrb.CheckRequest<ClientLogin>(req, out para,
                new string[]{
                    "LoginName",
                    "Password",
                });
            if (checkResult != null)
            {
                return checkResult;
            }

            #region 登录验证

            try
            {
                string loginCol = "Account";
                if (Regex.IsMatch(para.LoginName, "^[0-9]*$"))
                {
                    loginCol = "MobileNumber";
                }
                QueryCommandConfig CmdConfig = new QueryCommandConfig(string.Format("select mb_guid,Password from t_mobapp_reginfo where {0} = @LoginName", loginCol));
                CmdConfig.Params["LoginName"] = para.LoginName;
                DataTable dt = db.GetDataTable(CmdConfig);
                if (dt.Rows.Count == 0)
                {
                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                    },
                                    new ListDictionary {
                                {"respnum", -1},
                                {"respmsg", "未找到此用户"},
                                }
                                );
                }
                if (dt.Rows[0]["Password"].ToString().Equals(para.Password))
                {
                    return PackOrb.PackRespose(
                                    new HttpHeadInfo
                                    {
                                        StatusCode = HttpStatusCode.Succeed,
                                    },
                                    new ListDictionary {
                                {"respnum", 1},
                                {"respmsg", "用户登录成功"},
                                {"mb_guid", dt.Rows[0]["mb_guid"].ToString()},
                                }
                                );
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
                                {"respmsg", "密码错误"},
                                }
                                );
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

            #endregion
        }

        #endregion

    }
}
