using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventAggregator
{
    static readonly Lazy<EventAggregator> StaticInstance = new Lazy<EventAggregator>(() => new EventAggregator());
    public static EventAggregator Instance { get { return StaticInstance.Value; } }

    private readonly Dictionary<Type, List<object>> _actions = new Dictionary<Type, List<object>>();

    public int Subscribe<T>(Action<T> callback, bool topPriority = false)
    {
        try
        {
            lock (_actions)
            {
                if (_actions.ContainsKey(typeof(T)) == false)
                {
                    _actions.Add(typeof(T), new List<object>());
                }
                else
                {
                    if (_actions[typeof(T)].Contains(callback))
                    {
                        Debug.LogError("Callback already subscribed..");
                        return 0;
                    }
                }

                if (topPriority)
                {
                    _actions[typeof(T)].Insert(0, callback);
                }
                else
                {
                    _actions[typeof(T)].Add(callback);
                }
            }

            return callback.GetHashCode();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            return 0;
        }
    }

    public void Unsubscribe<T>(Action<T> callback)
    {
        try
        {
            lock (_actions)
            {
                List<object> actions;
                if (_actions.TryGetValue(typeof(T), out actions) == false)
                {
                    return;
                }

                if (actions.Contains(callback) == false)
                {
                    return;
                }

                if (actions.Remove(callback) == false)
                {
                    Debug.LogError(String.Format("remove subscription error for {0}", typeof(T).Name));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    public void Unsubscribe(Type type, int hash)
    {
        try
        {
            lock (_actions)
            {
                List<object> actions;
                if (_actions.TryGetValue(type, out actions) == false)
                {
                    Debug.Log(String.Format("remove subscription error for {0},{1}", type.Name, hash));
                    return;
                }

                var action = actions.FirstOrDefault(x => x.GetHashCode() == hash);
                if (action == null)
                {
                    Debug.Log(String.Format("remove subscription error for {0},{1}", type.Name, hash));
                    return;
                }

                actions.Remove(action);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    public void Publish<T>(T msg = null) where T : class
    {
        try
        {
            List<Action<T>> actionsCopy;

            lock (_actions)
            {
                if (_actions.ContainsKey(typeof(T)))
                {
                    actionsCopy = _actions[typeof(T)].OfType<Action<T>>().ToList();
                }
                else
                {
                    return;
                }
            }

            if (actionsCopy.Any())
            {
                foreach (var a in actionsCopy)
                {
                    try
                    {
                        var copyA = a;
                        if (copyA.Target == null)
                        {
                            Debug.LogError($"Invalid Target {copyA}");
                            return;
                        }

                        MainThreadDispatcher.Instance.Enqueue(() => copyA(msg));
                    }
                    catch (Exception ex)
                    {
                        // TODO: this entry should be removed
                        Debug.Log(ex.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }
}