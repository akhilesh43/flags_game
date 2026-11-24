using System;
using System.IO;
using UnityEngine;

namespace LovMates
{
    public static class SaveSystem
    {
        static string FilePath { get { return Path.Combine(Application.persistentDataPath, "lovmates_save.json"); } }

        public static SaveData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                    if (d != null) return d;
                }
            }
            catch (Exception e) { Debug.LogWarning("Save load failed: " + e.Message); }
            return new SaveData();
        }

        public static void Save(SaveData d)
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(d)); }
            catch (Exception e) { Debug.LogWarning("Save failed: " + e.Message); }
        }
    }
}
