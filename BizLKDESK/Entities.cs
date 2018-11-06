using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BizLKDESK
{
    public class Stat1Info
    {
        public string MerchandiseName { get; set; }
        public decimal KcAmount { get; set; }
        public decimal JhdSumAmount { get; set; }
        public decimal JhdSumPrices { get; set; }
        public decimal JhdAvgPrices { get; set; }

        public int ChdCount { get; set; }
        public DataTable ChdData { get; set; }
    }

    public class DbBackInfo
    {
        public string lk_guid { get; set; }
        public int filelen { get; set; }
        public string filename { get; set; }
    }

    public class DbBackQry
    {
        public string lk_guid { get; set; }
        public int max { get; set; }
    }

    public class PubData
    {
        public string lk_guid { get; set; }
        //public int pubdata_type { get; set; }
        public int count { get; set; }

        public DataTable data { get; set; }
    }

    public class RegisterData
    {
        public string lk_guid { get; set; }
        public int count { get; set; }

        public DataTable data { get; set; }
    }

    public class WorkData
    {
        public string lk_guid { get; set; }
        //public int workdata_type { get; set; }
        public int count { get; set; }

        public DataTable data { get; set; }
    }

    public class DateRangeQuery
    {
        public string lk_guid { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
    }

    public class ArriveDateRangeQuery
    {
        public string lk_guid { get; set; }
        public DateTime ArriveDateMin { get; set; }
        public DateTime ArriveDateMax { get; set; }
    }

    public class DateQuery
    {
        public string lk_guid { get; set; }
        public DateTime date { get; set; }
    }

    public class ClientListQuery
    {
        public string guid { get; set; }
        public string client { get; set; }
    }

    public class BillsNumQuery
    {
        public string lk_guid { get; set; }
        public string BillsNum { get; set; }
    }

    public class TransportingNumQuery
    {
        public string lk_guid { get; set; }
        public string TransportingNum { get; set; }
    }


    public class PriceRangeQuery
    {
        public string vegname { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string area { get; set; }
    }
}
