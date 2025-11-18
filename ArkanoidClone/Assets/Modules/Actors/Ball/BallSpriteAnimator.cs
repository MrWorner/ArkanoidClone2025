using UnityEngine;
using NaughtyAttributes;

namespace MiniIT.BALL
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallSpriteAnimator : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("ROTATION SETTINGS")]
        [Header("Rotation Settings")]
        [Tooltip("Speed multiplier relative to velocity.")]
        [SerializeField]
        private float rotationSpeedMultiplier = 100f;

        [BoxGroup("ROTATION SETTINGS")]
        [Tooltip("Z-axis direction: 1 for CW, -1 for CCW.")]
        [SerializeField]
        private float rotationDirectionZ = -1f;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private Rigidbody2D rb = null;
        private Transform ballTransform = null;

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ballTransform = transform;
        }

        private void FixedUpdate()
        {
            float horizontalVelocity = rb.velocity.x;

            if (Mathf.Abs(horizontalVelocity) > 0.01f)
            {
                float rotationAngle = horizontalVelocity * rotationSpeedMultiplier * Time.fixedDeltaTime * rotationDirectionZ;
                ballTransform.Rotate(0, 0, rotationAngle);
            }
        }
    }
}