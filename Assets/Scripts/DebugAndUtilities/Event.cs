using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingGenerator
{
    public class Event
    {
        private List<Action> listeners = new List<Action>();

        public void Invoke()
        {
            for(int i = 0; i < listeners.Count; i++)
            {
                listeners[i].Invoke();
            }
        }

        public bool Register(Action newListener)
        {
            if (newListener == null)
            {
                Debug.LogError("Listener can't be null.");
                return false;
            }

            if (listeners.Contains(newListener))
            {
                Debug.LogError("Listener already added.");
                return false;
            }

            listeners.Add(newListener);

            return true;
        }

        public bool Unregister(Action listenerToRemove)
        {
            if (!listeners.Remove(listenerToRemove))
            {
                return false;
            }

            return true;
        }
    }


    public class Event<T>
    {
        private List<Action<T>> listeners = new List<Action<T>>();

        public void Invoke(T value)
        {
            for(int i = 0; i < listeners.Count; i++)
            {
                listeners[i].Invoke(value);
            }
        }

        public bool Register(Action<T> newListener)
        {
            if (newListener == null)
            {
                Debug.LogError("Listener can't be null.");
                return false;
            }

            if (listeners.Contains(newListener))
            {
                Debug.LogError("Listener already added.");
                return false;
            }

            listeners.Add(newListener);

            return true;
        }

        public bool Unregister(Action<T> listenerToRemove)
        {
            if (!listeners.Remove(listenerToRemove))
            {
                return false;
            }

            return true;
        }
    }
}