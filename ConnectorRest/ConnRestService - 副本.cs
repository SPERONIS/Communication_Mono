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

namespace ConnectorRest
{
    public class ConnRestService
    {
        private Thread lsnrThrd;
        //private string lsnrUrl = "http://+:8080/";
        //private string localMngPort = "8090";

        public void Start()
        {
            IConfigSource cs = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectorRest.ini"));
            //lsnrUrl = cs.Configs["init"].Get("ListenPoint");
            //localMngPort = cs.Configs["init"].Get("LocalManagePort");

            lsnrThrd = new Thread(new ThreadStart(delegate()
            {
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

                // 创建监听器.
                HttpListener listener = new HttpListener();
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
                DCClient client = DCClient.Instance("ConnectorRest", "global");
                long length = 0;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    try
                    {
                        // 注意: GetContext 方法将阻塞线程，直到请求到达
                        HttpListenerContext context = listener.GetContext();
                        // 取得请求对象
                        HttpListenerRequest request = context.Request;

                        length += 1;

                        if (watch.ElapsedMilliseconds > 10000)
                        {
                            DCLogger.LogInfo(string.Format("Connect Pressure:{0}requests/s\t{1}", length * 1000 / (watch.ElapsedMilliseconds), DateTime.Now));
                            length = 0;
                            watch.Reset();
                            watch.Start();
                        }

                        //if (request.RawUrl.Equals("/connector/stop"))
                        //    break;
                        if (request.RawUrl.Equals("/helloworld"))
                        {
                            // 取得回应对象
                            HttpListenerResponse response = context.Response;
                            byte[] ack = Encoding.UTF8.GetBytes("Hello World!");
                            response.ContentLength64 = ack.LongLength;
                            response.ContentType = "text/plain; charset=UTF-8";
                            response.StatusCode = 200;
                            // 输出回应内容
                            try
                            {
                                using (BinaryWriter writer = new System.IO.BinaryWriter(response.OutputStream))
                                {
                                    writer.Write(ack, 0, (int)response.ContentLength64);
                                }
                            }
                            catch (Exception ex)
                            {
                                DCLogger.LogError(ex.Message);
                            }
                            continue;
                        }

                        if (request.ContentLength64 < 0 || request.ContentLength64 > 20000000)
                        {
                            string resp = string.Format("超长的Rest请求内容，长度{0}", request.ContentLength64);
                            DCLogger.LogError(resp);

                            // 取得回应对象
                            HttpListenerResponse response = context.Response;

                            // 设置回应头部内容，长度，编码
                            response.ContentLength64 = resp.Length;
                            response.ContentType = "text/plain; charset=UTF-8";
                            response.StatusCode = 500;
                            // 输出回应内容
                            try
                            {
                                using (BinaryWriter writer = new BinaryWriter(response.OutputStream))
                                {
                                    writer.Write(Encoding.UTF8.GetBytes(resp));
                                }
                            }
                            catch (Exception ex)
                            {
                                DCLogger.LogError(ex.Message);
                            }
                            continue;
                        }

                        byte[] postData = getPostData(request);
                        if (request.ContentLength64 > 0 && request.ContentLength64 <= 256)
                        {
                            DCLogger.LogTrace(string.Format(
                                "{0}-->{1}:::{2}",
                                request.RemoteEndPoint,
                                request.RawUrl,
                                Global.Encoding.GetString(postData)
                                )
                                );
                        }
                        else if (request.ContentLength64 > 256 && request.ContentLength64 <= 4096)
                        {
                            DCLogger.LogTrace(string.Format(
                                "{0}-->{1}:::{2}",
                                request.RemoteEndPoint,
                                request.RawUrl,
                                Global.Encoding.GetString(postData, 0, 256) + "..."
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
                        //if (postData == null)
                        //{
                        //    HttpListenerResponse response = context.Response;
                        //    response.StatusCode = 404;
                        //    response.Close();
                        //}
                        //else
                        {
                            try
                            {
                                client.Act(request.RawUrl, postData, (rst) =>
                                {
                                    // 取得回应对象
                                    HttpListenerResponse response = context.Response;

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
                                        using (BinaryWriter writer = new System.IO.BinaryWriter(response.OutputStream))
                                        {
                                            writer.Write(rst.ResultData, rawBytesIndex, (int)response.ContentLength64);
                                        }

                                        if (response.ContentType.Equals(HttpContentType.Json))
                                        {
                                            if (response.ContentLength64 > 256)
                                            {
                                                DCLogger.LogTrace(string.Format(
                                                    "{0}<--{1}:::{2}",
                                                    request.RemoteEndPoint,
                                                    request.RawUrl,
                                                    Global.Encoding.GetString(rst.ResultData, rawBytesIndex, 256) + "..."
                                                    )
                                                    );
                                            }
                                            else
                                            {
                                                DCLogger.LogTrace(string.Format(
                                                    "{0}<--{1}:::{2}",
                                                    request.RemoteEndPoint,
                                                    request.RawUrl,
                                                    Global.Encoding.GetString(rst.ResultData, rawBytesIndex, (int)response.ContentLength64)
                                                    )
                                                    );
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        DCLogger.LogError(ex.Message);
                                    }
                                }
                                );
                            }
                            catch (Exception ex)
                            {
                                DCLogger.LogError(ex.Message);

                                // 取得回应对象
                                HttpListenerResponse response = context.Response;

                                // 设置回应头部内容，长度，编码
                                response.ContentLength64 = ex.Message.Length;
                                response.ContentType = "text/plain; charset=UTF-8";
                                response.StatusCode = 500;
                                // 输出回应内容
                                try
                                {
                                    using (BinaryWriter writer = new BinaryWriter(response.OutputStream))
                                    {
                                        writer.Write(Encoding.UTF8.GetBytes(ex.Message));
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    DCLogger.LogError(ex2.Message);
                                }

                                //response.Close();
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch
                    { }

                }

                // 关闭服务器
                listener.Stop();
                DCLogger.LogInfo(string.Format("REST监听关闭：{0}", prefixes));
                DCClient.Dispose(client);
            }
            ));
            lsnrThrd.IsBackground = true;
            lsnrThrd.Start();
        }

        public void Stop()
        {
            //var client = new RestClient(string.Format("http://localhost:{0}", localMngPort));
            //var request = new RestRequest("/connector/stop", Method.POST);
            //client.Execute(request);
            if (lsnrThrd != null && lsnrThrd.IsAlive)
                lsnrThrd.Abort();
        }

        private byte[] getPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                //DCLogger.LogInfo("无POST BODY的请求.");
                return new byte[0];
            }

            int len = (int)request.ContentLength64;
            if (len <= 0)
            {
                return new byte[0];
            }

            byte[] buffer;
            using (BinaryReader reader = new BinaryReader(request.InputStream))
            {
                buffer = reader.ReadBytes(len);
            }
            //body.Read(buffer, 0, len);
            //body.Close();
            //File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "test.db"), buffer);
            return buffer;
            //System.Text.Encoding encoding = request.ContentEncoding;
            //System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            //string s = reader.ReadToEnd();
            //body.Close();
            //reader.Close();

            //return encoding.GetBytes(s);
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
