using NightBlade.UnityEditorUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace NightBlade
{
    [CustomEditor(typeof(GameDatabase))]
    public class GameDatabaseEditor : BaseCustomEditor
    {
        protected override void SetFieldCondition()
        {
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Open Manager"))
            {
                EditorGlobalData.WorkingDatabase = target as GameDatabase;
                GameDatabaseManagerEditor.CreateNewEditor();
            }
        }
    }
}







