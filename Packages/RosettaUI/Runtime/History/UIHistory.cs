﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace RosettaUI
{
    public static class UIHistory
    {
        private static IHistory _globalHistory;
        private static int _recordDelayMs;

        public static void Create<T>(int recordDelayMs = 500)
            where T : IHistory, new()
        {
            _globalHistory = new T();
            _recordDelayMs = recordDelayMs;
        }

        public static void Undo() => _globalHistory.Undo();
        public static void Redo() => _globalHistory.Redo();

        public static Action<Action<T>, T, T> GetHistoryRecorder<T>()
        {
            T firstBefore = default;
            bool hasFirst = false;
            var pushDebounced = Debounce(
                delegate(Action<T> setState, T beforeState, T afterState)
                {
                    hasFirst = false;
                    if (beforeState.Equals(afterState)) return;
                    _globalHistory?.PushRecord(setState, beforeState, afterState);
                }, _recordDelayMs);

            return delegate(Action<T> setState, T beforeState, T afterState)
            {
                if (!hasFirst)
                {
                    firstBefore = beforeState;
                    hasFirst = true;
                }

                pushDebounced(setState, firstBefore, afterState);
            };
        }

        /// <summary>
        /// Returns a function wrapping the given function that only calls the original function after waiting for the
        /// given time. If this function is called again within that time, the countdown is reset. Hence, the wrapped
        /// function will only be called after not being called for the given time.
        /// </summary>
        /// <param name="func">Function to call.</param>
        /// <param name="milliseconds">Time in milliseconds to wait for.</param>
        /// <returns>A function wrapping the original function.</returns>
        private static Action<T0, T1, T2> Debounce<T0, T1, T2>(Action<T0, T1, T2> func, int milliseconds = 300)
        {
            CancellationTokenSource cancelTokenSource = null;

            return (arg1, arg2, arg3) =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func(arg1, arg2, arg3);
                        }
                    }, TaskScheduler.Default);
            };
        }
    }
}
