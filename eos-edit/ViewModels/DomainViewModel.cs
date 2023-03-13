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
    internal class DomainViewModel : DataDetailViewModel<Domain>
    {
        public DomainViewModel() : base()
        {
        }

        public DomainViewModel(Domain domain) : base(domain)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(75, 0x48, 0x3D, 0x8B)); // 0x4B, 0x00, 0x82
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
