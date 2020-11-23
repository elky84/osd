using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;

public class Sheet1
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public bool? Value2 { get; set; }
}