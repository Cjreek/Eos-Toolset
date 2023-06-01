using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class RacialFeatsTable : BaseTable<RacialFeatsTableItem>
    {
        protected override string GetTypeName()
        {
            return "Racial Feats Table";
        } 

        protected override void SetDefaultValues()
        {
            Name = "RACE_FEAT_NEW";
        }
    }
}
