using UnityEngine;

namespace NightBlade
{
    [DefaultExecutionOrder(DefaultExecutionOrders.CHARACTER_ALIGN_ON_GROUND)]
    public class CharacterAlignOnGround : MonoBehaviour
    {
        public Transform rootBoneTransform;
        public Vector3 rootBoneRotation;
        public float alignOnGroundDistance = 1f;
        public LayerMask alignOnGroundLayerMask = ~0;
        public float alignSpeed = 20f;

        [Tooltip("How often to update ground alignment (lower = better performance, higher = smoother)")]
        [Range(0.02f, 0.2f)]
        public float updateInterval = 0.1f; // Update every 10th of a second instead of every frame

        private Quaternion _aligningQuaternion;
        private float _lastUpdateTime;

        public Transform CacheTransform { get; private set; }
        private void Start()
        {
            CacheTransform = transform;
            _aligningQuaternion = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (rootBoneTransform == null)
                return;

            // Performance optimization: only update periodically instead of every frame
            if (Time.time - _lastUpdateTime < updateInterval)
                return;

            _lastUpdateTime = Time.time;

            RaycastHit raycastHit;
            if (Physics.Raycast(CacheTransform.position, Vector3.down, out raycastHit, alignOnGroundDistance, alignOnGroundLayerMask, QueryTriggerInteraction.Ignore))
                _aligningQuaternion = Quaternion.Slerp(_aligningQuaternion, Quaternion.FromToRotation(Vector3.up, raycastHit.normal), Time.deltaTime * alignSpeed);
            else
                _aligningQuaternion = Quaternion.Slerp(_aligningQuaternion, Quaternion.identity, Time.deltaTime * alignSpeed);
            rootBoneTransform.rotation = _aligningQuaternion * Quaternion.AngleAxis(CacheTransform.eulerAngles.y, Vector3.up) * Quaternion.Euler(rootBoneRotation);
        }
    }
}







