using UnityEngine;
using NightBlade.UI.Utils.Pooling;

namespace NightBlade.UI.HUD
{
    /// <summary>
    /// Clean coordinator for combat text / damage numbers.
    /// Wraps the existing pooled systems (UIDamageNumberPool, UIFloatingTextPool)
    /// for easy integration with the new UI architecture.
    /// 
    /// Features:
    /// - Damage numbers (normal, critical, healing)
    /// - Floating text (miss, block, immune, etc.)
    /// - All pooled for zero GC!
    /// 
    /// Usage:
    /// ```csharp
    /// UICombatTextCoordinator.ShowDamage(enemy.transform.position, 1250, isCritical: true);
    /// UICombatTextCoordinator.ShowHealing(player.transform.position, 500);
    /// UICombatTextCoordinator.ShowMiss(target.transform.position);
    /// ```
    /// 
    /// Part of the sexy new HUD! 🎯
    /// </summary>
    public static class UICombatTextCoordinator
    {
        #region Damage Numbers
        
        /// <summary>
        /// Show damage number at world position
        /// </summary>
        public static void ShowDamage(Vector3 worldPosition, int amount, bool isCritical = false)
        {
            UIDamageNumberPool.ShowDamageNumber(worldPosition, amount, isCritical, false);
        }
        
        /// <summary>
        /// Show damage number following a transform
        /// </summary>
        public static void ShowDamage(Transform followTarget, int amount, bool isCritical = false)
        {
            if (followTarget != null)
            {
                ShowDamage(followTarget.position, amount, isCritical);
            }
        }
        
        /// <summary>
        /// Show healing number at world position
        /// </summary>
        public static void ShowHealing(Vector3 worldPosition, int amount)
        {
            UIDamageNumberPool.ShowDamageNumber(worldPosition, amount, false, true);
        }
        
        /// <summary>
        /// Show healing number following a transform
        /// </summary>
        public static void ShowHealing(Transform followTarget, int amount)
        {
            if (followTarget != null)
            {
                ShowHealing(followTarget.position, amount);
            }
        }
        
        #endregion
        
        #region Floating Text
        
        /// <summary>
        /// Show "MISS" text
        /// </summary>
        public static void ShowMiss(Vector3 worldPosition)
        {
            ShowFloatingText(worldPosition, "MISS", Color.gray);
        }
        
        /// <summary>
        /// Show "BLOCKED" text
        /// </summary>
        public static void ShowBlocked(Vector3 worldPosition)
        {
            ShowFloatingText(worldPosition, "BLOCKED", new Color(0.6f, 0.6f, 1f));
        }
        
        /// <summary>
        /// Show "IMMUNE" text
        /// </summary>
        public static void ShowImmune(Vector3 worldPosition)
        {
            ShowFloatingText(worldPosition, "IMMUNE", new Color(1f, 0.8f, 0.2f));
        }
        
        /// <summary>
        /// Show "DODGE" text
        /// </summary>
        public static void ShowDodge(Vector3 worldPosition)
        {
            ShowFloatingText(worldPosition, "DODGE", new Color(0.8f, 1f, 0.8f));
        }
        
        /// <summary>
        /// Show "RESISTED" text
        /// </summary>
        public static void ShowResisted(Vector3 worldPosition)
        {
            ShowFloatingText(worldPosition, "RESISTED", new Color(0.8f, 0.5f, 1f));
        }
        
        /// <summary>
        /// Show custom floating text
        /// </summary>
        public static void ShowFloatingText(Vector3 worldPosition, string text, Color color)
        {
            UIFloatingTextPool.ShowWorldText(worldPosition, text, color);
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Show appropriate combat text based on damage info
        /// </summary>
        public static void ShowCombatResult(Vector3 worldPosition, int amount, bool isCritical, bool isMiss, bool isBlocked, bool isImmune)
        {
            if (isMiss)
            {
                ShowMiss(worldPosition);
            }
            else if (isBlocked)
            {
                ShowBlocked(worldPosition);
            }
            else if (isImmune)
            {
                ShowImmune(worldPosition);
            }
            else if (amount > 0)
            {
                ShowDamage(worldPosition, amount, isCritical);
            }
        }
        
        
        #endregion
    }
}
