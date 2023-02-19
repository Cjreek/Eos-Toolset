using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class SpellViewModel : DataDetailViewModel<Spell>
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

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 193, 104, 171));
        }
    }
}
