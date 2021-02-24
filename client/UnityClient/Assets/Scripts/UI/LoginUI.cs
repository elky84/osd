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

            NettyClient.Instance.SetDestination("127.0.0.1", 18008);

            NettyClient.Instance.Connect();
        }
    }

    public void Message(string message)
    {
        Debug.Log(message);
    }
}

