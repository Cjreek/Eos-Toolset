using Eos.Models.Tables;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Eos.ViewModels
{
    internal class SpellSlotTableViewModel : DataDetailViewModel<SpellSlotTable>
    {
        public SpellSlotTableViewModel() : base()
        {
        }

        public SpellSlotTableViewModel(SpellSlotTable spellSlotTable) : base(spellSlotTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
