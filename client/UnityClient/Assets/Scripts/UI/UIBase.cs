using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected UIBase()
    { }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
