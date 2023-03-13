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
    internal class KnownSpellsTableViewModel : DataDetailViewModel<KnownSpellsTable>
    {
        public KnownSpellsTableViewModel() : base()
        {
        }

        public KnownSpellsTableViewModel(KnownSpellsTable knownSpellsTable) : base(knownSpellsTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
