using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.LogicalTree;

namespace Eos.Extensions
{
    public class ReferenceExtension : MarkupExtension
    {
        private String? controlName;

        public ReferenceExtension()
        {
            controlName = null;
        }

        public ReferenceExtension(String? controlName)
        {
            this.controlName = controlName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (controlName != null)
            {
                var service = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
                var root = service?.IntermediateRootObject as Control;

                if (root != null)
                {
                    foreach (var child in root.GetLogicalChildren())
                    {
                        if ((child is Visual visual) && (visual.Name == controlName))
                            return child;
                    }
                }

                return 0;
            }

            return new object { };
        }
    }
}
