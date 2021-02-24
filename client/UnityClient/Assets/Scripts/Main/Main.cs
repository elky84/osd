using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public LoginUI LoginUI;

    void Start()
    {
        NettyClient.Instance.OnConnected += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                LoginUI.gameObject.SetActive(false);
            });
        };
        NettyClient.Instance.OnClose += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                LoginUI.gameObject.SetActive(true);
            });
        };

        NettyClient.Instance.OnConnectFailed += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                LoginUI.Message("접속 불가");
            });
        };
    }

    public static void OnClickDisconnect()
    {
        NettyClient.Instance.Close();
    }

    void OnApplicationQuit()
    {
        NettyClient.Instance.OnApplicationQuit();
    }

}
