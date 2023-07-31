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
    public class PoisonViewModel : DataDetailViewModel<Poison>
    {
        public PoisonViewModel() : base()
        {
        }

        public PoisonViewModel(Poison poison) : base(poison)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 132, 182, 74));
        }
    }
}
