using System;
using UnityEngine;
using UnityEngine.InputSystem;
using CoreBreaker.Systems;
using CoreBreaker.Gameplay;

namespace CoreBreaker.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private WeakRing weakRing;
        [SerializeField] private SafeBoss safeBoss;
        [SerializeField] private Camera worldCamera;

        [Header("Hold Settings")]
        [SerializeField] private float chargedThreshold = 0.35f;

        public event Action<float, bool> OnInputPerformed;

        private InputAction pressAction;
        private float pressStartTime;
        private bool isPressing;

        private void Awake()
        {
            pressAction = new InputAction("Press", InputActionType.Button, "<Pointer>/press");
        }

        private void OnEnable()
        {
            pressAction.Enable();
            pressAction.started += OnPressStarted;
            pressAction.canceled += OnPressCanceled;
        }

        private void OnDisable()
        {
            pressAction.started -= OnPressStarted;
            pressAction.canceled -= OnPressCanceled;
            pressAction.Disable();
        }

        private void OnPressStarted(InputAction.CallbackContext context)
        {
            if (!combatSystem || !combatSystem.CanInput())
            {
                return;
            }

            pressStartTime = Time.time;
            isPressing = true;
        }

        private void OnPressCanceled(InputAction.CallbackContext context)
        {
            if (!combatSystem || !combatSystem.CanInput() || !isPressing)
            {
                return;
            }

            var holdTime = Time.time - pressStartTime;
            var isCharged = holdTime >= chargedThreshold;
            var hitAngle = GetPointerAngle();

            OnInputPerformed?.Invoke(hitAngle, isCharged);
            ResolveHit(hitAngle, isCharged);
            isPressing = false;
        }

        private float GetPointerAngle()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            var screenPos = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
            if (worldCamera == null)
            {
                return 0f;
            }

            var worldPos = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            var dir = worldPos - weakRing.transform.position;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        private void ResolveHit(float angle, bool isCharged)
        {
            if (weakRing == null || safeBoss == null)
            {
                return;
            }

            var grade = safeBoss.Phase == BossPhase.WeakPoint ? weakRing.EvaluateHit(angle) : HitGrade.Good;
            var comboBonus = grade == HitGrade.Perfect ? 2 : grade == HitGrade.Great || grade == HitGrade.Good ? 1 : 0;
            var rawDamage = combatSystem.GetBaseDamage(isCharged);
            var comboMultiplier = combatSystem.GetComboMultiplier(combatSystem.Combo);
            var finalDamage = combatSystem.ApplyHit(grade, rawDamage, comboBonus);
            var totalDamage = Mathf.RoundToInt(finalDamage * comboMultiplier);

            safeBoss.ApplyDamage(totalDamage);
        }
    }
}
