using Avalonia.Media;
using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 149, 167, 139));
        }
    }
}
