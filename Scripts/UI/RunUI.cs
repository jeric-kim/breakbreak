using System;
using UnityEngine;
using UnityEngine.UI;
using CoreBreaker.Systems;
using CoreBreaker.Gameplay;

namespace CoreBreaker.UI
{
    public class RunUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private SafeBoss safeBoss;

        [Header("UI")]
        [SerializeField] private Text timerText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Text comboText;
        [SerializeField] private Slider comboTimerSlider;
        [SerializeField] private Slider counterGaugeSlider;
        [SerializeField] private Slider chargeSlider;

        [Header("Run Settings")]
        [SerializeField] private float runLimitSeconds = 40f;

        public event Action OnRunFinished;

        private float remainingTime;
        private bool isRunning;

        private void Start()
        {
            remainingTime = runLimitSeconds;
            isRunning = true;
            UpdateTimer();

            if (combatSystem != null)
            {
                combatSystem.OnComboChanged += HandleCombo;
                combatSystem.OnComboTimerChanged += HandleComboTimer;
                combatSystem.OnCounterGaugeChanged += HandleCounterGauge;
                combatSystem.OnStun += HandleStun;
            }

            if (safeBoss != null)
            {
                safeBoss.OnHpChanged += HandleHp;
                safeBoss.OnPhaseChanged += HandlePhase;
            }
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnComboChanged -= HandleCombo;
                combatSystem.OnComboTimerChanged -= HandleComboTimer;
                combatSystem.OnCounterGaugeChanged -= HandleCounterGauge;
                combatSystem.OnStun -= HandleStun;
            }

            if (safeBoss != null)
            {
                safeBoss.OnHpChanged -= HandleHp;
                safeBoss.OnPhaseChanged -= HandlePhase;
            }
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                FinishRun();
            }

            UpdateTimer();
        }

        public float RemainingSeconds => remainingTime;

        public void UpdateCharge(float normalized)
        {
            if (chargeSlider != null)
            {
                chargeSlider.value = normalized;
            }
        }

        private void UpdateTimer()
        {
            if (timerText != null)
            {
                timerText.text = $"{remainingTime:0.0}s";
            }
        }

        private void FinishRun()
        {
            isRunning = false;
            OnRunFinished?.Invoke();
        }

        private void HandleCombo(int value)
        {
            if (comboText != null)
            {
                comboText.text = $"Combo {value}";
            }
        }

        private void HandleComboTimer(float normalized)
        {
            if (comboTimerSlider != null)
            {
                comboTimerSlider.value = normalized;
            }
        }

        private void HandleCounterGauge(int value)
        {
            if (counterGaugeSlider != null)
            {
                counterGaugeSlider.value = value / 100f;
            }
        }

        private void HandleHp(int current, int max)
        {
            if (hpSlider != null)
            {
                hpSlider.value = max > 0 ? (float)current / max : 0f;
            }
        }

        private void HandlePhase(BossPhase phase)
        {
            if (hpSlider != null)
            {
                hpSlider.value = 1f;
            }
        }

        private void HandleStun(float duration)
        {
            if (comboText != null)
            {
                comboText.text = "Stunned";
            }
        }
    }
}
