using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
