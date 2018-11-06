using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Quartz;
using Quartz.Impl;
using BizUtils.Data;
using Nini.Config;
using System.Data;

namespace ConsoleJob
{
    public class JobService
    {
        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

        public void Start()
        {
            try
            {
                scheduler.Start();
                DC.DCLogger.LogInfo("Job Scheduler started...");

                IJobDetail Job_GenerateXsmxKilo = JobBuilder.Create<Job_GenerateXsmxKilo>()
                    .WithIdentity("Job_GenerateXsmxKilo", "group1")
                    .Build();

                IJobDetail Job_CalLkgh = JobBuilder.Create<Job_CalLkgh>()
                    .WithIdentity("Job_CalLkgh", "group1")
                    .Build();

                IJobDetail Job_CalXsgh = JobBuilder.Create<Job_CalXsgh>()
                    .WithIdentity("Job_CalXsgh", "group1")
                    .Build();

                IJobDetail Job_GenerateLkBusiness = JobBuilder.Create<Job_GenerateLkBusiness>()
                    .WithIdentity("Job_GenerateLkBusiness", "group1")
                    .Build();

                IJobDetail Job_GenerateXsBusiness = JobBuilder.Create<Job_GenerateXsBusiness>()
                    .WithIdentity("Job_GenerateXsBusiness", "group1")
                    .Build();

                IJobDetail Job_DelMarketVeghidden = JobBuilder.Create<Job_DelMarketVeghidden>()
                    .WithIdentity("Job_DelMarketVeghidden", "group1")
                    .Build();

                IJobDetail Job_AddAdvReadcount = JobBuilder.Create<Job_AddAdvReadcount>()
                    .WithIdentity("Job_AddAdvReadcount", "group1")
                    .Build();

                ITrigger trigger1 = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0/15 * * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                ITrigger trigger2 = TriggerBuilder.Create()
                    .WithIdentity("trigger2", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0/15 * * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                ITrigger trigger3 = TriggerBuilder.Create()
                    .WithIdentity("trigger3", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0/15 * * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                ITrigger trigger4 = TriggerBuilder.Create()
                    .WithIdentity("trigger4", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0/15 * * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                ITrigger trigger5 = TriggerBuilder.Create()
                    .WithIdentity("trigger5", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0/15 * * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                ITrigger trigger6 = TriggerBuilder.Create()
                    .WithIdentity("trigger6", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0 1 * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                ITrigger trigger7 = TriggerBuilder.Create()
                    .WithIdentity("trigger7", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0 9-21 * * ?")
                    //.WithSimpleSchedule(x => x
                    //    .WithIntervalInSeconds(10)
                    //    .RepeatForever())
                    .Build();

                scheduler.ScheduleJob(Job_GenerateXsmxKilo, trigger1);
                scheduler.ScheduleJob(Job_CalLkgh, trigger2);
                scheduler.ScheduleJob(Job_CalXsgh, trigger5);
                scheduler.ScheduleJob(Job_GenerateLkBusiness, trigger3);
                scheduler.ScheduleJob(Job_GenerateXsBusiness, trigger4);
                scheduler.ScheduleJob(Job_DelMarketVeghidden, trigger6);
                scheduler.ScheduleJob(Job_AddAdvReadcount, trigger7);
            }
            catch (SchedulerException se)
            {
                DC.DCLogger.LogError("Job Scheduler started failed.{0}", se);
            }
        }

        public void Stop()
        {
            try
            {
                scheduler.Shutdown();
                DC.DCLogger.LogInfo("Job Scheduler stopped...");
            }
            catch (SchedulerException se)
            {
                DC.DCLogger.LogError("Job Scheduler stopped failed.{0}", se);
            }
        }
    }

    public class TestJob1 : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("TestJob1:{0}", DateTime.Now);
        }
    }

    public class TestJob2 : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("TestJob2:{0}", DateTime.Now);
        }
    }

    public class Job_AddAdvReadcount : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();

                QueryCommandConfig qry = new QueryCommandConfig("select adv_id,adv_date,adv_cate,attachments from t_adv_vegsad where adv_date > @startdate and approved = 1 and dealed != 1");
                qry.Params["startdate"] = DateTime.Now.AddDays(-4);
                DataTable dt = db.GetDataTable(qry);

                if (dt.Rows.Count > 0)
                {
                    db.BeginTransaction();
                    Random ran = new Random();
                    //do market_xs
                    foreach (DataRow r in dt.Rows)
                    {
                        CommandConfig updCmd = new CommandConfig("update t_adv_vegsad set readcount_fake = readcount_fake + @add where adv_cate=@adv_cate and adv_id=@adv_id and adv_date=@adv_date");
                        updCmd.Params["adv_cate"] = r["adv_cate"];
                        updCmd.Params["adv_id"] = r["adv_id"];
                        updCmd.Params["adv_date"] = r["adv_date"];
                        if (r["attachments"] != DBNull.Value && r["attachments"].ToString().Length > 0)
                        {
                            updCmd.Params["add"] = ran.Next(4);
                        }
                        else
                        {
                            updCmd.Params["add"] = ran.Next(2);
                        }
                        db.ExecuteNonQuery(updCmd);
                    }

                    db.Commit();
                }

                DC.DCLogger.LogTrace("Job successed [Job_AddAdvReadcount]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_AddAdvReadcount]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }

    public class Job_DelMarketVeghidden : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();
                db.BeginTransaction();

                //do market_xs
                QueryCommandConfig XsQry = new QueryCommandConfig("select sn,orgs,veg_hidden from t_dic_market_xs where orgs is not null and orgs !='' and veg_hidden is not null and veg_hidden !=''");
                DataTable xsDt = db.GetDataTable(XsQry);
                foreach (DataRow r in xsDt.Rows)
                {
                    List<string> orgs = new List<string>(Convert.ToString(r["orgs"]).Split(','));
                    List<string> veg_hidden = new List<string>(Convert.ToString(r["veg_hidden"]).Split(','));
                    List<string> veg_hidden_new = new List<string>(veg_hidden);

                    foreach (string veg in veg_hidden)
                    {
                        QueryCommandConfig xsmxQry = new QueryCommandConfig(string.Format("select count(*) from t_xs_wdat_xsmx where recdate >= @startdate and vegname = @vegname and xs_guid in ({0})", "'" + string.Join("','", orgs) + "'"));
                        xsmxQry.Params["startdate"] = DateTime.Now.AddDays(-3);
                        xsmxQry.Params["vegname"] = veg;

                        int count = Convert.ToInt32(db.ExecuteScalar(xsmxQry));
                        if (count > 0)
                        {
                            veg_hidden_new.Remove(veg);
                        }
                    }

                    if (veg_hidden.Count > veg_hidden_new.Count)
                    {
                        UpdateCommandConfig upd = new UpdateCommandConfig("t_dic_market_xs");
                        upd.FilterParams["sn"] = r["sn"];

                        upd.Params["veg_hidden"] = string.Join(",", veg_hidden_new);

                        db.ExecuteUpdate(upd);
                    }
                }

                //do market_lk
                QueryCommandConfig LkQry = new QueryCommandConfig("select sn,orgs,veg_hidden from t_dic_market_lk where orgs is not null and orgs !='' and veg_hidden is not null and veg_hidden !=''");
                DataTable lkDt = db.GetDataTable(LkQry);
                foreach (DataRow r in lkDt.Rows)
                {
                    List<string> orgs = new List<string>(Convert.ToString(r["orgs"]).Split(','));
                    List<string> veg_hidden = new List<string>(Convert.ToString(r["veg_hidden"]).Split(','));
                    List<string> veg_hidden_new = new List<string>(veg_hidden);

                    foreach (string veg in veg_hidden)
                    {
                        QueryCommandConfig xsmxQry = new QueryCommandConfig(string.Format("select count(*) from t_lkdesk_wdat_jhd where Date >= @startdate and MerchandiseName = @vegname and lk_guid in ({0})", "'" + string.Join("','", orgs) + "'"));
                        xsmxQry.Params["startdate"] = DateTime.Now.AddDays(-3);
                        xsmxQry.Params["vegname"] = veg;

                        int count = Convert.ToInt32(db.ExecuteScalar(xsmxQry));
                        if (count > 0)
                        {
                            veg_hidden_new.Remove(veg);
                        }
                    }

                    if (veg_hidden.Count > veg_hidden_new.Count)
                    {
                        UpdateCommandConfig upd = new UpdateCommandConfig("t_dic_market_lk");
                        upd.FilterParams["sn"] = r["sn"];

                        upd.Params["veg_hidden"] = string.Join(",", veg_hidden_new);

                        db.ExecuteUpdate(upd);
                    }
                }

                db.Commit();

                DC.DCLogger.LogTrace("Job successed [Job_DelMarketVeghidden]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_DelMarketVeghidden]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }

    public class Job_GenerateXsmxKilo : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();
                db.BeginTransaction();

                Dictionary<string, VegunitTrans> vtDic = new Dictionary<string, VegunitTrans>();
                QueryCommandConfig XsQry = new QueryCommandConfig("select * from t_xs_wdat_xsmx where recdate >= @startdate and recdate <= @enddate and amount=kilo");
                XsQry.Params["startdate"] = DateTime.Now.AddDays(-30);
                XsQry.Params["enddate"] = DateTime.Now;
                DataTable xsDt = db.GetDataTable(XsQry);

                foreach (DataRow r in xsDt.Rows)
                {
                    string xs_guid = Convert.ToString(r["xs_guid"]);
                    if (!vtDic.ContainsKey(xs_guid))
                        vtDic.Add(xs_guid, new VegunitTrans(null, xs_guid.ToString(), dbp));
                    VegunitTrans vt = vtDic[xs_guid];

                    UpdateCommandConfig upd = new UpdateCommandConfig("t_xs_wdat_xsmx");
                    upd.FilterParams["recid"] = r["recid"];
                    upd.FilterParams["recdate"] = r["recdate"];
                    upd.FilterParams["gs_guid"] = r["gs_guid"];
                    upd.FilterParams["xs_guid"] = r["xs_guid"];
                    upd.FilterParams["vegname"] = r["vegname"];

                    upd.Params["kilo"] = vt.CalWeight(Convert.ToString(r["vegname"]), Convert.ToDecimal(r["amount"]));

                    db.ExecuteUpdate(upd);
                }
                db.Commit();

                DC.DCLogger.LogTrace("Job successed [Job_GenerateXsmxKilo]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_GenerateXsmxKilo]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }

    public class Job_CalLkgh : IJob
    {
        public class ResponseData
        {
            public decimal max { get; set; }
            public decimal min { get; set; }
        }

        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();
                db.BeginTransaction();

                Dictionary<string, VegunitTrans> vtDic = new Dictionary<string, VegunitTrans>();
                QueryCommandConfig lkkcQry = new QueryCommandConfig("select * from t_lkdesk_wdat_kc where MerchandiseTypeID = 1");
                DataTable lkkcDt = db.GetDataTable(lkkcQry);

                foreach (DataRow r in lkkcDt.Rows)
                {
                    string statSql = "SELECT vegname,date_format(recdate, '%Y-%m-%d') as date,round(max(totalprice/kilo),3) as max,round(min(totalprice/kilo),3) as min,round(avg(totalprice/kilo),3) as avg FROM t_xs_wdat_xsmx where recdate >= @startdate and recdate < @enddate and amount != kilo and vegname = @vegname and xs_guid in ({0}) group by vegname,date_format(recdate, '%Y-%m-%d')";
                    QueryCommandConfig xss = new QueryCommandConfig("select xs_guid from t_lk_chcfg where lk_guid = @lk_guid");
                    xss.Params["lk_guid"] = r["lk_guid"];
                    DataTable xsDt = dbp.GetDataTable(xss);
                    var xsEnum = from xs in xsDt.AsEnumerable() select xs.Field<string>("xs_guid");

                    QueryCommandConfig statQry = new QueryCommandConfig(string.Format(statSql, "'" + string.Join("','", xsEnum) + "'"));
                    statQry.Params["startdate"] = DateTime.Now.Date.AddMonths(-6);
                    statQry.Params["enddate"] = DateTime.Now.Date.AddDays(1);
                    statQry.Params["vegname"] = r["MerchandiseName"];
                    DataTable statDt = dbp.GetDataTable(statQry);
                    if (statDt.Rows.Count > 0)
                    {
                        var result =
                              from statTbl in statDt.AsEnumerable()
                              orderby DateTime.Parse(statTbl.Field<string>("date")) descending
                              where DateTime.Parse(statTbl.Field<string>("date")) <= DateTime.Now.Date.AddDays(1)
                              select new ResponseData
                              {
                                  max = statTbl.Field<decimal?>("max") ?? 0,
                                  min = statTbl.Field<decimal?>("min") ?? 0,
                              };
                        decimal min = 0;
                        decimal max = 0;
                        foreach (ResponseData d in result)
                        {
                            if (d.max > 0 || d.min > 0)
                            {
                                min = d.min;
                                max = d.max;
                                break;
                            }
                        }
                        DataRow statR = statDt.Rows[0];
                        UpdateCommandConfig lkkcUpd = new UpdateCommandConfig("t_lkdesk_wdat_kc");
                        lkkcUpd.FilterParams["lk_guid"] = r["lk_guid"];
                        lkkcUpd.FilterParams["InventoryID"] = r["InventoryID"];
                        lkkcUpd.Params["xs_price_min"] = min;
                        lkkcUpd.Params["xs_price_max"] = max;
                        db.ExecuteUpdate(lkkcUpd);
                    }
                }
                db.Commit();

                DC.DCLogger.LogTrace("Job successed [Job_CalLkgh]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_CalLkgh]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }

    public class Job_CalXsgh : IJob
    {
        public class ResponseData
        {
            public decimal max { get; set; }
            public decimal min { get; set; }
        }

        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();
                db.BeginTransaction();

                Dictionary<string, VegunitTrans> vtDic = new Dictionary<string, VegunitTrans>();
                QueryCommandConfig xskcQry = new QueryCommandConfig("select * from t_xs_wdat_kc");
                DataTable xskcDt = db.GetDataTable(xskcQry);

                foreach (DataRow r in xskcDt.Rows)
                {
                    string statSql = "SELECT vegname,date_format(recdate, '%Y-%m-%d') as date,round(max(totalprice/kilo),3) as max,round(min(totalprice/kilo),3) as min,round(avg(totalprice/kilo),3) as avg FROM t_xs_wdat_xsmx where recdate >= @startdate and recdate < @enddate and amount != kilo and vegname = @vegname and xs_guid in ({0}) group by vegname,date_format(recdate, '%Y-%m-%d')";

                    QueryCommandConfig statQry = new QueryCommandConfig(string.Format(statSql, "'" + r["xs_guid"] + "'"));
                    statQry.Params["startdate"] = DateTime.Now.Date.AddMonths(-6);
                    statQry.Params["enddate"] = DateTime.Now.Date.AddDays(1);
                    statQry.Params["vegname"] = r["vegname"];
                    DataTable statDt = dbp.GetDataTable(statQry);
                    if (statDt.Rows.Count > 0)
                    {
                        var result =
                              from statTbl in statDt.AsEnumerable()
                              orderby DateTime.Parse(statTbl.Field<string>("date")) descending
                              where DateTime.Parse(statTbl.Field<string>("date")) <= DateTime.Now.Date.AddDays(1)
                              select new ResponseData
                              {
                                  max = statTbl.Field<decimal?>("max") ?? 0,
                                  min = statTbl.Field<decimal?>("min") ?? 0,
                              };
                        decimal min = 0;
                        decimal max = 0;
                        foreach (ResponseData d in result)
                        {
                            if (d.max > 0 || d.min > 0)
                            {
                                min = d.min;
                                max = d.max;
                                break;
                            }
                        }
                        DataRow statR = statDt.Rows[0];
                        UpdateCommandConfig xskcUpd = new UpdateCommandConfig("t_xs_wdat_kc");
                        xskcUpd.FilterParams["xs_guid"] = r["xs_guid"];
                        xskcUpd.FilterParams["vegname"] = r["vegname"];
                        xskcUpd.Params["xs_price_min"] = min;
                        xskcUpd.Params["xs_price_max"] = max;
                        db.ExecuteUpdate(xskcUpd);
                    }
                }
                db.Commit();

                DC.DCLogger.LogTrace("Job successed [Job_CalXsgh]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_CalXsgh]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }

    public class Job_GenerateLkBusiness : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();
                db.BeginTransaction();

                QueryCommandConfig lkQry = new QueryCommandConfig("select org_guid from t_org_reginfo where org_type =2");
                DataTable lkDt = db.GetDataTable(lkQry);

                foreach (DataRow r in lkDt.Rows)
                {
                    QueryCommandConfig vegQry = new QueryCommandConfig("select MerchandiseName,Amount from t_lkdesk_wdat_kc where lk_guid = @lk_guid and MerchandiseTypeID = 1");
                    vegQry.Params["lk_guid"] = r["org_guid"];
                    DataTable vegDt = dbp.GetDataTable(vegQry);

                    if (vegDt.Rows.Count > 0)
                    {
                        var vegEnum = from veg in vegDt.AsEnumerable() orderby veg.Field<decimal>("Amount") descending select veg.Field<string>("MerchandiseName");
                        UpdateCommandConfig lkkcUpd = new UpdateCommandConfig("t_org_reginfo");
                        lkkcUpd.FilterParams["org_guid"] = r["org_guid"];
                        lkkcUpd.Params["business"] = string.Join("|", vegEnum);
                        db.ExecuteUpdate(lkkcUpd);
                    }
                }
                db.Commit();

                DC.DCLogger.LogTrace("Job successed [Job_GenerateLkBusiness]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_GenerateLkBusiness]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }

    public class Job_GenerateXsBusiness : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            IConfig c = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleJob.ini")).Configs["config"];
            DbNetDataProxy dbp = new DbNetDataProxy(c.Get("DbConnectionString"));
            DbNetData db = null;
            try
            {
                db = dbp.GetDb();
                db.BeginTransaction();

                QueryCommandConfig xsQry = new QueryCommandConfig("select org_guid from t_org_reginfo where org_type =3");
                DataTable xsDt = db.GetDataTable(xsQry);

                foreach (DataRow r in xsDt.Rows)
                {
                    QueryCommandConfig vegQry = new QueryCommandConfig("select vegname,amount from t_xs_wdat_kc where xs_guid = @xs_guid");
                    vegQry.Params["xs_guid"] = r["org_guid"];
                    DataTable vegDt = dbp.GetDataTable(vegQry);

                    if (vegDt.Rows.Count > 0)
                    {
                        var vegEnum = from veg in vegDt.AsEnumerable() orderby veg.Field<decimal>("amount") descending select veg.Field<string>("vegname");
                        UpdateCommandConfig xskcUpd = new UpdateCommandConfig("t_org_reginfo");
                        xskcUpd.FilterParams["org_guid"] = r["org_guid"];
                        xskcUpd.Params["business"] = string.Join("|", vegEnum);
                        db.ExecuteUpdate(xskcUpd);
                    }
                }
                db.Commit();

                DC.DCLogger.LogTrace("Job successed [Job_GenerateXsBusiness]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [Job_GenerateXsBusiness]:{0}", ex);
            }
            finally
            {
                if (db.Transaction != null)
                    db.Transaction.Dispose();
                dbp.PutDb(db);
            }
        }
    }
}
