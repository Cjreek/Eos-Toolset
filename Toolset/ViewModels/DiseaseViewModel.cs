using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 189, 146, 74));
        }
    }
}
