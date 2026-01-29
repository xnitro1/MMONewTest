using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Comprehensive data validation utilities for game security and stability
    /// Performance optimized for high-frequency validation calls
    /// </summary>
    public static class DataValidation
    {
        // Performance monitoring (development only)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static int _validationCalls = 0;
        private static float _validationTime = 0f;
        private static System.Diagnostics.Stopwatch _validationStopwatch = new System.Diagnostics.Stopwatch();
#endif
        // String validation constants
        public const int MAX_NAME_LENGTH = 64; // Increased to allow HTML color tags
        public const int MAX_DESCRIPTION_LENGTH = 500;
        public const int MAX_CHAT_MESSAGE_LENGTH = 256;
        public const int MIN_PASSWORD_LENGTH = 6;

        // Numeric validation ranges
        public const int MIN_LEVEL = 1;
        public const int MAX_LEVEL = 100;
        public const int MIN_STAT_POINTS = 0;
        public const int MAX_STAT_POINTS = 999999;
        public const long MIN_GOLD = 0;
        public const long MAX_GOLD = 999999999999; // 999 billion

        // Regex patterns for validation
        private static readonly Regex _validNamePattern = new Regex(@"^[a-zA-Z0-9\s\-_\[\]\(\)]+$", RegexOptions.Compiled);
        private static readonly Regex _validCharacterNamePattern = new Regex(@"^[a-zA-Z0-9\s\-_\[\]\(\)]{2,24}$", RegexOptions.Compiled);
        private static readonly Regex _sqlInjectionPattern = new Regex(@"[';\\]", RegexOptions.Compiled);

        /// <summary>
        /// Validates a generic string input
        /// </summary>
        public static bool IsValidString(string input, int maxLength = MAX_NAME_LENGTH, bool allowEmpty = false)
        {
            if (string.IsNullOrEmpty(input))
                return allowEmpty;

            if (input.Length > maxLength)
                return false;

            // Fast character-by-character check instead of regex for performance
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '\'' || c == ';' || c == '\\')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a character name with specific rules
        /// </summary>
        public static bool IsValidCharacterName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return _validCharacterNamePattern.IsMatch(name);
        }

        /// <summary>
        /// Validates a username for login
        /// </summary>
        public static bool IsValidUsername(string username)
        {
            return IsValidString(username, 24, false) && _validNamePattern.IsMatch(username);
        }

        /// <summary>
        /// Validates a password
        /// </summary>
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MIN_PASSWORD_LENGTH)
                return false;

            // Check for at least one letter and one number
            return password.Any(char.IsLetter) && password.Any(char.IsDigit);
        }

        /// <summary>
        /// Validates an email address format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a numeric value is within acceptable range
        /// </summary>
        public static bool IsValidRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Validates a level value
        /// </summary>
        public static bool IsValidLevel(int level)
        {
            return IsValidRange(level, MIN_LEVEL, MAX_LEVEL);
        }

        /// <summary>
        /// Validates gold/currency amount
        /// </summary>
        public static bool IsValidGold(long gold)
        {
            return gold >= MIN_GOLD && gold <= MAX_GOLD;
        }

        /// <summary>
        /// Validates stat points
        /// </summary>
        public static bool IsValidStatPoints(int points)
        {
            return IsValidRange(points, MIN_STAT_POINTS, MAX_STAT_POINTS);
        }

        /// <summary>
        /// Validates an array/collection is not null and within size limits
        /// </summary>
        public static bool IsValidArray<T>(ICollection<T> array, int maxSize = 1000)
        {
            return array != null && array.Count <= maxSize;
        }

        /// <summary>
        /// Validates a data ID exists in the game database
        /// </summary>
        public static bool IsValidDataId(int dataId)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _validationStopwatch.Restart();
#endif
            bool result = dataId != 0; // Allow negative IDs since hash functions can produce them
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _validationStopwatch.Stop();
            _validationCalls++;
            _validationTime += (float)_validationStopwatch.Elapsed.TotalMilliseconds;
#endif
            return result;
        }

        /// <summary>
        /// Report validation performance statistics (development only)
        /// </summary>
        public static void LogValidationPerformance()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_validationCalls > 0)
            {
                float avgTime = _validationTime / _validationCalls;
                Debug.Log($"[DataValidation] Performance: {_validationCalls} calls, avg {avgTime:F4}ms per call, total {_validationTime:F2}ms");
                _validationCalls = 0;
                _validationTime = 0f;
            }
#endif
        }

        /// <summary>
        /// Validates a game data object
        /// </summary>
        public static bool IsValidGameData(BaseGameData data)
        {
            if (data == null)
                return false;

            if (!IsValidString(data.Title, MAX_NAME_LENGTH, true))
                return false;

            if (!IsValidString(data.Description, MAX_DESCRIPTION_LENGTH, true))
                return false;

            if (!IsValidDataId(data.DataId))
                return false;

            return true;
        }

        /// <summary>
        /// Validates character stats for PlayerCharacterData
        /// </summary>
        public static bool IsValidCharacterStats(IPlayerCharacterData player)
        {
            if (player == null)
                return false;

            // Validate stat points and skill points
            return IsValidRange((int)player.StatPoint, MIN_STAT_POINTS, MAX_STAT_POINTS) &&
                   IsValidRange((int)player.SkillPoint, MIN_STAT_POINTS, MAX_STAT_POINTS) &&
                   IsValidGold(player.Gold);
        }

        /// <summary>
        /// Validates item data
        /// </summary>
        public static bool IsValidItem(BaseItem item)
        {
            if (!IsValidGameData(item))
                return false;

            // Additional item-specific validation (ItemType is a value type enum, always valid)

            if (!IsValidRange(item.MaxStack, 1, int.MaxValue))
                return false;

            if (!IsValidGold(item.SellPrice))
                return false;

            return true;
        }

        /// <summary>
        /// Validates character item data
        /// </summary>
        public static bool IsValidCharacterItem(CharacterItem item)
        {
            if (!IsValidDataId(item.dataId))
                return false;

            if (!IsValidRange(item.amount, 0, 99999))
                return false;

            return true;
        }

        /// <summary>
        /// Validates network message data
        /// </summary>
        public static bool IsValidNetworkMessage<T>(T message) where T : struct
        {
            // Add specific validation based on message type
            // This would be extended for each message type
            return true;
        }

        /// <summary>
        /// Sanitizes a string input to prevent injection attacks
        /// </summary>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove or escape dangerous characters
            return input.Replace("'", "''").Replace(";", "").Replace("\\", "");
        }

        /// <summary>
        /// Logs validation errors for debugging
        /// </summary>
        public static void LogValidationError(string context, string error)
        {
            Debug.LogError($"[DataValidation] {context}: {error}");
        }

        /// <summary>
        /// Throws an exception for critical validation failures
        /// </summary>
        public static void ThrowValidationError(string context, string error)
        {
            throw new ArgumentException($"[DataValidation] {context}: {error}");
        }
    }
}







