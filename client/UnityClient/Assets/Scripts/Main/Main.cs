using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject LoginUI;

    void Start()
    {
        NettyClient.Instance.OnConnected += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                LoginUI.SetActive(false);
            });
        };
        NettyClient.Instance.OnClose += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                LoginUI.SetActive(true);
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
