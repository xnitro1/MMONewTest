using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBlade.Core.UI
{
    public class UIInstanceInfo : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI instanceNameText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private TextMeshProUGUI loadPercentageText;
        [SerializeField] private Image loadBarImage;
        [SerializeField] private GameObject recommendedBadge;

        [Header("Colors")]
        [SerializeField] private Color lowLoadColor = Color.green;
        [SerializeField] private Color mediumLoadColor = Color.yellow;
        [SerializeField] private Color highLoadColor = Color.red;

        private string _instanceId;
        private int _playerCount;
        private int _maxPlayers;
        private bool _isRecommended;

        public void SetInstanceInfo(string instanceId, int playerCount, int maxPlayers, bool isRecommended = false)
        {
            _instanceId = instanceId;
            _playerCount = playerCount;
            _maxPlayers = maxPlayers;
            _isRecommended = isRecommended;

            UpdateDisplay();
        }

        public string InstanceId => _instanceId;
        public float LoadPercentage => _maxPlayers > 0 ? (float)_playerCount / _maxPlayers : 0f;
        public bool IsRecommended => _isRecommended;

        private void UpdateDisplay()
        {
            if (instanceNameText != null)
            {
                // Extract a friendly name from instance ID (e.g., "ForestRealm_abc123" -> "Forest Realm #1")
                string friendlyName = GetFriendlyInstanceName(_instanceId);
                instanceNameText.text = friendlyName;
            }

            if (playerCountText != null)
            {
                playerCountText.text = $"{_playerCount}/{_maxPlayers}";
            }

            float loadPercentage = LoadPercentage;
            if (loadPercentageText != null)
            {
                loadPercentageText.text = $"{loadPercentage:P0}";
            }

            if (loadBarImage != null)
            {
                loadBarImage.fillAmount = loadPercentage;
                loadBarImage.color = GetLoadColor(loadPercentage);
            }

            if (recommendedBadge != null)
            {
                recommendedBadge.SetActive(_isRecommended);
            }
        }

        private string GetFriendlyInstanceName(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return "Unknown Instance";

            // Parse instance ID like "ForestRealm_abc123" -> "Forest Realm #1"
            int underscoreIndex = instanceId.LastIndexOf('_');
            if (underscoreIndex > 0)
            {
                string mapName = instanceId.Substring(0, underscoreIndex);
                string instanceSuffix = instanceId.Substring(underscoreIndex + 1);

                // Convert PascalCase to readable text
                mapName = System.Text.RegularExpressions.Regex.Replace(mapName, "([a-z])([A-Z])", "$1 $2");

                return $"{mapName} #{(instanceSuffix.Length > 3 ? instanceSuffix.Substring(0, 3) : instanceSuffix)}";
            }

            return instanceId;
        }

        private Color GetLoadColor(float loadPercentage)
        {
            if (loadPercentage < 0.5f)
                return lowLoadColor;
            else if (loadPercentage < 0.8f)
                return mediumLoadColor;
            else
                return highLoadColor;
        }

        public void SetRecommended(bool recommended)
        {
            _isRecommended = recommended;
            if (recommendedBadge != null)
            {
                recommendedBadge.SetActive(recommended);
            }
        }
    }
}