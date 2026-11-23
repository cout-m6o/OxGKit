using UnityEditor;

namespace OxGKit.NoticeSystem.Editor
{
    public static class NoticeSystemCreateScriptEditor
    {
        private const string TPL_NOTICE_CONDITION_REGISTERS_SCRIPT_PATH = "TplScripts/TplNoticeConditionRegisters.cs.txt";
        private const string TPL_NOTICE_CONDITION_SCRIPT_PATH = "TplScripts/TplNoticeCondition.cs.txt";
        private const string TPL_NOTICE_CONDITION_WITH_BEFORE_SCENE_LOAD_SCRIPT_PATH = "TplScripts/TplNoticeConditionWithBeforeSceneLoad.cs.txt";

        // find current file path
        private static string _pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script NoticeSystemCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Notice System/Template Notice Condition Registers.cs (Manually)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplNoticeConditionRegisters()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("NoticeSystemCreateScriptEditor.cs", "") + TPL_NOTICE_CONDITION_REGISTERS_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplNoticeConditionRegisters.cs");
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Notice System/Template Notice Condition.cs (Manually)", isValidateFunction: false, priority: 52)]
        public static void CreateScriptTplNoticeCondition()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("NoticeSystemCreateScriptEditor.cs", "") + TPL_NOTICE_CONDITION_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplNoticeCondition.cs");
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Notice System/Template Notice Condition.cs (RuntimeInitializeLoadType.BeforeSceneLoad)", isValidateFunction: false, priority: 52)]
        public static void CreateScriptTplNoticeConditionWithBeforeSceneLoad()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("NoticeSystemCreateScriptEditor.cs", "") + TPL_NOTICE_CONDITION_WITH_BEFORE_SCENE_LOAD_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplNoticeConditionWithBeforeSceneLoad.cs");
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/NoticeConditionRegister.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplNoticeConditionRegistersAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("NoticeSystemCreateScriptEditor.cs", "") + TPL_NOTICE_CONDITION_REGISTERS_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/NewMailCond.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplNoticeConditionAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("NoticeSystemCreateScriptEditor.cs", "") + TPL_NOTICE_CONDITION_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }

        /// <summary>
        /// Create the script from the same template directly at the asset path, without the name editing of the Project window (for automation, e.g. Unity CLI)
        /// </summary>
        /// <param name="assetPath">e.g. Assets/Scripts/NewMailCond.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string CreateScriptTplNoticeConditionWithBeforeSceneLoadAtPath(string assetPath)
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("NoticeSystemCreateScriptEditor.cs", "") + TPL_NOTICE_CONDITION_WITH_BEFORE_SCENE_LOAD_SCRIPT_PATH;

            return ScriptTemplateCreator.Create(finalPath, assetPath);
        }
    }
}
