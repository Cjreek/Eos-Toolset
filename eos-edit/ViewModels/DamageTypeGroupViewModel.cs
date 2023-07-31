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
    public class DamageTypeGroupViewModel : DataDetailViewModel<DamageTypeGroup>
    {
        public DamageTypeGroupViewModel() : base()
        {
        }
        public DamageTypeGroupViewModel(DamageTypeGroup damageTypeGroup) : base(damageTypeGroup)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.SourceLabel ?? "";
        }
    }
}
