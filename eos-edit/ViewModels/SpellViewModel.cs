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
    public class SpellViewModel : DataDetailViewModel<Spell>
    {
        public SpellViewModel() : base()
        {
        }

        public SpellViewModel(Spell spell) : base(spell)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 193, 104, 171));
        }
    }
}
