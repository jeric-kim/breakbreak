using UnityEngine;
using CoreBreaker.Systems;

namespace CoreBreaker.Gameplay
{
    public class WeakRing : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private Transform perfectZone;

        [Header("Angles")]
        [SerializeField] private float perfectAngle = 12f;
        [SerializeField] private float greatAngle = 24f;
        [SerializeField] private float goodAngle = 40f;

        public float CurrentAngle => transform.eulerAngles.z;

        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        public HitGrade EvaluateHit(float inputAngle)
        {
            var zoneAngle = perfectZone != null ? perfectZone.eulerAngles.z : transform.eulerAngles.z;
            var delta = Mathf.Abs(Mathf.DeltaAngle(inputAngle, zoneAngle));

            if (delta <= perfectAngle)
            {
                return HitGrade.Perfect;
            }

            if (delta <= greatAngle)
            {
                return HitGrade.Great;
            }

            if (delta <= goodAngle)
            {
                return HitGrade.Good;
            }

            return HitGrade.Miss;
        }
    }
}
