using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Eos.Usercontrols
{
    public class ResetIsEnabled : ContentControl
    {
        static ResetIsEnabled()
        {
            IsEnabledProperty.OverrideMetadata(
                typeof(ResetIsEnabled),
                new UIPropertyMetadata(
                    defaultValue: true,
                    propertyChangedCallback: (_, __) => { },
                    coerceValueCallback: (_, x) => x));
        }
    }
}
