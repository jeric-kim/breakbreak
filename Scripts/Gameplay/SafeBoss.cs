using System;
using UnityEngine;

namespace CoreBreaker.Gameplay
{
    public enum BossPhase
    {
        Shield,
        WeakPoint,
        Core,
        Defeated
    }

    public class SafeBoss : MonoBehaviour
    {
        [Header("Phase HP")]
        [SerializeField] private int shieldHp = 180;
        [SerializeField] private int weakPointHp = 120;
        [SerializeField] private int coreHp = 220;

        [Header("Feedback")]
        [SerializeField] private ParticleSystem shardParticles;
        [SerializeField] private GameObject crackOverlayRoot;
        [SerializeField] private Transform coreVisual;
        [SerializeField] private AnimationCurve corePumpCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.08f);

        public event Action<int, int> OnHpChanged;
        public event Action<BossPhase> OnPhaseChanged;
        public event Action OnCoreFinalHit;

        private BossPhase phase = BossPhase.Shield;
        private int currentHp;
        private int maxHp;
        private int crackStage;

        public BossPhase Phase => phase;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;

        private void Awake()
        {
            SetPhase(BossPhase.Shield);
        }

        public void ApplyDamage(int damage)
        {
            if (phase == BossPhase.Defeated || damage <= 0)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - damage);
            OnHpChanged?.Invoke(currentHp, maxHp);
            UpdateCrackFeedback();

            if (phase == BossPhase.Core && currentHp <= Mathf.RoundToInt(maxHp * 0.1f))
            {
                TriggerCorePump();
            }

            if (currentHp <= 0)
            {
                AdvancePhase();
            }
        }

        private void AdvancePhase()
        {
            switch (phase)
            {
                case BossPhase.Shield:
                    SetPhase(BossPhase.WeakPoint);
                    break;
                case BossPhase.WeakPoint:
                    SetPhase(BossPhase.Core);
                    break;
                case BossPhase.Core:
                    phase = BossPhase.Defeated;
                    OnPhaseChanged?.Invoke(phase);
                    OnCoreFinalHit?.Invoke();
                    break;
            }
        }

        private void SetPhase(BossPhase next)
        {
            phase = next;
            crackStage = 0;

            switch (phase)
            {
                case BossPhase.Shield:
                    maxHp = shieldHp;
                    break;
                case BossPhase.WeakPoint:
                    maxHp = weakPointHp;
                    break;
                case BossPhase.Core:
                    maxHp = coreHp;
                    break;
            }

            currentHp = maxHp;
            OnPhaseChanged?.Invoke(phase);
            OnHpChanged?.Invoke(currentHp, maxHp);
            ResetCracks();
        }

        private void UpdateCrackFeedback()
        {
            if (crackOverlayRoot == null)
            {
                return;
            }

            var hpRatio = (float)currentHp / maxHp;
            var nextStage = hpRatio <= 0.25f ? 3 : hpRatio <= 0.5f ? 2 : hpRatio <= 0.75f ? 1 : 0;

            if (nextStage > crackStage)
            {
                crackStage = nextStage;
                shardParticles?.Play();
                for (var i = 0; i < crackOverlayRoot.transform.childCount; i++)
                {
                    crackOverlayRoot.transform.GetChild(i).gameObject.SetActive(i < crackStage);
                }
            }
        }

        private void ResetCracks()
        {
            if (crackOverlayRoot == null)
            {
                return;
            }

            for (var i = 0; i < crackOverlayRoot.transform.childCount; i++)
            {
                crackOverlayRoot.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void TriggerCorePump()
        {
            if (coreVisual == null)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(PumpRoutine());
        }

        private System.Collections.IEnumerator PumpRoutine()
        {
            float elapsed = 0f;
            const float duration = 0.45f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                var scale = corePumpCurve.Evaluate(t);
                coreVisual.localScale = Vector3.one * scale;
                yield return null;
            }
        }
    }
}
