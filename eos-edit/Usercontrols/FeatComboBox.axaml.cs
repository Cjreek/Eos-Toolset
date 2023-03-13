using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using System;
using System.Reflection;

namespace Eos.Usercontrols
{
    public partial class FeatComboBox : UserControl
    {
        public FeatComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Feat?> SelectedValueProperty = AvaloniaProperty.Register<FeatComboBox, Feat?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<FeatComboBox, bool>("IsNullable", true);

        public Feat? SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public bool IsNullable
        {
            get { return GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            SetValue(SelectedValueProperty, null);
        }

        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new FeatSearchViewModel(MasterRepository.Feats);
            WindowService.OpenDialog(viewModel);
            if (viewModel.ResultModel != null)
                SetValue(SelectedValueProperty, viewModel.ResultModel);
        }

        private void btGoto_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.OpenDetail, SelectedValue, true);
        }
    }
}
