using System;
using UnityEngine;
using UnityEngine.UI;

public class Button : MonoBehaviour
{
    public Action OnClickAction;

    public Text ButtonText;

    public void OnClick()
    {
        OnClickAction?.Invoke();
    }

    public void Setup(string text, Action onClickAction)
    {
        ButtonText.text = text;
        OnClickAction = onClickAction;
    }
}

