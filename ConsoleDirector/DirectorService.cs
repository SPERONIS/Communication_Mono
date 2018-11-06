using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DC;
using System.Collections.Concurrent;
using System.Threading;

namespace ConsoleDirector
{
    public class DirectorService
    {
        DCClient client;
        bool disposed = false;
        Thread mngThread;

        public void Start()
        {
            client = DCClient.Instance(Global.DC_DIR_SRVID, "global");
            client.Srv(Global.DC_DIR_CLTSRVINFO_RPT, this.updateCltSrvInfo);
            client.Srv(Global.DC_DIR_CLTSRVINFO_QRY, this.queryCltSrvInfo);

            mngThread = new Thread(new ThreadStart(delegate()
            {
                while (!disposed)
                {
                    try
                    {
                        Thread.Sleep(8000);
                    }
                    catch
                    {
                        break;
                    }

                    IEnumerator<List<StringAndTime>> srvInfoItor = cltSrvInfo.Values.GetEnumerator();
                    while (srvInfoItor.MoveNext())
                    {
                        srvInfoItor.Current.RemoveAll((item) => { return (DateTime.Now.Ticks - item.LastRefresh) > (60 * 10000000); });
                    }
                }
            }
            ));
            mngThread.IsBackground = true;
            mngThread.Start();
        }

        public void Stop()
        {
            if (client != null && client.Disposed == false)
            {
                DCClient.Dispose(client);
            }
            disposed = true;
            mngThread.Interrupt();
        }

        private ConcurrentDictionary<string, List<StringAndTime>> cltSrvInfo = new ConcurrentDictionary<string, List<StringAndTime>>();
        private byte[] updateCltSrvInfo(string extInfo, byte[] infoData)
        {
            List<CltSrvInfo> infos = DCSerializer.BytesToObj<List<CltSrvInfo>>(infoData);
            foreach (CltSrvInfo info in infos)
            {
                StringAndTime tnt = new StringAndTime { UniqueString = info.ClientId, LastRefresh = DateTime.Now.Ticks };
                List<StringAndTime> val = new List<StringAndTime>();
                val.Add(tnt);
                cltSrvInfo.AddOrUpdate(
                    info.ClientSrvType,
                    val,
                    (key, oldValue) =>
                    {
                        if (oldValue.Contains(tnt))
                        {
                            oldValue[oldValue.IndexOf(tnt)].LastRefresh = DateTime.Now.Ticks;
                        }
                        else
                        {
                            oldValue.Add(tnt);
                            //Console.WriteLine("List<TypeAndTime>.Count: {0}", oldValue.Count);
                        }
                        //foreach (TypeAndTime t in oldValue)
                        //{
                        //    Console.WriteLine("{0}  {1}", t.TypeString, t.LastRefresh);
                        //}
                        //Console.WriteLine("------------------------------------------");
                        return oldValue;
                    }
                    );
            }

            return new byte[0];
        }

        private byte[] queryCltSrvInfo(string extInfo, byte[] infoData)
        {
            string qryType = Global.Encoding.GetString(infoData);
            List<StringAndTime> cltIds;
            if (cltSrvInfo.TryGetValue(qryType, out cltIds))
            {
                DCLogger.LogTrace("{0} has {1} srv in time", qryType, cltIds.Count);
                //if (cltIds.Count > 0)
                //{
                //    return Global.Encoding.GetBytes(cltIds[new Random().Next(0, cltIds.Count)].UniqueString);
                //}
                if (cltIds.Count > 0)
                {
                    return DCSerializer.ObjToBytes(cltIds);
                }
            }
            return new byte[0];
        }
    }
}
