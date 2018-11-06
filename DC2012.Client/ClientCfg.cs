using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DC
{
    public class ClientCfg
    {
        public string ClientID { get; set; }

        public string ServerR { get; set; }
        public string ServerS { get; set; }

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
                ClientID,
                ServerR,
                ServerS
                );
        }
    }
}
