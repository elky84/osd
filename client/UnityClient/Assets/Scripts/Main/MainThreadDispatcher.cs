using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance = null;
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static MainThreadDispatcher Instance { get { return _instance; } }
    void Awake()
    {
        var inst = GetComponent<MainThreadDispatcher>();
        if (_instance == null)
        {
            _instance = inst;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != inst)
            Destroy(gameObject);
    }

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() =>
            {
                StartCoroutine(action);
            });
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }
    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
}
