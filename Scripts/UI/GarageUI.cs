using UnityEngine;
using UnityEngine.UI;
using CoreBreaker.Systems;

namespace CoreBreaker.UI
{
    public class GarageUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text gloveLevelText;
        [SerializeField] private Text hammerLevelText;
        [SerializeField] private Text droneLevelText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text partsText;

        private SaveData saveData;

        private void Start()
        {
            saveData = SaveSystem.Load();
            RefreshUI();
        }

        public void OnUpgradeGlove()
        {
            UpgradeLevel(ref saveData.levels.gloveLevel);
        }

        public void OnUpgradeHammer()
        {
            UpgradeLevel(ref saveData.levels.hammerLevel);
        }

        public void OnUpgradeDrone()
        {
            UpgradeLevel(ref saveData.levels.droneLevel);
        }

        public void OnResetSave()
        {
            SaveSystem.ResetSave();
            saveData = SaveSystem.Load();
            RefreshUI();
        }

        private void UpgradeLevel(ref int level)
        {
            var cost = level * 100;
            if (saveData.coins < cost || level >= 10)
            {
                return;
            }

            saveData.coins -= cost;
            level += 1;
            SaveSystem.Save(saveData);
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (gloveLevelText != null)
            {
                gloveLevelText.text = $"Lv {saveData.levels.gloveLevel}";
            }

            if (hammerLevelText != null)
            {
                hammerLevelText.text = $"Lv {saveData.levels.hammerLevel}";
            }

            if (droneLevelText != null)
            {
                droneLevelText.text = $"Lv {saveData.levels.droneLevel}";
            }

            if (coinsText != null)
            {
                coinsText.text = $"Coins {saveData.coins}";
            }

            if (partsText != null)
            {
                partsText.text = $"Parts {saveData.parts}";
            }

            if (statsText != null)
            {
                var baseDamage = 10 + saveData.levels.gloveLevel - 1;
                var chargedDamage = 45 + (saveData.levels.hammerLevel - 1) * 3;
                var droneBonus = (saveData.levels.droneLevel - 1);
                statsText.text = $"Base +{baseDamage}\nCharge +{chargedDamage}\nPerfect Gauge -{15 + droneBonus}";
            }
        }
    }
}
