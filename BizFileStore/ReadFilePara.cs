using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BizFileStore
{
    public class ReadFilePara
    {
        public string filename { get; set; }
        public int offset { get; set; }
        public int size { get; set; }
    }
}
