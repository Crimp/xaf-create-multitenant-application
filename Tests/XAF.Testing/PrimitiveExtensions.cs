﻿using System.Collections.Concurrent;
using System.Text;

namespace XAF.Testing{
    public static class PrimitiveExtensions{
        public static byte[] Bytes(this string s, Encoding encoding = null) 
            => s == null ? Array.Empty<byte>() : (encoding ?? Encoding.UTF8).GetBytes(s);
        
        public static void Await(this object any, Func<Task> invoker) {
            var originalContext = SynchronizationContext.Current;
            try {
                var context = new SingleThreadedSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
                var task = invoker.Invoke();
                task.ContinueWith(_ => context.Queue.CompleteAdding());
                while (context.Queue.TryTake(out var work, Timeout.Infinite))
                    work.d.Invoke(work.state);
                task.GetAwaiter().GetResult();
            }
            finally {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        internal sealed class SingleThreadedSynchronizationContext : SynchronizationContext {
            public BlockingCollection<(SendOrPostCallback d, object state)> Queue{ get; } = new();

            public override void Post(SendOrPostCallback d, object state){
                if (!Queue.IsAddingCompleted) {
                    Queue.Add((d, state));
                }
            }
        }

    }
}