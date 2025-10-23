using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
                new GameObject("Temp").AddComponent<AsyncExceptionThrower>().Throw(exception);
            }
        }
    }

    public class AsyncExceptionThrower : MonoBehaviour
    {
        private ExceptionDispatchInfo exception;
        private bool canThrow = false;
        public void Throw(ExceptionDispatchInfo e)
        {
            exception = e;
            canThrow = true;
        }

        private void Update()
        {
            if (canThrow)
            {
                DestroyImmediate(gameObject);
                exception.Throw();
            }
        }
    }
}