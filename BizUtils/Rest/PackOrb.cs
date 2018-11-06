using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BizUtils.Rest
{
    //HTTP状态码	描述
    //200 OK	操作成功
    //400 Bad Request	错误的请求
    //404 Not Found	文件不存在
    //413 Request Entity Too Large 请求实体过大
    //500 Internal Server Error	服务器内部错误   
    public enum HttpStatusCode
    {
        Succeed = 200,
        BadRequest = 400,
        NotFound = 404,
        TooLarge = 413,
        ServerSideError = 500,
    }

    public static class HttpContentType
    {
        public static string Json = "text/json; charset=UTF-8";
        public static string Image = "image";
        public static string Data = "application";
    }

    
    public class PackOrb
    {
        public static Encoding Encoding = Encoding.UTF8;
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        static PackOrb()
        {
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Error;
            jsonSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            jsonSettings.FloatParseHandling = FloatParseHandling.Decimal;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
        }

        public static T JsonToObj<T>(string json)
        {
           return JsonConvert.DeserializeObject<T>(json, jsonSettings);
        }

        public static string ObjToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static byte[] PackRespose(HttpHeadInfo httpHeadInfo, ListDictionary msgItems)
        {
            byte[] codeBytes = HttpHeadInfo.ToBytes(httpHeadInfo);
            byte[] rawBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgItems, jsonSettings));
            return codeBytes.Concat(rawBytes).ToArray();
        }

        public static byte[] PackRespose(HttpHeadInfo httpHeadInfo, byte[] rawBytes)
        {
            byte[] codeBytes = HttpHeadInfo.ToBytes(httpHeadInfo);
            return codeBytes.Concat(rawBytes).ToArray();
        }

        public static byte[] CheckRequest<T>(byte[] req, out T request, string[] neededItems)
        {
            Type type = typeof(T);
            request = default(T);
            ListDictionary result = new ListDictionary();

            if (req == null || req.Length == 0)
            {
                result.Add("respnum", -1);
                result.Add("respmsg", "请求参数为空");
                return PackRespose(new HttpHeadInfo(), result);
            }

            try
            {
                request = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(req), jsonSettings);
            }
            catch (Exception e)
            {
                //DC.DCLogger.LogTrace(e.Message);
                //DC.DCLogger.LogTrace(Encoding.UTF8.GetString(req));
                result.Add("respnum", -1);
                result.Add("respmsg", string.Format("请求参数无法被解析{0}", e.Message));
                return PackRespose(new HttpHeadInfo(), result);
            }

            List<string> missed = new List<string>();

            foreach (string needed in neededItems)
            {
                System.Reflection.PropertyInfo p = type.GetProperty(needed); //获取指定名称的属性
                if (p == null || p.GetValue(request, null) == null)
                {
                    missed.Add(needed);
                }
            }

            if (missed.Count > 0)
            {
                result.Add("respnum", -1);
                result.Add("respmsg", string.Format("请求参数{0}为空", string.Join(",", missed)));
                return PackRespose(new HttpHeadInfo(), result);
            }

            return null;
        }

        public static byte[] CheckPagePara(int pageno, int pagesize, out int startrow)
        {
            ListDictionary result = new ListDictionary();
            startrow = 0;

            if (pageno < 1)
            {
                result.Add("respnum", -1);
                result.Add("respmsg", string.Format("pageno参数值无效 值为{0}", pageno));
                return PackRespose(new HttpHeadInfo(), result);
            }
            if (pagesize < 1)
            {
                result.Add("respnum", -1);
                result.Add("respmsg", string.Format("pagesize参数值无效 值为{0}", pagesize));
                return PackRespose(new HttpHeadInfo(), result);
            }

            startrow = (pageno - 1) * pagesize;
            return null;
        }

        public static IDictionary<string, string>  ParaStrToDic(string extInfo)
        {
            return DC.DCUtil.ParaStrToDic(extInfo);
        }

    }
}
