using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public InputField IDInput;
    public InputField PasswordInput;

    void Start()
    {
        IDInput.text = PlayerPrefs.GetString("ID", "");
    }

    public void OnClickLogin()
    {
        if (!string.IsNullOrEmpty(IDInput.text) && !string.IsNullOrEmpty(PasswordInput.text))
        {
            GlobalData.UserName = IDInput.text;

            //var bytes = PlayerInfo.Bytes("cshyeon", GlobalData.UserName, 123);
            //NettyClient.Instance.Send<PlayerInfo>(bytes);

            NettyClient.Instance.Connect("127.0.0.1", 18008);
        }
    }

    public void OnClickReconnect()
    {
        NettyClient.Instance.ReConnect();
    }
}

