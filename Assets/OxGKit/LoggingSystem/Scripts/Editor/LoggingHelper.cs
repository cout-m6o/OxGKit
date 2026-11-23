using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OxGKit.LoggingSystem.Editor
{
    public static class LoggingHelper
    {
        [MenuItem(itemName: "Assets/Create/OxGKit/Logging System/[JSON - Plaintext] Create LoggerConfig.dat (In StreamingAssets)", isValidateFunction: false, priority: 0)]
        private static void _ExportAndCreateJsonLoggersConfig()
        {
            // Initialized loggers
            LoggingLauncher.InitLoggers();
            var loggersConfig = new LoggersConfig();
            LoggingLauncher.ReloadLoggersConfigAndSet(loggersConfig, (result) =>
            {
                WriteConfig(loggersConfig, ConfigFileType.Json);
                LoggingLauncherEditor.SaveEditorLoggersConfigData(loggersConfig);
            });
        }

        [MenuItem(itemName: "Assets/Create/OxGKit/Logging System/[BYTES - Cipher] Create LoggerConfig.dat (In StreamingAssets)", isValidateFunction: false, priority: 1)]
        private static void _ExportAndCreateBytesLoggersConfig()
        {
            // Initialized loggers
            LoggingLauncher.InitLoggers();
            var loggersConfig = new LoggersConfig();
            LoggingLauncher.ReloadLoggersConfigAndSet(loggersConfig, (result) =>
            {
                WriteConfig(loggersConfig, ConfigFileType.Bytes);
                LoggingLauncherEditor.SaveEditorLoggersConfigData(loggersConfig);
            });
        }

        [MenuItem("Assets/OxGKit/Logging System/Convert LoggerConfig.dat (BYTES [Cipher] <-> JSON [Plaintext])", false, -99)]
        private static void _ConvertConfigFile()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject != null)
            {
                // 獲取選中的資源的相對路徑
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);

                // 檢查選中的文件是否位於 StreamingAssets 資料夾內
                if (assetPath.StartsWith("Assets/StreamingAssets"))
                {
                    // Application.dataPath 返回的是 Assets 資料夾的完整路徑
                    string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetPath);

                    // 確保文件存在
                    if (File.Exists(fullPath))
                    {
                        string fileName = $"{LoggingSettings.settings.loggerCfgName}{LoggingSettings.settings.loggerCfgExtension}";
                        if (fullPath.IndexOf(fileName) == -1)
                        {
                            Debug.LogWarning($"Incorrect file selected. Please select the {fileName} file.");
                            return;
                        }

                        // 讀取文件內容
                        byte[] data = File.ReadAllBytes(fullPath);
                        var info = BinaryHelper.DecryptToString(data);
                        LoggersConfig loggersConfig = null;
                        bool isJsonConvertible;

                        try
                        {
                            loggersConfig = JsonUtility.FromJson<LoggersConfig>(info.content);
                            isJsonConvertible = true;
                        }
                        catch (Exception ex)
                        {
                            isJsonConvertible = false;
                            Debug.LogException(new Exception("Convert failed: The content format is not valid JSON or it does not match the expected structure.", ex));
                        }

                        if (isJsonConvertible && loggersConfig != null)
                        {
                            // 根據文件類型進行轉換
                            switch (info.type)
                            {
                                case ConfigFileType.Json:
                                    // JSON to Bytes
                                    WriteConfig(loggersConfig, ConfigFileType.Bytes);
                                    break;
                                case ConfigFileType.Bytes:
                                    // Bytes to JSON
                                    WriteConfig(loggersConfig, ConfigFileType.Json);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"File does not exist at path: {fullPath}");
                    }
                }
                else
                {
                    Debug.LogWarning("Selected file is not in StreamingAssets folder.");
                }
            }
            else
            {
                Debug.LogWarning("No file selected.");
            }
        }

        /// <summary>
        /// 寫入配置文件
        /// </summary>
        /// <param name="loggersConfig"></param>
        /// <param name="configFileType"></param>
        public static void WriteConfig(LoggersConfig loggersConfig, ConfigFileType configFileType = ConfigFileType.Bytes)
        {
            string fileName = $"{LoggingSettings.settings.loggerCfgName}{LoggingSettings.settings.loggerCfgExtension}";
            string savePath = Path.Combine(Application.streamingAssetsPath, fileName);

            // 獲取文件夾路徑
            string directoryPath = Path.GetDirectoryName(savePath);

            // 檢查文件夾是否存在，如果不存在則創建
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Created directory: {directoryPath}");
            }

            switch (configFileType)
            {
                // Json 類型
                case ConfigFileType.Json:
                    {
                        // 將配置轉換為 JSON 字符串
                        string json = JsonUtility.ToJson(loggersConfig, true);

                        // 寫入文件
                        File.WriteAllText(savePath, json);
                        AssetDatabase.Refresh();
                        Debug.Log($"Saved Loggers Config JSON to: {savePath}");
                    }
                    break;

                // Bytes 類型
                case ConfigFileType.Bytes:
                    {
                        // 將配置轉換為 JSON 字符串
                        string json = JsonUtility.ToJson(loggersConfig, false);

                        // Binary
                        var writeBuffer = BinaryHelper.EncryptToBytes(json);

                        // 寫入配置文件
                        File.WriteAllBytes(savePath, writeBuffer);
                        AssetDatabase.Refresh();
                        Debug.Log($"Saved Loggers Config BYTES to: {savePath}");
                    }
                    break;
            }
        }

        #region 自動化接口（不需要選取資產或操作 Inspector，提供給自動化流程使用，例如 Unity CLI）
        /// <summary>
        /// 取得配置文件的完整路徑（位於 StreamingAssets）
        /// </summary>
        /// <returns></returns>
        public static string GetConfigFilePath()
        {
            string fileName = $"{LoggingSettings.settings.loggerCfgName}{LoggingSettings.settings.loggerCfgExtension}";
            return Path.Combine(Application.streamingAssetsPath, fileName);
        }

        /// <summary>
        /// 嘗試讀取配置文件
        /// </summary>
        /// <param name="loggersConfig">讀取到的配置</param>
        /// <param name="configFileType">配置文件目前的類型</param>
        /// <returns>配置文件不存在或內容無法解析時返回 false</returns>
        public static bool TryReadConfig(out LoggersConfig loggersConfig, out ConfigFileType configFileType)
        {
            loggersConfig = null;
            configFileType = ConfigFileType.Bytes;

            string path = GetConfigFilePath();
            if (!File.Exists(path))
                return false;

            try
            {
                var info = BinaryHelper.DecryptToString(File.ReadAllBytes(path));
                configFileType = info.type;
                loggersConfig = JsonUtility.FromJson<LoggersConfig>(info.content);
            }
            catch (Exception ex)
            {
                loggersConfig = null;
                Debug.LogException(new Exception("Read failed: The content format is not valid JSON or it does not match the expected structure.", ex));
            }

            return loggersConfig != null;
        }

        /// <summary>
        /// 寫入配置文件，並同步 LoggingLauncher Inspector 所使用的編輯器緩存（Inspector 顯示的是緩存，只寫入文件的話 Inspector 仍會顯示舊的配置）
        /// </summary>
        /// <param name="loggersConfig"></param>
        /// <param name="configFileType"></param>
        public static void SaveConfig(LoggersConfig loggersConfig, ConfigFileType configFileType)
        {
            WriteConfig(loggersConfig, configFileType);
            LoggingLauncherEditor.SaveEditorLoggersConfigData(loggersConfig);
        }

        /// <summary>
        /// 建立配置文件，與選單 Create LoggerConfig.dat 相同：以目前找到的所有日誌器與其預設值重新建立（既有的配置文件會被覆蓋）
        /// </summary>
        /// <param name="configFileType"></param>
        /// <returns>建立的配置</returns>
        public static LoggersConfig CreateConfig(ConfigFileType configFileType)
        {
            // Initialized loggers
            LoggingLauncher.InitLoggers();
            var loggersConfig = new LoggersConfig();
            LoggingLauncher.ReloadLoggersConfigAndSet(loggersConfig, (result) =>
            {
                SaveConfig(loggersConfig, configFileType);
            });
            return loggersConfig;
        }

        /// <summary>
        /// 重載配置文件，與 LoggingLauncher Inspector 的 Reload 相同：保留既有設定，補上新增的日誌器，並移除已不存在的日誌器（維持配置文件原本的類型）
        /// </summary>
        /// <returns>重載後的配置，配置文件不存在或無法解析時返回 null</returns>
        public static LoggersConfig ReloadConfig()
        {
            if (!TryReadConfig(out var loggersConfig, out var configFileType))
                return null;

            // 舊版或手動編輯的配置文件可能缺少列表
            if (loggersConfig.loggerSettings == null)
                loggersConfig.loggerSettings = new System.Collections.Generic.List<LoggerSettings>();

            // Initialized loggers
            LoggingLauncher.InitLoggers();
            LoggingLauncher.ReloadLoggersConfigAndSet(loggersConfig, (result) =>
            {
                SaveConfig(result, configFileType);
            });
            return loggersConfig;
        }

        /// <summary>
        /// 轉換配置文件的類型（BYTES [Cipher] 與 JSON [Plaintext]），內容不變
        /// </summary>
        /// <param name="configFileType">要轉換成的類型</param>
        /// <returns>配置文件不存在或無法解析時返回 false</returns>
        public static bool ConvertConfig(ConfigFileType configFileType)
        {
            if (!TryReadConfig(out var loggersConfig, out var currentConfigFileType))
                return false;

            // 類型相同就不需要重寫
            if (currentConfigFileType != configFileType)
                WriteConfig(loggersConfig, configFileType);

            return true;
        }
        #endregion
    }
}