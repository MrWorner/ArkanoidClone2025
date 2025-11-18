using UnityEngine;
using TMPro;
using NaughtyAttributes;

namespace MiniIT.UI
{
    /// <summary>
    /// Manages activation/deactivation of major game state screens (Game Over, Victory, Level Transition).
    /// </summary>
    public class GameScreenManager : MonoBehaviour
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("VIEWS")]
        [SerializeField, Required]
        private GameObject gameOverScreen = null;

        [BoxGroup("VIEWS")]
        [SerializeField, Required]
        private GameObject victoryScreen = null;

        [BoxGroup("VIEWS")]
        [SerializeField, Required]
        private GameObject levelTransitionScreen = null;

        [BoxGroup("VIEWS")]
        [SerializeField, Required]
        private TextMeshProUGUI levelTransitionText = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void ShowGameOver(bool show)
        {
            if (gameOverScreen != null)
            {
                gameOverScreen.SetActive(show);
            }
        }

        public void ShowVictory(bool show)
        {
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(show);
            }
        }

        public void ShowLevelTransition(string text)
        {
            if (levelTransitionText != null)
            {
                levelTransitionText.text = text;
            }

            if (levelTransitionScreen != null)
            {
                levelTransitionScreen.SetActive(true);
            }
        }

        public void HideLevelTransition()
        {
            if (levelTransitionScreen != null)
            {
                levelTransitionScreen.SetActive(false);
            }
        }
    }
}