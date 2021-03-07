using UnityEngine;
using UnityEngine.UI;

public class UIWindow : UIBase
{
    [Header("Prefabs")]
    public Text UIText;

    [Header("Values")]
    public string Contents;

    public void Start()
    {
        UIText.text = Contents;
    }
}
