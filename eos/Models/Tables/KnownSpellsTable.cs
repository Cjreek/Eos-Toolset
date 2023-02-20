using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class KnownSpellsTable : BaseTable<KnownSpellsTableItem>
    {
        protected override void SetDefaultValues()
        {
            Name = "CLS_SPKN_NEW";
        }

        protected override void InitializeData()
        {
            for (int i = 0; i < GetMaximumItems(); i++)
            {
                Add(new KnownSpellsTableItem()
                {
                    Level = i + 1,
                    SpellLevel0 = 0,
                    SpellLevel1 = 0,
                    SpellLevel2 = 0,
                    SpellLevel3 = 0,
                    SpellLevel4 = 0,
                    SpellLevel5 = 0,
                    SpellLevel6 = 0,
                    SpellLevel7 = 0,
                    SpellLevel8 = 0,
                    SpellLevel9 = 0,
                });
            }
        }
        protected override int GetMaximumItems()
        {
            return 60;
        }
    }
}
