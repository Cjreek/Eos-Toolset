using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class ClassViewModel : DataDetailViewModel<CharacterClass>
    {
        public ClassViewModel() : base()
        {
        }

        public ClassViewModel(CharacterClass cls) : base(cls)
        {
        }

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 218, 165, 32));
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
