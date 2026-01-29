using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NightBlade
{
    /// <summary>
    /// Test utility for validating message batching system performance
    /// </summary>
    public class MessageBatchingTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Enable automated testing")]
        [SerializeField] private bool enableTesting = false;

        [Tooltip("Number of test messages to send per second")]
        [SerializeField] private int messagesPerSecond = 100;

        [Tooltip("Test duration in seconds")]
        [SerializeField] private float testDuration = 60f;

        [Tooltip("Use batching for test messages")]
        [SerializeField] private bool useBatching = true;

        [Header("Test Results")]
        [SerializeField] private bool isTesting = false;
        [SerializeField] private float testProgress = 0f;
        [SerializeField] private int messagesSent = 0;
        [SerializeField] private float messagesPerSecondActual = 0f;

        // Test data
        private float _testStartTime;
        private Coroutine _testCoroutine;
        private List<long> _testConnections = new List<long> { 1, 2, 3, 4, 5 }; // Mock connection IDs

        private BaseGameNetworkManager _networkManager;
        private MessageBatchingProfiler _profiler;

        private void Awake()
        {
            _networkManager = GetComponent<BaseGameNetworkManager>();
            _profiler = GetComponent<MessageBatchingProfiler>();
        }

        private void Update()
        {
            if (enableTesting && !isTesting)
            {
                StartTest();
            }
            else if (!enableTesting && isTesting)
            {
                StopTest();
            }

            if (isTesting)
            {
                testProgress = (Time.time - _testStartTime) / testDuration;
                messagesPerSecondActual = messagesSent / (Time.time - _testStartTime);
            }
        }

        [ContextMenu("Start Performance Test")]
        public void StartTest()
        {
            if (isTesting) return;

            Debug.Log("Starting Message Batching Performance Test...");
            isTesting = true;
            _testStartTime = Time.time;
            messagesSent = 0;
            testProgress = 0f;

            if (_profiler != null)
            {
                _profiler.ResetStats();
            }

            _testCoroutine = StartCoroutine(RunTest());
        }

        [ContextMenu("Stop Performance Test")]
        public void StopTest()
        {
            if (!isTesting) return;

            Debug.Log("Stopping Message Batching Performance Test...");
            if (_testCoroutine != null)
            {
                StopCoroutine(_testCoroutine);
                _testCoroutine = null;
            }

            isTesting = false;
            LogTestResults();
        }

        private IEnumerator RunTest()
        {
            float messageInterval = 1f / messagesPerSecond;
            float nextMessageTime = Time.time;

            while (Time.time - _testStartTime < testDuration)
            {
                if (Time.time >= nextMessageTime)
                {
                    SendTestMessage();
                    messagesSent++;
                    nextMessageTime += messageInterval;
                }

                yield return null;
            }

            StopTest();
        }

        private void SendTestMessage()
        {
            // Rotate through test connections
            long connectionId = _testConnections[messagesSent % _testConnections.Count];

            if (useBatching && _networkManager != null)
            {
                // Send a reward message using batching
                var rewardMessage = new GameMessage { message = UITextKeys.UI_ERROR_UNKNOW };
                _networkManager.SendMessageBatched(connectionId, LiteNetLib.DeliveryMethod.ReliableOrdered,
                    GameNetworkingConsts.GameMessage, rewardMessage, MessagePriority.Medium);
            }
            else if (_networkManager != null)
            {
                // Send directly without batching
                var rewardMessage = new GameMessage { message = UITextKeys.UI_ERROR_UNKNOW };
                _networkManager.ServerSendPacket(connectionId, 0, LiteNetLib.DeliveryMethod.ReliableOrdered,
                    GameNetworkingConsts.GameMessage, rewardMessage);

                // Record for profiling comparison
                if (_profiler != null)
                {
                    _profiler.RecordMessageWithoutBatching(50); // Estimate 50 bytes per message
                }
            }
        }

        private void LogTestResults()
        {
            float totalTime = Time.time - _testStartTime;
            float actualMPS = messagesSent / totalTime;

            Debug.Log($"=== Message Batching Test Results ===");
            Debug.Log($"Test Duration: {totalTime:F1} seconds");
            Debug.Log($"Messages Sent: {messagesSent}");
            Debug.Log($"Target MPS: {messagesPerSecond}, Actual MPS: {actualMPS:F1}");
            Debug.Log($"Batching Enabled: {useBatching}");

            if (_profiler != null)
            {
                var stats = _profiler.GetStats();
                Debug.Log($"Batching Stats: {stats}");
                Debug.Log($"Bandwidth Savings: {stats.BandwidthSavingsPercent:F1}%");
                Debug.Log($"Average Messages per Batch: {stats.AverageMessagesPerBatch:F1}");
            }
        }

        [ContextMenu("Run Character State Test")]
        public void RunCharacterStateTest()
        {
            if (_networkManager == null || _networkManager.CharacterStateBatcher == null)
            {
                Debug.LogError("Character state batcher not available");
                return;
            }

            Debug.Log("Running Character State Batching Test...");

            // Simulate character movement updates
            for (int i = 0; i < 10; i++)
            {
                string characterId = $"TestCharacter_{i}";
                Vector3 position = new Vector3(i * 2f, 0, i * 2f);
                Quaternion rotation = Quaternion.Euler(0, i * 36f, 0);

                // This would normally be called from character movement scripts
                _networkManager.CharacterStateBatcher.QueueCharacterStateUpdate(
                    characterId, position, rotation, 1);
            }

            Debug.Log("Character state updates queued. Check profiler for batching results.");
        }

        [ContextMenu("Flush All Batches")]
        public void FlushAllBatches()
        {
            if (_networkManager != null)
            {
                _networkManager.FlushAllMessageBatches();
                if (_networkManager.CharacterStateBatcher != null)
                {
                    _networkManager.CharacterStateBatcher.FlushCharacterStateUpdates();
                }
            }
            Debug.Log("All message batches flushed");
        }

        [ContextMenu("Log Current Stats")]
        public void LogCurrentStats()
        {
            if (_profiler != null)
            {
                _profiler.LogPerformanceStats();
            }
            else
            {
                Debug.Log("Profiler not available");
            }
        }
    }
}