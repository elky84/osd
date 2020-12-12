using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;

public class Sheet23
{
    [Key]
    public string Id { get; set; }
    public double? Value { get; set; }
    public int Cshyeon { get; set; }
    public string Relation { get; set; }
}