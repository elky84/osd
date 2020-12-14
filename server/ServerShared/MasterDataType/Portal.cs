using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

public class Portal
{
    [Key]
    public string Id { get; set; }
    public string BeforeMap { get; set; }
    public Point BeforePosition { get; set; }
    public string AfterMap { get; set; }
    public Point AfterPosition { get; set; }
}