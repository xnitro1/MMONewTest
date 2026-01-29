using UnityEngine;
using UnityEditor;
using NightBlade.Core.Utils;

namespace NightBlade
{
    [CustomEditor(typeof(DistanceBasedUpdater))]
    public class DistanceBasedUpdaterEditor : Editor
    {
        private DistanceBasedUpdater distanceUpdater;

        private void OnEnable()
        {
            distanceUpdater = (DistanceBasedUpdater)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding and documentation button
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("📏 NightBlade Distance-Based Updater", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenDistanceBasedUpdaterDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Show help text
            EditorGUILayout.HelpBox("Automatically reduces update frequency for distant entities to improve performance and enable higher CCU capacities.", MessageType.Info);

            // Draw the default inspector for all other properties
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void OpenDistanceBasedUpdaterDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "DistanceBasedUpdater.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened DistanceBasedUpdater documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/DistanceBasedUpdater.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}