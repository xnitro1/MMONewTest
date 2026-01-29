using UnityEngine;
using UnityEngine.AI;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Example AI controller that uses distance-based update frequency.
    /// Demonstrates how to apply the DistanceBasedUpdater pattern to AI systems.
    /// </summary>
    public class DistanceBasedAIController : DistanceBasedUpdater
    {
        [Header("AI Configuration")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float chaseRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3f;

        [Header("Tier-Specific Behavior")]
        [SerializeField] private bool[] canWanderByTier = { true, true, false, false, false }; // Updated for 5 tiers
        [SerializeField] private bool[] canChaseByTier = { true, true, true, false, false };   // Updated for 5 tiers
        [SerializeField] private bool[] canAttackByTier = { true, true, false, false, false }; // Updated for 5 tiers

        private NavMeshAgent navAgent;
        private Transform target;
        private Vector3 wanderTarget;
        private float lastWanderTime;

        public enum AIState { Idle, Wandering, Chasing, Attacking }
        private AIState currentState = AIState.Idle;

        protected override void Start()
        {
            base.Start();

            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
                navAgent.stoppingDistance = attackRange * 0.8f;
            }

            // Set initial wander target
            SetRandomWanderTarget();
        }

        protected override void PerformUpdate()
        {
            if (navAgent == null || !navAgent.isOnNavMesh) return;

            int currentTier = GetCurrentTier();

            // Update target detection
            FindTarget();

            // State machine with tier-based restrictions
            switch (currentState)
            {
                case AIState.Idle:
                    HandleIdleState(currentTier);
                    break;

                case AIState.Wandering:
                    HandleWanderingState(currentTier);
                    break;

                case AIState.Chasing:
                    HandleChasingState(currentTier);
                    break;

                case AIState.Attacking:
                    HandleAttackingState(currentTier);
                    break;
            }
        }

        private void FindTarget()
        {
            // Simple target finding - in real implementation, use a more sophisticated system
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                    if (distanceToPlayer <= chaseRange)
                    {
                        target = player.transform;
                    }
                }
            }
        }

        private void HandleIdleState(int tier)
        {
            // Always allow transitioning from idle
            if (target != null && canChaseByTier[Mathf.Min(tier, canChaseByTier.Length - 1)])
            {
                currentState = AIState.Chasing;
                navAgent.SetDestination(target.position);
            }
            else if (canWanderByTier[Mathf.Min(tier, canWanderByTier.Length - 1)])
            {
                currentState = AIState.Wandering;
                SetRandomWanderTarget();
            }
        }

        private void HandleWanderingState(int tier)
        {
            // Check for chase opportunity
            if (target != null && canChaseByTier[Mathf.Min(tier, canChaseByTier.Length - 1)])
            {
                currentState = AIState.Chasing;
                navAgent.SetDestination(target.position);
                return;
            }

            // Continue wandering if allowed
            if (!canWanderByTier[Mathf.Min(tier, canWanderByTier.Length - 1)])
            {
                currentState = AIState.Idle;
                navAgent.ResetPath();
                return;
            }

            // Check if reached wander target
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                // Set new wander target
                SetRandomWanderTarget();
            }
        }

        private void HandleChasingState(int tier)
        {
            // Check if target is still valid
            if (target == null)
            {
                currentState = AIState.Idle;
                navAgent.ResetPath();
                return;
            }

            // Check distance to target
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange && canAttackByTier[Mathf.Min(tier, canAttackByTier.Length - 1)])
            {
                // Enter attack range
                currentState = AIState.Attacking;
                navAgent.ResetPath();
            }
            else if (distanceToTarget > chaseRange)
            {
                // Target too far, stop chasing
                target = null;
                currentState = AIState.Idle;
                navAgent.ResetPath();
            }
            else
            {
                // Continue chasing if allowed
                if (canChaseByTier[Mathf.Min(tier, canChaseByTier.Length - 1)])
                {
                    navAgent.SetDestination(target.position);
                }
                else
                {
                    // Can't chase at this tier, go back to idle
                    currentState = AIState.Idle;
                    navAgent.ResetPath();
                }
            }
        }

        private void HandleAttackingState(int tier)
        {
            // Check if target is still valid and in range
            if (target == null)
            {
                currentState = AIState.Idle;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget > attackRange)
            {
                // Target moved out of range
                if (canChaseByTier[Mathf.Min(tier, canChaseByTier.Length - 1)])
                {
                    currentState = AIState.Chasing;
                    navAgent.SetDestination(target.position);
                }
                else
                {
                    currentState = AIState.Idle;
                }
            }
            else
            {
                // Still in range - perform attack if allowed
                if (canAttackByTier[Mathf.Min(tier, canAttackByTier.Length - 1)])
                {
                    PerformAttack();
                }
                else
                {
                    // Can't attack at this tier, chase instead
                    currentState = AIState.Chasing;
                    navAgent.SetDestination(target.position);
                }
            }
        }

        private void PerformAttack()
        {
            // Placeholder attack logic - replace with your actual attack system
            Debug.Log($"{gameObject.name} attacks {target.name}");

            // Add attack cooldown, damage calculation, etc.
            // This is where you'd integrate with your combat system
        }

        private void SetRandomWanderTarget()
        {
            // Find random point within wander radius
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
                navAgent.SetDestination(wanderTarget);
            }
        }

        /// <summary>
        /// Gets AI-specific statistics.
        /// </summary>
        public new DistanceBasedAIStats GetStats()
        {
            return new DistanceBasedAIStats
            {
                BaseStats = base.GetStats(),
                CurrentState = currentState,
                HasTarget = target != null,
                DistanceToTarget = target != null ? Vector3.Distance(transform.position, target.position) : 0f,
                IsMoving = navAgent != null && navAgent.velocity.magnitude > 0.1f
            };
        }

        /// <summary>
        /// Extended statistics for AI monitoring.
        /// </summary>
        public struct DistanceBasedAIStats
        {
            public DistanceUpdateStats BaseStats;
            public AIState CurrentState;
            public bool HasTarget;
            public float DistanceToTarget;
            public bool IsMoving;
        }

        protected override void DrawDebugInfo()
        {
            base.DrawDebugInfo();

#if UNITY_EDITOR
            // Draw AI state
            GUI.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3.5f,
                $"State: {currentState}\nTarget: {(target != null ? "Yes" : "No")}");
#endif

            // Draw behavior ranges
            if (showDebugInfo)
            {
                DrawDebugRanges();
            }
        }

        private void DrawDebugRanges()
        {
            // Draw chase range
            DebugDrawCircle(transform.position, chaseRange, Color.yellow, 32);

            // Draw attack range
            DebugDrawCircle(transform.position, attackRange, Color.red, 16);

            // Draw wander radius
            DebugDrawCircle(transform.position, wanderRadius, Color.cyan, 24);
        }

        private void DebugDrawCircle(Vector3 center, float radius, Color color, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Debug.DrawLine(prevPoint, point, color, 0.1f);
                prevPoint = point;
            }
        }
    }
}
