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
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ContainerBiz
{
    internal class ContainerBizSrv
    {
        private DCClient clt;
        //private string dbConnStr = "server=localhost;user id=root;password=9143;database=nxkj;DataProvider=MySql;";
        //DbNetData db = null;

        internal ContainerBizSrv()
        { }

        internal void Start()
        {
            IConfigSource cs = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContainerBiz.ini"));
            IConfig c = cs.Configs["config"];
            //dbConnStr = c.Get("DbConnectionString");
            //db = new DbNetData(dbConnStr);
            //db.CloseConnectionOnError = false;
            //db.Open();

            clt = DCClient.Instance(c.Get("ServiceId"), "global");
            System.Threading.Thread.Sleep(1000);
            //clt.Srv("/sysman/dic/list", dic_list);
            Dictionary<string, IBizSrv> bizs = this.getBizs();
            foreach (string url in bizs.Keys)
            {
                //bizs[url].SetDb(db);
                //bizs[url].SetConfig(c);
                bizs[url].SetContext(new BizSrvContext(url, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContainerBiz.ini")));
                clt.Srv(url, bizs[url].RequestProc);
                DCLogger.LogInfo("注册接口{0}，来源{1}", url, bizs[url].ToString());
            }
        }

        internal void Stop()
        {
            //db.Dispose();
            DCClient.Dispose(clt);
        }

        private Dictionary<string, IBizSrv> getBizs()
        {
            Dictionary<string, IBizSrv> result = new Dictionary<string, IBizSrv>();
            //获取项目根目录下的Plugins文件夹
            string dir = Directory.GetCurrentDirectory();
            //遍历目标文件夹中包含dll后缀的文件
            foreach (var file in Directory.GetFiles(dir + @"\", "*.dll"))
            {
                try
                {
                    //加载程序集
                    var asm = Assembly.LoadFrom(file);
                    //遍历程序集中的类型
                    foreach (var type in asm.GetTypes())
                    {
                        //如果是IBizSrv接口
                        if (type.GetInterfaces().Contains(typeof(IBizSrv)))
                        {
                            //创建接口类型实例
                            var ibiz = Activator.CreateInstance(type) as IBizSrv;
                            var sign = "/" + Path.GetFileNameWithoutExtension(file).Replace('.', '/');
                            if (ibiz != null)
                            {
                                result.Add(sign, ibiz);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DCLogger.LogError("Recognise {0} failed for:{1}", file, e.Message);
                    continue;
                }
            }
            return result;
        }
    }
}
