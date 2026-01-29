using Cysharp.Threading.Tasks;
using NightBlade.AddressableAssetTools;
using NightBlade.CameraAndInput;
using NightBlade.Core.Utils;
using NightBlade.DevExtension;
using NightBlade.UI.Utils.Pooling;
using NightBlade.UnityEditorUtils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
using UnityEngine.Purchasing;
#endif

namespace NightBlade
{
    public enum InventorySystem
    {
        Simple,
        LimitSlots,
    }

    public enum CurrentPositionSaveMode
    {
        UseCurrentPosition,
        UseRespawnPosition
    }

    public enum PlayerDropItemMode
    {
        DropOnGround,
        DestroyItem,
    }

    public enum DeadDropItemMode
    {
        DropOnGround,
        CorpseLooting,
    }

    public enum RewardingItemMode
    {
        DropOnGround,
        CorpseLooting,
        Immediately,
    }

    public enum RewardingMode
    {
        Immediately,
        DropOnGround,
    }

    public enum GoldStoreMode
    {
        Default,
        UserGoldOnly,
    }

    public enum TestInEditorMode
    {
        Standalone,
        Mobile,
        MobileWithKeyInputs,
        Console,
    }

    [DefaultExecutionOrder(DefaultExecutionOrders.GAME_INSTANCE)]
    [RequireComponent(typeof(EventSystemManager))]
    public partial class GameInstance : MonoBehaviour
    {
        public const string KEY_REFRESH_TOKEN = "__REFRESH_TOKEN";

        public static readonly string LogTag = nameof(GameInstance);
        public static GameInstance Singleton { get; protected set; }

        /// <summary>
        /// Ensures a GameInstance exists in the scene. Creates one if necessary.
        /// This is useful for scenes that don't have GameInstance manually placed.
        /// </summary>
        public static GameInstance EnsureExists()
        {
            if (Singleton != null)
                return Singleton;

            // Look for existing GameInstance in scene
            GameInstance existing = FindObjectOfType<GameInstance>();
            if (existing != null)
                return existing;

            // Create new GameInstance
            Debug.Log("[GameInstance] Auto-creating GameInstance as it was not found in scene...");
            GameObject go = new GameObject("GameInstance");
            GameInstance instance = go.AddComponent<GameInstance>();
            DontDestroyOnLoad(go);
            return instance;
        }
        public static IClientCashShopHandlers ClientCashShopHandlers { get; set; }
        public static IClientMailHandlers ClientMailHandlers { get; set; }
        public static IClientCharacterHandlers ClientCharacterHandlers { get; set; }
        public static IClientInventoryHandlers ClientInventoryHandlers { get; set; }

        // Validation optimization flag
        private static bool _gameDataValidated = false;
        public static IClientStorageHandlers ClientStorageHandlers { get; set; }
        public static IClientPartyHandlers ClientPartyHandlers { get; set; }
        public static IClientGuildHandlers ClientGuildHandlers { get; set; }
        public static IClientGachaHandlers ClientGachaHandlers { get; set; }
        public static IClientFriendHandlers ClientFriendHandlers { get; set; }
        public static IClientBankHandlers ClientBankHandlers { get; set; }
        public static IClientOnlineCharacterHandlers ClientOnlineCharacterHandlers { get; set; }
        public static IClientChatHandlers ClientChatHandlers { get; set; }
        public static IServerMailHandlers ServerMailHandlers { get; set; }
        public static IServerUserHandlers ServerUserHandlers { get; set; }
        public static IServerBuildingHandlers ServerBuildingHandlers { get; set; }
        public static IServerGameMessageHandlers ServerGameMessageHandlers { get; set; }
        public static IServerCharacterHandlers ServerCharacterHandlers { get; set; }
        public static IServerStorageHandlers ServerStorageHandlers { get; set; }
        public static IServerPartyHandlers ServerPartyHandlers { get; set; }
        public static IServerGuildHandlers ServerGuildHandlers { get; set; }
        public static IServerChatHandlers ServerChatHandlers { get; set; }
        public static IServerLogHandlers ServerLogHandlers { get; set; }
        public static IItemUIVisibilityManager ItemUIVisibilityManager { get; set; }
        public static IItemsContainerUIVisibilityManager ItemsContainerUIVisibilityManager { get; set; }
        public static ICustomSummonManager CustomSummonManager { get; set; }
        public static string UserId { get; set; }
        public static string AccessToken { get; set; }
        public static string RefreshToken
        {
            get => PlayerPrefs.GetString(KEY_REFRESH_TOKEN, string.Empty);
            set => PlayerPrefs.SetString(KEY_REFRESH_TOKEN, value);
        }
        public static string SelectedCharacterId { get; set; }
        private static IPlayerCharacterData s_playingCharacter;
        public static IPlayerCharacterData PlayingCharacter
        {
            get { return s_playingCharacter; }
            set
            {
                s_playingCharacter = value;
                if (OnSetPlayingCharacterEvent != null)
                    OnSetPlayingCharacterEvent.Invoke(value);
            }
        }
        public static BasePlayerCharacterEntity PlayingCharacterEntity { get { return PlayingCharacter as BasePlayerCharacterEntity; } }
        public static PartyData JoinedParty { get; set; }
        public static GuildData JoinedGuild { get; set; }
        public static Dictionary<StorageId, List<CharacterItem>> OpenedStorages { get; set; } = new Dictionary<StorageId, List<CharacterItem>>();

        [Header("Gameplay Systems")]
        [SerializeField]
        private BaseMessageManager messageManager = null;
        [SerializeField]
        private BaseGameSaveSystem saveSystem = null;
        [SerializeField]
        private BaseGameplayRule gameplayRule = null;

        /// <summary>
        /// Public access to the gameplay rule. Can be assigned in editor or at runtime.
        /// </summary>
        public BaseGameplayRule GameplayRule
        {
            get { return gameplayRule; }
            set { gameplayRule = value; }
        }
        [SerializeField]
        private BaseInventoryManager inventoryManager = null;
        [SerializeField]
        private BaseDayNightTimeUpdater dayNightTimeUpdater = null;
        [SerializeField]
        private BaseGMCommands gmCommands = null;
        [SerializeField]
        private BaseEquipmentModelBonesSetupManager equipmentModelBonesSetupManager = null;
        [SerializeField]
        private NetworkSetting networkSetting = null;

        [Header("Gameplay Objects")]
        [SerializeField]
        private BaseItem expDropRepresentItem = null;
        [SerializeField]
        private BaseItem goldDropRepresentItem = null;
        [SerializeField]
        private CurrencyItemPair[] currencyDropRepresentItems = new CurrencyItemPair[0];
#if UNITY_EDITOR && EXCLUDE_PREFAB_REFS
        public UnityHelpBox entityHelpBox = new UnityHelpBox("`EXCLUDE_PREFAB_REFS` is set, you have to use only addressable assets!", UnityHelpBox.Type.Warning);
#endif
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        private ItemDropEntity itemDropEntityPrefab = null;
        [SerializeField]
        private ExpDropEntity expDropEntityPrefab = null;
        [SerializeField]
        private GoldDropEntity goldDropEntityPrefab = null;
        [SerializeField]
        private CurrencyDropEntity currencyDropEntityPrefab = null;
        [SerializeField]
        private WarpPortalEntity warpPortalEntityPrefab = null;
        [SerializeField]
        private ItemsContainerEntity playerCorpsePrefab = null;
        [SerializeField]
        private ItemsContainerEntity monsterCorpsePrefab = null;
        [SerializeField]
        private BaseUISceneGameplay uiSceneGameplayPrefab = null;
        [Tooltip("If this is empty, it will use `UI Scene Gameplay Prefab` as gameplay UI prefab")]
        [SerializeField]
        private BaseUISceneGameplay uiSceneGameplayMobilePrefab = null;
        [Tooltip("If this is empty, it will use `UI Scene Gameplay Prefab` as gameplay UI prefab")]
        [SerializeField]
        private BaseUISceneGameplay uiSceneGameplayConsolePrefab = null;
        [Tooltip("Default controller prefab will be used when controller prefab at player character entity is null")]
        [SerializeField]
        private BasePlayerCharacterController defaultControllerPrefab = null;
#endif
        [SerializeField]
        private AssetReferenceItemDropEntity addressableItemDropEntityPrefab = null;
        [SerializeField]
        private AssetReferenceExpDropEntity addressableExpDropEntityPrefab = null;
        [SerializeField]
        private AssetReferenceGoldDropEntity addressableGoldDropEntityPrefab = null;
        [SerializeField]
        private AssetReferenceCurrencyDropEntity addressableCurrencyDropEntityPrefab = null;
        [SerializeField]
        private AssetReferenceWarpPortalEntity addressableWarpPortalEntityPrefab = null;
        [SerializeField]
        private AssetReferenceItemsContainerEntity addressablePlayerCorpsePrefab = null;
        [SerializeField]
        private AssetReferenceItemsContainerEntity addressableMonsterCorpsePrefab = null;
        [SerializeField]
        private AssetReferenceBaseUISceneGameplay addressableUiSceneGameplayPrefab = null;
        [Tooltip("If this is empty, it will use `Addressable UI Scene Gameplay Prefab` as gameplay UI prefab")]
        [SerializeField]
        private AssetReferenceBaseUISceneGameplay addressableUiSceneGameplayMobilePrefab = null;
        [Tooltip("If this is empty, it will use `Addressable UI Scene Gameplay Prefab` as gameplay UI prefab")]
        [SerializeField]
        private AssetReferenceBaseUISceneGameplay addressableUiSceneGameplayConsolePrefab = null;
        [SerializeField]
        private AssetReferenceBasePlayerCharacterController addressableDefaultControllerPrefab = null;

        [Tooltip("This is camera controller when start game as server (not start with client as host)")]
        public ServerCharacter serverCharacterPrefab = null;
        [Tooltip("These objects will be instantiate as owning character's children")]
        public GameObject[] owningCharacterObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as owning character's children to show in minimap")]
        public GameObject[] owningCharacterMiniMapObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as non-owning character's children")]
        public GameObject[] nonOwningCharacterObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as non-owning character's children to show in minimap")]
        public GameObject[] nonOwningCharacterMiniMapObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as monster character's children")]
        public GameObject[] monsterCharacterObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as monster character's children to show in minimap")]
        public GameObject[] monsterCharacterMiniMapObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as npc's children")]
        public GameObject[] npcObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as npc's children to show in minimap")]
        public GameObject[] npcMiniMapObjects = new GameObject[0];
        [Tooltip("This UI will be instaniate as owning character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity owningCharacterUI = null;
        [Tooltip("This UI will be instaniate as non owning character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity nonOwningCharacterUI = null;
        [Tooltip("This UI will be instaniate as monster character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity monsterCharacterUI = null;
        [Tooltip("This UI will be instaniate as NPC's child to show character name")]
        public UINpcEntity npcUI = null;
        [Tooltip("This UI will be instaniate as NPC's child to show quest indecator")]
        public NpcQuestIndicator npcQuestIndicator = null;

        [Header("Gameplay Effects")]
#if UNITY_EDITOR && EXCLUDE_PREFAB_REFS
        public UnityHelpBox effectHelpBox = new UnityHelpBox("`EXCLUDE_PREFAB_REFS` is set, you have to use only addressable assets!", UnityHelpBox.Type.Warning);
#endif
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [HideInInspector]
        // TODO: Deprecated, use `levelUpEffects` instead.
        private GameEffect levelUpEffect = null;
        [SerializeField]
        private GameEffect[] levelUpEffects = new GameEffect[0];
        [SerializeField]
        private GameEffect[] stunEffects = new GameEffect[0];
        [SerializeField]
        private GameEffect[] muteEffects = new GameEffect[0];
        [SerializeField]
        private GameEffect[] freezeEffects = new GameEffect[0];
#endif
        [SerializeField]
        private AssetReferenceGameEffect[] addressableLevelUpEffects = new AssetReferenceGameEffect[0];
        [SerializeField]
        private AssetReferenceGameEffect[] addressableStunEffects = new AssetReferenceGameEffect[0];
        [SerializeField]
        private AssetReferenceGameEffect[] addressableMuteEffects = new AssetReferenceGameEffect[0];
        [SerializeField]
        private AssetReferenceGameEffect[] addressableFreezeEffects = new AssetReferenceGameEffect[0];

        [Header("Gameplay Database and Default Data")]
        [Tooltip("Exp tree for both player character, monster character and item, this may be deprecated in the future, you should setup `Exp Table` instead.")]
        [SerializeField]
        private int[] expTree = new int[0];
        [SerializeField]
        private ExpTable expTable = null;
        [Tooltip("You should add game data to game database and set the game database to this. If you leave this empty, it will load game data from `Resources` folders")]
        [SerializeField]
        private BaseGameDatabase gameDatabase = null;
        [SerializeField]
        private BaseEntitySetting entitySetting = null;
        [Tooltip("You can add NPCs to NPC database or may add NPCs into the scene directly, so you can leave this empty if you are going to add NPCs into the scene directly only")]
        [SerializeField]
        private NpcDatabase npcDatabase = null;
        [Tooltip("You can add warp portals to warp portal database or may add warp portals into the scene directly, So you can leave this empty if you are going to add warp portals into the scene directly only")]
        [SerializeField]
        private WarpPortalDatabase warpPortalDatabase = null;
        [Tooltip("You can add social system settings or leave this empty to use default settings")]
        [SerializeField]
        private SocialSystemSetting socialSystemSetting = null;
        [Tooltip("Default weapon item, will be used when character not equip any weapon")]
        [SerializeField]
        private BaseItem defaultWeaponItem = null;
        [Tooltip("Default damage element, will be used when attacks to enemies or receives damages from enemies")]
        [SerializeField]
        private DamageElement defaultDamageElement = null;
        [Tooltip("Default hit effects, will be used when attack to enemies or receive damages from enemies")]
#if UNITY_EDITOR && EXCLUDE_PREFAB_REFS
        public UnityHelpBox damageHitEffectHelpBox = new UnityHelpBox("`EXCLUDE_PREFAB_REFS` is set, you have to use only addressable assets!", UnityHelpBox.Type.Warning);
#endif
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        private GameEffect[] defaultDamageHitEffects = new GameEffect[0];
#endif
        [SerializeField]
        private AssetReferenceGameEffect[] addressableDefaultDamageHitEffects = new AssetReferenceGameEffect[0];

        [Header("Object Tags and Layers")]
        [Tooltip("Tag for player character entities, this tag will set to player character entities game object when instantiated")]
        public UnityTag playerTag = new UnityTag("PlayerTag");
        [Tooltip("Tag for monster character entities, this tag will set to monster character entities game object when instantiated")]
        public UnityTag monsterTag = new UnityTag("MonsterTag");
        [Tooltip("Tag for NPC entities, this tag will set to NPC entities game object when instantiated")]
        public UnityTag npcTag = new UnityTag("NpcTag");
        [Tooltip("Tag for vehicle entities, this tag will set to vehicle entities game object when instantiated")]
        public UnityTag vehicleTag = new UnityTag("VehicleTag");
        [Tooltip("Tag for item drop entities, this tag will set to item drop entities game object when instantiated")]
        public UnityTag itemDropTag = new UnityTag("ItemDropTag");
        [Tooltip("Tag for building entities, this tag will set to building entities game object when instantiated")]
        public UnityTag buildingTag = new UnityTag("BuildingTag");
        [Tooltip("Tag for harvestable entities, this tag will set to harvestable entities game object when instantiated")]
        public UnityTag harvestableTag = new UnityTag("HarvestableTag");
        [Tooltip("Layer for player character entities, this layer will be set to player character entities game object when instantiated")]
        public UnityLayer playerLayer = new UnityLayer(17);
        [Tooltip("Layer for playing character entities, this layer will be set to playing character entities game object when instantiated")]
        public UnityLayer playingLayer = new UnityLayer(17);
        [Tooltip("Layer for monster character entities, this layer will be set to monster character entities game object when instantiated")]
        public UnityLayer monsterLayer = new UnityLayer(18);
        [Tooltip("Layer for NPC entities, this layer will be set to NPC entities game object when instantiated")]
        public UnityLayer npcLayer = new UnityLayer(19);
        [Tooltip("Layer for vehicle entities, this layer will be set to vehicle entities game object when instantiated")]
        public UnityLayer vehicleLayer = new UnityLayer(20);
        [Tooltip("Layer for item drop entities, this layer will set to item drop entities game object when instantiated")]
        public UnityLayer itemDropLayer = new UnityLayer(9);
        [Tooltip("Layer for building entities, this layer will set to building entities game object when instantiated")]
        public UnityLayer buildingLayer = new UnityLayer(13);
        [Tooltip("Layer for harvestable entities, this layer will set to harvestable entities game object when instantiated")]
        public UnityLayer harvestableLayer = new UnityLayer(14);
        [Tooltip("Layers which will be used when raycasting to find hitting obstacle/wall/floor/ceil when attacking damageable objects")]
        public UnityLayer[] attackObstacleLayers = new UnityLayer[]
        {
            new UnityLayer(0),
        };
        [Tooltip("Layers which will be ignored when raycasting")]
        [FormerlySerializedAs("nonTargetingLayers")]
        public UnityLayer[] ignoreRaycastLayers = new UnityLayer[]
        {
            new UnityLayer(11)
        };

        [Header("Gameplay Configs - Generic")]
        [Tooltip("If dropped items does not picked up within this duration, it will be destroyed from the server")]
        public float itemAppearDuration = 60f;
        [Tooltip("If dropped items does not picked up by killer within this duration, anyone can pick up the items")]
        public float itemLootLockDuration = 5f;
        [Tooltip("Dropped item picked up by Looters, will go to 1 random party member, Instead of only to whoever picked first if item share is on")]
        public bool itemLootRandomPartyMember;
        [Tooltip("If this is `TRUE` anyone can pick up items which drops by players immediately")]
        public bool canPickupItemsWhichDropsByPlayersImmediately = false;
        [Tooltip("If dealing request does not accepted within this duration, the request will be cancelled")]
        public float dealingRequestDuration = 5f;
        [Tooltip("If this is > 0, it will limit amount of dealing items")]
        public int dealingItemsLimit = 16;
        [Tooltip("If this is `TRUE`, dealing feature will be disabled, all players won't be able to deal items to each other")]
        public bool disableDealing = false;
        [Tooltip("If this is > 0, it will limit amount of vending items")]
        public int vendingItemsLimit = 16;
        [Tooltip("If this is `TRUE`, vending feature will be disabled, all players won't be able to deal items to each other")]
        public bool disableVending = false;
        [Tooltip("If dueling request does not accepted within this duration, the request will be cancelled")]
        public float duelingRequestDuration = 5f;
        [Tooltip("Count down duration before start a dueling")]
        public float duelingCountDownDuration = 3f;
        [Tooltip("Dueling duration (in seconds)")]
        public float duelingDuration = 60f * 3f;
        [Tooltip("If this is `TRUE`, dueling feature will be disabled, all players won't be able to deal items to each other")]
        public bool disableDueling = false;
        [Tooltip("This is a distance that allows a player to pick up an item")]
        public float pickUpItemDistance = 1f;
        [Tooltip("This is a distance that random drop item around a player")]
        public float dropDistance = 1f;
        [Tooltip("This is a distance that allows players to start converstion with NPC, send requests to other player entities and activate an building entities")]
        public float conversationDistance = 1f;
        [Tooltip("This is a distance that other players will receives local chat")]
        public float localChatDistance = 10f;
        [Tooltip("This is a distance from controlling character that combat texts will instantiates")]
        public float combatTextDistance = 20f;
        [Tooltip("This is a distance from monster killer to other characters in party to share EXP, if this value is <= 0, it will share EXP to all other characters in the same map")]
        public float partyShareExpDistance = 0f;
        [Tooltip("This is a distance from monster killer to other characters in party to share item (allow other characters to pickup item immediately), if this value is <= 0, it will share item to all other characters in the same map")]
        public float partyShareItemDistance = 0f;
        [Tooltip("Maximum number of equip weapon set")]
        [Range(1, 16)]
        public byte maxEquipWeaponSet = 2;
        [Tooltip("How character position load when start game")]
        public CurrentPositionSaveMode currentPositionSaveMode = CurrentPositionSaveMode.UseCurrentPosition;
        [Tooltip("How player drop item")]
        public PlayerDropItemMode playerDropItemMode = PlayerDropItemMode.DropOnGround;
        [Tooltip("How player character drop item when dying (it will drop items if map info was set to drop items)")]
        public DeadDropItemMode playerDeadDropItemMode = DeadDropItemMode.DropOnGround;
        [Tooltip("If all items does not picked up from corpse within this duration, it will be destroyed from the server")]
        public float playerCorpseAppearDuration = 60f;
        [Tooltip("How monster character drop item when dying")]
        public RewardingItemMode monsterDeadDropItemMode = RewardingItemMode.DropOnGround;
        [Tooltip("How monster character drop exp when dying")]
        public RewardingMode monsterExpRewardingMode = RewardingMode.Immediately;
        [Tooltip("How monster character drop gold when dying")]
        public RewardingMode monsterGoldRewardingMode = RewardingMode.Immediately;
        [Tooltip("How monster character drop currency when dying")]
        public RewardingMode monsterCurrencyRewardingMode = RewardingMode.Immediately;
        [Tooltip("If all items does not picked up from corpse within this duration, it will be destroyed from the server")]
        public float monsterCorpseAppearDuration = 60f;
        [Tooltip("Delay before return move speed while attack or use skill to generic move speed")]
        public float returnMoveSpeedDelayAfterAction = 0.1f;
        [Tooltip("Delay before mount again")]
        public float mountDelay = 1f;
        [Tooltip("Delay before use item again")]
        public float useItemDelay = 0.25f;
        [Tooltip("If this is `TRUE`, it will clear skills cooldown when character dead")]
        public bool clearSkillCooldownOnDead = true;
        [Tooltip("How the gold stored and being used, If this is `UserGoldOnly`, it won't have character's gold, all gold will being used from user's gold")]
        public GoldStoreMode goldStoreMode = GoldStoreMode.Default;

        [Header("Gameplay Configs - Items, Inventory and Storage")]
        public ItemTypeFilter dismantleFilter = new ItemTypeFilter()
        {
            includeArmor = true,
            includeShield = true,
            includeWeapon = true
        };
        [Tooltip("If this is `TRUE`, player will be able to refine an items by themself, doesn't have to talk to NPCs")]
        public bool canRefineItemByPlayer = false;
        [Tooltip("If this is > 0, it will limit amount of refine enhancer items")]
        public int refineEnhancerItemsLimit = 16;
        [Tooltip("If this is `TRUE`, player will be able to dismantle an items by themself, doesn't have to talk to NPCs")]
        public bool canDismantleItemByPlayer = false;
        [Tooltip("If this is `TRUE`, player will be able to repair an items by themself, doesn't have to talk to NPCs")]
        public bool canRepairItemByPlayer = false;
        [Tooltip("How player's inventory works")]
        public InventorySystem inventorySystem = InventorySystem.Simple;
        [Tooltip("If this is `TRUE`, weight limit won't be applied")]
        public bool noInventoryWeightLimit;
        [Tooltip("If this is `TRUE` it won't fill empty slots")]
        public bool doNotFillEmptySlots = false;
        [Tooltip("Base slot limit for all characters, it will be used when `InventorySystem` is `LimitSlots`")]
        public int baseSlotLimit = 0;
        public Storage playerStorage = default;
        public Storage guildStorage = default;
        public EnhancerRemoval enhancerRemoval = new EnhancerRemoval();

        [Header("Gameplay Configs - Summon Monster")]
        [Tooltip("This is a distance that random summon around a character")]
        public float minSummonDistance = 2f;
        [Tooltip("This is a distance that random summon around a character")]
        public float maxSummonDistance = 3f;
        [Tooltip("Min distance to follow summoner")]
        public float minFollowSummonerDistance = 5f;
        [Tooltip("Max distance to follow summoner, if distance between characters more than this it will teleport to summoner")]
        public float maxFollowSummonerDistance = 10f;

        [Header("Gameplay Configs - Summon Pet Item")]
        [Tooltip("This is duration to lock item before it is able to summon later after character dead")]
        public float petDeadLockDuration = 60f;
        [Tooltip("This is duration to lock item before it is able to summon later after unsummon")]
        public float petUnSummonLockDuration = 30f;

        [Header("Gameplay Configs - Instance Dungeon")]
        [Tooltip("Distance from party leader character to join instance map")]
        public float joinInstanceMapDistance = 20f;

        [Header("New Character")]
        [Tooltip("If this is NULL, it will use `startGold` and `startItems`")]
        public NewCharacterSetting newCharacterSetting;
        [Tooltip("Amount of gold that will be added to character when create new character")]
        public int startGold = 0;
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        public ItemAmount[] startItems = new ItemAmount[0];
        [Tooltip("If it is running in editor, and if this is not NULL, it will use data from this setting for testing purpose")]
        public NewCharacterSetting testingNewCharacterSetting;

        [Header("Server Settings")]
        public bool updateAnimationAtServer = true;

        [Header("Player Configs")]
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        [Tooltip("Max characters that player can create, set it to 0 to unlimit")]
        public byte maxCharacterSaves = 5;

        [Header("Platforms Configs")]
        public int serverTargetFrameRate = 30;

        // Editor foldout states for better organization
        [HideInInspector] public bool gameplaySystemsFoldout = true;
        [HideInInspector] public bool gameplayObjectsFoldout = false;
        [HideInInspector] public bool gameplayEffectsFoldout = false;
        [HideInInspector] public bool gameplayDatabaseFoldout = true;
        [HideInInspector] public bool objectTagsFoldout = false;
        [HideInInspector] public bool gameplayConfigsFoldout = true;
        [HideInInspector] public bool itemsConfigsFoldout = false;
        [HideInInspector] public bool summonConfigsFoldout = false;
        [HideInInspector] public bool newCharacterFoldout = true;
        [HideInInspector] public bool serverSettingsFoldout = true;
        [HideInInspector] public bool playerConfigsFoldout = true;
        [HideInInspector] public bool platformConfigsFoldout = false;
        [HideInInspector] public bool editorConfigsFoldout = false;

#if UNITY_EDITOR
        [Header("Playing In Editor")]
        public TestInEditorMode testInEditorMode = TestInEditorMode.Standalone;
        public AssetReferenceLanRpgNetworkManager networkManagerForOfflineTesting;

        [Header("Performance Monitoring")]
        [Tooltip("Enable performance monitor component for debugging and optimization")]
        public bool enablePerformanceMonitor = false;
        [Tooltip("Show performance GUI overlay in game view (for debugging)")]
        public bool showPerformanceGUI = false;
#endif

        // Static events
        public static event System.Action<IPlayerCharacterData> OnSetPlayingCharacterEvent;
        public static event System.Action OnGameDataLoadedEvent;

        #region Cache Data
        public EventSystemManager EventSystemManager { get; private set; }


        public bool IsLimitInventorySlot
        {
            get { return inventorySystem == InventorySystem.LimitSlots; }
        }

        public bool IsLimitInventoryWeight
        {
            get { return !noInventoryWeightLimit; }
        }

        public BaseMessageManager MessageManager
        {
            get { return messageManager; }
        }

        public BaseGameSaveSystem SaveSystem
        {
            get { return saveSystem; }
        }


        public BaseInventoryManager InventoryManager
        {
            get { return inventoryManager; }
        }

        public BaseDayNightTimeUpdater DayNightTimeUpdater
        {
            get { return dayNightTimeUpdater; }
        }

        public BaseGMCommands GMCommands
        {
            get { return gmCommands; }
        }

        public BaseEquipmentModelBonesSetupManager EquipmentModelBonesSetupManager
        {
            get { return equipmentModelBonesSetupManager; }
        }

        public NetworkSetting NetworkSetting
        {
            get { return networkSetting; }
        }

        public BaseGameDatabase GameDatabase
        {
            get { return gameDatabase; }
        }

        public BaseEntitySetting EntitySetting
        {
            get { return entitySetting; }
        }

        public SocialSystemSetting SocialSystemSetting
        {
            get { return socialSystemSetting; }
        }

        public BaseUISceneGameplay UISceneGameplayPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                if ((Application.isMobilePlatform || IsMobileTestInEditor()) && uiSceneGameplayMobilePrefab != null)
                    return uiSceneGameplayMobilePrefab;
                if ((Application.isConsolePlatform || IsConsoleTestInEditor()) && uiSceneGameplayConsolePrefab != null)
                    return uiSceneGameplayConsolePrefab;
                return uiSceneGameplayPrefab;
#else
                return null;
#endif
            }
        }

        public AssetReferenceBaseUISceneGameplay AddressableUISceneGameplayPrefab
        {
            get
            {
                if ((Application.isMobilePlatform || IsMobileTestInEditor()) && addressableUiSceneGameplayMobilePrefab.IsDataValid())
                    return addressableUiSceneGameplayMobilePrefab;
                if ((Application.isConsolePlatform || IsConsoleTestInEditor()) && addressableUiSceneGameplayConsolePrefab.IsDataValid())
                    return addressableUiSceneGameplayConsolePrefab;
                return addressableUiSceneGameplayPrefab;
            }
        }

        public ExpTable ExpTable
        {
            get { return expTable; }
        }

        public BaseItem ExpDropRepresentItem
        {
            get { return expDropRepresentItem; }
        }

        public BaseItem GoldDropRepresentItem
        {
            get { return goldDropRepresentItem; }
        }

        public ItemDropEntity ItemDropEntityPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return itemDropEntityPrefab;
#else
                return null;
#endif
            }
        }

        public ExpDropEntity ExpDropEntityPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return expDropEntityPrefab;
#else
                return null;
#endif
            }
        }

        public GoldDropEntity GoldDropEntityPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return goldDropEntityPrefab;
#else
                return null;
#endif
            }
        }

        public CurrencyDropEntity CurrencyDropEntityPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return currencyDropEntityPrefab;
#else
                return null;
#endif
            }
        }

        public WarpPortalEntity WarpPortalEntityPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return warpPortalEntityPrefab;
#else
                return null;
#endif
            }
        }

        public ItemsContainerEntity PlayerCorpsePrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return playerCorpsePrefab;
#else
                return null;
#endif
            }
        }

        public ItemsContainerEntity MonsterCorpsePrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return monsterCorpsePrefab;
#else
                return null;
#endif
            }
        }

        public BaseUISceneGameplay UiSceneGameplayPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
            return uiSceneGameplayPrefab;
#else
                return null;
#endif
            }
        }

        public BaseUISceneGameplay UiSceneGameplayMobilePrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return uiSceneGameplayMobilePrefab;
#else
                return null;
#endif
            }
        }

        public BaseUISceneGameplay UiSceneGameplayConsolePrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return uiSceneGameplayConsolePrefab;
#else
                return null;
#endif
            }
        }

        public BasePlayerCharacterController DefaultControllerPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return defaultControllerPrefab;
#else
                return null;
#endif
            }
        }

        public AssetReferenceItemDropEntity AddressableItemDropEntityPrefab
        {
            get { return addressableItemDropEntityPrefab; }
        }

        public AssetReferenceExpDropEntity AddressableExpDropEntityPrefab
        {
            get { return addressableExpDropEntityPrefab; }
        }

        public AssetReferenceGoldDropEntity AddressableGoldDropEntityPrefab
        {
            get { return addressableGoldDropEntityPrefab; }
        }

        public AssetReferenceCurrencyDropEntity AddressableCurrencyDropEntityPrefab
        {
            get { return addressableCurrencyDropEntityPrefab; }
        }

        public AssetReferenceWarpPortalEntity AddressableWarpPortalEntityPrefab
        {
            get { return addressableWarpPortalEntityPrefab; }
        }

        public AssetReferenceItemsContainerEntity AddressablePlayerCorpsePrefab
        {
            get { return addressablePlayerCorpsePrefab; }
        }

        public AssetReferenceItemsContainerEntity AddressableMonsterCorpsePrefab
        {
            get { return addressableMonsterCorpsePrefab; }
        }

        public AssetReferenceBaseUISceneGameplay AddressableUiSceneGameplayPrefab
        {
            get { return addressableUiSceneGameplayPrefab; }
        }

        public AssetReferenceBaseUISceneGameplay AddressableUiSceneGameplayMobilePrefab
        {
            get { return addressableUiSceneGameplayMobilePrefab; }
        }

        public AssetReferenceBaseUISceneGameplay AddressableUiSceneGameplayConsolePrefab
        {
            get { return addressableUiSceneGameplayConsolePrefab; }
        }

        public AssetReferenceBasePlayerCharacterController AddressableDefaultControllerPrefab
        {
            get { return addressableDefaultControllerPrefab; }
        }

        public async UniTask<ItemDropEntity> GetLoadedItemDropEntityPrefab()
        {
            return await AddressableItemDropEntityPrefab.GetOrLoadAssetAsyncOrUsePrefab(ItemDropEntityPrefab);
        }

        public async UniTask<ExpDropEntity> GetLoadedExpDropEntityPrefab()
        {
            return await AddressableExpDropEntityPrefab.GetOrLoadAssetAsyncOrUsePrefab(ExpDropEntityPrefab);
        }

        public async UniTask<GoldDropEntity> GetLoadedGoldDropEntityPrefab()
        {
            return await AddressableGoldDropEntityPrefab.GetOrLoadAssetAsyncOrUsePrefab(GoldDropEntityPrefab);
        }

        public async UniTask<CurrencyDropEntity> GetLoadedCurrencyDropEntityPrefab()
        {
            return await AddressableCurrencyDropEntityPrefab.GetOrLoadAssetAsyncOrUsePrefab(CurrencyDropEntityPrefab);
        }

        public async UniTask<WarpPortalEntity> GetLoadedWarpPortalEntityPrefab()
        {
            return await AddressableWarpPortalEntityPrefab.GetOrLoadAssetAsyncOrUsePrefab(WarpPortalEntityPrefab);
        }

        public async UniTask<ItemsContainerEntity> GetLoadedPlayerCorpsePrefab()
        {
            return await AddressablePlayerCorpsePrefab.GetOrLoadAssetAsyncOrUsePrefab(PlayerCorpsePrefab);
        }

        public async UniTask<ItemsContainerEntity> GetLoadedMonsterCorpsePrefab()
        {
            return await AddressableMonsterCorpsePrefab.GetOrLoadAssetAsyncOrUsePrefab(MonsterCorpsePrefab);
        }

        public async UniTask<BaseUISceneGameplay> GetLoadedUiSceneGameplayPrefab()
        {
            return await AddressableUiSceneGameplayPrefab.GetOrLoadAssetAsyncOrUsePrefab(UiSceneGameplayPrefab);
        }

        public async UniTask<BaseUISceneGameplay> GetLoadedUiSceneGameplayMobilePrefab()
        {
            return await AddressableUiSceneGameplayMobilePrefab.GetOrLoadAssetAsyncOrUsePrefab(UiSceneGameplayMobilePrefab);
        }

        public async UniTask<BaseUISceneGameplay> GetLoadedUiSceneGameplayConsolePrefab()
        {
            return await AddressableUiSceneGameplayConsolePrefab.GetOrLoadAssetAsyncOrUsePrefab(UiSceneGameplayConsolePrefab);
        }

        public async UniTask<BasePlayerCharacterController> GetLoadedDefaultControllerPrefab()
        {
            return await AddressableDefaultControllerPrefab.GetOrLoadAssetAsyncOrUsePrefab(DefaultControllerPrefab);
        }

        public GameEffect[] LevelUpEffects
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return levelUpEffects;
#else
                return System.Array.Empty<GameEffect>();
#endif
            }
        }

        public AssetReferenceGameEffect[] AddressableLevelUpEffects
        {
            get { return addressableLevelUpEffects; }
        }

        public GameEffect[] StunEffects
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return stunEffects;
#else
                return System.Array.Empty<GameEffect>();
#endif
            }
        }

        public AssetReferenceGameEffect[] AddressableStunEffects
        {
            get { return addressableStunEffects; }
        }

        public GameEffect[] MuteEffects
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return muteEffects;
#else
                return System.Array.Empty<GameEffect>();
#endif
            }
        }

        public AssetReferenceGameEffect[] AddressableMuteEffects
        {
            get { return addressableMuteEffects; }
        }

        public GameEffect[] FreezeEffects
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return freezeEffects;
#else
                return System.Array.Empty<GameEffect>();
#endif
            }
        }

        public AssetReferenceGameEffect[] AddressableFreezeEffects
        {
            get { return addressableFreezeEffects; }
        }

        public ArmorType DefaultArmorType
        {
            get; private set;
        }

        public WeaponType DefaultWeaponType
        {
            get; private set;
        }

        public IWeaponItem DefaultWeaponItem
        {
            get { return defaultWeaponItem as IWeaponItem; }
        }

        public IWeaponItem MonsterWeaponItem
        {
            get; private set;
        }

        public DamageElement DefaultDamageElement
        {
            get { return defaultDamageElement; }
        }

        public GameEffect[] DefaultDamageHitEffects
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return defaultDamageHitEffects;
#else
                return System.Array.Empty<GameEffect>();
#endif
            }
        }

        public AssetReferenceGameEffect[] AddressableDefaultDamageHitEffects
        {
            get { return addressableDefaultDamageHitEffects; }
        }

        public NewCharacterSetting NewCharacterSetting
        {
            get
            {
#if UNITY_EDITOR
                if (testingNewCharacterSetting != null)
                    return testingNewCharacterSetting;
#endif
                return newCharacterSetting;
            }
        }

        public bool HasNewCharacterSetting
        {
            get
            {
                return NewCharacterSetting != null;
            }
        }
        
        public HashSet<int> IgnoreRaycastLayersValues { get; private set; }

        public static readonly Dictionary<string, bool> LoadHomeScenePreventions = new Dictionary<string, bool>();
        public static bool DoNotLoadHomeScene
        {
            get
            {
                foreach (bool doNotLoad in LoadHomeScenePreventions.Values)
                {
                    if (doNotLoad)
                        return true;
                }
                return false;
            }
        }
#endregion

        protected virtual void Awake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                // Set target framerate when running headless to reduce CPU usage
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = serverTargetFrameRate;
            }
            Application.runInBackground = true;
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Singleton = this;
            s_playingCharacter = default;
            JoinedParty = null;
            JoinedGuild = null;
            OpenedStorages.Clear();
            LoadHomeScenePreventions.Clear();
            EventSystemManager = gameObject.GetOrAddComponent<EventSystemManager>();
#if UNITY_EDITOR
            InputManager.UseMobileInputOnNonMobile = IsMobileTestInEditor();
            InputManager.UseNonMobileInput = testInEditorMode == TestInEditorMode.MobileWithKeyInputs && Application.isEditor;
#endif

            DefaultArmorType = ScriptableObject.CreateInstance<ArmorType>()
                .GenerateDefaultArmorType();

            DefaultWeaponType = ScriptableObject.CreateInstance<WeaponType>()
                .GenerateDefaultWeaponType();

            // Setup default weapon item if not existed
            if (defaultWeaponItem == null)
            {
                defaultWeaponItem = ScriptableObject.CreateInstance<Item>()
                    .GenerateDefaultItem(DefaultWeaponType);
                // Use the same item with default weapon item (if default weapon not set by user)
                MonsterWeaponItem = defaultWeaponItem as IWeaponItem;
            }

            if (defaultDamageElement == null)
            {
                defaultDamageElement = ScriptableObject.CreateInstance<DamageElement>()
                    .GenerateDefaultDamageElement(DefaultDamageHitEffects, AddressableDefaultDamageHitEffects);
            }

            // Setup message manager if not existed
            if (messageManager == null)
                messageManager = ScriptableObject.CreateInstance<DefaultMessageManager>();

            // Setup save system if not existed
            if (saveSystem == null)
                saveSystem = ScriptableObject.CreateInstance<DefaultGameSaveSystem>();

            // Setup gameplay rule if not existed
            if (GameplayRule == null)
                GameplayRule = ScriptableObject.CreateInstance<DefaultGameplayRule>();

            // Reset gold and exp rate
            GameplayRule.GoldRate = 1f;
            GameplayRule.ExpRate = 1f;

            // Setup inventory manager if not existed
            if (inventoryManager == null)
                inventoryManager = ScriptableObject.CreateInstance<DefaultInventoryManager>();

            // Setup day night time updater if not existed
            if (dayNightTimeUpdater == null)
                dayNightTimeUpdater = ScriptableObject.CreateInstance<DefaultDayNightTimeUpdater>();

            // Setup GM commands if not existed
            if (gmCommands == null)
                gmCommands = ScriptableObject.CreateInstance<DefaultGMCommands>();

            // Setup equipment model bones setup manager if not existed
            if (equipmentModelBonesSetupManager == null)
                equipmentModelBonesSetupManager = ScriptableObject.CreateInstance<EquipmentModelBonesSetupByHumanBodyBonesManager>();

            // Setup network setting if not existed
            if (networkSetting == null)
                networkSetting = ScriptableObject.CreateInstance<NetworkSetting>();

            // Setup exp table if not existed, and use exp tree
            if (expTable == null)
            {
                expTable = ScriptableObject.CreateInstance<ExpTable>();
                expTable.expTree = expTree;
            }

            // Setup game database if not existed
            if (gameDatabase == null)
                gameDatabase = ScriptableObject.CreateInstance<ResourcesFolderGameDatabase>();

            // Setup entity setting if not existed
            if (entitySetting == null)
                entitySetting = ScriptableObject.CreateInstance<DefaultEntitySetting>();

            // Setup social system setting if not existed
            if (socialSystemSetting == null)
                socialSystemSetting = ScriptableObject.CreateInstance<SocialSystemSetting>();
            socialSystemSetting.Migrate();

            // Setup non target layers and ignore raycast layers
            if (ignoreRaycastLayers.Length > 0)
            {
                IgnoreRaycastLayersValues = new HashSet<int>();
                foreach (UnityLayer ignoreRaycastLayer in ignoreRaycastLayers)
                {
                    IgnoreRaycastLayersValues.Add(ignoreRaycastLayer.LayerIndex);
                }
            }

            this.InvokeInstanceDevExtMethods("Awake");

            // Auto-create essential components if they don't exist (for server mode)
            // This must happen after Singleton is set
            EnsureEssentialComponentsExist();
        }

        /// <summary>
        /// Ensures essential NightBlade components exist, especially for server mode.
        /// This auto-creates components that are normally expected to be in scenes.
        /// </summary>
        private void EnsureEssentialComponentsExist()
        {
            // Note: We create essential components regardless of mode for reliability

            try
            {
                Debug.Log("[GameInstance] Ensuring essential components exist...");

                // Auto-create SmartAssetManager if it doesn't exist
                if (NightBlade.Core.Utils.SmartAssetManager.Instance == null)
                {
                    GameObject assetManagerGO = new GameObject("SmartAssetManager");
                    var smartAssetManager = assetManagerGO.AddComponent<NightBlade.Core.Utils.SmartAssetManager>();
                    DontDestroyOnLoad(assetManagerGO);
                    Debug.Log("[GameInstance] Auto-created SmartAssetManager");
                }

                // Auto-create MapAssetManager for map servers
                if (Application.isBatchMode && FindObjectOfType<NightBlade.MMO.MapServer.Map.MapAssetManager>() == null)
                {
                    GameObject mapAssetManagerGO = new GameObject("MapAssetManager");
                    var mapAssetManager = mapAssetManagerGO.AddComponent<NightBlade.MMO.MapServer.Map.MapAssetManager>();
                    DontDestroyOnLoad(mapAssetManagerGO);
                    Debug.Log("[GameInstance] Auto-created MapAssetManager for map server");
                }

                Debug.Log("[GameInstance] Essential components verification complete");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameInstance] Failed to ensure essential components: {ex.Message}");
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;
            s_playingCharacter = default;
            JoinedParty = null;
            JoinedGuild = null;
            OpenedStorages.Clear();
            LoadHomeScenePreventions.Clear();
            EventSystemManager = gameObject.GetOrAddComponent<EventSystemManager>();
#if UNITY_EDITOR
            InputManager.UseMobileInputOnNonMobile = IsMobileTestInEditor();
            InputManager.UseNonMobileInput = testInEditorMode == TestInEditorMode.MobileWithKeyInputs && Application.isEditor;
#endif

            DefaultArmorType = ScriptableObject.CreateInstance<ArmorType>()
                .GenerateDefaultArmorType();

            DefaultWeaponType = ScriptableObject.CreateInstance<WeaponType>()
                .GenerateDefaultWeaponType();

            // Setup default weapon item if not existed
            if (defaultWeaponItem == null || !defaultWeaponItem.IsWeapon())
            {
                defaultWeaponItem = ScriptableObject.CreateInstance<Item>()
                    .GenerateDefaultItem(DefaultWeaponType);
                // Use the same item with default weapon item (if default weapon not set by user)
                MonsterWeaponItem = defaultWeaponItem as IWeaponItem;
            }

            // Setup monster weapon item if not existed
            if (MonsterWeaponItem == null)
            {
                MonsterWeaponItem = ScriptableObject.CreateInstance<Item>()
                    .GenerateDefaultItem(DefaultWeaponType);
            }

            // Setup default damage element if not existed
            if (defaultDamageElement == null)
            {
                defaultDamageElement = ScriptableObject.CreateInstance<DamageElement>()
                    .GenerateDefaultDamageElement(DefaultDamageHitEffects, AddressableDefaultDamageHitEffects);
            }

            // Setup string formatter if not existed
            if (messageManager == null)
                messageManager = ScriptableObject.CreateInstance<DefaultMessageManager>();

            // Setup save system if not existed
            if (saveSystem == null)
                saveSystem = ScriptableObject.CreateInstance<DefaultGameSaveSystem>();

            // Setup gameplay rule if not existed
            if (GameplayRule == null)
                GameplayRule = ScriptableObject.CreateInstance<DefaultGameplayRule>();

            // Reset gold and exp rate
            GameplayRule.GoldRate = 1f;
            GameplayRule.ExpRate = 1f;

            // Setup inventory manager
            if (inventoryManager == null)
                inventoryManager = ScriptableObject.CreateInstance<DefaultInventoryManager>();

            // Setup day night time updater if not existed
            if (dayNightTimeUpdater == null)
                dayNightTimeUpdater = ScriptableObject.CreateInstance<DefaultDayNightTimeUpdater>();

            // Setup GM commands if not existed
            if (gmCommands == null)
                gmCommands = ScriptableObject.CreateInstance<DefaultGMCommands>();

            // Setup equipment model bones setup manager if not existed
            if (equipmentModelBonesSetupManager == null)
                equipmentModelBonesSetupManager = ScriptableObject.CreateInstance<EquipmentModelBonesSetupByHumanBodyBonesManager>();

            // Setup network setting if not existed
            if (networkSetting == null)
                networkSetting = ScriptableObject.CreateInstance<NetworkSetting>();

            // Setup exp table if not existed, and use exp tree
            if (expTable == null)
            {
                expTable = ScriptableObject.CreateInstance<ExpTable>();
                expTable.expTree = expTree;
            }

            // Setup game database if not existed
            if (gameDatabase == null)
                gameDatabase = ScriptableObject.CreateInstance<ResourcesFolderGameDatabase>();

            // Setup entity setting if not existed
            if (entitySetting == null)
                entitySetting = ScriptableObject.CreateInstance<DefaultEntitySetting>();

            // Setup social system setting if not existed
            if (socialSystemSetting == null)
                socialSystemSetting = ScriptableObject.CreateInstance<SocialSystemSetting>();
            socialSystemSetting.Migrate();

            // Setup non target layers
            IgnoreRaycastLayersValues = new HashSet<int>();
            foreach (UnityLayer layer in ignoreRaycastLayers)
            {
                IgnoreRaycastLayersValues.Add(layer.LayerIndex);
            }

            // Setup default home scenes
            if (!addressableHomeMobileScene.IsDataValid())
                addressableHomeMobileScene = addressableHomeScene;
            if (!addressableHomeConsoleScene.IsDataValid())
                addressableHomeConsoleScene = addressableHomeScene;
            if (!homeMobileScene.IsDataValid())
                homeMobileScene = homeScene;
            if (!homeConsoleScene.IsDataValid())
                homeConsoleScene = homeScene;

            ClearData();

            // Initialize performance optimizations (safe initialization)
            // Only initialize after a short delay to avoid interfering with scene loading
            Invoke(nameof(DelayedPerformanceOptimizationInit), 0.1f);

            this.InvokeInstanceDevExtMethods("Awake");
        }

        /// <summary>
        /// Initialize NightBlade performance optimizations.
        /// This sets up all the core performance systems for optimal MMO gameplay.
        /// </summary>
        private void InitializePerformanceOptimizations()
        {
            try
            {
                // Initialize network string caching for bandwidth optimization
                // Delay to ensure network system is fully initialized
                Invoke(nameof(InitializeNetworkStringCache), 1.5f);

                // Initialize coroutine pooling for GC reduction (required for UI animations)
                // Initialize immediately since UI pooling depends on it
                if (this != null)
                {
                    NightBlade.Core.Utils.CoroutinePool.Initialize(this);
                }

                // Initialize smart asset management for automatic memory optimization
                // This runs immediately as it's critical for memory management
                GameObject assetManagerGO = new GameObject("SmartAssetManager");
                assetManagerGO.AddComponent<NightBlade.Core.Utils.SmartAssetManager>();
                DontDestroyOnLoad(assetManagerGO);

                // Enable performance monitoring integration
                NightBlade.Core.Utils.SmartAssetIntegration.SmartAssetMonitor.EnablePerformanceMonitoring();

                // Initialize UI object pooling for performance optimization
                GameObject uiPoolManagerGO = new GameObject("UIPoolManager");
                uiPoolManagerGO.AddComponent<NightBlade.UI.Utils.Pooling.UIPoolManager>();
                DontDestroyOnLoad(uiPoolManagerGO);

                // Initialize specialized UI pools for combat and floating text
                GameObject damagePoolGO = new GameObject("UIDamageNumberPool");
                var damagePool = damagePoolGO.AddComponent<NightBlade.UI.Utils.Pooling.UIDamageNumberPool>();
                DontDestroyOnLoad(damagePoolGO);

                GameObject floatingPoolGO = new GameObject("UIFloatingTextPool");
                var floatingPool = floatingPoolGO.AddComponent<NightBlade.UI.Utils.Pooling.UIFloatingTextPool>();
                DontDestroyOnLoad(floatingPoolGO);

                // Defer UI pool template creation until TMP is ready
                StartCoroutine(InitializeUIPoolsWhenReady(damagePool, floatingPool));

                // Optional: Initialize performance monitoring (disabled by default)
                // Can be enabled programmatically or via inspector for debugging
                // Delay initialization to avoid interfering with UI during scene loading
                Invoke(nameof(AddPerformanceMonitorIfNeeded), 1.0f);

                Debug.Log("[GameInstance] Performance optimizations initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameInstance] Failed to initialize performance optimizations: {ex.Message}");
                // Continue execution even if performance optimizations fail
            }
        }

        private System.Collections.IEnumerator PreWarmUIPools()
        {
            // Wait for UI systems to initialize
            yield return new WaitForSeconds(2f);

            // Pre-warm common UI pools for combat and gameplay
            try
            {
                // Damage numbers for combat
                if (UIPoolManager.Instance != null)
                {
                    UIPoolManager.Instance.PreWarmPool("DamageNumbers", 20);
                    int totalPooled = UIPoolManager.GetTotalPooledObjects();
                    Debug.Log($"[GameInstance] Pre-warmed 20 damage number objects. Total pooled: {totalPooled}");

                    // Debug pool stats
                    var poolStats = UIPoolManager.Instance.GetPoolStats();
                    if (poolStats.Count > 0)
                    {
                        string statsString = StringBuilderPool.Use(sb =>
                        {
                            foreach (var kvp in poolStats)
                            {
                                if (sb.Length > 0) sb.Append(", ");
                                sb.Append($"{kvp.Key}:{kvp.Value}");
                            }
                            return sb.ToString();
                        });
                        Debug.Log($"[GameInstance] Pool stats: {statsString}");
                    }
                }

                // Floating text for various messages
                // Note: UIFloatingTextPool handles its own pre-warming in Awake()

                Debug.Log("[GameInstance] UI pool pre-warming completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInstance] UI pool pre-warming failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes UI pools after ensuring TMP is ready.
        /// </summary>
        private System.Collections.IEnumerator InitializeUIPoolsWhenReady(
            NightBlade.UI.Utils.Pooling.UIDamageNumberPool damagePool,
            NightBlade.UI.Utils.Pooling.UIFloatingTextPool floatingPool)
        {
            // First, check if TMP Essential Resources are imported
            bool tmpResourcesAvailable = CheckTMPResourcesAvailable();

            if (!tmpResourcesAvailable)
            {
                Debug.LogWarning("[GameInstance] TMP Essential Resources not imported. UI pooling disabled. Import via: Window > TextMesh Pro > Import TMP Essential Resources");
                yield break;
            }

            // Wait for TMP to be fully initialized
            float timeout = 10f; // 10 second timeout
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                // Check if TMP is ready by trying to access TMP resources
                try
                {
                    // Try to access TMP default font - this will fail if TMP isn't ready
                    if (TMPro.TMP_Settings.defaultFontAsset != null)
                    {
                        break; // TMP is ready
                    }
                }
                catch
                {
                    // TMP not ready yet
                }

                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            if (elapsed >= timeout)
            {
                Debug.LogWarning("[GameInstance] TMP initialization timeout - UI pools may not work properly");
                yield break;
            }

            // Clear existing pools from previous server instances (since UIPoolManager is shared)
            if (UIPoolManager.Instance != null)
            {
                UIPoolManager.Instance.ClearAllPools();
            }

            // Now create templates and register them
            try
            {
                GameObject damageTemplate = CreateDamageNumberTemplate();
                if (damageTemplate != null && UIPoolManager.Instance != null)
                {
                    UIPoolManager.Instance.RegisterTemplate("DamageNumbers", damageTemplate);
                    damagePool.DamageNumberTemplate = damageTemplate;
                    Debug.Log("[GameInstance] Registered damage number template");
                }
                else
                {
                    Debug.LogWarning($"[GameInstance] Failed to register damage number template - template: {(damageTemplate == null ? "null" : "valid")}, manager: {(UIPoolManager.Instance == null ? "null" : "exists")}");
                }

                GameObject floatingTemplate = CreateFloatingTextTemplate();
                if (floatingTemplate != null && UIPoolManager.Instance != null)
                {
                    var templateField = floatingPool.GetType().GetField("floatingTextTemplate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (templateField != null)
                    {
                        templateField.SetValue(floatingPool, floatingTemplate);
                        UIPoolManager.Instance.RegisterTemplate("FloatingText", floatingTemplate);
                        Debug.Log("[GameInstance] Registered floating text template");
                    }
                }

                // Pre-warm common UI pools now that templates are registered
                StartCoroutine(PreWarmUIPools());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameInstance] Failed to initialize UI pools: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if TMP Essential Resources are available.
        /// </summary>
        private bool CheckTMPResourcesAvailable()
        {
            try
            {
                // Check for TMP resources by looking for the default sprite asset
                // This is a reliable indicator that TMP resources have been imported
                var defaultSpriteAsset = TMPro.TMP_Settings.defaultSpriteAsset;
                if (defaultSpriteAsset != null)
                {
                    return true;
                }

                // Also check if we can find the LiberationSans SDF font
                var fonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
                foreach (var font in fonts)
                {
                    if (font.name.Contains("LiberationSans") || font.name.Contains("TMP"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a simple damage number template for the UI pool.
        /// </summary>
        private GameObject CreateDamageNumberTemplate()
        {
            try
            {
                GameObject template = new GameObject("DamageNumberTemplate");
                template.SetActive(false);
                Debug.Log($"[GameInstance] Created damage number template: {template.name}");

                // Add UI components
                var canvasRenderer = template.AddComponent<UnityEngine.CanvasRenderer>();
                var rectTransform = template.GetComponent<RectTransform>();
                if (rectTransform == null) rectTransform = template.AddComponent<RectTransform>();

                // Create TextMeshPro component with proper font
                var textComponent = template.AddComponent<TMPro.TextMeshProUGUI>();
                textComponent.text = "123";
                textComponent.fontSize = 24;
                textComponent.color = Color.red;
                textComponent.alignment = TMPro.TextAlignmentOptions.Center;
                Debug.Log($"[GameInstance] Added TextMeshPro component to template");

                // Try to assign TMP default font
                try
                {
                    if (TMPro.TMP_Settings.defaultFontAsset != null)
                    {
                        textComponent.font = TMPro.TMP_Settings.defaultFontAsset;
                        Debug.Log("[GameInstance] Assigned TMP default font to damage number template");
                    }
                    else
                    {
                        Debug.LogWarning("[GameInstance] TMP default font asset is null - damage numbers may not display properly");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GameInstance] Font assignment failed: {ex.Message} - component will use fallback");
                }

                // Set up rect transform
                rectTransform.sizeDelta = new Vector2(200, 50);

                Debug.Log($"[GameInstance] Damage number template created successfully: {template != null}");
                return template;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInstance] Failed to create damage number template: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a simple floating text template for the UI pool.
        /// </summary>
        private GameObject CreateFloatingTextTemplate()
        {
            try
            {
                GameObject template = new GameObject("FloatingTextTemplate");
                template.SetActive(false);

                // Add UI components
                var canvasRenderer = template.AddComponent<UnityEngine.CanvasRenderer>();
                var rectTransform = template.GetComponent<RectTransform>();
                if (rectTransform == null) rectTransform = template.AddComponent<RectTransform>();

                // Create TextMeshPro component with proper font
                var textComponent = template.AddComponent<TMPro.TextMeshProUGUI>();
                textComponent.text = "MISS";
                textComponent.fontSize = 20;
                textComponent.color = Color.gray;
                textComponent.alignment = TMPro.TextAlignmentOptions.Center;

                // Try to assign TMP default font
                try
                {
                    if (TMPro.TMP_Settings.defaultFontAsset != null)
                    {
                        textComponent.font = TMPro.TMP_Settings.defaultFontAsset;
                    }
                }
                catch
                {
                    // Font assignment failed, TMP might not be ready - component will use fallback
                }

                // Set up rect transform
                rectTransform.sizeDelta = new Vector2(150, 40);

                return template;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInstance] Failed to create floating text template: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delays performance optimization initialization to avoid interfering with scene loading.
        /// </summary>
        private void DelayedPerformanceOptimizationInit()
        {
            try
            {
                InitializePerformanceOptimizations();
                Debug.Log("[GameInstance] Performance optimizations initialized successfully after scene loading");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInstance] Performance optimizations failed to initialize: {ex.Message}. Continuing without optimizations.");
            }
        }

        /// <summary>
        /// Initialize network string caching after network system is fully set up.
        /// </summary>
        private void InitializeNetworkStringCache()
        {
            try
            {
                LiteNetLib.Utils.NetworkStringCache.InitializeCommonStrings();
                Debug.Log("[GameInstance] Network string cache initialized");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInstance] Failed to initialize network string cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds performance monitor component if needed (called via Invoke to avoid Awake timing issues).
        /// </summary>
        private void AddPerformanceMonitorIfNeeded()
        {
#if UNITY_EDITOR
            try
            {
                if (enablePerformanceMonitor)
                {
                    var monitor = GetComponent<NightBlade.Core.Utils.Performance.PerformanceMonitor>();
                    if (monitor == null)
                    {
                        monitor = gameObject.AddComponent<NightBlade.Core.Utils.Performance.PerformanceMonitor>();
                        Debug.Log("[GameInstance] Performance Monitor component added");
                    }

                    // Configure GUI visibility based on settings
                    if (monitor != null)
                    {
                        monitor.ShowGUIStats = showPerformanceGUI;
                        Debug.Log($"[GameInstance] Performance Monitor GUI set to: {(showPerformanceGUI ? "Visible" : "Hidden")}");
                    }
                }
                else
                {
                    // Remove performance monitor if disabled
                    var monitor = GetComponent<NightBlade.Core.Utils.Performance.PerformanceMonitor>();
                    if (monitor != null)
                    {
                        Destroy(monitor);
                        Debug.Log("[GameInstance] Performance Monitor component removed (disabled)");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInstance] Failed to configure performance monitor: {ex.Message}");
            }
#endif
        }

        protected virtual void Start()
        {
            GameDatabase.LoadData(this).Forget();

            // Perform runtime validation in development builds only (once per session)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_gameDataValidated)
            {
                ValidateGameDataIntegrity();
                _gameDataValidated = true;
            }
#endif
        }

        /// <summary>
        /// Validates game data integrity at runtime
        /// </summary>
        private void ValidateGameDataIntegrity()
        {
            if (RuntimeValidation.ValidateGameData())
            {
                Debug.Log("[GameInstance] Game data validation passed");
            }
            else
            {
                RuntimeValidation.LogValidationResults("GameInstance");
                Debug.LogError("[GameInstance] Game data validation failed! Check errors above.");
            }
        }

        protected virtual void OnDestroy()
        {
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }

        public static void ClearData()
        {
            Attributes.Clear();
            Currencies.Clear();
            CurrencyDropRepresentItems.Clear();
            Items.Clear();
            ItemsByAmmoType.Clear();
            ItemCraftFormulas.Clear();
            Harvestables.Clear();
            Characters.Clear();
            PlayerCharacters.Clear();
            PlayerCharacterEntityMetaDataList.Clear();
            MonsterCharacters.Clear();
            ArmorTypes.Clear();
            WeaponTypes.Clear();
            AmmoTypes.Clear();
            Skills.Clear();
            NpcDialogs.Clear();
            Quests.Clear();
            GuildSkills.Clear();
            GuildIcons.Clear();
            Gachas.Clear();
            StatusEffects.Clear();
            DamageElements.Clear();
            EquipmentSets.Clear();
#if !EXCLUDE_PREFAB_REFS
            BuildingEntities.Clear();
            PlayerCharacterEntities.Clear();
            MonsterCharacterEntities.Clear();
            ItemDropEntities.Clear();
            HarvestableEntities.Clear();
            VehicleEntities.Clear();
            WarpPortalEntities.Clear();
            NpcEntities.Clear();
#endif
            AddressableBuildingEntities.Clear();
            AddressablePlayerCharacterEntities.Clear();
            AddressableMonsterCharacterEntities.Clear();
            AddressableItemDropEntities.Clear();
            AddressableHarvestableEntities.Clear();
            AddressableVehicleEntities.Clear();
            AddressableWarpPortalEntities.Clear();
            AddressableNpcEntities.Clear();
            MapWarpPortals.Clear();
            MapNpcs.Clear();
            MapInfos.Clear();
            Factions.Clear();
#if !EXCLUDE_PREFAB_REFS
            OtherNetworkObjectPrefabs.Clear();
#endif
            AddressableOtherNetworkObjectPrefabs.Clear();

			//Support for addons
            DevExtUtils.InvokeStaticDevExtMethods(typeof(GameInstance), "ClearData");
        }

        public static bool UseMobileInput()
        {
            return Application.isMobilePlatform || IsMobileTestInEditor();
        }

        public static bool UseConsoleInput()
        {
            return Application.isConsolePlatform || IsConsoleTestInEditor();
        }

        public static bool IsMobileTestInEditor()
        {
#if UNITY_EDITOR
            return (Singleton.testInEditorMode == TestInEditorMode.Mobile || Singleton.testInEditorMode == TestInEditorMode.MobileWithKeyInputs) && Application.isEditor;
#else
            return false;
#endif
        }

        public static bool IsConsoleTestInEditor()
        {
#if UNITY_EDITOR
            return Singleton.testInEditorMode == TestInEditorMode.Console && Application.isEditor;
#else
            return false;
#endif
        }

        public void LoadedGameData()
        {
            this.InvokeInstanceDevExtMethods("LoadedGameData");
            // Add ammo items to dictionary
            foreach (BaseItem item in Items.Values)
            {
                if (item.IsAmmo())
                {
                    IAmmoItem ammoItem = item as IAmmoItem;
                    if (ammoItem.AmmoType == null)
                        continue;
                    if (!ItemsByAmmoType.ContainsKey(ammoItem.AmmoType.DataId))
                        ItemsByAmmoType.Add(ammoItem.AmmoType.DataId, new Dictionary<int, BaseItem>());
                    ItemsByAmmoType[ammoItem.AmmoType.DataId][item.DataId] = item;
                }
            }

            var representCurrencies = NightBlade.Core.Utils.ListPool<Currency>.GetList();
            var representItems = NightBlade.Core.Utils.ListPool<BaseItem>.GetList();
            representItems.Add(DefaultWeaponItem as BaseItem);
            representItems.Add(MonsterWeaponItem as BaseItem);

            if (ExpDropRepresentItem != null)
            {
                ExpDropRepresentItem.MaxStack = int.MaxValue;
                representItems.Add(ExpDropRepresentItem);
            }

            if (GoldDropRepresentItem != null)
            {
                GoldDropRepresentItem.MaxStack = int.MaxValue;
                representItems.Add(GoldDropRepresentItem);
            }

            foreach (CurrencyItemPair currencyItemPair in currencyDropRepresentItems)
            {
                if (currencyItemPair.currency == null || currencyItemPair.item == null)
                    continue;
                representCurrencies.Add(currencyItemPair.currency);
                representItems.Add(currencyItemPair.item);
                currencyItemPair.item.MaxStack = int.MaxValue;
                CurrencyDropRepresentItems[currencyItemPair.currency.DataId] = currencyItemPair.item;
            }

            // Add required represent game data
            AddCurrencies(new List<Currency>(representCurrencies));
            AddItems(new List<BaseItem>(representItems));

            // Return pooled collections
            NightBlade.Core.Utils.ListPool<Currency>.ReturnList(representCurrencies);
            NightBlade.Core.Utils.ListPool<BaseItem>.ReturnList(representItems);
            MigrateLevelUpEffect();

            if (newCharacterSetting != null && newCharacterSetting.startItems != null)
                AddItems(newCharacterSetting.startItems);

#if UNITY_EDITOR
            if (testingNewCharacterSetting != null && testingNewCharacterSetting.startItems != null)
                AddItems(testingNewCharacterSetting.startItems);
#endif

            if (startItems != null)
                AddItems(startItems);

            if (warpPortalDatabase != null && warpPortalDatabase.maps != null)
                AddMapWarpPortals(warpPortalDatabase.maps);

            if (npcDatabase != null && npcDatabase.maps != null)
                AddMapNpcs(npcDatabase.maps);

            if (Application.isPlaying)
                InitializePurchasing();

            System.GC.Collect();
            OnGameDataLoaded();
        }

        public void OnGameDataLoaded()
        {
            if (OnGameDataLoadedEvent != null)
                OnGameDataLoadedEvent.Invoke();
            if (Application.isPlaying && !DoNotLoadHomeScene)
                LoadHomeScene();
        }

        public bool IsRepresentItem(BaseItem item)
        {
            return
                IsExpDropRepresentItem(item) ||
                IsGoldDropRepresentItem(item) ||
                IsCurrencyDropRepresentItem(item, out _);
        }

        public bool IsExpDropRepresentItem(BaseItem item)
        {
            if (expDropRepresentItem != null && expDropRepresentItem == item)
                return true;
            return false;
        }

        public bool IsGoldDropRepresentItem(BaseItem item)
        {
            if (goldDropRepresentItem != null && goldDropRepresentItem == item)
                return true;
            return false;
        }

        public bool IsCurrencyDropRepresentItem(BaseItem item, out Currency currency)
        {
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            if (currencyDropRepresentItems != null && currencyDropRepresentItems.Length > 0)
            {
                for (int i = 0; i < currencyDropRepresentItems.Length; ++i)
                {
                    if (currencyDropRepresentItems[i].item != null && currencyDropRepresentItems[i].item == item)
                    {
                        currency = currencyDropRepresentItems[i].currency;
                        return true;
                    }
                }
            }
#endif
            currency = null;
            return false;
        }

        public List<string> GetGameMapIds()
        {
            var mapIds = NightBlade.Core.Utils.ListPool<string>.GetList();
            foreach (BaseMapInfo mapInfo in MapInfos.Values)
            {
                if (mapInfo != null && !string.IsNullOrEmpty(mapInfo.Id) && !mapIds.Contains(mapInfo.Id))
                    mapIds.Add(mapInfo.Id);
            }

            // Return a copy since the pooled list will be reused
            var result = new List<string>(mapIds);
            NightBlade.Core.Utils.ListPool<string>.ReturnList(mapIds);
            return result;
        }

        public int GetAttackObstacleLayerMask()
        {
            int layerMask = 0;
            if (attackObstacleLayers.Length > 0)
            {
                foreach (UnityLayer attackObstacleLayer in attackObstacleLayers)
                {
                    layerMask = layerMask | attackObstacleLayer.Mask;
                }
            }
            return layerMask;
        }

        public int MixWithAttackObstacleLayers(int layerMask)
        {
            return layerMask | GetAttackObstacleLayerMask();
        }

        public int GetIgnoreRaycastLayerMask()
        {
            int layerMask = 0;
            if (ignoreRaycastLayers.Length > 0)
            {
                foreach (UnityLayer ignoreRaycastLayer in ignoreRaycastLayers)
                {
                    layerMask = layerMask | ignoreRaycastLayer.Mask;
                }
            }
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            return layerMask;
        }

        public int MixWithIgnoreRaycastLayers(int layerMask)
        {
            return layerMask | GetIgnoreRaycastLayerMask();
        }

        /// <summary>
        /// All layers except `nonTargetingLayers`, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetTargetLayerMask()
        {
            // 0 = Nothing, -1 = AllLayers
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// Check is layer is layer for any damageable entities or not
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsDamageableLayer(int layer)
        {
            return layer == playerLayer ||
                layer == playingLayer ||
                layer == monsterLayer ||
                layer == vehicleLayer ||
                layer == buildingLayer ||
                layer == harvestableLayer;
        }

        /// <summary>
        /// Only `playerLayer`, `playingLayer`, `monsterLayer`, `vehicleLayer`, `buildingLayer`, `harvestableLayer` will be used for hit detection casting
        /// </summary>
        /// <returns></returns>
        public int GetDamageableLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return layerMask;
        }

        /// <summary>
        /// Only `playerLayer`, `playingLayer`, `monsterLayer`, `vehicleLayer`, `buildingLayer`, `harvestableLayer` and wall layers will be used for hit detection casting
        /// </summary>
        /// <returns></returns>
        public int GetDamageEntityHitLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            layerMask = MixWithAttackObstacleLayers(layerMask);
            return layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer, `harvestableLayer`, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetBuildLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetItemDropGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetGameEntityGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer`, `buildingLayer`, `harvestableLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetHarvestableSpawnGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer`, `harvestableLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetAreaSkillGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }
    }
}







