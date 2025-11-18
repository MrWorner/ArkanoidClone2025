using NaughtyAttributes;
using UnityEngine;

namespace MiniIT.LEVELS
{
    /// <summary>
    /// Positions side walls at the screen edges and stretches them.
    /// Requirements: SpriteRenderer.DrawMode = Sliced/Tiled, BoxCollider2D.AutoTiling = true.
    /// </summary>
    public class BoundaryManager : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("WALLS")]
        [Tooltip("Reference to the Left Wall SpriteRenderer.")]
        [SerializeField, Required]
        private SpriteRenderer leftWall = null;

        [BoxGroup("WALLS")]
        [Tooltip("Reference to the Right Wall SpriteRenderer.")]
        [SerializeField, Required]
        private SpriteRenderer rightWall = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        [Button("Execute Positioning")]
        public void Execute()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null || leftWall == null || rightWall == null)
            {
                return;
            }

            // --- Get screen dimensions in World Units ---
            float screenHeight = mainCamera.orthographicSize * 2;
            float screenWidth = screenHeight * mainCamera.aspect;
            float zPos = 10f; // Depth

            // --- 1. Position walls at edges ---

            // (0, 0.5) = Left Edge Center
            leftWall.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, zPos));

            // (1, 0.5) = Right Edge Center
            rightWall.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, zPos));

            // --- 2. Stretch while preserving thickness ---

            // Left Wall: Keep X (thickness), change Y (height)
            leftWall.size = new Vector2(leftWall.size.x, screenHeight);

            // Right Wall: Keep X (thickness), change Y (height)
            rightWall.size = new Vector2(rightWall.size.x, screenHeight);
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Start()
        {
            Execute();
        }
    }
}