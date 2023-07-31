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
    public class ProgrammedEffectViewModel : DataDetailViewModel<ProgrammedEffect>
    {
        public ProgrammedEffectViewModel() : base()
        {
        }
        public ProgrammedEffectViewModel(ProgrammedEffect progFX) : base(progFX)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
