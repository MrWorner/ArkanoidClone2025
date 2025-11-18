using UnityEngine;
using System.Collections;
using NaughtyAttributes;
using MiniIT.AUDIO;
using MiniIT.CORE;

namespace MiniIT.BALL
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private float initialSpeed = 7f;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private float minTotalSpeed = 7f;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private float launchDelay = 3f;

        [BoxGroup("PHYSICS FIXES")]
        [Header("Anti-Stuck Settings")]
        [Tooltip("Prevents horizontal locking")]
        [SerializeField]
        private float minVerticalVelocity = 0.5f;

        [BoxGroup("PHYSICS FIXES")]
        [Tooltip("Prevents vertical locking")]
        [SerializeField]
        private float minHorizontalVelocity = 0.5f;

        [BoxGroup("BONUSES")]
        [SerializeField]
        private float speedBoostMultiplier = 1.5f;

        [BoxGroup("BONUSES")]
        [SerializeField]
        private float homingStrength = 0.1f;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private Rigidbody2D rb = null;
        private bool isLaunched = false;
        private float currentSpeed = 0f;
        private Transform homingTarget = null;
        private Coroutine launchCoroutine = null;
        private float launchTime = 0f;
        private Transform anchorPoint = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Resets the ball state and attaches it to the paddle.
        /// </summary>
        public void ResetToPaddle()
        {
            isLaunched = false;
            currentSpeed = initialSpeed;
            homingTarget = null;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;

            if (anchorPoint == null)
            {
                GameObject paddle = GameObject.FindGameObjectWithTag("Paddle");

                if (paddle != null)
                {
                    Transform spawnPoint = paddle.transform.Find("BallSpawnPoint");

                    if (spawnPoint != null)
                    {
                        anchorPoint = spawnPoint;
                    }
                    else
                    {
                        anchorPoint = paddle.transform;
                    }
                }
            }

            if (anchorPoint != null)
            {
                transform.position = anchorPoint.position;
            }

            if (launchCoroutine != null)
            {
                StopCoroutine(launchCoroutine);
            }

            launchCoroutine = StartCoroutine(LaunchDelayCoroutine());
        }

        /// <summary>
        /// Spawns the ball as an active clone (e.g. for multi-ball powerup).
        /// </summary>
        public void SpawnAsClone(Vector2 position, Vector2 velocity)
        {
            anchorPoint = null;
            transform.position = position;

            isLaunched = true;
            currentSpeed = velocity.magnitude;

            if (currentSpeed < minTotalSpeed)
            {
                currentSpeed = minTotalSpeed;
            }

            homingTarget = null;
            launchTime = Time.time;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = velocity.normalized * currentSpeed;
        }

        /// <summary>
        /// Applies a speed boost multiplier.
        /// </summary>
        public void ActivateSpeedBoost()
        {
            currentSpeed = initialSpeed * speedBoostMultiplier;

            if (isLaunched)
            {
                rb.velocity = rb.velocity.normalized * currentSpeed;
            }
        }

        /// <summary>
        /// Sets a target for the homing projectile logic.
        /// </summary>
        public void SetHomingTarget(Transform target)
        {
            homingTarget = target;
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!isLaunched && anchorPoint != null)
            {
                transform.position = anchorPoint.position;
            }
        }

        private void FixedUpdate()
        {
            if (!isLaunched)
            {
                return;
            }

            // Homing logic
            if (homingTarget != null)
            {
                Vector2 currentVelocity = rb.velocity;
                Vector2 targetDirection = (homingTarget.position - transform.position).normalized;
                Vector2 newDirection = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.fixedDeltaTime);
                rb.velocity = newDirection * currentSpeed;
            }

            // Fix stuck trajectories
            Vector2 velocity = rb.velocity;
            bool velocityChanged = false;

            // Case 1: Ball moving purely horizontally
            if (Mathf.Abs(velocity.y) < minVerticalVelocity)
            {
                float sign = (velocity.y == 0) ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(velocity.y);
                velocity.y = sign * minVerticalVelocity;
                velocityChanged = true;
            }

            // Case 2: Ball moving purely vertically
            if (Mathf.Abs(velocity.x) < minHorizontalVelocity)
            {
                float sign = (velocity.x == 0) ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(velocity.x);
                velocity.x = sign * minHorizontalVelocity;
                velocityChanged = true;
            }

            if (velocityChanged)
            {
                rb.velocity = velocity.normalized * currentSpeed;
            }

            // Enforce minimum speed
            if (rb.velocity.magnitude < minTotalSpeed)
            {
                rb.velocity = rb.velocity.normalized * minTotalSpeed;
            }
        }

        private IEnumerator LaunchDelayCoroutine()
        {
            yield return new WaitForSeconds(launchDelay);
            LaunchMainBall();
        }

        private void LaunchMainBall()
        {
            if (isLaunched)
            {
                return;
            }

            isLaunched = true;
            anchorPoint = null;

            rb.bodyType = RigidbodyType2D.Dynamic;
            launchTime = Time.time;

            float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
            Vector2 direction = new Vector2(startX, 1f).normalized;
            rb.velocity = direction * currentSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Paddle collision
            if (isLaunched && collision.gameObject.CompareTag("Paddle"))
            {
                SoundManager.Instance.PlayOneShot(SoundType.PaddleHit);

                // Prevent double hits on launch
                if (Time.time - launchTime < 0.2f)
                {
                    return;
                }

                CalculateRebound(collision);
                return;
            }
            else
            {
                // Wall/Brick collision: add microscopic deviation to break infinite loops
                Vector2 randomTweak = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                rb.velocity += randomTweak;

                // Normalize back to intended speed
                rb.velocity = rb.velocity.normalized * currentSpeed;
            }

            if (collision.gameObject.CompareTag("Wall"))
            {
                SoundManager.Instance.PlayOneShot(SoundType.WallHit);
            }

            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(1);
                return;
            }
        }

        private void CalculateRebound(Collision2D collision)
        {
            Vector3 paddleCenter = collision.transform.position;
            float paddleWidth = collision.collider.bounds.size.x;
            Vector3 hitPoint = collision.contacts[0].point;

            float xOffset = hitPoint.x - paddleCenter.x;
            float normalizedX = Mathf.Clamp(xOffset / (paddleWidth / 2f), -1f, 1f);

            Vector2 newDirection = new Vector2(normalizedX, 1f).normalized;
            rb.velocity = newDirection * currentSpeed;
        }
    }
}