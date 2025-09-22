using System.Text.Json.Nodes;
using Eos.Nwn.Tlk;

namespace Eos.Models.Tables;

public class CustomTlkStringItem : TableItem
{
    public TLKStringSet Text { get; set; } = new TLKStringSet();
    public String Constant { get; set; } = "";
    
    public CustomTlkStringItem() : base()
    {
    }

    public CustomTlkStringItem(TlkStringTable parentTable) : base(parentTable)
    {
    }
    
    public override void FromJson(JsonObject json)
    {
        base.FromJson(json);
        this.Text.FromJson(json["Text"]?.AsObject());
        this.Constant = json["Constant"]?.GetValue<String>() ?? "";
    }

    public override JsonObject ToJson()
    {
        var json = base.ToJson();
        json.Add("Text", this.Text.ToJson());
        json.Add("Constant", this.Constant);

        return json;
    }
}
