using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace NightBlade
{
    public class PoolDescriptor : MonoBehaviour, IPoolDescriptor
    {
        public IPoolDescriptor ObjectPrefab { get; set; }

        [SerializeField]
        private int poolSize = 30;
        public int PoolSize { get { return poolSize; } set { poolSize = value; } }

        public UnityEvent onInitPrefab = new UnityEvent();
        public UnityEvent onGetInstance = new UnityEvent();
        public UnityEvent onPushBack = new UnityEvent();

        public virtual void InitPrefab()
        {
            onInitPrefab.Invoke();
        }

        public virtual void OnGetInstance()
        {
            onGetInstance.Invoke();
        }

        protected void PushBack(float delay)
        {
            if (delay <= 0f)
            {
                PushBack();
            }
            else
            {
                // Use coroutine-based delay to avoid UniTask allocations
                StartCoroutine(PushBackCoroutine(delay));
            }
        }

        private System.Collections.IEnumerator PushBackCoroutine(float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            PushBack();
        }

        protected virtual void PushBack()
        {
            OnPushBack();
            PoolSystem.PushBack(this);
        }

        protected virtual void OnPushBack()
        {
            onPushBack.Invoke();
        }
    }
}







