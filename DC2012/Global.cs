using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMQ;

namespace DC
{
    public static class Global
    {
        public static Encoding Encoding = Encoding.UTF8;

        public static string DC_DIR_SRVID = "DC_DIR_SRVID";

        public static string DC_DIR_CLTSRVINFO_RPT = "DC_DIR_CLTSRVINFO_RPT";

        public static string DC_DIR_CLTSRVINFO_QRY = "DC_DIR_CLTSRVINFO_QRY";

        public static byte[] DC_NULL_SNAP = Global.Encoding.GetBytes("DC_NULL_SNAP");

        public static byte[] DC_NULL_RPLY = Global.Encoding.GetBytes("DC_NULL_RPLY");

        public static byte[] DC_RPLY_UNKNOWNREQ = Global.Encoding.GetBytes("DC_RPLY_UNKNOWNREQ");

        public static byte[] DC_RPLY_EXCEPTION = Global.Encoding.GetBytes("DC_RPLY_EXCEPTION");

        public static byte[] DC_CTRL_EXIT = Global.Encoding.GetBytes("DC_CTRL_EXIT");

        public static byte[] DC_NULL_DEST = Global.Encoding.GetBytes("DC_NULL_DEST");

        public static string DC_NULL_DEST_STR = "DC_NULL_DEST";

        public static string DC_CTRL_PING = "DC_CTRL_PING";

        public static byte[] DC_CTRL_ACK = Global.Encoding.GetBytes("DC_CTRL_ACK");

        public static byte[] DC_CTRL_HB = Global.Encoding.GetBytes("DC_CTRL_HB");

        public static byte[] NANOMSG_SPLITTER = Global.Encoding.GetBytes("$$$$");

        public static string ENVELOPE_SPLITTER = "____";

        public static string DC_HEAD_PS_PUB = "DC_HEAD_PS_PUB";

        public static string DC_HEAD_AS_ACT = "DC_HEAD_AS_ACT";

        public static string DC_HEAD_AS_RPLY = "DC_HEAD_AS_RPLY";

        public static string DC_HEAD_SR_SND = "DC_HEAD_SR_SND";

        public static string DC_HEAD_CLT_RPT = "DC_HEAD_CLT_RPT";

        public static byte[] DC_HEAD_PS_PUB_BYTES = Global.Encoding.GetBytes("DC_HEAD_PS_PUB");

        public static byte[] DC_HEAD_AS_ACT_BYTES = Global.Encoding.GetBytes("DC_HEAD_AS_ACT");

        public static byte[] DC_HEAD_AS_RPLY_BYTES = Global.Encoding.GetBytes("DC_HEAD_AS_RPLY");

        public static byte[] DC_HEAD_SR_SND_BYTES = Global.Encoding.GetBytes("DC_HEAD_SR_SND");

        public static byte[] DC_HEAD_CLT_RPT_BYTES = Global.Encoding.GetBytes("DC_HEAD_CLT_RPT");

        public static string INNER_CTRL_ADDR = "inproc://dc/innerctrl";
        public static string INNER_CTRL_MNGTHR = "inproc://dc/innerctrl/mngthr";
        public static string INNER_CTRL_PUBTHR = "inproc://dc/innerctrl/pubthr";
        public static string INNER_CTRL_REQTHR = "inproc://dc/innerctrl/reqthr";
        public static string INNER_CTRL_ACTTHR = "inproc://dc/innerctrl/actthr";
        public static string INNER_CTRL_SRVTHR = "inproc://dc/innerctrl/srvthr";
        public static string INNER_CTRL_SNDTHR = "inproc://dc/innerctrl/sndthr";
        public static string INNER_CTRL_SERVERTHR = "inproc://dc/innerctrl/serverthr";
    }
}
