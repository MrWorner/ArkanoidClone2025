using DG.Tweening;
using MiniIT.BRICK;
using MiniIT.CORE;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace MiniIT.BRICK
{
    public class Brick : MonoBehaviour, IDamageable
    {
        public static event Action<Vector3> OnAnyBrickDestroyed;

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("CONFIG")]
        [SerializeField]
        private BrickTypeSO brickType = null;

        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public BrickTypeSO BrickType
        {
            get
            {
                return brickType;
            }
        }

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private BrickPool pool = null;
        private SpriteRenderer spriteRenderer = null;
        private Collider2D col = null;
        private int currentHealth = 0;
        private bool isDestroyed = false;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Initializes the brick with its owning pool.
        /// </summary>
        public void Init(BrickPool ownerPool)
        {
            pool = ownerPool;
        }

        /// <summary>
        /// Sets up the brick with a specific type and resets visual state.
        /// </summary>
        public void Setup(BrickTypeSO type)
        {
            brickType = type;

            // --- RESET POOL STATE ---
            // Reset transform and rotation as previous animations might have altered them
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = brickType != null ? brickType.color : Color.white;

                if (brickType != null)
                {
                    spriteRenderer.sprite = brickType.sprite;
                }

                // Reset alpha channel if animation changed it
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            // Re-enable collider
            if (col == null)
            {
                col = GetComponent<Collider2D>();
            }

            if (col != null)
            {
                col.enabled = true;
            }

            currentHealth = brickType != null ? brickType.health : 1;
            isDestroyed = false;
        }

        /// <summary>
        /// Applies damage to the brick and handles destruction logic/animation.
        /// </summary>
        public void TakeDamage(int damageAmount)
        {
            if (isDestroyed || brickType == null)
            {
                return;
            }

            if (brickType.isIndestructible)
            {
                SoundManager.Instance.PlayOneShot(SoundType.IndestructibleHit);
                // Bonus: Shake animation for indestructible blocks
                transform.DOShakePosition(0.2f, 0.1f, 10, 90, false, true);
                return;
            }

            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                PerformDestruction();
            }
            else
            {
                SoundManager.Instance.PlayOneShot(SoundType.BrickHit);
                // Shake if hurt but not dead
                transform.DOShakeScale(0.15f, 0.2f);
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        private void PerformDestruction()
        {
            SoundManager.Instance.PlayOneShot(SoundType.BrickDestroyed);
            isDestroyed = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(brickType.points);
            }

            if (OnAnyBrickDestroyed != null)
            {
                OnAnyBrickDestroyed.Invoke(transform.position);
            }

            // --- ANIMATION MAGIC (DOTween) ---

            // 1. Disable physics immediately
            if (col != null)
            {
                col.enabled = false;
            }

            // 2. Create animation sequence
            Sequence seq = DOTween.Sequence();

            // Scale down to 0 over 0.2 seconds (implosion effect)
            seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));

            // Rotate slightly simultaneously
            seq.Join(transform.DORotate(new Vector3(0, 0, 45), 0.2f));

            // 3. Return to pool upon completion
            seq.OnComplete(() =>
            {
                if (pool != null)
                {
                    pool.ReturnBrick(this);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            });
        }
    }
}