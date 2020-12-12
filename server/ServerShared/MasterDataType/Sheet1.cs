using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;

public class Sheet1
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public bool? Value2 { get; set; }
    [JsonConverter(typeof(JsonEnumConverter<Direction>))]
    public Direction Direction { get; set; }
}