using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    internal sealed class BackgroundTask
    {
        private static CancellationTokenSource source;
        private static BlockingCollection<BackgroundTask> collection;

        private Action action;
        private bool completed;
        private System.Diagnostics.StackTrace l;

        public BackgroundTask(Action action) => this.action = action;

        [DidReloadScripts(-1)]
        private static void Execute()
        {
            source?.Cancel();
            collection?.CompleteAdding();
            collection = new BlockingCollection<BackgroundTask>();
            source = new CancellationTokenSource();
            Task.Factory.StartNew(o =>
            {
                while (true)
                {
                    BackgroundTask task = collection.Take();
                    Action action = Interlocked.Exchange(ref task.action, null);
                    if (!(action is null))
                    {
                        try
                        {
                            action.Invoke();
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                        task.completed = true;
                    }
                }
            }, source.Token, TaskCreationOptions.LongRunning);
        }

        public void EnsureExecute()
        {
            Action action = Interlocked.Exchange(ref this.action, null);
            if (!(action is null))
            {
                action.Invoke();
                completed = true;
            }
            else
            {
                int sleep = 0;
                while (!completed)
                {
                    sleep = Math.Max(sleep + 5, 50);
                    Thread.Sleep(sleep);
                }
            }
        }

        public static BackgroundTask Enqueue(Action action)
        {
            BackgroundTask task = new BackgroundTask(action);
            task.l = new System.Diagnostics.StackTrace();
            collection.Add(task);
            return task;
        }
    }
}