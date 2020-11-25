using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject LoginUI;

    public GameObject ReconnectButton;

    void Start()
    {
        //ClientHandler.AddPacketDelegate(Protocols.Id.Response.Login, OnLogin);

        NettyClient.Instance.OnConnected += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                ReconnectButton.SetActive(false);
                if (!LoginUI.activeSelf)
                    LoginUI.SetActive(true);
            });
        };
        NettyClient.Instance.OnClose += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                ReconnectButton.SetActive(true);
            });
        };
        NettyClient.Instance.Connect("127.0.0.1", 18008);
    }

    //public bool OnLogin(Protocols.Response.Header header)
    //{
    //    if (header.Result != Protocols.Code.Result.Success)
    //    {
    //        return false;
    //    }

    //    LoginUI.SetActive(false);
    //    return true;
    //}

    public static void OnClickDisconnect()
    {
        NettyClient.Instance.Close();
    }

    public static void ReConnect()
    {
        NettyClient.Instance.ReConnect();
    }

    void OnApplicationQuit()
    {
        NettyClient.Instance.OnApplicationQuit();
    }

}
