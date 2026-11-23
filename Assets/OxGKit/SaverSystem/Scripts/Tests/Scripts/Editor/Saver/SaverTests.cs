using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace OxGKit.SaverSystem.Editor.Tests
{
    public class SaverTests
    {
        private Saver _plyaerPrefSaver;
        private Saver _editorPrefSaver;

        private const string _CONTENT_KEY = "_CONTENT_KEY";

        [SetUp]
        public void Setup()
        {
            this._plyaerPrefSaver = new PlayerPrefsSaver();
            this._editorPrefSaver = new EditorPrefsSaver();
        }

        #region PlayerPrefs
        [Test]
        public void PlayerPrefsSaveAndGetData()
        {
            for (int i = 0; i < 10; i++)
                this._plyaerPrefSaver.SaveData(_CONTENT_KEY, $"{i}_key", $"{i}_value");

            Debug.Log(this._plyaerPrefSaver.GetString(_CONTENT_KEY));

            for (int i = 0; i < 10; i++)
                Debug.Log(this._plyaerPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }

        [Test]
        public void PlayerPrefsGetData()
        {
            for (int i = 0; i < 10; i++)
                Debug.Log(this._plyaerPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }

        [Test]
        public void PlayerPrefsDeleteAndGetData()
        {
            for (int i = 5; i < 10; i++)
                this._plyaerPrefSaver.DeleteData(_CONTENT_KEY, $"{i}_key");

            for (int i = 0; i < 10; i++)
                Debug.Log(this._plyaerPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }

        [Test]
        public void PlayerPrefsDeleteContextAndGetData()
        {
            this._plyaerPrefSaver.DeleteContext(_CONTENT_KEY);

            for (int i = 0; i < 10; i++)
                Debug.Log(this._plyaerPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }
        #endregion

        #region EditorPrefs
        [Test]
        public void EditorPrefsSaveAndGetData()
        {
            for (int i = 0; i < 10; i++)
                this._editorPrefSaver.SaveData(_CONTENT_KEY, $"{i}_key", $"{i}_value");

            Debug.Log(this._editorPrefSaver.GetString(_CONTENT_KEY));

            for (int i = 0; i < 10; i++)
                Debug.Log(this._editorPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }

        [Test]
        public void EditorPrefsGetData()
        {
            for (int i = 0; i < 10; i++)
                Debug.Log(this._editorPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }

        [Test]
        public void EditorPrefsDeleteAndGetData()
        {
            for (int i = 5; i < 10; i++)
                this._editorPrefSaver.DeleteData(_CONTENT_KEY, $"{i}_key");

            for (int i = 0; i < 10; i++)
                Debug.Log(this._editorPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }

        [Test]
        public void EditorPrefsDeleteContextAndGetData()
        {
            this._editorPrefSaver.DeleteContext(_CONTENT_KEY);

            for (int i = 0; i < 10; i++)
                Debug.Log(this._editorPrefSaver.GetData(_CONTENT_KEY, $"{i}_key", string.Empty));
        }
        #endregion

        #region Data Lines (in-memory saver, does not touch PlayerPrefs / EditorPrefs)
        private class MemorySaver : Saver
        {
            private readonly Dictionary<string, string> _store = new Dictionary<string, string>();

            public override void SaveString(string key, string value) { this._store[key] = value; }

            public override string GetString(string key, string defaultValue = "") { return this._store.TryGetValue(key, out var value) ? value : defaultValue; }

            public override void SaveInt(string key, int value) { }

            public override int GetInt(string key, int defaultValue = 0) { return defaultValue; }

            public override void SaveFloat(string key, float value) { }

            public override float GetFloat(string key, float defaultValue = 0f) { return defaultValue; }

            public override bool HasKey(string key) { return this._store.ContainsKey(key); }

            public override void DeleteKey(string key) { this._store.Remove(key); }

            public override void DeleteAll() { this._store.Clear(); }
        }

        [Test]
        public void SaveDataDoesNotChangeKeysEndingWithTheSameName()
        {
            var saver = new MemorySaver();
            saver.SaveData(_CONTENT_KEY, "bgm_volume", "1");
            saver.SaveData(_CONTENT_KEY, "volume", "1");

            saver.SaveData(_CONTENT_KEY, "volume", "0");

            Assert.AreEqual("0", saver.GetData(_CONTENT_KEY, "volume"));
            Assert.AreEqual("1", saver.GetData(_CONTENT_KEY, "bgm_volume"));
            Assert.AreEqual("bgm_volume 1\nvolume 0\n", saver.GetString(_CONTENT_KEY));
        }

        [Test]
        public void DeleteDataDoesNotChangeKeysEndingWithTheSameName()
        {
            var saver = new MemorySaver();
            saver.SaveData(_CONTENT_KEY, "bgm_volume", "1");
            saver.SaveData(_CONTENT_KEY, "volume", "1");

            saver.DeleteData(_CONTENT_KEY, "volume");

            Assert.AreEqual(null, saver.GetData(_CONTENT_KEY, "volume"));
            Assert.AreEqual("1", saver.GetData(_CONTENT_KEY, "bgm_volume"));
            Assert.AreEqual("bgm_volume 1\n", saver.GetString(_CONTENT_KEY));
        }

        [Test]
        public void SaveDataDoesNotChangeValuesStartingWithTheSameText()
        {
            var saver = new MemorySaver();
            saver.SaveData(_CONTENT_KEY, "a", "1");
            saver.SaveData(_CONTENT_KEY, "ba", "12");

            saver.SaveData(_CONTENT_KEY, "a", "2");

            Assert.AreEqual("2", saver.GetData(_CONTENT_KEY, "a"));
            Assert.AreEqual("12", saver.GetData(_CONTENT_KEY, "ba"));
        }

        [Test]
        public void SaveAndDeleteDataKeepCommentAndOtherLines()
        {
            var saver = new MemorySaver();
            saver.SaveString(_CONTENT_KEY, "# settings\na 1\nb 2\n");

            saver.SaveData(_CONTENT_KEY, "a", "9");
            saver.SaveData(_CONTENT_KEY, "c", "3");
            saver.DeleteData(_CONTENT_KEY, "b");

            Assert.AreEqual("# settings\na 9\nc 3\n", saver.GetString(_CONTENT_KEY));
            Assert.AreEqual("9", saver.GetData(_CONTENT_KEY, "a"));
            Assert.AreEqual(null, saver.GetData(_CONTENT_KEY, "b"));
            Assert.AreEqual("3", saver.GetData(_CONTENT_KEY, "c"));
        }

        [Test]
        public void ParsingDataMapReadsKeysValuesAndSkipsComments()
        {
            var map = Saver.ParsingDataMap("# comment\nname John Smith\r\nempty \nno_value\nname ignored duplicate\n");

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual("John Smith", map["name"]);
            Assert.AreEqual(string.Empty, map["empty"]);
        }
        #endregion
    }
}
