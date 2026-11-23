using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace OxGKit.NoticeSystem.Editor
{
    /// <summary>
    /// Creates a script from a script template directly at an asset path, without the name editing of the Project window
    /// (for automation, e.g. Unity CLI).
    /// <para>
    /// The substitutions are the same as Unity's ProjectWindowUtil.CreateScriptAssetFromTemplateFile,
    /// so the content is identical to what the "Assets/Create" menu produces.
    /// </para>
    /// </summary>
    internal static class ScriptTemplateCreator
    {
        private const string _ROOT_NAMESPACE_BEGIN_TAG = "#ROOTNAMESPACEBEGIN#";
        private const string _ROOT_NAMESPACE_END_TAG = "#ROOTNAMESPACEEND#";

        /// <summary>
        /// Create a script from the template at the given asset path
        /// </summary>
        /// <param name="templatePath">Path of the template file</param>
        /// <param name="assetPath">Asset path of the script, e.g. Assets/Scripts/NewScript.cs (the folder must exist, an existing file is never overwritten)</param>
        /// <returns>The asset path of the created script</returns>
        public static string Create(string templatePath, string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("The asset path of the script is required.", nameof(assetPath));

            assetPath = assetPath.Replace('\\', '/');
            if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The asset path of the script must end with .cs: {assetPath}", nameof(assetPath));

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            if (fileName.Replace(" ", "").Length == 0)
                throw new ArgumentException($"The asset path of the script has no file name: {assetPath}", nameof(assetPath));

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"The template file could not be found: {templatePath}", templatePath);

            string folder = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
                throw new DirectoryNotFoundException($"The destination folder does not exist: {folder}");

            if (File.Exists(assetPath))
                throw new IOException($"A file already exists at: {assetPath}");

            // Same root namespace Unity uses for a new script at this path (assembly definition first, then project settings)
            string rootNamespace = CompilationPipeline.GetAssemblyRootNamespaceFromScriptPath(assetPath);
            string content = Generate(File.ReadAllText(templatePath), fileName, rootNamespace);
            content = _SetLineEndings(content, EditorSettings.lineEndingsForNewScripts);

            File.WriteAllText(assetPath, content, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath);

            return assetPath;
        }

        /// <summary>
        /// Apply the script template substitutions
        /// </summary>
        /// <param name="templateContent">Content of the template</param>
        /// <param name="fileName">File name of the script without extension</param>
        /// <param name="rootNamespace">Root namespace, null or empty for none</param>
        /// <returns>The script content (with \n line endings)</returns>
        internal static string Generate(string templateContent, string fileName, string rootNamespace)
        {
            string content = templateContent.Replace("\r\n", "\n").Replace("\r", "\n");

            // #NOTRIM# only protects trailing whitespace of a line inside the template
            content = content.Replace("#NOTRIM#", "");

            // #NAME# = file name, #SCRIPTNAME# = file name without spaces
            string fileNameNoSpaces = fileName.Replace(" ", "");
            content = content.Replace("#NAME#", fileName);
            content = content.Replace("#SCRIPTNAME#", fileNameNoSpaces);

            content = _RemoveOrInsertNamespace(content, rootNamespace);

            // #SCRIPTNAME_LOWER# = lower camel case variant
            if (fileNameNoSpaces.Length > 0)
            {
                string lowerName = char.IsUpper(fileNameNoSpaces, 0)
                    ? char.ToLower(fileNameNoSpaces[0]) + fileNameNoSpaces.Substring(1)
                    : "my" + char.ToUpper(fileNameNoSpaces[0]) + fileNameNoSpaces.Substring(1);
                content = content.Replace("#SCRIPTNAME_LOWER#", lowerName);
            }

            return content;
        }

        private static string _RemoveOrInsertNamespace(string content, string rootNamespace)
        {
            if (!content.Contains(_ROOT_NAMESPACE_BEGIN_TAG) || !content.Contains(_ROOT_NAMESPACE_END_TAG))
                return content;

            // No root namespace: remove the two marker lines
            if (string.IsNullOrEmpty(rootNamespace))
            {
                content = Regex.Replace(content, $"\n[ \t]*{_ROOT_NAMESPACE_BEGIN_TAG}[ \t]*", string.Empty);
                content = Regex.Replace(content, $"\n[ \t]*{_ROOT_NAMESPACE_END_TAG}[ \t]*", string.Empty);
                return content;
            }

            var lines = new List<string>(content.Split('\n'));

            int i = 0;
            for (; i < lines.Count; ++i)
            {
                if (lines[i].Contains(_ROOT_NAMESPACE_BEGIN_TAG))
                    break;
            }

            // The whitespace in front of #ROOTNAMESPACEBEGIN# is the indentation of the wrapped lines
            string beginTagLine = lines[i];
            string indentation = beginTagLine.Substring(0, beginTagLine.IndexOf("#", StringComparison.Ordinal));

            lines[i] = $"namespace {rootNamespace}";
            lines.Insert(i + 1, "{");

            i += 2;
            for (; i < lines.Count; ++i)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line) || line.Trim().Length == 0)
                    continue;

                if (line.Contains(_ROOT_NAMESPACE_END_TAG))
                {
                    lines[i] = "}";
                    break;
                }

                lines[i] = $"{indentation}{line}";
            }

            return string.Join("\n", lines.ToArray());
        }

        private static string _SetLineEndings(string content, LineEndingsMode lineEndingsMode)
        {
            string lineEndings;
            switch (lineEndingsMode)
            {
                case LineEndingsMode.OSNative:
                    lineEndings = Application.platform == RuntimePlatform.WindowsEditor ? "\r\n" : "\n";
                    break;
                case LineEndingsMode.Windows:
                    lineEndings = "\r\n";
                    break;
                default:
                    lineEndings = "\n";
                    break;
            }

            return Regex.Replace(content, "\r\n?|\n", lineEndings);
        }
    }
}
