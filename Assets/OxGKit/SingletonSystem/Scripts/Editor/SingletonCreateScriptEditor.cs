using UnityEditor;

namespace OxGKit.SingletonSystem.Editor
{
    public static class SingletonCreateScriptEditor
    {
        private const string TPL_MONO_SINGLETON_SCRIPT_PATH = "TplScripts/TplMonoSingleton.cs.txt";

        // find current file path
        private static string _pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script SingletonCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/SingletonSystem/Template Mono Singleton.cs", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplMonoSingleton()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("SingletonCreateScriptEditor.cs", "") + TPL_MONO_SINGLETON_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplMonoSingleton.cs");
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/GameSystem.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplMonoSingletonAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("SingletonCreateScriptEditor.cs", "") + TPL_MONO_SINGLETON_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }
    }
}
