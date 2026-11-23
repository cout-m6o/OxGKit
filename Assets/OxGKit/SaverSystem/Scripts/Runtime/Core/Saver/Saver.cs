using System;
using System.Collections.Generic;

namespace OxGKit.SaverSystem
{
    public abstract class Saver : ISaver, IDisposable
    {
        /// <summary>
        /// DataMap dirty 標記處理
        /// </summary>
        private Dictionary<string, bool> _dataMapDirtyFlags = new Dictionary<string, bool>();

        /// <summary>
        /// DataMap 緩存
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _dataMaps = new Dictionary<string, Dictionary<string, string>>();

        public abstract void SaveString(string key, string value);

        public abstract string GetString(string key, string defaultValue = "");

        public abstract void SaveInt(string key, int value);

        public abstract int GetInt(string key, int defaultValue = 0);

        public abstract void SaveFloat(string key, float value);

        public abstract float GetFloat(string key, float defaultValue = 0);

        public abstract bool HasKey(string key);

        public abstract void DeleteKey(string key);

        public abstract void DeleteAll();

        /// <summary>
        /// 透過文本形式儲存數據
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SaveData(string contentKey, string key, string value)
        {
            string content = this.GetString(contentKey);

            // 以「行」為單位尋找該 key 的數據行，僅替換該行的值
            // （不可對整份文本做子字串取代，否則 key 為其他 key 的結尾時會一併誤改，例如 volume 與 bgm_volume）
            if (_TryFindDataLine(content, key, 0, out int lineStart, out int lineEnd))
            {
                int valueStart = lineStart + key.Length + 1;
                int valueEnd = lineEnd;
                // 保留行尾的 \r（CRLF 文本）
                if (valueEnd > valueStart && content[valueEnd - 1] == '\r')
                    valueEnd--;

                content = string.Concat(content.Substring(0, valueStart), value, content.Substring(valueEnd));
            }
            else
            {
                // 文本結尾沒有換行時先補上，避免新數據接在上一行的尾端
                if (!string.IsNullOrEmpty(content) && content[content.Length - 1] != '\n')
                    content += "\n";

                content += $"{key} {value}\n";
            }

            this.SaveString(contentKey, content);

            // Dirty check
            this._dataMapDirtyFlags[contentKey] = true;
        }

        /// <summary>
        /// 獲取文本中的特定數據
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetData(string contentKey, string key, string defaultValue = null)
        {
            string content = this.GetString(contentKey);
            if (string.IsNullOrEmpty(content))
                return defaultValue;

            // Check dirty state
            this._dataMapDirtyFlags.TryGetValue(contentKey, out bool isDirty);
            Dictionary<string, string> dataMap;
            if (isDirty)
            {
                dataMap = ParsingDataMap(content);
                this._dataMaps[contentKey] = dataMap;
                this._dataMapDirtyFlags[contentKey] = false;
            }
            else if (!this._dataMaps.TryGetValue(contentKey, out dataMap))
            {
                dataMap = ParsingDataMap(content);
                this._dataMaps.Add(contentKey, dataMap);
            }

            dataMap.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value;
        }

        /// <summary>
        /// 刪除文本中的特定數據
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="key"></param>
        public void DeleteData(string contentKey, string key)
        {
            string content = this.GetString(contentKey);

            // 以「行」為單位移除該 key 的數據行（含換行），不影響其他 key
            int searchStart = 0;
            while (_TryFindDataLine(content, key, searchStart, out int lineStart, out int lineEnd))
            {
                int removeEnd = lineEnd < content.Length ? lineEnd + 1 : lineEnd;
                content = content.Remove(lineStart, removeEnd - lineStart);
                searchStart = lineStart;
            }

            this.SaveString(contentKey, content);

            // Dirty check
            this._dataMapDirtyFlags[contentKey] = true;
        }

        /// <summary>
        /// 刪除全文本數據
        /// </summary>
        /// <param name="contextKey"></param>
        public void DeleteContext(string contextKey)
        {
            this.DeleteKey(contextKey);
            this._dataMapDirtyFlags.Remove(contextKey);
            this._dataMaps.Remove(contextKey);
        }

        /// <summary>
        /// 解析文本數據
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParsingDataMap(string content)
        {
            Dictionary<string, string> dataMap = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(content))
                return dataMap;

            // 直接掃描文本逐行解析（規則與結果同先前的 Split 寫法，但不再產生每行的字串與陣列）
            int length = content.Length;
            int lineStart = 0;
            while (lineStart <= length)
            {
                int lineEnd = content.IndexOf('\n', lineStart);
                if (lineEnd == -1)
                    lineEnd = length;

                // 略過空行與註解行（# 開頭）
                if (lineEnd > lineStart && content[lineStart] != '#')
                {
                    // 以第一個空白分隔 key 與 value（value 可包含空白）
                    int spaceIndex = content.IndexOf(' ', lineStart, lineEnd - lineStart);
                    if (spaceIndex != -1)
                    {
                        string key = content.Substring(lineStart, spaceIndex - lineStart);
                        if (!dataMap.ContainsKey(key))
                        {
                            string value = content.Substring(spaceIndex + 1, lineEnd - spaceIndex - 1);
                            if (value.IndexOf('\r') != -1)
                                value = value.Replace("\r", "");
                            dataMap.Add(key, value);
                        }
                    }
                }

                lineStart = lineEnd + 1;
            }

            return dataMap;
        }

        /// <summary>
        /// 自 startIndex（必須為行首）開始尋找第一個 key 相符的數據行，判斷規則與 ParsingDataMap 一致
        /// <para> 數據行：非 # 開頭，且第一個空白之前的內容即為 key。 </para>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key"></param>
        /// <param name="startIndex"></param>
        /// <param name="lineStart">該行起始位置</param>
        /// <param name="lineEnd">該行結束位置（不含換行）</param>
        /// <returns></returns>
        private static bool _TryFindDataLine(string content, string key, int startIndex, out int lineStart, out int lineEnd)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lineStart = 0;
            lineEnd = 0;

            if (string.IsNullOrEmpty(content))
                return false;

            int length = content.Length;
            int start = startIndex;
            while (start <= length)
            {
                int end = content.IndexOf('\n', start);
                if (end == -1)
                    end = length;

                // 略過空行與註解行（# 開頭）
                if (end > start && content[start] != '#')
                {
                    int spaceIndex = content.IndexOf(' ', start, end - start);
                    if (spaceIndex != -1 &&
                        spaceIndex - start == key.Length &&
                        string.CompareOrdinal(content, start, key, 0, key.Length) == 0)
                    {
                        lineStart = start;
                        lineEnd = end;
                        return true;
                    }
                }

                start = end + 1;
            }

            return false;
        }

        public virtual void Dispose()
        {
            this._dataMapDirtyFlags = null;
            this._dataMaps = null;
        }
    }
}
