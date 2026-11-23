using UnityEditor;

namespace OxGKit.InfiniteScrollView.Editor
{
    public static class InfiniteScrollViewCreateScriptEditor
    {
        private const string TPL_INFINITE_CELL_SCRIPT_PATH = "TplScripts/TplInfiniteCell.cs.txt";

        // find current file path
        private static string _pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script InfiniteScrollViewCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Infinite ScrollView/Template Infinite Cell.cs (Script)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplInfiniteCell()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("InfiniteScrollViewCreateScriptEditor.cs", "") + TPL_INFINITE_CELL_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplInfiniteCell.cs");
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/ItemCell.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplInfiniteCellAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("InfiniteScrollViewCreateScriptEditor.cs", "") + TPL_INFINITE_CELL_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }
    }
}
