using System;
using System.Collections.Generic;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Delegate pooling to reduce GC pressure from anonymous method and lambda allocations.
    /// Particularly useful for temporary event handlers and callback systems.
    /// </summary>
    public static class DelegatePool
    {
        private static readonly object _lock = new object();
        private static readonly Stack<Action> _actionPool = new Stack<Action>();
        private static readonly Stack<Action<object>> _actionObjPool = new Stack<Action<object>>();
        private const int MaxPoolSize = 8;

        /// <summary>
        /// Gets a reusable Action delegate from the pool.
        /// </summary>
        /// <param name="action">The action to wrap</param>
        /// <returns>A pooled delegate that can be invoked safely</returns>
        public static Action GetAction(Action action)
        {
            if (action == null) return null;

            lock (_lock)
            {
                if (_actionPool.Count > 0)
                {
                    var pooledAction = _actionPool.Pop();
                    // We can't reassign the underlying action, so return new wrapper
                }
            }

            // For now, return the original action
            // In a more advanced implementation, we could use a wrapper that gets reused
            return action;
        }

        /// <summary>
        /// Gets a reusable Action&lt;object&gt; delegate from the pool.
        /// </summary>
        /// <param name="action">The action to wrap</param>
        /// <returns>A pooled delegate that can be invoked safely</returns>
        public static Action<object> GetAction(Action<object> action)
        {
            if (action == null) return null;

            lock (_lock)
            {
                if (_actionObjPool.Count > 0)
                {
                    var pooledAction = _actionObjPool.Pop();
                    // We can't reassign the underlying action, so return new wrapper
                }
            }

            return action;
        }

        /// <summary>
        /// Returns an Action delegate to the pool for reuse.
        /// </summary>
        /// <param name="action">The delegate to return (not used in current implementation)</param>
        public static void ReturnAction(Action action)
        {
            if (action == null) return;

            lock (_lock)
            {
                if (_actionPool.Count < MaxPoolSize)
                {
                    _actionPool.Push(action);
                }
            }
        }

        /// <summary>
        /// Returns an Action&lt;object&gt; delegate to the pool for reuse.
        /// </summary>
        /// <param name="action">The delegate to return (not used in current implementation)</param>
        public static void ReturnAction(Action<object> action)
        {
            if (action == null) return;

            lock (_lock)
            {
                if (_actionObjPool.Count < MaxPoolSize)
                {
                    _actionObjPool.Push(action);
                }
            }
        }

        /// <summary>
        /// Advanced delegate pooling using a wrapper pattern for better reuse.
        /// </summary>
        public static class Advanced
        {
            private static readonly object _lock = new object();
            private static readonly Stack<PooledAction> _pooledActions = new Stack<PooledAction>();
            private static readonly Stack<PooledAction<object>> _pooledActionsObj = new Stack<PooledAction<object>>();
            private const int MaxPoolSize = 16;

            /// <summary>
            /// Gets a pooled action wrapper that can be reused.
            /// </summary>
            /// <param name="action">The action to execute</param>
            /// <returns>A reusable pooled action</returns>
            public static PooledAction Get(Action action)
            {
                lock (_lock)
                {
                    if (_pooledActions.Count > 0)
                    {
                        var pooled = _pooledActions.Pop();
                        pooled.SetAction(action);
                        return pooled;
                    }
                }

                return new PooledAction(action);
            }

            /// <summary>
            /// Gets a pooled action wrapper that can be reused.
            /// </summary>
            /// <param name="action">The action to execute</param>
            /// <returns>A reusable pooled action</returns>
            public static PooledAction<object> Get(Action<object> action)
            {
                lock (_lock)
                {
                    if (_pooledActionsObj.Count > 0)
                    {
                        var pooled = _pooledActionsObj.Pop();
                        pooled.SetAction(action);
                        return pooled;
                    }
                }

                return new PooledAction<object>(action);
            }

            /// <summary>
            /// Returns a pooled action to the pool for reuse.
            /// </summary>
            /// <param name="pooledAction">The pooled action to return</param>
            public static void Return(PooledAction pooledAction)
            {
                if (pooledAction == null) return;

                lock (_lock)
                {
                    if (_pooledActions.Count < MaxPoolSize)
                    {
                        pooledAction.ClearAction();
                        _pooledActions.Push(pooledAction);
                    }
                }
            }

            /// <summary>
            /// Returns a pooled action to the pool for reuse.
            /// </summary>
            /// <param name="pooledAction">The pooled action to return</param>
            public static void Return(PooledAction<object> pooledAction)
            {
                if (pooledAction == null) return;

                lock (_lock)
                {
                    if (_pooledActionsObj.Count < MaxPoolSize)
                    {
                        pooledAction.ClearAction();
                        _pooledActionsObj.Push(pooledAction);
                    }
                }
            }

            /// <summary>
            /// Gets the current pool sizes for debugging.
            /// </summary>
            public static (int actionPool, int actionObjPool) PoolSizes
            {
                get
                {
                    lock (_lock)
                    {
                        return (_pooledActions.Count, _pooledActionsObj.Count);
                    }
                }
            }
        }

        /// <summary>
        /// A reusable action wrapper for advanced pooling.
        /// </summary>
        public class PooledAction
        {
            private Action _action;

            public PooledAction(Action action = null)
            {
                _action = action;
            }

            public void SetAction(Action action)
            {
                _action = action;
            }

            public void ClearAction()
            {
                _action = null;
            }

            public void Invoke()
            {
                _action?.Invoke();
            }
        }

        /// <summary>
        /// A reusable action wrapper for advanced pooling with parameters.
        /// </summary>
        public class PooledAction<T>
        {
            private Action<T> _action;

            public PooledAction(Action<T> action = null)
            {
                _action = action;
            }

            public void SetAction(Action<T> action)
            {
                _action = action;
            }

            public void ClearAction()
            {
                _action = null;
            }

            public void Invoke(T param)
            {
                _action?.Invoke(param);
            }
        }
    }
}