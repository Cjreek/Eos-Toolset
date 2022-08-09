using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class PoisonViewModel : DataDetailViewModel<Poison>
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

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 132, 182, 74));
        }
    }
}
