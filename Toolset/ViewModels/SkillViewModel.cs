using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class SkillViewModel : DataDetailViewModel<Skill>
    {
        public SkillViewModel() : base()
        {
        }

        public SkillViewModel(Skill skill) : base(skill)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 149, 167, 139));
        }
    }
}
