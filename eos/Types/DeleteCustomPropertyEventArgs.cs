using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public class DeleteCustomPropertyEventArgs : EventArgs
    {
        public CustomObjectProperty Property { get; }
        public bool CanDelete { get; set; } = true;

        public DeleteCustomPropertyEventArgs(CustomObjectProperty property)
        {
            this.Property = property;
        }
    }
}
