using Enderlook.Collections.LowLevel;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using Progress = UnityEditor.Progress;
#endif

namespace Enderlook.Unity.Toolset.Utils
{
    internal sealed class BackgroundTask
    {
#if !UNITY_2020_1_OR_NEWER
        private static readonly Action<CancellationToken> EmptyInitialization = e => { };
#endif

        private static int globalLock;
        private static RawQueue<BackgroundTask> queue = RawQueue<BackgroundTask>.Create();
        private static bool hasThread;

#if UNITY_2020_1_OR_NEWER
        private readonly int id;
        private Action<int, CancellationToken> action;
#else
        private Action<CancellationToken> action;
#endif
        private CancellationTokenSource source;
        private Guid guid;
        private ManualResetEventSlim slim;
        private bool completed;

#if UNITY_2020_1_OR_NEWER
        public BackgroundTask(Guid guid, int id, Action<int, CancellationToken> action)
        {
            this.id = id;
#else
        public BackgroundTask(Guid guid, Action<CancellationToken> action)
        {
#endif
            this.guid = guid;
            this.action = action;
        }

        private static void Lock()
        {
            while (Interlocked.Exchange(ref globalLock, 1) == 1) ;
        }

        private static void Unlock() => globalLock = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureExecute()
        {
            if (completed)
                return;

            SlowPath();

            void SlowPath()
            {
                if (!Execute())
                {
                    slim = new ManualResetEventSlim();
                    while (!completed)
                        slim.Wait(100);
                    slim = null;
                }
            }
        }

        private void Cancel()
        {
            if (completed)
                return;

#if UNITY_2020_1_OR_NEWER
            Action<int, CancellationToken> action = Interlocked.Exchange(ref this.action, null);
#else
            Action<CancellationToken> action = Interlocked.Exchange(ref this.action, null);
#endif
            if (action is null)
            {
                while (source is null || !completed) ;
                source?.Cancel();
            }
#if UNITY_2020_1_OR_NEWER
            else
                Progress.Finish(id, Progress.Status.Canceled);
#endif
        }

        private bool Execute()
        {
#if UNITY_2020_1_OR_NEWER
            Action<int, CancellationToken> action = Interlocked.Exchange(ref this.action, null);
#else
            Action<CancellationToken> action = Interlocked.Exchange(ref this.action, null);
#endif
            if (!(action is null))
            {
                source = new CancellationTokenSource();
                Exception exception_ = null;
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
                    exception_ = exception;
                }
                completed = true;

                if (exception_ is null)
                {
#if UNITY_2020_1_OR_NEWER
                    Progress.Report(id, 1f);
                    if (source?.IsCancellationRequested ?? false)
                        Progress.Finish(id, Progress.Status.Canceled);
                    else
                        Progress.Finish(id, Progress.Status.Succeeded);
#endif
                }
                else
                {
                    if (exception_ is ThreadAbortException)
                    {
#if UNITY_2020_1_OR_NEWER
                        Progress.Finish(id, Progress.Status.Canceled);
#endif
                    }
                    else
                    {
                        Debug.LogException(exception_);
#if UNITY_2020_1_OR_NEWER
                        Progress.Finish(id, Progress.Status.Failed);
#endif
                    }
                }

                if (!(slim is null))
                    slim.Set();

                source = null;

                return true;
            }

            return false;
        }

#if UNITY_2020_1_OR_NEWER
        public static BackgroundTask Enqueue(Guid guid, int progressId, Action<int, CancellationToken> action)
#else
        public static BackgroundTask Enqueue(Guid guid, Action<CancellationToken> action)
#endif
        {
#if UNITY_2020_1_OR_NEWER
            BackgroundTask task = new BackgroundTask(guid, progressId, action);
#else
            BackgroundTask task = new BackgroundTask(guid, action);
#endif
            bool hasThread_;
            Lock();
            {
                // If the same task is enqueued twice, cancel the old one.
                foreach (BackgroundTask item in queue)
                {
                    if (item.guid == guid)
                    {
                        item.Cancel();
                        break;
                    }
                }

                queue.Enqueue(task);

                hasThread_ = hasThread;
                hasThread = true;
            }
            Unlock();

            if (!hasThread_)
                MakeThread();

            return task;

            void MakeThread() => Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    BackgroundTask task_;
                    bool found;
                    Lock();
                    {
                        found = queue.TryDequeue(out task_);
                        if (!found)
                            hasThread = false;
                    }
                    Unlock();

                    if (!found)
                        break;

                    task_.Execute();
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}