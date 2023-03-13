using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Eos.Models;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using Microsoft.VisualBasic;
using System;
using System.Reflection;

namespace Eos.Usercontrols
{
    public class EnabledButton : Button, IStyleable
    {
        Type IStyleable.StyleKey => typeof(Button);

        static EnabledButton()
        {
            IsEnabledProperty.AddOwner<EnabledButton>();
            IsEnabledProperty.Changed.Subscribe(e =>
            {
                if ((e.Sender is EnabledButton) || (e.Sender is ScrollViewer))
                    e.Sender.SetValue(e.Property, true);
            });
        }

        //protected override bool IsEnabledCore => true;

        //public EnabledButton()
        //{
        //    this.LayoutUpdated += EnabledButton_LayoutUpdated;
        //    setter = GetType().GetProperty("IsEffectivelyEnabled")?.GetSetMethod(true);
        //    update = typeof(InputElement).GetMethod("UpdateIsEffectivelyEnabled", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { typeof(InputElement) });
        //}

        //private void ForceEnable()
        //{
        //    IsEnabled = true;
        //    IsHitTestVisible = true;
        //    this.Focusable = true;
        //    this.PseudoClasses.Remove(":disabled");
        //    setter?.Invoke(this, new object[] { true });

        //    update?.Invoke(this, new object?[] { null });
        //    for (int i = 0; i < VisualChildren.Count; ++i)
        //    {
        //        var child = VisualChildren[i] as InputElement;

        //        update?.Invoke(child, new object?[] { null });
        //    }

        //    InvalidateVisual();
        //}

        //private void EnabledButton_LayoutUpdated(object? sender, EventArgs e)
        //{
        //    ForceEnable();
        //}
    }

    public partial class SkillComboBox : UserControl
    {
        public SkillComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Skill?> SelectedValueProperty = AvaloniaProperty.Register<SkillComboBox, Skill?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<SkillComboBox, bool>("IsNullable", true);

        public Skill? SelectedValue
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
            var viewModel = new SkillSearchViewModel(MasterRepository.Skills);
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
