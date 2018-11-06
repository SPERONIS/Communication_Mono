using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BizXSDESK
{
    public class Stat1Info
    {
        public string MerchandiseName { get; set; }
        public decimal KcLastday { get; set; }
        public decimal KcAmount { get; set; }
        public decimal KcAveragePrice { get; set; }
        public decimal DhAmount { get; set; }
        //public decimal DhNextday { get; set; }
        public decimal JhdAmount { get; set; }
        public decimal ShAmount { get; set; }
        public decimal XsmxAmount { get; set; }
        public decimal XsmxTotalPrice { get; set; }
        public decimal XsmxSalesPrice { get; set; }

        public int DhNextdayCount { get; set; }
        public DataTable DhNextdayData { get; set; }

    }

    public class Stat3Info
    {
        public decimal LkKcAmount { get; set; }
        public decimal LkChAmount { get; set; }

        public string MerchandiseName { get; set; }
        //public decimal KcLastday { get; set; }
        public decimal KcAmount { get; set; }
        public decimal KcAveragePrice { get; set; }
        public decimal DhAmount { get; set; }
        //public decimal DhNextday { get; set; }
        public decimal JhdAmount { get; set; }
        public decimal ShAmount { get; set; }
        public decimal XsmxAmount { get; set; }
        public decimal XsmxTotalPrice { get; set; }
        public decimal XsmxSalesPrice { get; set; }

        public decimal DhNextday { get; set; }

    }

    public class RegisterData
    {
        public string xs_guid { get; set; }
        public int count { get; set; }

        public DataTable data { get; set; }
    }

    public class WorkData
    {
        public string xs_guid { get; set; }
        //public int workdata_type { get; set; }
        public int count { get; set; }

        public DataTable data { get; set; }
    }

    public class DateRangeQuery
    {
        public string xs_guid { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
    }

    public class DateQuery
    {
        public string xs_guid { get; set; }
        public DateTime date { get; set; }
    }

    public class DateQueryWithLk
    {
        public string lk_guid { get; set; }
        public string xs_guid { get; set; }
        public DateTime date { get; set; }
    }


    public class XsmxQuery
    {
        public string xs_guid { get; set; }
        public string BillsNum { get; set; }
    }

    public class ClientListQuery
    {
        public string guid { get; set; }
        public string client { get; set; }
    }
}
