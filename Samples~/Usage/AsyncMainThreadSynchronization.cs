using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace WezzelNL.Util
{
    public class AsyncMainThreadSynchronization : MonoBehaviour
    {
        private readonly ConcurrentBag<Action> mainThreadQueue = new ConcurrentBag<Action>();

        private static AsyncMainThreadSynchronization instance;
        public static AsyncMainThreadSynchronization Current => instance;
        public static void Create() => instance ??= new GameObject("Async Main Thread Synchronization").AddComponent<AsyncMainThreadSynchronization>();

        public void Schedule(Action action) => mainThreadQueue.Add(action);

        private void Update()
        {
            while (!mainThreadQueue.IsEmpty)
            {
                if(!mainThreadQueue.TryTake(out var action)) continue;
                try { action(); } catch(Exception e) { Debug.LogException(e); }
            }
        }
    }
}