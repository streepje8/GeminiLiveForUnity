using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace WezzelNL.Util
{
    public static class AsyncDispatcher
    {
        public delegate Task AsyncFunction(CancellationToken ct = default);

        public static void DispatchNonBlocking(AsyncFunction function, CancellationToken ct = default)
        {
            function(ct).ContinueWith(t =>
            {
                //Collect exceptions and rethrow on main thread
                if (t.IsCompletedSuccessfully) return;
                if (t.Exception == null) return;
                foreach (var inner in t.Exception.InnerExceptions)
                {
                    var exception = ExceptionDispatchInfo.Capture(inner);
                    AsyncMainThreadSynchronization.Current.Schedule(() => exception.Throw());
                }
            }, ct);
        }
    
        public static void DispatchBlocking(AsyncFunction function, CancellationToken ct = default) => function(ct).GetAwaiter().GetResult();
        public static void SetMainThread() => AsyncMainThreadSynchronization.Create();
    }
}