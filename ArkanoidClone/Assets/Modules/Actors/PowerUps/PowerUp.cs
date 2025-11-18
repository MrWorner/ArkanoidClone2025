using MiniIT.POWERUP;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.POWERUP
{
    public class PowerUp : MonoBehaviour
    {
        /// <summary>
        /// Event triggered when picked up. Passes bonus points amount.
        /// </summary>
        public static event Action<int> OnPowerUpPickedUp;

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private float fallSpeed = 3f;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private int bonusPoints = 100;

        [BoxGroup("VISUALS")]
        [Header("Animation")]
        [SerializeField]
        private List<Sprite> animationFrames = null;

        [BoxGroup("VISUALS")]
        [SerializeField]
        private float animationSpeed = 0.1f;

        [BoxGroup("VISUALS")]
        [SerializeField, Required]
        private SpriteRenderer spriteRenderer = null;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private int currentFrame = 0;
        private float timer = 0f;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Resets the visual state and timers for pooling.
        /// </summary>
        public void ResetState()
        {
            currentFrame = 0;
            timer = 0;

            if (animationFrames != null && animationFrames.Count > 0)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = animationFrames[0];
                }
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Update()
        {
            // 1. Fall movement
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            // 2. Animation logic
            if (animationFrames != null && animationFrames.Count > 0)
            {
                timer += Time.deltaTime;

                if (timer >= animationSpeed)
                {
                    timer = 0;
                    currentFrame = (currentFrame + 1) % animationFrames.Count;

                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = animationFrames[currentFrame];
                    }
                }
            }

            // 3. Check if out of bounds (bottom) -> Return to pool
            if (transform.position.y < -10f)
            {
                if (PowerUpPool.Instance != null)
                {
                    PowerUpPool.Instance.ReturnPowerUp(this);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Paddle"))
            {
                ApplyBonus();

                // Return to pool
                if (PowerUpPool.Instance != null)
                {
                    PowerUpPool.Instance.ReturnPowerUp(this);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void ApplyBonus()
        {
            if (OnPowerUpPickedUp != null)
            {
                OnPowerUpPickedUp.Invoke(bonusPoints);
            }
        }
    }
}