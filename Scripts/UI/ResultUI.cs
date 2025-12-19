using UnityEngine;
using UnityEngine.UI;
using CoreBreaker.Systems;

namespace CoreBreaker.UI
{
    public class ResultUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text rankText;
        [SerializeField] private Text rewardText;

        private SaveData saveData;

        public void Present(ResultData result)
        {
            saveData = SaveSystem.Load();
            var score = CalculateScore(result);
            var rank = GetRank(score);
            var rewards = CalculateRewards(rank, result);

            saveData.coins += rewards.coins;
            saveData.parts += rewards.parts;
            SaveSystem.Save(saveData);

            if (scoreText != null)
            {
                scoreText.text = $"Score {score}";
            }

            if (rankText != null)
            {
                rankText.text = $"Rank {rank}";
            }

            if (rewardText != null)
            {
                rewardText.text = $"Coins {rewards.coins}\nParts {rewards.parts}\nBlueprint {(rewards.blueprint ? "YES" : "NO")}";
            }
        }

        private int CalculateScore(ResultData result)
        {
            return result.totalDamage + result.perfectCount * 50 + result.maxCombo * 20 + Mathf.RoundToInt(result.remainingSeconds * 10);
        }

        private string GetRank(int score)
        {
            if (score >= 2200)
            {
                return "S";
            }

            if (score >= 1700)
            {
                return "A";
            }

            if (score >= 1200)
            {
                return "B";
            }

            return "C";
        }

        private RewardResult CalculateRewards(string rank, ResultData result)
        {
            var coins = Random.Range(50, 91);
            var parts = Mathf.Max(1, Random.Range(1, 4));
            var blueprintChance = rank == "S" ? 0.1f : 0.05f;
            var blueprint = Random.value <= blueprintChance;

            if (result.totalDamage <= 0)
            {
                parts = 1;
            }

            return new RewardResult
            {
                coins = coins,
                parts = parts,
                blueprint = blueprint
            };
        }
    }

    [System.Serializable]
    public struct ResultData
    {
        public int totalDamage;
        public int perfectCount;
        public int maxCombo;
        public float remainingSeconds;
    }

    public struct RewardResult
    {
        public int coins;
        public int parts;
        public bool blueprint;
    }
}
