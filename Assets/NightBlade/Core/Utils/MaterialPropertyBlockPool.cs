using System.Collections.Generic;
using UnityEngine;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// MaterialPropertyBlock pooling to reduce GC pressure from material property modifications.
    /// Essential for dynamic material changes on multiple objects to avoid material instance creation.
    /// </summary>
    public static class MaterialPropertyBlockPool
    {
        private static readonly object _lock = new object();
        private static readonly Stack<MaterialPropertyBlock> _pool = new Stack<MaterialPropertyBlock>();
        private const int MaxPoolSize = 16;

        /// <summary>
        /// Gets a MaterialPropertyBlock from the pool or creates a new one.
        /// </summary>
        /// <returns>A clean MaterialPropertyBlock ready for use</returns>
        public static MaterialPropertyBlock Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var block = _pool.Pop();
                    block.Clear();
                    return block;
                }
            }

            return new MaterialPropertyBlock();
        }

        /// <summary>
        /// Returns a MaterialPropertyBlock to the pool for reuse.
        /// </summary>
        /// <param name="block">The MaterialPropertyBlock to return to the pool</param>
        public static void Return(MaterialPropertyBlock block)
        {
            if (block == null) return;

            lock (_lock)
            {
                if (_pool.Count < MaxPoolSize)
                {
                    block.Clear();
                    _pool.Push(block);
                }
            }
        }

        /// <summary>
        /// Gets a MaterialPropertyBlock, executes an action with it, then automatically returns it to the pool.
        /// </summary>
        /// <param name="action">Action to perform with the MaterialPropertyBlock</param>
        public static void Use(System.Action<MaterialPropertyBlock> action)
        {
            var block = Get();
            try
            {
                action(block);
            }
            finally
            {
                Return(block);
            }
        }

        /// <summary>
        /// Sets material properties on a renderer using a pooled MaterialPropertyBlock.
        /// </summary>
        /// <param name="renderer">The renderer to modify</param>
        /// <param name="setupAction">Action to configure the MaterialPropertyBlock</param>
        public static void SetProperties(Renderer renderer, System.Action<MaterialPropertyBlock> setupAction)
        {
            Use(block =>
            {
                setupAction(block);
                renderer.SetPropertyBlock(block);
            });
        }

        /// <summary>
        /// Gets the current pool size for debugging.
        /// </summary>
        public static int PoolSize
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// Clears the pool (useful for testing or memory management).
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }
    }
}