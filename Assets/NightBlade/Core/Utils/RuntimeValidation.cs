using System.Collections.Generic;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Runtime validation system for catching data integrity issues during gameplay
    /// Optimized for performance with minimal overhead in production
    /// </summary>
    public static class RuntimeValidation
    {
        private static readonly List<string> _validationErrors = new List<string>();
        private static readonly List<string> _validationWarnings = new List<string>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static float _lastValidationTime = 0f;
        private static int _totalValidations = 0;
#endif

        /// <summary>
        /// Validates player character data at runtime
        /// </summary>
        public static bool ValidatePlayerCharacter(IPlayerCharacterData character)
        {
            _validationErrors.Clear();
            _validationWarnings.Clear();

            if (character == null)
            {
                _validationErrors.Add("Player character is null");
                return false;
            }

            // Validate basic character data
            if (!DataValidation.IsValidCharacterName(character.CharacterName))
            {
                _validationErrors.Add($"Invalid character name: {character.CharacterName}");
            }

            if (!DataValidation.IsValidCharacterStats(character))
            {
                _validationErrors.Add("Invalid character stats");
            }

            if (!DataValidation.IsValidGold(character.Gold))
            {
                _validationErrors.Add($"Invalid gold amount: {character.Gold}");
            }

            // Validate inventory
            if (character.SelectableWeaponSets != null)
            {
                for (int i = 0; i < character.SelectableWeaponSets.Count; i++)
                {
                    var weaponSet = character.SelectableWeaponSets[i];
                    if (weaponSet.leftHand.dataId != 0 && !DataValidation.IsValidItem(weaponSet.leftHand.GetItem()))
                    {
                        _validationErrors.Add($"Invalid left hand weapon in set {i}");
                    }
                    if (weaponSet.rightHand.dataId != 0 && !DataValidation.IsValidItem(weaponSet.rightHand.GetItem()))
                    {
                        _validationErrors.Add($"Invalid right hand weapon in set {i}");
                    }
                }
            }

            // Validate non-critical items (warnings only)
            // Note: IPlayerCharacterData doesn't have direct Items access in this interface
            // Individual implementations may have different inventory access patterns

            return _validationErrors.Count == 0;
        }

        /// <summary>
        /// Validates game data integrity
        /// </summary>
        public static bool ValidateGameData()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            float startTime = Time.realtimeSinceStartup;
#endif

            _validationErrors.Clear();
            _validationWarnings.Clear();

            var gameInstance = GameInstance.Singleton;
            if (gameInstance == null)
            {
                _validationErrors.Add("GameInstance is null");
                return false;
            }

            // Validate all items
            foreach (var item in GameInstance.Items)
            {
                if (!DataValidation.IsValidItem(item.Value))
                {
                    string details = GetItemValidationDetails(item.Value);
                    _validationErrors.Add($"Invalid item data: {item.Key} - Title: '{item.Value.Title}' - {details}");

                    // Debug the problematic items
                    if (item.Key == 798029291 || item.Key == -2092367233 || item.Key == -1511677990)
                    {
                        Debug.LogWarning($"[DEBUG] Item {item.Key}: Title='{item.Value.Title}' (length: {item.Value.Title?.Length ?? 0}), DefaultTitle='{item.Value.DefaultTitle}'");
                    }
                }
            }

            // Validate all characters
            foreach (var character in GameInstance.Characters)
            {
                if (!DataValidation.IsValidGameData(character.Value))
                {
                    _validationErrors.Add($"Invalid character data: {character.Key}");
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _lastValidationTime = Time.realtimeSinceStartup - startTime;
            _totalValidations++;
            if (_lastValidationTime > 0.1f) // Log slow validations
            {
                Debug.LogWarning($"[RuntimeValidation] Slow validation detected: {_lastValidationTime:F3}s");
            }
#endif

            return _validationErrors.Count == 0;
        }

        /// <summary>
        /// Gets detailed validation failure information for an item
        /// </summary>
        private static string GetItemValidationDetails(BaseItem item)
        {
            var issues = new List<string>();

            if (!DataValidation.IsValidGameData(item))
            {
                issues.Add("invalid game data");

                // Debug the specific game data issues
                if (!DataValidation.IsValidString(item.Title, DataValidation.MAX_NAME_LENGTH, true))
                    issues.Add($"invalid title: '{item.Title}' (length: {item.Title?.Length ?? 0})");

                if (!DataValidation.IsValidString(item.Description, DataValidation.MAX_DESCRIPTION_LENGTH, true))
                    issues.Add($"invalid description: length {item.Description?.Length ?? 0}");

                if (!DataValidation.IsValidDataId(item.DataId))
                    issues.Add($"invalid dataId: {item.DataId}");
            }

            // ItemType is a value type enum, always has a valid value

            if (!DataValidation.IsValidRange(item.MaxStack, 1, int.MaxValue))
                issues.Add($"invalid MaxStack: {item.MaxStack}");

            if (!DataValidation.IsValidGold(item.SellPrice))
                issues.Add($"invalid SellPrice: {item.SellPrice}");

            return string.Join(", ", issues);
        }

        /// <summary>
        /// Reports validation performance statistics (development only)
        /// </summary>
        public static void LogPerformanceStats()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RuntimeValidation] Performance Stats: Total validations: {_totalValidations}, Last validation time: {_lastValidationTime:F4}s");
#endif
        }

        /// <summary>
        /// Logs all validation errors and warnings
        /// </summary>
        public static void LogValidationResults(string context = "")
        {
            string prefix = string.IsNullOrEmpty(context) ? "" : $"[{context}] ";

            foreach (var error in _validationErrors)
            {
                Debug.LogError($"{prefix}VALIDATION ERROR: {error}");
            }

            foreach (var warning in _validationWarnings)
            {
                Debug.LogWarning($"{prefix}VALIDATION WARNING: {warning}");
            }
        }

        /// <summary>
        /// Gets validation errors
        /// </summary>
        public static IReadOnlyList<string> GetErrors()
        {
            return _validationErrors;
        }

        /// <summary>
        /// Gets validation warnings
        /// </summary>
        public static IReadOnlyList<string> GetWarnings()
        {
            return _validationWarnings;
        }

        /// <summary>
        /// Clears all validation results
        /// </summary>
        public static void ClearResults()
        {
            _validationErrors.Clear();
            _validationWarnings.Clear();
        }
    }
}







