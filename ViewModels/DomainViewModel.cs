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

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 0x48, 0x3D, 0x8B)); // 0x4B, 0x00, 0x82
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
