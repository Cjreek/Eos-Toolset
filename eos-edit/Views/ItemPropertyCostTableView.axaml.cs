using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Repositories;
using Eos.Services;
using System;

namespace Eos.Views
{
    public partial class ItemPropertyCostTableView : LanguageAwarePage
    {
        public ItemPropertyCostTableView()
        {
            InitializeComponent();
            grItems.LoadingRow += GrItems_LoadingRow;
        }

        private void GrItems_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (grItems.Items is Repository<ItemPropertyCostTableItem> itemRepo)
            {
                var item = itemRepo[e.Row.GetIndex()];
                e.Row.DataContext = item;

                for (int i = 2; i < grItems.Columns.Count; i++)
                {
                    if (!grItems.Columns[i].IsVisible) break;

                    if (grItems.Columns[i].GetCellContent(e.Row) is ContentPresenter presenter)
                    {
                        CustomValueInstance? value = null;
                        switch (i)
                        {
                            case 2: value = item?.CustomColumnValue01; break;
                            case 3: value = item?.CustomColumnValue02; break;
                            case 4: value = item?.CustomColumnValue03; break;
                            case 5: value = item?.CustomColumnValue04; break;
                            case 6: value = item?.CustomColumnValue05; break;
                            case 7: value = item?.CustomColumnValue06; break;
                            case 8: value = item?.CustomColumnValue07; break;
                            case 9: value = item?.CustomColumnValue08; break;
                            case 10: value = item?.CustomColumnValue09; break;
                            case 11: value = item?.CustomColumnValue10; break;
                        }

                        presenter.Content = value;
                    }
                }
            }
        }
    }
}
