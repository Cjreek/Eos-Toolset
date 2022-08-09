using Eos.Models.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class SavingThrowTable : BaseTable<SavingThrowTableModel>
    {
        protected override void InitializeData()
        {
            for (int i = 0; i < GetMaximumItems(); i++)
            {
                Add(new SavingThrowTableModel()
                {
                    Level = i + 1,
                    FortitudeSave = 0,
                    ReflexSave = 0,
                    WillpowerSave = 0,
                });
            }
        }

        protected override int GetMaximumItems()
        {
            return 60;
        }
    }
}
