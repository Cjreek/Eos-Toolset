using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels;
using System;
using System.ComponentModel;

namespace Eos.Views
{
    public partial class ItemPropertyCostTableView : LanguageAwarePage
    {
        private ItemPropertyCostTableViewModel? oldViewModel = null;

        public ItemPropertyCostTableView()
        {
            InitializeComponent();
            grItems.LoadingRow += GrItems_LoadingRow;
            DataContextChanged += CustomDynamicTableInstanceView_DataContextChanged;
        }

        private void CustomDynamicTableInstanceView_DataContextChanged(object? sender, EventArgs e)
        {
            if (oldViewModel is ItemPropertyCostTableViewModel oldVM)
            {
                oldVM.Data.CustomColumn01.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn02.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn03.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn04.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn05.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn06.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn07.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn08.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn09.PropertyChanged -= CustomColumn_PropertyChanged;
                oldVM.Data.CustomColumn10.PropertyChanged -= CustomColumn_PropertyChanged;
            }

            oldViewModel = null;
            if (DataContext is ItemPropertyCostTableViewModel newVM)
            {
                newVM.Data.CustomColumn01.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn02.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn03.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn04.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn05.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn06.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn07.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn08.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn09.PropertyChanged += CustomColumn_PropertyChanged;
                newVM.Data.CustomColumn10.PropertyChanged += CustomColumn_PropertyChanged;

                oldViewModel = newVM;
            }
        }

        private void CustomColumn_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is ItemPropertyCostTableViewModel vm)
            {
                foreach (var item in vm.Data.Items)
                {
                    if (item == null) continue;
                    UpdateContentPresenters(item);
                }
            }
        }

        private void UpdateContentPresenters(ItemPropertyCostTableItem? item, DataGridRow? row = null)
        {
            for (int i = 3; i < grItems.Columns.Count; i++)
            {
                if (!grItems.Columns[i].IsVisible) break;

                var presenterObj = (row != null) ? grItems.Columns[i].GetCellContent(row) : grItems.Columns[i].GetCellContent(item);
                if (presenterObj is ContentPresenter presenter)
                {
                    CustomValueInstance? value = null;
                    switch (i)
                    {
                        case 3: value = item?.CustomColumnValue01; break;
                        case 4: value = item?.CustomColumnValue02; break;
                        case 5: value = item?.CustomColumnValue03; break;
                        case 6: value = item?.CustomColumnValue04; break;
                        case 7: value = item?.CustomColumnValue05; break;
                        case 8: value = item?.CustomColumnValue06; break;
                        case 9: value = item?.CustomColumnValue07; break;
                        case 10: value = item?.CustomColumnValue08; break;
                        case 11: value = item?.CustomColumnValue09; break;
                        case 12: value = item?.CustomColumnValue10; break;
                    }

                    presenter.Content = null;
                    presenter.Content = value;
                }
            }
        }

        private void GrItems_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (grItems.ItemsSource is Repository<ItemPropertyCostTableItem> itemRepo)
            {
                var item = itemRepo[e.Row.GetIndex()];
                e.Row.DataContext = item;
                UpdateContentPresenters(item, e.Row);
            }
        }
    }
}
