using NaughtyAttributes;
using UnityEngine;

namespace MiniIT.CAMERA
{
    /// <summary>
    /// Adjusts the Orthographic Camera size and position to fit specific corner transforms.
    /// </summary>
    public class FitCameraToCorners : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("REFERENCES")]
        [Tooltip("Object representing the Top-Left corner of the play area.")]
        [SerializeField, Required]
        private Transform topLeftCorner = null;

        [BoxGroup("REFERENCES")]
        [Tooltip("Object representing the Bottom-Right (or just Right edge) of the play area.")]
        [SerializeField, Required]
        private Transform bottomRightCorner = null;

        [BoxGroup("REFERENCES")]
        [Tooltip("Target Camera. If null, Camera.main is used.")]
        [SerializeField]
        private Camera targetCamera = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        [Button("Set Camera Now")]
        public void SetCameraInstant()
        {
            if (targetCamera == null || topLeftCorner == null || bottomRightCorner == null)
            {
                return;
            }

            Vector3 tlPos = topLeftCorner.position;
            Vector3 drPos = bottomRightCorner.position;
            float aspectRatio = targetCamera.aspect;

            // 1. Calculate required World dimensions
            float requiredWorldWidth = drPos.x - tlPos.x;
            float requiredWorldHeight = tlPos.y - drPos.y;

            // 2. Calculate Ortho size needed for Width
            float orthoSizeForWidth = (requiredWorldWidth / aspectRatio) / 2f;

            // 3. Calculate Ortho size needed for Height
            float orthoSizeForHeight = requiredWorldHeight / 2f;

            // 4. "Fit-or-Expand": Choose the larger size to ensure all corners are visible
            float finalOrthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);

            // 5. Calculate Center Position
            float newCamPosX = (tlPos.x + drPos.x) / 2f;
            float newCamPosY = (tlPos.y + drPos.y) / 2f;

            // 6. Apply
            targetCamera.transform.position = new Vector3(newCamPosX, newCamPosY, targetCamera.transform.position.z);
            targetCamera.orthographicSize = finalOrthoSize;
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                Debug.LogError("FitCameraToCorners: Camera not found!");
                return;
            }

            if (!targetCamera.orthographic)
            {
                Debug.LogError("FitCameraToCorners: Camera must be Orthographic!");
                return;
            }

            if (topLeftCorner == null || bottomRightCorner == null)
            {
                Debug.LogError("FitCameraToCorners: Corners not assigned!");
                return;
            }

            SetCameraInstant();
        }
    }
}