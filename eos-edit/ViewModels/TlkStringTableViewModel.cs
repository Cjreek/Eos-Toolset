using System.Reactive;
using Eos.Models.Tables;
using Eos.ViewModels.Base;
using ReactiveUI;

namespace Eos.ViewModels;

public class TlkStringTableViewModel : DataDetailViewModel<TlkStringTable>
{
    public TlkStringTableViewModel() : base()
    {
        DeleteTlkStringCommand = ReactiveCommand.Create<CustomTlkStringItem>(DeleteTlkString);
        AddTlkStringCommand = ReactiveCommand.Create(AddTlkString);

        MoveTlkStringUpCommand = ReactiveCommand.Create<CustomTlkStringItem>(MoveUp);
        MoveTlkStringDownCommand = ReactiveCommand.Create<CustomTlkStringItem>(MoveDown);
    }

    public TlkStringTableViewModel(TlkStringTable tlkStringTable) : base(tlkStringTable)
    {
        DeleteTlkStringCommand = ReactiveCommand.Create<CustomTlkStringItem>(DeleteTlkString);
        AddTlkStringCommand = ReactiveCommand.Create(AddTlkString);

        MoveTlkStringUpCommand = ReactiveCommand.Create<CustomTlkStringItem>(MoveUp);
        MoveTlkStringDownCommand = ReactiveCommand.Create<CustomTlkStringItem>(MoveDown);
    }
    
    protected override string GetHeader()
    {
        return "Custom TLK Strings";
    }
    
    private void AddTlkString()
    {
        var newItem = new CustomTlkStringItem();
        Data.Add(newItem);
        NotifyPropertyChanged("Data");
    }

    private void DeleteTlkString(CustomTlkStringItem item)
    {
        this.Data.Remove(item);
        NotifyPropertyChanged("Data");
    }

    private void MoveUp(CustomTlkStringItem item)
    {
        Data.MoveUp(item);
    }

    private void MoveDown(CustomTlkStringItem item)
    {
        Data.MoveDown(item);
    }
    
    public ReactiveCommand<CustomTlkStringItem, Unit> DeleteTlkStringCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> AddTlkStringCommand { get; private set; }

    public ReactiveCommand<CustomTlkStringItem, Unit> MoveTlkStringUpCommand { get; private set; }
    public ReactiveCommand<CustomTlkStringItem, Unit> MoveTlkStringDownCommand { get; private set; }
}