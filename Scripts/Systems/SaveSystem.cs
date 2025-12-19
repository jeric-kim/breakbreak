using System;
using UnityEngine;

namespace CoreBreaker.Systems
{
    [Serializable]
    public class EquipmentLevels
    {
        public int gloveLevel = 1;
        public int hammerLevel = 1;
        public int droneLevel = 1;
    }

    [Serializable]
    public class EquipmentSlots
    {
        public string gloveId = "starter_glove";
        public string hammerId = "starter_hammer";
        public string droneId = "starter_drone";
    }

    [Serializable]
    public class SaveData
    {
        public EquipmentLevels levels = new EquipmentLevels();
        public EquipmentSlots equipped = new EquipmentSlots();
        public int coins = 0;
        public int parts = 0;
        public int totalRuns = 0;
    }

    public static class SaveSystem
    {
        private const string SaveKey = "CoreBreaker_Save";

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                var fresh = new SaveData();
                Save(fresh);
                return fresh;
            }

            var json = PlayerPrefs.GetString(SaveKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                var fresh = new SaveData();
                Save(fresh);
                return fresh;
            }

            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static void ResetSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }
}
