using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DC
{
    public class ServerCfg
    {
        public string ServerR { get; set; }
        public string ServerS { get; set; }
        //public string MngAddr { get; set; }
        //public string PubAddr { get; set; }
        //public string SubAddr { get; set; }
        //public string ReqAddr { get; set; }
        //public string ActAddr { get; set; }
        //public string ActRply { get; set; }
        //public string SrvAddr { get; set; }
        //public string SrvRply { get; set; }
        //public string SndAddr { get; set; }
        //public string RcvAddr { get; set; }

        public string BindingType { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.ToString().Equals(obj.ToString()))
                return true;
            return false;
        }

        public override string ToString()
        {
            return string.Concat(
                ServerR,
                ServerS
                //MngAddr,
                //PubAddr,
                //SubAddr,
                //ReqAddr,
                //ActAddr,
                //ActRply,
                //SrvAddr,
                //SrvRply,
                //SndAddr,
                //RcvAddr
                );
        }
    }
}
