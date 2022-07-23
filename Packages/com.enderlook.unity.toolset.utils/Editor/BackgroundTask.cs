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
#if !UNITY_2020_1_OR_NEWER
        private static readonly Action<CancellationToken> EmptyInitialization = e => { };
#endif

        private static CancellationTokenSource source;
        private static BlockingCollection<BackgroundTask> collection;

#if UNITY_2020_1_OR_NEWER
        private int id;
        private Action<int, CancellationToken> action;
#else
        private Action<CancellationToken> action;
#endif
        private bool completed;

#if UNITY_2020_1_OR_NEWER
        public BackgroundTask(int id, Action<int, CancellationToken> action)
        {
            this.id = id;
#else
        public BackgroundTask(Action<CancellationToken> action)
        {
#endif
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
#if UNITY_2020_1_OR_NEWER
                    Action<int, CancellationToken> action = Interlocked.Exchange(ref task.action, null);
#else
                    Action<CancellationToken> action = Interlocked.Exchange(ref task.action, null);
#endif
                    if (!(action is null))
                    {
                        try
                        {
#if UNITY_2020_1_OR_NEWER
                            action.Invoke(task.id, source.Token);
#else
                            action.Invoke(source.Token);
#endif
                        }
                        catch (Exception exception)
                        {
#if UNITY_2020_1_OR_NEWER
                            Progress.Finish(task.id, Progress.Status.Failed);
#endif
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
#if UNITY_2020_1_OR_NEWER
                Action<int, CancellationToken> action = Interlocked.Exchange(ref this.action, null);
#else
                Action< CancellationToken> action = Interlocked.Exchange(ref this.action, null);
#endif
                if (!(action is null))
                {
                    try
                    {
#if UNITY_2020_1_OR_NEWER
                        action.Invoke(id, source.Token);
#else
                        action.Invoke(source.Token);
#endif
                    }
                    catch (Exception exception)
                    {
#if UNITY_2020_1_OR_NEWER
                        Progress.Finish(id, Progress.Status.Failed);
#endif
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

#if !UNITY_2020_1_OR_NEWER
        public static BackgroundTask Enqueue(Action<CancellationToken> action)
            => Enqueue(EmptyInitialization, action);
#endif

#if UNITY_2020_1_OR_NEWER
        public static BackgroundTask Enqueue(Func<CancellationToken, int> initialization, Action<int, CancellationToken> action)
#else
        public static BackgroundTask Enqueue(Action<CancellationToken> initialization, Action<CancellationToken> action)
#endif
        {
            if (source is null || collection is null) Throw();

#if UNITY_2020_1_OR_NEWER
            int id = initialization(source.Token);
            BackgroundTask task = new BackgroundTask(id, action);
#else
            initialization(source.Token);
            BackgroundTask task = new BackgroundTask(action);
#endif
            collection.Add(task);
            return task;

            void Throw() => throw new InvalidOperationException("Script has not reloaded yet");
        }
    }
}