using UnityEngine;
using System.Collections.Generic;

namespace NightBlade.UI
{
    /// <summary>
    /// Lightweight UI coordinator for NightBlade.
    /// Replaces the 913-line UISceneGameplay monolith with a clean, modular approach.
    /// 
    /// Responsibilities:
    /// - Track open panels
    /// - Handle ESC key priority
    /// - Coordinate focus management
    /// - Provide UI state queries
    /// 
    /// What it DOESN'T do:
    /// - Manage individual UI systems (they manage themselves)
    /// - Handle UI logic (that's in each UIBase/UIPanel)
    /// - Create/destroy UI (handled by individual systems)
    /// 
    /// Design Philosophy: KISS (Keep It Simple, Stupid)
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        
        private static UIManager instance;
        public static UIManager Instance => instance;
        
        #endregion
        
        #region State
        
        // Track all open panels for focus/ESC management
        private readonly List<UIPanel> openPanels = new List<UIPanel>();
        
        // Track if any UI is blocking player controls
        private bool isPlayerControlsBlocked = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Is any UI currently blocking player controls?
        /// </summary>
        public bool IsPlayerControlsBlocked => isPlayerControlsBlocked;
        
        /// <summary>
        /// Number of currently open panels
        /// </summary>
        public int OpenPanelCount => openPanels.Count;
        
        /// <summary>
        /// Is any panel currently open?
        /// </summary>
        public bool HasOpenPanels => openPanels.Count > 0;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[UIManager] Multiple UIManager instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[UIManager] Initialized - Clean, modular UI management ready!");
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        private void Update()
        {
            // Update player controls blocked state
            UpdateControlsBlockedState();
        }
        
        #endregion
        
        #region Panel Registration
        
        /// <summary>
        /// Register a panel as open (called by UIPanel.Show())
        /// </summary>
        public void RegisterPanel(UIPanel panel)
        {
            if (panel == null)
                return;
            
            if (!openPanels.Contains(panel))
            {
                openPanels.Add(panel);
                
#if DEBUG_UI
                Debug.Log($"[UIManager] Panel registered: {panel.PanelTitle} (Total: {openPanels.Count})");
#endif
            }
        }
        
        /// <summary>
        /// Unregister a panel as closed (called by UIPanel.Hide())
        /// </summary>
        public void UnregisterPanel(UIPanel panel)
        {
            if (panel == null)
                return;
            
            if (openPanels.Contains(panel))
            {
                openPanels.Remove(panel);
                
#if DEBUG_UI
                Debug.Log($"[UIManager] Panel unregistered: {panel.PanelTitle} (Remaining: {openPanels.Count})");
#endif
            }
        }
        
        /// <summary>
        /// Check if a specific panel is currently open
        /// </summary>
        public bool IsPanelOpen(UIPanel panel)
        {
            return panel != null && openPanels.Contains(panel);
        }
        
        /// <summary>
        /// Get the topmost (most recently opened) panel
        /// </summary>
        public UIPanel GetTopmostPanel()
        {
            return openPanels.Count > 0 ? openPanels[openPanels.Count - 1] : null;
        }
        
        #endregion
        
        #region Controls Management
        
        /// <summary>
        /// Update whether player controls should be blocked
        /// </summary>
        private void UpdateControlsBlockedState()
        {
            bool shouldBlock = false;
            
            // Check if any open panel blocks controls
            for (int i = 0; i < openPanels.Count; i++)
            {
                if (openPanels[i] != null && openPanels[i].BlockPlayerControls && openPanels[i].IsVisible)
                {
                    shouldBlock = true;
                    break;
                }
            }
            
            isPlayerControlsBlocked = shouldBlock;
        }
        
        /// <summary>
        /// Force update controls blocked state (call if panel properties change)
        /// </summary>
        public void RefreshControlsBlockedState()
        {
            UpdateControlsBlockedState();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Close all open panels
        /// </summary>
        public void CloseAllPanels(bool animate = true)
        {
            // Create a copy to avoid modification during iteration
            UIPanel[] panels = openPanels.ToArray();
            
            foreach (UIPanel panel in panels)
            {
                if (panel != null)
                {
                    panel.Hide(animate);
                }
            }
            
            openPanels.Clear();
        }
        
        /// <summary>
        /// Get all currently open panels (returns copy of list)
        /// </summary>
        public List<UIPanel> GetOpenPanels()
        {
            return new List<UIPanel>(openPanels);
        }
        
        /// <summary>
        /// Find a panel by title
        /// </summary>
        public UIPanel FindPanelByTitle(string title)
        {
            for (int i = 0; i < openPanels.Count; i++)
            {
                if (openPanels[i] != null && openPanels[i].PanelTitle == title)
                {
                    return openPanels[i];
                }
            }
            return null;
        }
        
        #endregion
        
        #region Debug
        
#if UNITY_EDITOR
        [Header("Debug Info (Runtime Only)")]
        [SerializeField] private bool showDebugInfo = false;
        
        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying)
                return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"<b>UIManager Debug</b>");
            GUILayout.Label($"Open Panels: {openPanels.Count}");
            GUILayout.Label($"Controls Blocked: {isPlayerControlsBlocked}");
            
            if (openPanels.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("<b>Open Panels:</b>");
                
                foreach (UIPanel panel in openPanels)
                {
                    if (panel != null)
                    {
                        string blockIcon = panel.BlockPlayerControls ? "🔒" : "🔓";
                        GUILayout.Label($"{blockIcon} {panel.PanelTitle}");
                    }
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
        
        #endregion
    }
}
