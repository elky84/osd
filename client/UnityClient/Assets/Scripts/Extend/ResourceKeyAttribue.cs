using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResourceKeyAttribue : Attribute
{
    public string key;

    public ResourceKeyAttribue(string key)
    {
        this.key = key;
    }
}
