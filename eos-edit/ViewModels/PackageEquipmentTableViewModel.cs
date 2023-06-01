using Avalonia.Media;
using Eos.Models;
using Eos.Models.Tables;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class PackageEquipmentTableViewModel : DataDetailViewModel<PackageEquipmentTable>
    {
        public PackageEquipmentTableViewModel() : base()
        {
            DeleteEquipmentItemCommand = ReactiveCommand.Create<PackageEquipmentTableItem>(DeleteEquipmentItem);
            AddEquipmentItemCommand = ReactiveCommand.Create(AddEquipmentItem);
        }

        public PackageEquipmentTableViewModel(PackageEquipmentTable packageEquipmentTable) : base(packageEquipmentTable)
        {
            DeleteEquipmentItemCommand = ReactiveCommand.Create<PackageEquipmentTableItem>(DeleteEquipmentItem);
            AddEquipmentItemCommand = ReactiveCommand.Create(AddEquipmentItem);
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddEquipmentItem()
        {
            var newItem = new PackageEquipmentTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteEquipmentItem(PackageEquipmentTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public ReactiveCommand<PackageEquipmentTableItem, Unit> DeleteEquipmentItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddEquipmentItemCommand { get; private set; }
    }
}
