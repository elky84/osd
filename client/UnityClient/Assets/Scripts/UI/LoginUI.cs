using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public InputField IDInput;
    public InputField PasswordInput;

    private void Awake()
    {

    }

    void Start()
    {
        IDInput.text = PlayerPrefs.GetString("ID", "");
    }

    public void OnClickLogin()
    {
        if (!string.IsNullOrEmpty(IDInput.text) && !string.IsNullOrEmpty(PasswordInput.text))
        {
            GlobalData.UserName = IDInput.text;
        }
    }

    public void OnClickReconnect()
    {
        NettyClient.Instance.ReConnect();
    }
}

