using UnityEditor;
using UnityEngine;

namespace NightBlade
{
    public class PlayerPrefsTools
    {
        [MenuItem(EditorMenuConsts.DELETE_ALL_PLAYER_PREFS_MENU, false, EditorMenuConsts.DELETE_ALL_PLAYER_PREFS_ORDER)]
        public static void DeleteAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}







