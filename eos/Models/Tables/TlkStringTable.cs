using System.Text.Json.Nodes;

namespace Eos.Models.Tables;

public class TlkStringTable : BaseTable<CustomTlkStringItem>
{
    public event EventHandler? OnChanged;

    protected override void Changed()
    {
        OnChanged?.Invoke(this, new EventArgs());
    }
}
