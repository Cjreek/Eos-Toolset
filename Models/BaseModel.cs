using Eos.Nwn;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class BaseModel
    {
        public Guid ID { get; set; }
        public bool IsReadonly { get; set; } = false;
        public int? Index { get; set; }
        public String? Icon { get; set; }
    }
}
