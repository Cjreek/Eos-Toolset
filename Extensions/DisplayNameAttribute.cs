using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Extensions
{
    internal class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string name)
        {
            this.DisplayName = name;
        }
        public String DisplayName { get; set; }
    }
}
