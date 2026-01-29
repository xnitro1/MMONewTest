using NightBlade.AudioManager;
using static NightBlade.AudioManager.AudioManager;
using UnityEngine;
using System.Collections.Generic;

namespace NightBlade
{
    /// <summary>
    /// Optimized FxCollection with pooling to reduce GC pressure during combat effects.
    /// </summary>
    public partial class FxCollection
    {
        // Pooled arrays to avoid repeated allocations
        private static readonly Stack<ParticleSystem[]> _particleArrayPool = new Stack<ParticleSystem[]>();
        private static readonly Stack<LineRenderer[]> _lineRendererArrayPool = new Stack<LineRenderer[]>();
        private static readonly Stack<AudioSource[]> _audioSourceArrayPool = new Stack<AudioSource[]>();
        private static readonly Stack<AudioSourceSetter[]> _audioSourceSetterArrayPool = new Stack<AudioSourceSetter[]>();
        private static readonly Stack<bool[]> _boolArrayPool = new Stack<bool[]>();

        private const int MaxPoolSize = 16;

        private ParticleSystem[] _particles;
        private LineRenderer[] _lineRenderers;
        private AudioSource[] _audioSources;
        private AudioSourceSetter[] _audioSourceSetters;
        private bool[] _particleDefaultLoops;
        private bool[] _lineRendererDefaultLoops;
        private bool[] _audioSourceDefaultLoops;

        // Pooled FxCollection instances
        private static readonly Stack<FxCollection> _fxCollectionPool = new Stack<FxCollection>();

        /// <summary>
        /// Gets a pooled FxCollection instance.
        /// </summary>
        public static FxCollection GetPooled(GameObject gameObject)
        {
            FxCollection fxCollection;
            if (_fxCollectionPool.Count > 0)
            {
                fxCollection = _fxCollectionPool.Pop();
                fxCollection.Reset(gameObject);
            }
            else
            {
                fxCollection = new FxCollection(gameObject);
            }
            return fxCollection;
        }

        /// <summary>
        /// Returns an FxCollection to the pool.
        /// </summary>
        public static void ReturnPooled(FxCollection fxCollection)
        {
            if (fxCollection != null && _fxCollectionPool.Count < MaxPoolSize)
            {
                fxCollection.Clear();
                _fxCollectionPool.Push(fxCollection);
            }
        }

        private FxCollection(GameObject gameObject)
        {
            InitializeArrays(gameObject);
        }

        private void Reset(GameObject gameObject)
        {
            Clear();
            InitializeArrays(gameObject);
        }

        private void InitializeArrays(GameObject gameObject)
        {
            // Get components once and cache them
            _particles = GetPooledArray<ParticleSystem>(gameObject.GetComponentsInChildren<ParticleSystem>(true));
            _particleDefaultLoops = GetPooledBoolArray(_particles.Length);
            for (int i = 0; i < _particles.Length; ++i)
            {
                _particleDefaultLoops[i] = _particles[i].main.loop;
            }

            _lineRenderers = GetPooledArray<LineRenderer>(gameObject.GetComponentsInChildren<LineRenderer>(true));
            _lineRendererDefaultLoops = GetPooledBoolArray(_lineRenderers.Length);
            for (int i = 0; i < _lineRenderers.Length; ++i)
            {
                _lineRendererDefaultLoops[i] = _lineRenderers[i].loop;
            }

            _audioSources = GetPooledArray<AudioSource>(gameObject.GetComponentsInChildren<AudioSource>(true));
            _audioSourceDefaultLoops = GetPooledBoolArray(_audioSources.Length);
            for (int i = 0; i < _audioSources.Length; ++i)
            {
                _audioSourceDefaultLoops[i] = _audioSources[i].loop;
            }

            _audioSourceSetters = GetPooledArray<AudioSourceSetter>(gameObject.GetComponentsInChildren<AudioSourceSetter>(true));
        }

        private T[] GetPooledArray<T>(T[] sourceArray)
        {
            T[] pooledArray;
            if (GetArrayPool<T>().Count > 0 && GetArrayPool<T>().Peek().Length >= sourceArray.Length)
            {
                pooledArray = GetArrayPool<T>().Pop();
                // Copy source array to pooled array
                System.Array.Copy(sourceArray, pooledArray, sourceArray.Length);
                // Clear any remaining elements
                for (int i = sourceArray.Length; i < pooledArray.Length; ++i)
                {
                    pooledArray[i] = default(T);
                }
            }
            else
            {
                pooledArray = new T[sourceArray.Length];
                System.Array.Copy(sourceArray, pooledArray, sourceArray.Length);
            }
            return pooledArray;
        }

        private bool[] GetPooledBoolArray(int length)
        {
            if (_boolArrayPool.Count > 0 && _boolArrayPool.Peek().Length >= length)
            {
                var array = _boolArrayPool.Pop();
                // Clear array
                for (int i = 0; i < length; ++i)
                {
                    array[i] = false;
                }
                return array;
            }
            return new bool[length];
        }

        private Stack<T[]> GetArrayPool<T>()
        {
            if (typeof(T) == typeof(ParticleSystem))
                return _particleArrayPool as Stack<T[]>;
            if (typeof(T) == typeof(LineRenderer))
                return _lineRendererArrayPool as Stack<T[]>;
            if (typeof(T) == typeof(AudioSource))
                return _audioSourceArrayPool as Stack<T[]>;
            if (typeof(T) == typeof(AudioSourceSetter))
                return _audioSourceSetterArrayPool as Stack<T[]>;
            return null;
        }

        private void Clear()
        {
            // Return arrays to pools
            if (_particles != null)
            {
                ReturnArray(_particles, _particleArrayPool);
                _particles = null;
            }
            if (_lineRenderers != null)
            {
                ReturnArray(_lineRenderers, _lineRendererArrayPool);
                _lineRenderers = null;
            }
            if (_audioSources != null)
            {
                ReturnArray(_audioSources, _audioSourceArrayPool);
                _audioSources = null;
            }
            if (_audioSourceSetters != null)
            {
                ReturnArray(_audioSourceSetters, _audioSourceSetterArrayPool);
                _audioSourceSetters = null;
            }

            // Return bool arrays to pool
            if (_particleDefaultLoops != null && _boolArrayPool.Count < MaxPoolSize)
                _boolArrayPool.Push(_particleDefaultLoops);
            if (_lineRendererDefaultLoops != null && _boolArrayPool.Count < MaxPoolSize)
                _boolArrayPool.Push(_lineRendererDefaultLoops);
            if (_audioSourceDefaultLoops != null && _boolArrayPool.Count < MaxPoolSize)
                _boolArrayPool.Push(_audioSourceDefaultLoops);

            _particleDefaultLoops = null;
            _lineRendererDefaultLoops = null;
            _audioSourceDefaultLoops = null;
        }

        private void ReturnArray<T>(T[] array, Stack<T[]> pool)
        {
            if (array != null && pool.Count < MaxPoolSize)
            {
                pool.Push(array);
            }
        }

        /// <summary>
        /// Gets the current FxCollection pool size for performance monitoring.
        /// </summary>
        public static int GetPoolSize()
        {
            return _fxCollectionPool.Count;
        }

        ~FxCollection()
        {
            _particles?.Nulling();
            _lineRenderers?.Nulling();
            _audioSources?.Nulling();
            _audioSourceSetters?.Nulling();
        }

        public void RevertLoop()
        {
            int i;
            ParticleSystem.MainModule mainEmitter;
            for (i = 0; i < _particles.Length; ++i)
            {
                mainEmitter = _particles[i].main;
                mainEmitter.loop = _particleDefaultLoops[i];
            }
            for (i = 0; i < _lineRenderers.Length; ++i)
            {
                _lineRenderers[i].loop = _lineRendererDefaultLoops[i];
            }
            for (i = 0; i < _audioSources.Length; ++i)
            {
                _audioSources[i].loop = _audioSourceDefaultLoops[i];
            }
        }

        public void SetLoop(bool loop)
        {
            int i;
            ParticleSystem.MainModule mainEmitter;
            for (i = 0; i < _particles.Length; ++i)
            {
                mainEmitter = _particles[i].main;
                mainEmitter.loop = loop;
            }
            for (i = 0; i < _lineRenderers.Length; ++i)
            {
                _lineRenderers[i].loop = loop;
            }
            for (i = 0; i < _audioSources.Length; ++i)
            {
                _audioSources[i].loop = loop;
            }
        }

        public void InitPrefab()
        {
            // Prepare particles
            ParticleSystem.MainModule mainEmitter;
            foreach (ParticleSystem particle in _particles)
            {
                mainEmitter = particle.main;
                mainEmitter.playOnAwake = false;
            }
            // Prepare audio sources
            foreach (AudioSource audioSource in _audioSources)
            {
                audioSource.playOnAwake = false;
            }
            // Prepare audio source setters
            foreach (AudioSourceSetter audioSourceSetter in _audioSourceSetters)
            {
                audioSourceSetter.playOnAwake = false;
                audioSourceSetter.playOnEnable = false;
            }
        }

        public void Play()
        {
            if (Application.isBatchMode)
                return;
            int i;
            // Play particles
            ParticleSystem.MainModule mainEmitter;
            for (i = 0; i < _particles.Length; ++i)
            {
                mainEmitter = _particles[i].main;
                mainEmitter.loop = _particleDefaultLoops[i];
                _particles[i].Play();
            }
            // Revert line renderers loop
            for (i = 0; i < _lineRenderers.Length; ++i)
            {
                _lineRenderers[i].loop = _lineRendererDefaultLoops[i];
            }
            // Play audio sources
            float volume = Singleton == null ? 1f : Singleton.sfxVolumeSetting.Level;
            for (i = 0; i < _audioSources.Length; ++i)
            {
                _audioSources[i].loop = _audioSourceDefaultLoops[i];
                _audioSources[i].volume = volume;
                _audioSources[i].Play();
            }
            // Play audio source setters
            for (i = 0; i < _audioSourceSetters.Length; ++i)
            {
                _audioSourceSetters[i].Play();
            }
        }

        public void Stop()
        {
            if (Application.isBatchMode)
                return;
            int i;
            // Stop particles
            for (i = 0; i < _particles.Length; ++i)
            {
                _particles[i].Stop();
            }
            // Stop audio sources
            for (i = 0; i < _audioSources.Length; ++i)
            {
                _audioSources[i].Stop();
            }
            // Stop audio source setters
            for (i = 0; i < _audioSourceSetters.Length; ++i)
            {
                _audioSourceSetters[i].Stop();
            }
        }
    }
}









