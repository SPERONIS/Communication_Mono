using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DC;
using System.Data;

namespace BizSampleSrv
{
    public class SampleSrv
    {
        private DCClient clt;
        public SampleSrv()
        { }

        public void Start()
        {
            clt = DCClient.Instance("BizSampleSrv", "global");
            //clt.Srv("/veginfo/list", veginfoList);
            clt.Srv("/test/helloworld", helloworld);
        }

        public void Stop()
        { }

        private byte[] helloworld(string extInfo, byte[] req)
        {
            byte[] bStatus = BitConverter.GetBytes(200);
            return bStatus.Concat(Encoding.UTF8.GetBytes(string.Format(
@"Hello World!
extInfo:
{0}
requestInfo:
{1}",
    extInfo, Encoding.UTF8.GetString(req)))).ToArray();
        }

        private byte[] veginfoList(string extInfo, byte[] req)
        {
            List<VegInfo> vegs = new List<VegInfo>();
            //for (int i = 1; i <= new Random().Next(2,20); i++)
            //{
            //    vegs.Add(new VegInfo { VegCode = "ACDOenFejao==/*XYZ", VegName = "萝卜", PriceToday = new Random().Next(1, 95) / 10.0f });
            //}

            DataTable dt = new DataTable();
            dt.Columns.Add("vegcode", typeof(int));
            dt.Columns.Add("VEGNAME", typeof(string));
            dt.Columns.Add("PriceToday", typeof(float));

            Random r = new Random();
            for (int i = 1; i <= 10; i++)
            {
                dt.Rows.Add(r.Next(1000, 9999), "土豆", r.Next(1, 95) / 10.0f);
            }

            //string json = ServiceStack.Text.JsonSerializer.SerializeToString <List<VegInfo>> (vegs);
            string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
            vegs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VegInfo>>(json2);
            int cnt = vegs.Count;
            byte[] bStatus = BitConverter.GetBytes(200);
            return bStatus.Concat(Encoding.UTF8.GetBytes(json2)).ToArray();

            //return Encoding.UTF8.GetBytes("Hello Rest");
        }
    }
}
