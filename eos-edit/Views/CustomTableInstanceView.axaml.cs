using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Eos.Extensions;
using Eos.Models.Tables;
using Eos.Usercontrols;
using Eos.ViewModels;
using System.Drawing.Printing;

namespace Eos.Views
{
    public class CustomTableColumnDataTemplate : IDataTemplate
    {
        private CustomObjectProperty _property;
        private IDataTemplate _templateSelector;

        public CustomTableColumnDataTemplate(CustomObjectProperty property, IDataTemplate templateSelector)
        {
            _property = property;
            _templateSelector = templateSelector;
        }

        public bool Match(object? data)
        {
            return true;
        }

        Control? ITemplate<object?, Control?>.Build(object? param)
        {
            if (param is CustomTableInstanceItem item)
            {
                var contentPresenter = new ContentPresenter();
                contentPresenter.Content = item.GetPropertyValue(_property);
                contentPresenter.ContentTemplate = _templateSelector;
                contentPresenter.Height = 25;
                contentPresenter.Margin = new Avalonia.Thickness(0, 2, 3, 0);
                contentPresenter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

                return contentPresenter;
            }

            return new TextBlock { Text = "Error" };
        }
    }

    public partial class CustomTableInstanceView : LanguageAwarePage
    {
        public CustomTableInstanceView()
        {
            InitializeComponent();
            DataContextChanged += CustomTableInstanceView_DataContextChanged;
        }

        private void CustomTableInstanceView_DataContextChanged(object? sender, System.EventArgs e)
        {
            grValues.Columns.Clear();
            if ((DataContext is CustomTableInstanceViewModel vm) && (vm.Data.Template != null) && 
                (Resources.TryGetResource("templateSelector", null, out var templateSelectorRes)) && (templateSelectorRes is IDataTemplate templateSelector))
            {
                foreach (var templateColumn in vm.Data.Template.Items)
                {
                    if (templateColumn == null) continue;

                    var column = new DataGridTemplateColumn();
                    column.Header = templateColumn.Label;
                    column.Width = new DataGridLength(150);
                    column.CellTemplate = new CustomTableColumnDataTemplate(templateColumn, templateSelector);
                    grValues.Columns.Add(column);
                }
                
                if ((Resources.TryGetResource("upDownTemplate", null, out var upDownTemplateRes)) && (upDownTemplateRes is IDataTemplate upDownTemplate))
                {
                    var column = new DataGridTemplateColumn();
                    column.Header = "";
                    column.Width = new DataGridLength(40);
                    column.CellTemplate = upDownTemplate;
                    grValues.Columns.Add(column);
                }

                if ((Resources.TryGetResource("deleteButtonTemplate", null, out var deleteButtonTemplateRes)) && (deleteButtonTemplateRes is IDataTemplate deleteButtonTemplate))
                {
                    var column = new DataGridTemplateColumn();
                    column.Header = "";
                    column.Width = new DataGridLength(25);
                    column.CellTemplate = deleteButtonTemplate;
                    grValues.Columns.Add(column);
                }
            }
        }
    }
}
