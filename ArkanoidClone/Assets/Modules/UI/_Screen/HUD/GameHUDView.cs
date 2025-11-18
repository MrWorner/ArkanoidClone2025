using UnityEngine;
using TMPro;
using NaughtyAttributes;

namespace MiniIT.UI
{
    /// <summary>
    /// View responsible for displaying persistent game statistics (Score, Lives, Level).
    /// </summary>
    public class GameHUDView : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("HUD ELEMENTS")]
        [SerializeField, Required]
        private TextMeshProUGUI scoreText = null;

        [BoxGroup("HUD ELEMENTS")]
        [SerializeField, Required]
        private TextMeshProUGUI livesText = null;

        [BoxGroup("HUD ELEMENTS")]
        [SerializeField, Required]
        private TextMeshProUGUI levelText = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Updates the score display.
        /// </summary>
        /// <param name="score">Current score value.</param>
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                // Format with thousands separator (N0)
                scoreText.text = $"Score: {score:N0}";
            }
        }

        /// <summary>
        /// Updates the lives display.
        /// </summary>
        /// <param name="lives">Current lives count.</param>
        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }

        /// <summary>
        /// Updates the level display.
        /// </summary>
        /// <param name="level">Current level index.</param>
        public void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level: {level}";
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Start()
        {
            // Validate references
            if (scoreText == null || livesText == null || levelText == null)
            {
                Debug.LogError("GameHUDView: Missing TextMeshProUGUI references in Inspector!", this);
            }
        }
    }
}