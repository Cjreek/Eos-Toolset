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
    internal class DiseaseViewModel : DataDetailViewModel<Disease>
    {
        public DiseaseViewModel() : base()
        {
        }

        public DiseaseViewModel(Disease disease) : base(disease)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 189, 146, 74));
        }
    }
}
