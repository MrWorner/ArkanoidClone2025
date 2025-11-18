using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace MiniIT.UI
{
    public class MainMenuView : BaseView
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("MAIN MENU UI")]
        [SerializeField, Required]
        private Button newGameButton = null;

        [BoxGroup("MAIN MENU UI")]
        [SerializeField, Required]
        private Button quitButton = null;

        [BoxGroup("CONFIRMATION POPUP")]
        [SerializeField, Required]
        private GameObject confirmationPanel = null;

        [BoxGroup("CONFIRMATION POPUP")]
        [SerializeField, Required]
        private Button confirmYesButton = null;

        [BoxGroup("CONFIRMATION POPUP")]
        [SerializeField, Required]
        private Button confirmNoButton = null;

        // ========================================================================
        // --- PROPERTIES (Exposing UI elements for Presenter) ---
        // ========================================================================

        public Button NewGameButton => newGameButton;
        public Button QuitButton => quitButton;
        public Button ConfirmYesButton => confirmYesButton;
        public Button ConfirmNoButton => confirmNoButton;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void SetConfirmationActive(bool isActive)
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(isActive);
            }
        }

        // ========================================================================
        // --- PROTECTED METHODS ---
        // ========================================================================

        protected override void Awake()
        {
            base.Awake();
            // Ensure popup is hidden on start
            SetConfirmationActive(false);
        }
    }
}