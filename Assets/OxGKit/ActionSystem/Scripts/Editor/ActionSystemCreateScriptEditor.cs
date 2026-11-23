using UnityEditor;

namespace OxGKit.ActionSystem.Editor
{
    public static class ActionSystemCreateScriptEditor
    {
        private const string TPL_ACTION_SCRIPT_PATH = "TplScripts/TplAction.cs.txt";

        private static string _pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script ActionSystemCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Action System/Template Action.cs", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplAction()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("ActionSystemCreateScriptEditor.cs", "") + TPL_ACTION_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplAction.cs");
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/MoveAction.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplActionAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("ActionSystemCreateScriptEditor.cs", "") + TPL_ACTION_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }
    }
}
