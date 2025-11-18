using UnityEngine;
using NaughtyAttributes;

namespace MiniIT.LEVELS
{
    /// <summary>
    /// Anchors this object (e.g., a wall) to the bottom edge of a specific UI element (RectTransform).
    /// </summary>
    public class AnchorToUIEdge : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Tooltip("The UI element (RectTransform) to anchor to.")]
        [SerializeField, Required]
        private RectTransform uiElement = null;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private Camera mainCamera = null;

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Start()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("AnchorToUIEdge: Main Camera not found!");
                return;
            }

            if (uiElement == null)
            {
                Debug.LogError("AnchorToUIEdge: UI Element is not assigned!");
                return;
            }

            ApplyPosition();
        }

        private void ApplyPosition()
        {
            // 1. Get UI corners in Screen Space (Pixels)
            Vector3[] corners = new Vector3[4];
            uiElement.GetWorldCorners(corners);
            // corners[0] = bottom-left
            // corners[1] = top-left

            // 2. Get Y coordinate of the UI's bottom edge
            float uiEdgeY_Screen = corners[0].y;

            // 3. Get Screen X center
            float screenCenterX = Screen.width / 2f;

            // 4. Calculate Z distance
            float zDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);

            // 5. Convert "Point under UI" from Screen Space to World Space
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenCenterX, uiEdgeY_Screen, zDistance));

            // 6. Set Position
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);

            // 7. Stretch horizontally (similar to AnchorToBottom)
            float screenWidth = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x -
                                mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

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
    }
}