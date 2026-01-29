using NightBlade.UnityEditorUtils;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NightBlade
{
    public class MinimapCreator : MonoBehaviour
    {
#if UNITY_EDITOR
        public const int TEXTURE_WIDTH_AND_HEIGHT = 2048;
        public const int TEXTURE_DEPTH = 24;
        public const float SPRITE_PIXELS_PER_UNIT = 100f;

        public BaseMapInfo targetMapInfo;
        public int widthAndHeight = 2048;
        public int depth = 24;
        public LayerMask cullingMask = ~0;
        public Color clearFlagsBackgroundColor = Color.black;
        public string minimapSuffix = "_minimap";
        public float cameraYPosition = 50f;
        public bool makeByTerrain = false;
        public bool makeByCollider = true;
        public bool makeByCollider2D = true;
        public bool makeByRenderer = false;
        public bool doNotDestroyCamera = false;
        public Camera customCamera;

        [InspectorButton(nameof(CreateByComponent))]
        public bool create;

        [ContextMenu("Create")]
        public void CreateByComponent()
        {
            Create(
                targetMapInfo,
                cullingMask,
                clearFlagsBackgroundColor,
                minimapSuffix,
                cameraYPosition,
                makeByTerrain,
                makeByCollider,
                makeByCollider2D,
                makeByRenderer,
                doNotDestroyCamera,
                customCamera,
                widthAndHeight,
                depth);
        }

        public static void Create(
            BaseMapInfo targetMapInfo,
            LayerMask cullingMask,
            Color clearFlagsBackgroundColor,
            string minimapSuffix,
            float cameraYPosition,
            bool makeByTerrain,
            bool makeByCollider,
            bool makeByCollider2D,
            bool makeByRenderer,
            bool doNotDestroyCamera = false,
            Camera customCamera = null,
            int? widthAndHeight = null,
            int? depth = null)
        {
            // Find bounds
            Bounds bounds = default;
            bool setBoundsOnce = false;
            for (int i = 0; i < SceneManager.loadedSceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] rootGameObjects = scene.GetRootGameObjects();
                for (int j = 0; j < rootGameObjects.Length; ++j)
                {
                    if (makeByTerrain)
                    {
                        TerrainCollider[] objects = rootGameObjects[j].GetComponentsInChildren<TerrainCollider>();
                        foreach (var obj in objects)
                        {
                            if (!setBoundsOnce)
                                bounds = obj.bounds;
                            else
                                bounds.Encapsulate(obj.bounds);
                            setBoundsOnce = true;
                        }
                    }
                    if (makeByCollider)
                    {
                        Collider[] objects = rootGameObjects[j].GetComponentsInChildren<Collider>();
                        foreach (var obj in objects)
                        {
                            if (obj is TerrainCollider)
                                continue;
                            if (!setBoundsOnce)
                                bounds = obj.bounds;
                            else
                                bounds.Encapsulate(obj.bounds);
                            setBoundsOnce = true;
                        }
                    }
                    if (makeByCollider2D)
                    {
                        Collider2D[] objects = rootGameObjects[j].GetComponentsInChildren<Collider2D>();
                        foreach (var obj in objects)
                        {
                            if (!setBoundsOnce)
                                bounds = obj.bounds;
                            else
                                bounds.Encapsulate(obj.bounds);
                            setBoundsOnce = true;
                        }
                    }
                    if (makeByRenderer)
                    {
                        Renderer[] objects = rootGameObjects[j].GetComponentsInChildren<Renderer>();
                        foreach (var obj in objects)
                        {
                            if (!setBoundsOnce)
                                bounds = obj.bounds;
                            else
                                bounds.Encapsulate(obj.bounds);
                            setBoundsOnce = true;
                        }
                    }
                }
            }

            // Create camera
            GameObject cameraGameObject = customCamera != null ? customCamera.gameObject : new GameObject("_MinimapMakerCamera");
            Camera camera = customCamera != null ? customCamera : cameraGameObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(bounds.center.x, cameraYPosition, bounds.center.z);
            camera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = clearFlagsBackgroundColor;
            camera.orthographic = true;
            camera.cullingMask = cullingMask.value;

            // Make texture
            int tempWidthAndHeight = widthAndHeight.HasValue ? widthAndHeight.Value : TEXTURE_WIDTH_AND_HEIGHT;
            int tempDepth = depth.HasValue ? depth.Value : TEXTURE_DEPTH;
            RenderTexture renderTexture = new RenderTexture(tempWidthAndHeight, tempWidthAndHeight, TEXTURE_DEPTH);
            Rect rect = new Rect(0, 0, tempWidthAndHeight, tempWidthAndHeight);
            Texture2D texture = new Texture2D(tempWidthAndHeight, tempWidthAndHeight, TextureFormat.RGBA32, false);

            camera.targetTexture = renderTexture;
            camera.Render();

            // Switch render texture to apply pixel to texture
            RenderTexture currentRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            // Switch render texture back
            camera.targetTexture = null;
            RenderTexture.active = currentRenderTexture;

            // Save texture
            string path;
            if (targetMapInfo != null)
            {
                path = AssetDatabase.GetAssetPath(targetMapInfo);
                path = path.Substring(0, path.Length - ".asset".Length);
                path += minimapSuffix + ".png";
            }
            else
            {
                path = EditorUtility.SaveFilePanel("Save texture as", "Assets", "minimap", "png");
            }
            Debug.Log("Saving mini-map data to " + path);
            AssetDatabase.DeleteAsset(path);
            var pngData = texture.EncodeToPNG();
            if (pngData != null)
                File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();

            TextureImporter tempTextureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            tempTextureImporter.textureType = TextureImporterType.Sprite;
            tempTextureImporter.spriteImportMode = SpriteImportMode.Single;
            tempTextureImporter.spritePivot = Vector2.one * 0.5f;
            tempTextureImporter.spritePixelsPerUnit = SPRITE_PIXELS_PER_UNIT;
            EditorUtility.SetDirty(tempTextureImporter);
            tempTextureImporter.SaveAndReimport();
            var tempSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (targetMapInfo != null)
            {
                targetMapInfo.MinimapSprite = tempSprite;
                targetMapInfo.MinimapPosition = new Vector3(bounds.center.x, 0, bounds.center.z);
                targetMapInfo.MinimapBoundsWidth = bounds.size.x;
                targetMapInfo.MinimapBoundsLength = bounds.size.z;
                targetMapInfo.MinimapOrthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z);
                EditorUtility.SetDirty(targetMapInfo);
            }

            DestroyImmediate(texture);
            DestroyImmediate(renderTexture);
            if (!doNotDestroyCamera && customCamera == null)
                DestroyImmediate(cameraGameObject);
        }
#endif
    }
}







