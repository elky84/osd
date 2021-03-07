using System;
using UnityEngine;

public class UIPool : MonoBehaviour
{
    public static Lazy<UIPool> Instance { get; private set; } = new Lazy<UIPool>(() => GameObject.FindObjectOfType<UIPool>());

    public static T Show<T>() where T : UIBase
    {
        var found = Instance.Value.GetComponentInChildren<T>(true);
        found.gameObject.SetActive(true);
        return found;
    }
}