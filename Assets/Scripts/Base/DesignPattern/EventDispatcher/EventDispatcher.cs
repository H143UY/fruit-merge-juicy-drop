using UnityEngine;
using System.Collections.Generic;
using System;
using UniRx;
using UniRx.Triggers;

public class EventDispatcher : SingletonMonoDontDestroy<EventDispatcher>
{
    #region Init, main component declare

    public EventDispatcher(string className) : base(className)
    {
    }

    /// Store all "listener"
    private static Dictionary<EventID, List<Action<Component, object>>> _listenersDict
      = new Dictionary<EventID, List<Action<Component, object>>>();

    #endregion

    #region Add Listeners, Post events, Remove listener

    /// <summary>
    /// Register to listen for eventID
    /// </summary>
    /// <param name="eventId">EventID that object want to listen</param>
    /// <param name="callback">Callback will be invoked when this eventID be raised</param>
    public void RegisterListener(EventID eventId, Action<Component, object> callback)
    {
        // checking params

       // Common.Assert(callback != null, "AddListener, event {0}, callback = null !!", eventID.ToString());
      //  Common.Assert(eventID != EventID.None, "RegisterListener, event = None !!");

        // check if listener exist in distionary
        if (_listenersDict.ContainsKey(eventId))
        {
            // add callback to our collection
            _listenersDict[eventId].Add(callback);
        }
        else
        {
            // add new key-value pair
            var newList = new List<Action<Component, object>>();
            newList.Add(callback);
            _listenersDict.Add(eventId, newList);
        }
    }

    /// <summary>
    /// Posts the event. This will notify all listener that register for this event
    /// </summary>
    /// <param name="eventId">EventID.</param>
    /// <param name="sender">Sender, in some case, the Listener will need to know who send this message.</param>
    /// <param name="param">Parameter. Can be anything (struct, class ...), Listener will make a cast to get the data</param>
    public void PostEvent(EventID eventId, Component sender, object param)
    {

        // checking params

      //  Common.Assert(eventID != EventID.None, "PostEvent, event = None !!");
       // Common.Assert(sender != null, "PostEvent, event {0}, sender = null !!", eventID.ToString());

        List<Action<Component, object>> actionList;
        if (_listenersDict.TryGetValue(eventId, out actionList))
        {
            //Common.Log("Post event {0} - Listener: {1}", eventID, actionList.Count);

            for (int i = 0, amount = actionList.Count; i < amount; i++)
            {
                try
                {
                    actionList[i](sender, param);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                //    Common.LogWarning(this, "Error when PostEvent : {0}, message : {1}", eventID.ToString(), e.Message);
                    // remove listener at i - that cause the exception
                    Debug.Log($"Remove listener at {eventId.ToString()} that cause the exception, message: {e.Message}");
                    actionList.RemoveAt(i);
                    if (actionList.Count == 0)
                    {
                        // no listener remain, then delete this key
                        _listenersDict.Remove(eventId);
                    }
                    // reduce amount and index for the next loop
                    amount--;
                    i--;
                }
            }
        }
        else
        {
            // if not exist, just warning, don't throw exceptoin
           // Common.LogWarning(this, "PostEvent, event : {0}, no listener for this event", eventID.ToString());
        }
    }

    /// <summary>
    /// Removes the listener. Use to Unregister listener
    /// </summary>
    /// <param name="eventId">EventID.</param>
    /// <param name="callback">Callback.</param>
    public void RemoveListener(EventID eventId, Action<Component, object> callback)
    {
        // checking params
     //   Common.Assert(callback != null, "RemoveListener, event {0}, callback = null !!", eventID.ToString());
     //   Common.Assert(eventID != EventID.None, "AddListener, event = None !!");

        List<Action<Component, object>> actionList;
        if (_listenersDict.TryGetValue(eventId, out actionList))
        {
            if (actionList.Contains(callback))
            {
                actionList.Remove(callback);
                if (actionList.Count == 0)// no listener remain for this event
                {
                    _listenersDict.Remove(eventId);
                }
            }
        }
        else
        {
            // the listeners not exist
            Debug.LogWarning($"RemoveListener, event : {eventId.ToString()}, no listener found");
        }
    }

    /// <summary>
    /// Clean the ListenerList, remove the listener that have a null target. This happen when an object that
    /// already be "delete" in Hirachy, but still have a callback remain in listenerList
    /// </summary>
    public void RemoveRedundancies()
    {
        foreach (var keyPairs in _listenersDict)
        {
            var listenerList = keyPairs.Value;
            for (int amount = listenerList.Count, i = amount - 1; i >= 0; i--)
            {
                var listener = listenerList[i];
                // Use Target.Equal(null) instead of Target == null, it won't work
                if (listener == null || listener.Target.Equals(null))
                {
                    listenerList.RemoveAt(i);
                    if (listenerList.Count == 0)
                    {
                        // no listener remain, then delete this key
                        _listenersDict.Remove(keyPairs.Key);
                    }
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// Clears all the listener.
    /// </summary>
    public void ClearAllListener()
    {
        _listenersDict.Clear();
    }

    public int ListenerCount()
    {
        return _listenersDict.Count;
    }

    #endregion
}

#region Extension class
/// <summary>
/// Delare some "shortcut" for using EventDispatcher easier
/// </summary>
public static class EventDispatcherExtension
{
    /// Use for registering with EventsManager
    public static void RegisterListener(this MonoBehaviour sender, EventID eventId, Action<Component, object> callback)
    {
        EventDispatcher.Instance.RegisterListener(eventId, callback);
        sender.OnDestroyAsObservable().Subscribe(_ =>
        {
            if (EventDispatcher.Instance != null)
            {
                EventDispatcher.Instance.RemoveListener(eventId, callback);
            }
        });
    }

    public static void RemoveListener(this MonoBehaviour sender, EventID eventId, Action<Component, object> callback)
    {
        EventDispatcher.Instance.RemoveListener(eventId, callback);
    }

    /// Post event with param
    public static void PostEvent(this MonoBehaviour sender, EventID eventId, object param)
    {
        EventDispatcher.Instance.PostEvent(eventId, sender, param);
    }

    /// Post event with no param (param = null)
    public static void PostEvent(this MonoBehaviour sender, EventID eventId)
    {
        EventDispatcher.Instance.PostEvent(eventId, sender, null);
    }
}
#endregion