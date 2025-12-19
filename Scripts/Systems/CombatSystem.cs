using System;
using UnityEngine;

namespace CoreBreaker.Systems
{
    public enum HitGrade
    {
        Perfect,
        Great,
        Good,
        Miss
    }

    public class CombatSystem : MonoBehaviour
    {
        [Header("Base Damage")]
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private int chargedDamage = 45;

        [Header("Combo")]
        [SerializeField] private float comboBaseDuration = 1.2f;
        [SerializeField] private float comboPerfectBonus = 0.3f;
        [SerializeField] private float comboDurationMax = 2.0f;

        [Header("Counter Gauge")]
        [SerializeField] private int counterGaugeMax = 100;
        [SerializeField] private int counterGaugePerMiss = 10;
        [SerializeField] private int counterGaugePerfectReduce = 15;
        [SerializeField] private float stunDuration = 0.7f;

        public event Action<int> OnComboChanged;
        public event Action<int> OnCounterGaugeChanged;
        public event Action<float> OnComboTimerChanged;
        public event Action<HitGrade> OnHitGrade;
        public event Action<float> OnStun;

        private int combo;
        private int counterGauge;
        private int bonusBaseDamage;
        private int bonusChargedDamage;
        private int bonusPerfectGaugeReduction;
        private float comboTimer;
        private float currentComboDuration;
        private bool isStunned;
        private float stunTimer;

        public int Combo => combo;
        public int CounterGauge => counterGauge;
        public bool IsStunned => isStunned;

        private void Update()
        {
            if (isStunned)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0f)
                {
                    isStunned = false;
                }
            }

            if (combo > 0)
            {
                comboTimer -= Time.deltaTime;
                OnComboTimerChanged?.Invoke(Mathf.Max(0f, comboTimer / currentComboDuration));
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        private void Start()
        {
            var data = SaveSystem.Load();
            ApplyEquipment(data.levels);
        }

        public bool CanInput()
        {
            return !isStunned;
        }

        public int GetBaseDamage(bool charged)
        {
            return charged ? chargedDamage + bonusChargedDamage : baseDamage + bonusBaseDamage;
        }

        public float GetComboMultiplier(int currentCombo)
        {
            if (currentCombo >= 60)
            {
                return 1.6f;
            }

            if (currentCombo >= 40)
            {
                return 1.4f;
            }

            if (currentCombo >= 20)
            {
                return 1.25f;
            }

            if (currentCombo >= 10)
            {
                return 1.1f;
            }

            return 1.0f;
        }

        public int ApplyHit(HitGrade grade, int rawDamage, int comboBonusFromGrade)
        {
            if (isStunned)
            {
                return 0;
            }

            var finalDamage = rawDamage;

            switch (grade)
            {
                case HitGrade.Perfect:
                    finalDamage = Mathf.RoundToInt(rawDamage * 2.0f);
                    AdjustCombo(comboBonusFromGrade);
                    AddComboTime(comboPerfectBonus);
                    ModifyCounterGauge(-(counterGaugePerfectReduce + bonusPerfectGaugeReduction));
                    break;
                case HitGrade.Great:
                    finalDamage = Mathf.RoundToInt(rawDamage * 1.5f);
                    AdjustCombo(comboBonusFromGrade);
                    break;
                case HitGrade.Good:
                    finalDamage = rawDamage;
                    AdjustCombo(comboBonusFromGrade);
                    break;
                case HitGrade.Miss:
                    finalDamage = 0;
                    ResetCombo();
                    ModifyCounterGauge(counterGaugePerMiss);
                    break;
            }

            OnHitGrade?.Invoke(grade);

            return finalDamage;
        }

        public void ApplyEquipment(EquipmentLevels levels)
        {
            bonusBaseDamage = Mathf.Max(0, levels.gloveLevel - 1);
            bonusChargedDamage = Mathf.Max(0, (levels.hammerLevel - 1) * 3);
            bonusPerfectGaugeReduction = Mathf.Max(0, levels.droneLevel - 1);
        }

        public void ResetCombo()
        {
            combo = 0;
            comboTimer = 0f;
            currentComboDuration = comboBaseDuration;
            OnComboChanged?.Invoke(combo);
            OnComboTimerChanged?.Invoke(0f);
        }

        public void AdjustCombo(int amount)
        {
            combo = Mathf.Max(0, combo + amount);
            currentComboDuration = Mathf.Min(comboDurationMax, comboBaseDuration + (combo - 1) * 0.02f);
            comboTimer = currentComboDuration;
            OnComboChanged?.Invoke(combo);
        }

        public void AddComboTime(float bonus)
        {
            currentComboDuration = Mathf.Min(comboDurationMax, currentComboDuration + bonus);
            comboTimer = currentComboDuration;
            OnComboTimerChanged?.Invoke(1f);
        }

        public void ModifyCounterGauge(int amount)
        {
            counterGauge = Mathf.Clamp(counterGauge + amount, 0, counterGaugeMax);
            OnCounterGaugeChanged?.Invoke(counterGauge);

            if (counterGauge >= counterGaugeMax)
            {
                counterGauge = 0;
                OnCounterGaugeChanged?.Invoke(counterGauge);
                TriggerStun();
            }
        }

        private void TriggerStun()
        {
            isStunned = true;
            stunTimer = stunDuration;
            ResetCombo();
            OnStun?.Invoke(stunDuration);
        }
    }
}
