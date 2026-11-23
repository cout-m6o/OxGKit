using UnityEditor;

namespace OxGKit.InputSystem.Editor
{
    public static class InputSystemCreateScriptEditor
    {
        private const string TPL_INPUT_BINDING_COMPOSITE_SCRIPT_PATH = "TplScripts/TplInputBindingComposite.cs.txt";
        private const string TPL_INPUT_ACTION_SCRIPT_PATH = "TplScripts/TplInputAction.cs.txt";

        // find current file path
        private static string _pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script InputSystemCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Input System/New Input System (Extension)/Template Input Binding Composite.cs (For Unity New Input System)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplInputBindingComposite()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("InputSystemCreateScriptEditor.cs", "") + TPL_INPUT_BINDING_COMPOSITE_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplInputBindingComposite.cs");
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Input System/Template Input Action.cs (Input Interface For Any)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplInputAction()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("InputSystemCreateScriptEditor.cs", "") + TPL_INPUT_ACTION_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplInputAction.cs");
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/MoveInput.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplInputBindingCompositeAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("InputSystemCreateScriptEditor.cs", "") + TPL_INPUT_BINDING_COMPOSITE_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/PlayerAction.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplInputActionAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("InputSystemCreateScriptEditor.cs", "") + TPL_INPUT_ACTION_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }
    }
}
