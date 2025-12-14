using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VRC.SDKBase.Editor
{
    internal class DebouncedCall
    {
        private bool _debounced;
        private DateTime _lastCall;
        private readonly TimeSpan _delay;
        private readonly Action _action;
        private readonly ExecuteMode _mode;

        /// <summary>
        /// Debounce execution mode
        /// </summary>
        internal enum ExecuteMode
        {
            /// <summary>
            /// Always execute on the first call. Consecutive calls are debounced
            /// </summary>
            Start,
            /// <summary>
            /// Only execute after debounce delay
            /// </summary>
            End
        }
        
        /// <summary>
        /// Creates a Debounced action wrapper
        /// </summary>
        /// <param name="delay">How long to wait until executing the call. The actual will always be executed after the specified from the first invocation instead of being delayed more</param>
        /// <param name="action">Action to call</param>
        /// <param name="executeMode">Execution mode</param>
        internal DebouncedCall(TimeSpan delay, Action action, ExecuteMode executeMode)
        {
            _delay = delay;
            _action = action;
            _mode = executeMode;
            
        }

        internal void Invoke()
        {
            if (_debounced) return;

            // If we haven't called the action yet or the delay has passed, call the action
            if (_mode == ExecuteMode.Start && _lastCall.Add(_delay) <= DateTime.Now)
            {
                _lastCall = DateTime.Now;
                _action();
                return;
            }

            _debounced = true;
            // Wait full delay before calling the action
            _lastCall = DateTime.Now;
            
            // Wait for the delay to pass before calling the action
            Task.Run(async () =>
                {
                    await UniTask.WaitUntil(() => _lastCall.Add(_delay) < DateTime.Now);
                    await UniTask.SwitchToMainThread();
                    
                    _debounced = false;
                    _lastCall = DateTime.Now;
                    _action();
                })
                .ConfigureAwait(false);
        }
    }
}