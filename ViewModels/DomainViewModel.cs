using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class DomainViewModel : DataDetailViewModel<Domain>
    {
        public DomainViewModel() : base()
        {
        }

        public DomainViewModel(Domain domain) : base(domain)
        {
        }

        protected override Brush GetColor()
        {
            return Brushes.LightGoldenrodYellow;
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
