using UnityEngine;
using UnityEditor;
using NightBlade.Core.Utils;

namespace NightBlade
{
    [CustomEditor(typeof(CombatZoneManager))]
    public class CombatZoneManagerEditor : Editor
    {
        private CombatZoneManager combatZoneManager;

        private void OnEnable()
        {
            combatZoneManager = (CombatZoneManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding and documentation button
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("⚔️ NightBlade Combat Zone Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenCombatZoneManagerDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Show help text
            EditorGUILayout.HelpBox("Manages combat zones for CCU scaling by maintaining high performance in active combat areas while reducing performance in non-combat zones.", MessageType.Info);

            // Draw the default inspector for all other properties
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void OpenCombatZoneManagerDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "CombatZoneManager.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened CombatZoneManager documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/CombatZoneManager.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}