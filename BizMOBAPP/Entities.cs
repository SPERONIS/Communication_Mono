using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BizMOBAPP
{

    public class RegisterData
    {
        public string MobileNumber { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class ClientLogin
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
    }

}
