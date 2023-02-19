using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class SoundsetViewModel : DataDetailViewModel<Soundset>
    {
        public SoundsetViewModel() : base()
        {
        }

        public SoundsetViewModel(Soundset soundset) : base(soundset)
        {

        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
