using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class FeatRepository : ModelRepository<Feat>
    {
        static FeatRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Feat>(typeof(FeatRepository));
        }

        public FeatRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public IEnumerable<Feat?> ClassOrRacial => this.Where(feat => feat?.ToolsetCategory == FeatCategory.ClassOrRacial);
        public IEnumerable<Feat?> CombatActive => this.Where(feat => feat?.ToolsetCategory == FeatCategory.CombatActive);
        public IEnumerable<Feat?> Combat => this.Where(feat => feat?.ToolsetCategory == FeatCategory.Combat);
        public IEnumerable<Feat?> Defensive => this.Where(feat => feat?.ToolsetCategory == FeatCategory.Defensive);
        public IEnumerable<Feat?> Magical => this.Where(feat => feat?.ToolsetCategory == FeatCategory.Magical);
        public IEnumerable<Feat?> Other => this.Where(feat => feat?.ToolsetCategory == FeatCategory.Other);

        protected override int GetCustomDataStartIndex()
        {
            return 1116;
        }


        protected override void Changed()
        {
            RaisePropertyChanged("ClassOrRacial");
            RaisePropertyChanged("CombatActive");
            RaisePropertyChanged("Combat");
            RaisePropertyChanged("Defensive");
            RaisePropertyChanged("Magical");
            RaisePropertyChanged("Other");
        }
    }
}
