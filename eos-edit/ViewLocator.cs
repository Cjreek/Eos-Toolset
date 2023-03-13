using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using System;
using System.Xml.Linq;

namespace Eos
{
    public class ViewLocator : IDataTemplate
    {
        public bool Match(object? data)
        {
            return (data is ViewModelBase vmb) && !(vmb is DataDetailViewModelBase) & !(vmb is DialogViewModel);
        }

        Control? ITemplate<object?, Control?>.Build(object? param)
        {
            if (param != null)
            {
                var name = param.GetType().FullName!.Replace("ViewModel", "View");
                var type = Type.GetType(name);

                if (type != null)
                {
                    return (Control)Activator.CreateInstance(type)!;
                }
                else
                {
                    return new TextBlock { Text = "Not Found: " + name };
                }
            }

            return new TextBlock { Text = "Not Found" };
        }
    }
}
