using UnityEngine;
using NaughtyAttributes;

namespace MiniIT.PADDLE
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PaddleController : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Tooltip("Speed at which the paddle follows the input.")]
        [SerializeField]
        private float moveSpeed = 15f;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private Camera mainCamera = null;
        private float yPosition = 0f;
        private float minX = 0f;
        private float maxX = 0f;
        private float paddleHalfWidth = 0f;
        private Transform ballSpawnPoint = null;

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            // Find or create the spawn point anchor for the ball
            ballSpawnPoint = transform.Find("BallSpawnPoint");

            if (ballSpawnPoint == null)
            {
                GameObject point = new GameObject("BallSpawnPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = new Vector3(0, 0.5f, 0);
                ballSpawnPoint = point.transform;
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            paddleHalfWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
            yPosition = transform.position.y;

            // Calculate screen boundaries
            if (mainCamera != null)
            {
                float camHeight = 2f * mainCamera.orthographicSize;
                float camWidth = camHeight * mainCamera.aspect;
                float camHalfWidth = camWidth / 2f;

                // 0,0,0 is usually center, but we calculate relative to camera position
                float camCenterX = mainCamera.transform.position.x;

                minX = camCenterX - camHalfWidth + paddleHalfWidth;
                maxX = camCenterX + camHalfWidth - paddleHalfWidth;
            }
        }

        private void Update()
        {
            // Input.GetMouseButton(0) works for both mouse and first touch
            if (Input.GetMouseButton(0))
            {
                MovePaddle(Input.mousePosition);
            }
            // Fallback for specific touch handling
            else if (Input.touchCount > 0)
            {
                MovePaddle(Input.GetTouch(0).position);
            }
        }

        private void MovePaddle(Vector3 inputScreenPosition)
        {
            if (mainCamera == null)
            {
                return;
            }

            // 1. Convert screen position to world coordinates
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(inputScreenPosition);

            // 2. Clamp X position within screen bounds
            float targetX = Mathf.Clamp(worldPosition.x, minX, maxX);

            // 3. Create target vector
            Vector3 targetPosition = new Vector3(targetX, yPosition, transform.position.z);

            // 4. Move smoothly
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
}