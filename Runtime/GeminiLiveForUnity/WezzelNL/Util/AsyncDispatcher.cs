using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WezzelNL.Util
{
    public static class AsyncDispatcher
    {
        public delegate Task AsyncFunction(CancellationToken ct = default);

        public static async void DispatchNonBlocking(AsyncFunction function, CancellationToken ct = default)
        {
            try { await function(ct); }
            catch (Exception e)
            {
                await Awaitable.MainThreadAsync();
                var exception = ExceptionDispatchInfo.Capture(e);
                #if UNITY_EDITOR
                EditorApplication.delayCall += exception.Throw;
                #else
                Debug.LogException(e);
                #endif
            }
        }
    }
}