using UnityEngine;

namespace NightBlade
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.DEFAULT_MESSAGE_MANAGER_FILE, menuName = GameDataMenuConsts.DEFAULT_MESSAGE_MANAGER_MENU, order = GameDataMenuConsts.DEFAULT_MESSAGE_MANAGER_ORDER)]
    public class DefaultMessageManager : BaseMessageManager
    {
        public override string ReplaceKeysToMessages(string format)
        {
            // Cache PlayingCharacter reference to avoid repeated null checks
            var playingChar = GameInstance.PlayingCharacter;
            
            if (playingChar != null)
            {
                format = format.Replace("@characterName", playingChar.CharacterName);
                format = format.Replace("@level", playingChar.Level.ToString("N0"));
                format = format.Replace("@characterClass", playingChar.GetDatabase().Title);
                format = format.Replace("@exp", playingChar.Exp.ToString("N0"));
                format = format.Replace("@nextExp", playingChar.GetNextLevelExp().ToString("N0"));
                format = format.Replace("@currentHp", playingChar.CurrentHp.ToString("N0"));
                format = format.Replace("@maxHp", playingChar.GetCaches().MaxHp.ToString("N0"));
                format = format.Replace("@currentMp", playingChar.CurrentMp.ToString("N0"));
                format = format.Replace("@maxMp", playingChar.GetCaches().MaxMp.ToString("N0"));
                format = format.Replace("@currentMapName", playingChar.CurrentMapName);
                format = format.Replace("@currentPosition", playingChar.CurrentPosition.ToString());
#if !DISABLE_DIFFER_MAP_RESPAWNING
                format = format.Replace("@respawnMapName", playingChar.RespawnMapName);
                format = format.Replace("@respawnPosition", playingChar.RespawnPosition.ToString());
#endif
            }
            else
            {
                // Replace all with fallback values when character is null
                format = format.Replace("@characterName", "?");
                format = format.Replace("@level", "?");
                format = format.Replace("@characterClass", "?");
                format = format.Replace("@exp", "?");
                format = format.Replace("@nextExp", "?");
                format = format.Replace("@currentHp", "?");
                format = format.Replace("@maxHp", "?");
                format = format.Replace("@currentMp", "?");
                format = format.Replace("@maxMp", "?");
                format = format.Replace("@currentMapName", "?");
                format = format.Replace("@currentPosition", "?");
#if !DISABLE_DIFFER_MAP_RESPAWNING
                format = format.Replace("@respawnMapName", "?");
                format = format.Replace("@respawnPosition", "?");
#endif
            }
            
            return format;
        }
    }
}







