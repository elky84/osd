
using UnityEngine;

public static class ResourcesUtil
{
    static public T ResourceLoad<T>(this MonoBehaviour monoBehaviour, string path, Transform parent = null) where T : Component
    {
        var gameObj = monoBehaviour.ResourceLoad(path, parent);
        return gameObj.GetComponent<T>();
    }

    static public GameObject ResourceLoad(this MonoBehaviour monoBehaviour, string path, Vector3 position, Transform parent)
    {
        var gameObj = Object.Instantiate(Resources.Load(path) as GameObject, position, Quaternion.identity, parent);
        gameObj.transform.SetParent(parent);
        return gameObj;
    }

    static public GameObject ResourceLoad(this MonoBehaviour monoBehaviour, string path, Transform parent)
    {
        var gameObj = Object.Instantiate(Resources.Load(path) as GameObject, parent);
        gameObj.transform.SetParent(parent);
        return gameObj;
    }

    static public GameObject ResourceLoad(this MonoBehaviour monoBehaviour, string path)
    {
        var gameObj = Object.Instantiate(Resources.Load(path) as GameObject);
        return gameObj;
    }
}