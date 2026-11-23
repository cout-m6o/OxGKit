using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace OxGKit.InfiniteScrollView.Editor
{
    public static class InfiniteScrollViewCreatePrefabEditor
    {
        class DoCreatePrefabAsset : EndNameEditAction
        {
            // Subclass and override this method to create specialised prefab asset creation functions
            protected virtual GameObject CreateGameObject(string name)
            {
                return new GameObject(name);
            }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                GameObject go = CreateGameObject(Path.GetFileNameWithoutExtension(pathName));
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, pathName);
                GameObject.DestroyImmediate(go);
            }
        }

        class DoCreateRectTransformPrefabAsset : DoCreatePrefabAsset
        {
            protected override GameObject CreateGameObject(string name)
            {
                var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));

                // calibrate RectTransform
                RectTransform objRect = obj.GetComponent<RectTransform>();
                objRect.anchorMin = new Vector2(0.5f, 0.5f);
                objRect.anchorMax = new Vector2(0.5f, 0.5f);
                objRect.sizeDelta = new Vector2(128, 128);
                objRect.localScale = Vector3.one;
                objRect.localPosition = Vector3.zero;

                return obj;
            }
        }

        static void CreatePrefabAsset(string name, DoCreatePrefabAsset createAction)
        {
            string directory = GetSelectedAssetDirectory();
            // 資產路徑一律使用正斜線；沒有選取任何資產時僅給檔名，由 Unity 決定建立位置（與腳本模板的行為一致）。
            string path = string.IsNullOrEmpty(directory) ? $"{name}.prefab" : $"{directory}/{name}.prefab";
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, createAction, path, EditorGUIUtility.FindTexture("Prefab Icon"), null);
        }

        static string GetSelectedAssetDirectory()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            // 沒有選取任何資產（或選取的不是資產）。
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            if (Directory.Exists(path))
                return path;
            else
                return Path.GetDirectoryName(path).Replace('\\', '/');
        }

        [MenuItem("Assets/Create/OxGKit/Infinite ScrollView/Template Infinite Cell (RectTransform Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplRectTransformCP()
        {
            CreatePrefabAsset("NewTplInfiniteCell", ScriptableObject.CreateInstance<DoCreateRectTransformPrefabAsset>());
        }

        /// <summary>
        /// Create the same template prefab directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Prefabs/ItemCell.prefab (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created prefab</returns>
        public static string CreateTplRectTransformCPAtPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new System.ArgumentException("The asset path of the prefab is required.", nameof(assetPath));

            assetPath = assetPath.Replace('\\', '/');
            if (!assetPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                throw new System.ArgumentException($"The asset path of the prefab must end with .prefab: {assetPath}", nameof(assetPath));

            if (Path.GetFileNameWithoutExtension(assetPath).Trim().Length == 0)
                throw new System.ArgumentException($"The asset path of the prefab has no file name: {assetPath}", nameof(assetPath));

            string folder = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
                throw new DirectoryNotFoundException($"The destination folder does not exist: {folder}");

            if (File.Exists(assetPath))
                throw new IOException($"A file already exists at: {assetPath}");

            // Same action the menu runs once the name has been confirmed
            var createAction = ScriptableObject.CreateInstance<DoCreateRectTransformPrefabAsset>();
            try
            {
                createAction.Action(0, assetPath, null);
            }
            finally
            {
                Object.DestroyImmediate(createAction);
            }

            return assetPath;
        }
    }
}