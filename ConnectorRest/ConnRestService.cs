using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using DC;
using RestSharp;
using System.Diagnostics;
using Nini.Config;
using BizUtils.Rest;
using System.IO;
using System.Collections.Specialized;

namespace ConnectorRest
{
    public class ConnRestService
    {
        DCClient client = DCClient.Instance("ConnectorRest", "global");
        HttpListener listener = new HttpListener();

        void GetContextCallBack(IAsyncResult ar)
        {
            HttpListener listener = ar.AsyncState as HttpListener;
            listener.BeginGetContext(new AsyncCallback(GetContextCallBack), listener);
            try
            {
                HttpListenerContext context = listener.EndGetContext(ar);

                //Thread procThr = new Thread(() => procContext(context));
                //procThr.IsBackground = true;
                //procThr.Start();

                //procContext(context);
                ThreadPool.QueueUserWorkItem(o =>
                    {
                        HttpListenerContext c = o as HttpListenerContext;
                        procContext(c);
                    }, context);
            }
            catch (Exception ex)
            {
                DCLogger.LogError("listener.BeginGetContext error:{0}", ex.Message);
            }
            //finally
            //{
            //    listener.BeginGetContext(new AsyncCallback(GetContextCallBack), listener);
            //}

        }

        private void procContext(HttpListenerContext context)
        {
            // 取得请求对象
            HttpListenerRequest request = context.Request;
            // 取得回应对象
            HttpListenerResponse response = context.Response;
            try
            {
                //if (request.RawUrl.Equals("/helloworld"))
                //{
                //    byte[] ack = Encoding.UTF8.GetBytes("Hello World!");
                //    response.ContentLength64 = ack.LongLength;
                //    response.ContentType = "text/plain; charset=UTF-8";
                //    response.StatusCode = 200;
                //    // 输出回应内容
                //    using (BinaryWriter writer = new System.IO.BinaryWriter(response.OutputStream))
                //    {
                //        writer.Write(ack, 0, (int)response.ContentLength64);
                //    }

                //    return;
                //}

                //if (request.ContentLength64 < 0 || request.ContentLength64 > 20000000)
                //{
                //    string resp = string.Format("无效的Rest请求内容长度:{0}", request.ContentLength64);
                //    byte[] respbytes = Encoding.UTF8.GetBytes(resp);
                //    DCLogger.LogError(resp);

                //    // 设置回应头部内容，长度，编码
                //    response.ContentLength64 = respbytes.LongLength;
                //    response.ContentType = "text/plain; charset=UTF-8";
                //    response.StatusCode = 500;
                //    // 输出回应内容
                //    try
                //    {
                //        using (BinaryWriter writer = new BinaryWriter(response.OutputStream))
                //        {
                //            writer.Write(respbytes, 0, (int)response.ContentLength64);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        DCLogger.LogError(ex.Message);
                //    }
                //    return;
                //}

                byte[] postData = parseRequest(request);

                client.Act(request.RawUrl, postData,
                    (rst) =>
                    {
                        issueResponse(request, response, rst);
                    }
                );
            }
            catch (Exception ex)
            {
                // 设置回应头部内容，长度，编码
                byte[] respbytes = Encoding.UTF8.GetBytes(ex.Message);
                response.ContentLength64 = respbytes.LongLength;
                response.ContentType = "text/plain; charset=UTF-8";
                response.StatusCode = 500;
                // 输出回应内容
                try
                {
                    if (ex is MissingMethodException)
                    {
                        DCLogger.LogTrace(ex.Message);
                    }
                    else
                    {
                        DCLogger.LogError(ex.Message);
                    }
                    using (BinaryWriter writer = new BinaryWriter(response.OutputStream))
                    {
                        writer.Write(respbytes, 0, (int)response.ContentLength64);
                    }
                }
                catch (Exception ex2)
                {
                    DCLogger.LogError(ex2.Message);
                }

                //response.Close();
            }
        }

        static int logTxtLen = 2048;
        private static void issueResponse(HttpListenerRequest request, HttpListenerResponse response, ActResult rst)
        {
            if (rst.Exception != null)
            {
                rst.ResultData = PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = BizUtils.Rest.HttpStatusCode.ServerSideError },
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", rst.Exception},
                                    }
                            );

                DCLogger.LogError("issueResponse get a biz srv error:{0}", rst.Exception.Message);
            }

            // 设置回应头部内容，长度，编码
            int httpHeadInfoLength = BitConverter.ToInt32(rst.ResultData, 0);
            HttpHeadInfo httpHeadInfo = HttpHeadInfo.FromBytes(rst.ResultData, 4, httpHeadInfoLength);
            int rawBytesIndex = httpHeadInfoLength + 4;
            response.ContentLength64 = rst.ResultData.LongLength - rawBytesIndex;
            response.ContentType = httpHeadInfo.ContentType;
            response.StatusCode = (int)httpHeadInfo.StatusCode;
            // 输出回应内容
            try
            {
                using (BinaryWriter writer = new BinaryWriter(response.OutputStream))
                {
                    writer.Write(rst.ResultData, rawBytesIndex, (int)response.ContentLength64);
                }
            }
            catch (Exception ex)
            {
                DCLogger.LogError(ex.Message);
            }

            if (response.ContentType.Equals(HttpContentType.Json))
            {
                if (response.ContentLength64 > logTxtLen)
                {
                    if (response.StatusCode == (int)BizUtils.Rest.HttpStatusCode.Succeed)
                    {
                        DCLogger.LogTrace(string.Format(
                            "{0}<--{1}:::{2}",
                            request.RemoteEndPoint,
                            request.RawUrl,
                            Global.Encoding.GetString(rst.ResultData, rawBytesIndex, logTxtLen) + "..."
                            )
                            );
                    }
                    else
                    {
                        DCLogger.LogWarn(string.Format(
                            "{0}<--{1}:::{2}",
                            request.RemoteEndPoint,
                            request.RawUrl,
                            Global.Encoding.GetString(rst.ResultData, rawBytesIndex, logTxtLen) + "..."
                            )
                            );
                    }
                }
                else
                {
                    if (response.StatusCode == (int)BizUtils.Rest.HttpStatusCode.Succeed)
                    {
                        DCLogger.LogTrace(string.Format(
                           "{0}<--{1}:::{2}",
                           request.RemoteEndPoint,
                           request.RawUrl,
                           Global.Encoding.GetString(rst.ResultData, rawBytesIndex, (int)response.ContentLength64)
                           )
                           );
                    }
                    else
                    {
                        DCLogger.LogWarn(string.Format(
                           "{0}<--{1}:::{2}",
                           request.RemoteEndPoint,
                           request.RawUrl,
                           Global.Encoding.GetString(rst.ResultData, rawBytesIndex, (int)response.ContentLength64)
                           )
                           );
                    }
                }
            }
        }

        private byte[] parseRequest(HttpListenerRequest request)
        {
            int len = (int)request.ContentLength64;
            if (!request.HasEntityBody || len <= 0)
            {
                DCLogger.LogTrace(string.Format(
                      "{0}-->{1}:::{2}",
                      request.RemoteEndPoint,
                      request.RawUrl,
                      "无POST BODY的请求."
                      )
                      );
                return new byte[0];
            }

            byte[] buffer;
            using (BinaryReader reader = new BinaryReader(request.InputStream))
            {
                buffer = reader.ReadBytes(len);
            }

            if (request.ContentLength64 > 0 && request.ContentLength64 <= 1024)
            {
                DCLogger.LogTrace(string.Format(
                    "{0}-->{1}:::{2}",
                    request.RemoteEndPoint,
                    request.RawUrl,
                    Global.Encoding.GetString(buffer)
                    )
                    );
            }
            else if (request.ContentLength64 > 1024 && request.ContentLength64 <= 4096)
            {
                DCLogger.LogTrace(string.Format(
                    "{0}-->{1}:::{2}",
                    request.RemoteEndPoint,
                    request.RawUrl,
                    Global.Encoding.GetString(buffer, 0, 1024) + "..."
                    )
                    );
            }
            else
            {
                DCLogger.LogTrace(string.Format(
                    "{0}-->{1}:::{2}",
                    request.RemoteEndPoint,
                    request.RawUrl,
                    string.Format("{0} bytes long content.", request.ContentLength64)
                    )
                    );
            }

            return buffer;
        }

        public void Start()
        {
            IConfigSource cs = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectorRest.ini"));
            // 检查系统是否支持
            if (!HttpListener.IsSupported)
            {
                DCLogger.LogError("软件运行需要 Windows XP SP2 或 Server 2003 以上系统.");
            }

            // 注意前缀必须以 / 正斜杠结尾
            string[] prefixes = cs.Configs["init"].Get("ListenPoint").Split(',');

            // 设置manifest UAC权限
            foreach (string lsnrUrl in prefixes)
            {
                AddAddress(lsnrUrl);
            }

            // 增加监听的前缀.
            foreach (string lsnrUrl in prefixes)
            {
                listener.Prefixes.Add(lsnrUrl);
            }

            //开始监听
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                DCLogger.LogError(ex.Message);
                throw ex;
            }

            foreach (string lsnrUrl in prefixes)
            {
                DCLogger.LogInfo(string.Format("REST监听启动于：{0}", lsnrUrl));

            }


            //for (int i = 0; i < 60; i++)
            {
                try
                {
                    listener.BeginGetContext(new AsyncCallback(GetContextCallBack), listener);
                }
                catch (Exception ex)
                {
                    DCLogger.LogError("listener.BeginGetContext error:{0}", ex.Message);
                }
            }

            Console.Read();
        }

        public void Stop()
        {
            try
            {
                //listener.Stop();
                listener.Close();
            }
            catch (Exception ex)
            {
                DCLogger.LogError(ex.Message);
                throw ex;
            }
        }


        void AddAddress(string address)
        {
            try
            {
                AddAddress(address, Environment.UserDomainName, Environment.UserName);
            }
            catch (Exception ex)
            {
                DCLogger.LogError(ex.Message);
            }
        }

        void AddAddress(string address, string domain, string user)
        {
            string argsDll = String.Format(@"http delete urlacl url={0}", address);
            string args = string.Format(@"http add urlacl url={0} user={1}\{2}", address, domain, user);
            ProcessStartInfo psi = new ProcessStartInfo("netsh", argsDll);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            Process.Start(psi).WaitForExit();//删除urlacl
            psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            Process.Start(psi).WaitForExit();//添加urlacl
        }
    }
}
