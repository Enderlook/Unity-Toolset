using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    internal sealed class BackgroundTask
    {
        private static CancellationTokenSource source;
        private static BlockingCollection<BackgroundTask> collection;

        private int id;
        private Action<int, CancellationToken> action;
        private bool completed;

        public BackgroundTask(int id, Action<int, CancellationToken> action)
        {
            this.id = id;
            this.action = action;
        }

        [DidReloadScripts(-1)]
        private static void Execute()
        {
            source?.Cancel();
            collection?.CompleteAdding();
            collection = new BlockingCollection<BackgroundTask>();
            source = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    BackgroundTask task = collection.Take();
                    Action<int, CancellationToken> action = Interlocked.Exchange(ref task.action, null);
                    if (!(action is null))
                    {
                        try
                        {
                            action.Invoke(task.id, source.Token);
                        }
                        catch (Exception exception)
                        {
                            Progress.Finish(task.id, Progress.Status.Failed);
                            Debug.LogException(exception);
                        }
                        task.completed = true;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureExecute()
        {
            if (completed)
                return;

            SlowPath();

            void SlowPath()
            {
                Action<int, CancellationToken> action = Interlocked.Exchange(ref this.action, null);
                if (!(action is null))
                {
                    try
                    {
                        action.Invoke(id, source.Token);
                    }
                    catch (Exception exception)
                    {
                        Progress.Finish(id, Progress.Status.Failed);
                        Debug.LogException(exception);
                    }
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
        }

        public static BackgroundTask Enqueue(Func<CancellationToken, int> initialization, Action<int, CancellationToken> action)
        {
            if (source is null || collection is null) Throw();

            int id = initialization(source.Token);
            BackgroundTask task = new BackgroundTask(id, action);
            collection.Add(task);
            return task;

            void Throw() => throw new InvalidOperationException("Script has not reloaded yet");
        }
    }
}