using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DC
{
    public class StringAndTime
    {
        public long LastRefresh { get; set; }

        public string UniqueString { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is StringAndTime))
                return false;
            return this.UniqueString.Equals((obj as StringAndTime).UniqueString);
        }

        public override int GetHashCode()
        {
            return this.UniqueString.GetHashCode();
        }
    }
}
