using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NightBlade.UI.HUD
{
    /// <summary>
    /// Modern target frame for showing selected entity info.
    /// Clean, informative, and sexy. 🎯
    /// 
    /// Features:
    /// - Entity name, level, type
    /// - Health bar (smooth, color-coded)
    /// - Entity icon/portrait
    /// - Status effects/buffs
    /// - Distance indicator
    /// - Event-driven (no Update() spam!)
    /// 
    /// Design Philosophy:
    /// - Show when target selected
    /// - Hide when target cleared
    /// - Update only when data changes
    /// </summary>
    public class UITargetFrame : UIBase
    {
        #region Serialized Fields
        
        [Header("Target Frame Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image typeIcon; // Enemy, NPC, Player, etc.
        [SerializeField] private UIResourceBar healthBar;
        [SerializeField] private Transform buffsContainer;
        
        [Header("Type Icons")]
        [SerializeField] private Sprite enemyIcon;
        [SerializeField] private Sprite npcIcon;
        [SerializeField] private Sprite playerIcon;
        [SerializeField] private Sprite bossIcon;
        
        [Header("Settings")]
        [SerializeField] private Color enemyNameColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color npcNameColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color playerNameColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] private Color bossNameColor = new Color(1f, 0.5f, 0f);
        
        [Tooltip("Should distance update every frame?")]
        [SerializeField] private bool updateDistanceLive = false;
        
        [Tooltip("Distance update interval (if updateDistanceLive is true)")]
        [SerializeField] private float distanceUpdateInterval = 0.5f;
        
        #endregion
        
        #region State
        
        private DamageableEntity currentTarget;
        private float lastDistanceUpdateTime;
        
        public enum EntityType
        {
            Enemy,
            NPC,
            Player,
            Boss
        }
        
        #endregion
        
        #region Properties
        
        public DamageableEntity CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Start hidden
            Hide(animate: false);
        }
        
        private void Update()
        {
            // Only update distance if enabled and we have a target
            if (updateDistanceLive && HasTarget && distanceText != null)
            {
                if (Time.time - lastDistanceUpdateTime >= distanceUpdateInterval)
                {
                    UpdateDistance();
                    lastDistanceUpdateTime = Time.time;
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set the current target and display info
        /// </summary>
        public void SetTarget(DamageableEntity entity)
        {
            if (entity == null)
            {
                ClearTarget();
                return;
            }
            
            currentTarget = entity;
            
            // Update all UI elements
            UpdateName();
            UpdateLevel();
            UpdateHealth();
            UpdatePortrait();
            UpdateType();
            UpdateDistance();
            
            // Show the frame
            Show();
        }
        
        /// <summary>
        /// Clear current target and hide frame
        /// </summary>
        public void ClearTarget()
        {
            currentTarget = null;
            Hide();
        }
        
        /// <summary>
        /// Update health bar (call when target's health changes)
        /// </summary>
        public void UpdateHealth()
        {
            if (!HasTarget || healthBar == null)
                return;
            
            int currentHp = currentTarget.CurrentHp;
            int maxHp = currentTarget.MaxHp;
            
            healthBar.UpdateResource(currentHp, maxHp);
        }
        
        /// <summary>
        /// Update distance display (call when player/target moves)
        /// </summary>
        public void UpdateDistance()
        {
            if (!HasTarget || distanceText == null)
                return;
            
            if (GameInstance.PlayingCharacterEntity != null)
            {
                float distance = Vector3.Distance(
                    GameInstance.PlayingCharacterEntity.transform.position,
                    currentTarget.transform.position
                );
                
                distanceText.text = $"{distance:F1}m";
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateName()
        {
            if (!HasTarget || nameText == null)
                return;
            
            string entityName = currentTarget.Title;
            nameText.text = entityName;
            
            // Color-code by type
            EntityType type = DetermineEntityType();
            nameText.color = GetTypeColor(type);
        }
        
        private void UpdateLevel()
        {
            if (!HasTarget || levelText == null)
                return;
            
            // Level is only on character entities
            if (currentTarget is BaseCharacterEntity characterEntity)
            {
                int level = characterEntity.Level;
                levelText.text = $"Lv. {level}";
                
                // Color-code by level difference
                if (GameInstance.PlayingCharacterEntity != null)
                {
                    int playerLevel = GameInstance.PlayingCharacterEntity.Level;
                    int levelDiff = level - playerLevel;
                    
                    if (levelDiff >= 5)
                        levelText.color = new Color(1f, 0.2f, 0.2f); // Much higher = red
                    else if (levelDiff >= 2)
                        levelText.color = new Color(1f, 0.7f, 0.2f); // Higher = orange
                    else if (levelDiff <= -5)
                        levelText.color = new Color(0.5f, 0.5f, 0.5f); // Much lower = gray
                    else
                        levelText.color = Color.white; // Similar level = white
                }
            }
            else
            {
                // Non-character entities (buildings, harvestables) have no level
                levelText.text = "";
            }
        }
        
        private void UpdatePortrait()
        {
            if (!HasTarget || portraitImage == null)
                return;
            
            // TODO: Load entity portrait/icon
            // For now, just show a placeholder
            // portraitImage.sprite = currentTarget.GetPortraitSprite();
        }
        
        private void UpdateType()
        {
            if (!HasTarget || typeIcon == null)
                return;
            
            EntityType type = DetermineEntityType();
            typeIcon.sprite = GetTypeIcon(type);
        }
        
        private EntityType DetermineEntityType()
        {
            if (!HasTarget)
                return EntityType.Enemy;
            
            // Determine entity type based on properties
            if (currentTarget is BasePlayerCharacterEntity)
            {
                return EntityType.Player;
            }
            else if (currentTarget is BaseMonsterCharacterEntity monsterEntity)
            {
                // Check if boss (you can add boss detection logic here)
                // For example: if (monsterEntity.IsBoss) return EntityType.Boss;
                return EntityType.Enemy;
            }
            else if (currentTarget is NpcEntity)
            {
                return EntityType.NPC;
            }
            
            return EntityType.Enemy;
        }
        
        private Color GetTypeColor(EntityType type)
        {
            switch (type)
            {
                case EntityType.Enemy:
                    return enemyNameColor;
                case EntityType.NPC:
                    return npcNameColor;
                case EntityType.Player:
                    return playerNameColor;
                case EntityType.Boss:
                    return bossNameColor;
                default:
                    return Color.white;
            }
        }
        
        private Sprite GetTypeIcon(EntityType type)
        {
            switch (type)
            {
                case EntityType.Enemy:
                    return enemyIcon;
                case EntityType.NPC:
                    return npcIcon;
                case EntityType.Player:
                    return playerIcon;
                case EntityType.Boss:
                    return bossIcon;
                default:
                    return enemyIcon;
            }
        }
        
        #endregion
        
        #region UIBase Overrides
        
        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            currentTarget = null;
        }
        
        protected override void OnReturnToPool()
        {
            base.OnReturnToPool();
            currentTarget = null;
        }
        
        #endregion
    }
}
