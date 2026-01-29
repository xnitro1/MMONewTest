using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NightBlade
{
    public class MinimapCreatorEditor : EditorWindow
    {
        private BaseMapInfo targetMapInfo;
        private LayerMask cullingMask = ~0;
        private Color clearFlagsBackgroundColor = Color.black;
        private string minimapSuffix = "_minimap";
        private float cameraYPosition = 50f;
        private bool makeByTerrain = false;
        private bool makeByCollider = true;
        private bool makeByCollider2D = true;
        private bool makeByRenderer = false;

        [MenuItem(EditorMenuConsts.MINIMAP_CREATOR_MENU, false, EditorMenuConsts.MINIMAP_CREATOR_ORDER)]
        public static void CreateNewMinimap()
        {
            GetWindow<MinimapCreatorEditor>();
        }

        private void OnGUI()
        {
            Vector2 wndRect = new Vector2(500, 500);
            maxSize = wndRect;
            minSize = wndRect;
            titleContent = new GUIContent("Minimap", null, "Minimap Creator");
            GUILayout.BeginVertical("Minimap Creator", "window");
            {
                GUILayout.BeginVertical("box");
                {
                    targetMapInfo = EditorGUILayout.ObjectField("Map Info", targetMapInfo, typeof(BaseMapInfo), true, GUILayout.ExpandWidth(true)) as BaseMapInfo;
                    LayerMask tempMask = EditorGUILayout.MaskField("Culling Mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(cullingMask), InternalEditorUtility.layers);
                    cullingMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
                    clearFlagsBackgroundColor = EditorGUILayout.ColorField("Clear Flags Background Color", Color.black);
                    minimapSuffix = EditorGUILayout.TextField("Minimap Suffix", minimapSuffix);
                    cameraYPosition = EditorGUILayout.FloatField("Camera Y Position", cameraYPosition);
                    makeByTerrain = EditorGUILayout.Toggle("Create By Terrain", makeByTerrain);
                    makeByCollider = EditorGUILayout.Toggle("Create By Collider", makeByCollider);
                    makeByCollider2D = EditorGUILayout.Toggle("Create By Collider2D", makeByCollider2D);
                    makeByRenderer = EditorGUILayout.Toggle("Create By Renderer", makeByRenderer);
                    if (GUILayout.Button("Create", GUILayout.ExpandWidth(true), GUILayout.Height(40)))
                        Create();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        private void Create()
        {
            MinimapCreator.Create(
                targetMapInfo,
                cullingMask,
                clearFlagsBackgroundColor,
                minimapSuffix,
                cameraYPosition,
                makeByTerrain,
                makeByCollider,
                makeByCollider2D,
                makeByRenderer);
        }
    }
}







