using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Updates
{
    internal class Update0630 : Update
    {
        public override int Version => 630;
        public override DateTime GameDataMinimumBuildDate => new DateTime(2022, 07, 26); // 6273521c

        public override void Apply(EosProject project)
        {
            // add standard "Short" column data to overriden project data that's missing it
            foreach (var cls in project.Classes)
            {
                if (cls == null) continue;

                if (cls.IsOverride)
                {
                    var standardCls = MasterRepository.Standard.Classes.GetByID(cls.Overrides ?? Guid.Empty);
                    if (standardCls != null)
                    {
                        foreach (var lang in Enum.GetValues<TLKLanguage>())
                        {
                            if (cls.Abbreviation[lang].Text.Trim() == "")
                            {
                                cls.Abbreviation[lang].Text = standardCls.Abbreviation[lang].Text;
                                cls.Abbreviation[lang].TextF = standardCls.Abbreviation[lang].TextF;
                            }
                        }
                    }
                }
            }
        }
    }
}
