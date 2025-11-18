using UnityEngine;
using TMPro;
using NaughtyAttributes;

namespace MiniIT.UI
{
    public class FPSCounter : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("REQUIRED")]
        [SerializeField, Required]
        private TextMeshProUGUI fpsText = null;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private float updateInterval = 0.5f;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private float accumulatedFrames = 0;
        private float timeLeft = 0f;
        private int lastFPS = 0;

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (fpsText == null)
            {
                enabled = false;
                return;
            }

            timeLeft = updateInterval;
        }

        private void Update()
        {
            timeLeft -= Time.unscaledDeltaTime;
            accumulatedFrames++;

            if (timeLeft <= 0.0f)
            {
                lastFPS = (int)(accumulatedFrames / updateInterval);
                fpsText.text = "FPS: " + lastFPS;

                timeLeft = updateInterval;
                accumulatedFrames = 0;
            }
        }
    }
}