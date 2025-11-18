using UnityEngine;

namespace MiniIT.CORE
{
    public class AppInitializer : MonoBehaviour
    {
        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Awake()
        {
            // 1. Disable V-Sync.
            // This is required for Application.targetFrameRate to work correctly.
            QualitySettings.vSyncCount = 0;

            // 2. Set target FPS.
            Application.targetFrameRate = 90;
        }
    }
}