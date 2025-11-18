using MiniIT.AUDIO;
using MiniIT.BALL;
using MiniIT.CORE;
using UnityEngine;

namespace MiniIT.LEVELS
{
    /// <summary>
    /// Anchors the object to the bottom of the camera view and acts as a trigger for the "Death Zone".
    /// </summary>
    public class AnchorToBottom : MonoBehaviour
    {
        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private Camera mainCamera = null;

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Start()
        {
            ApplyPosition();
        }

        private void ApplyPosition()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }

            // 1. Position at the bottom edge of the viewport
            Vector3 bottomEdgePos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, mainCamera.nearClipPlane));

            transform.position = new Vector3(bottomEdgePos.x, bottomEdgePos.y, transform.position.z);

            // 2. Calculate screen width in world units
            float screenWidth = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x -
                                mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

            // 3. Stretch SpriteRenderer or Collider
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.size = new Vector2(screenWidth, sr.size.y);
            }
            else
            {
                BoxCollider2D bc = GetComponent<BoxCollider2D>();

                if (bc != null)
                {
                    bc.size = new Vector2(screenWidth, bc.size.y);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ball"))
            {
                // Note: GetComponent in Trigger is acceptable for infrequent events like death
                BallController ball = other.GetComponent<BallController>();

                if (ball != null)
                {
                    SoundManager.Instance.PlayOneShot(SoundType.BallLost);

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.HandleBallLost(ball);
                    }
                }
            }
        }
    }
}