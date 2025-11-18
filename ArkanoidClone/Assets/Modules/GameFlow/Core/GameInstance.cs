using UnityEngine;
using NaughtyAttributes;

namespace MiniIT.CORE
{
    public class GameInstance : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static GameInstance Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Tooltip("Change this number to completely alter the generation of all game levels.")]
        [SerializeField]
        private int masterSeed = 777;

        [BoxGroup("DATA")]
        [ReadOnly]
        [SerializeField]
        private int selectedLevelIndex = 1;

        [BoxGroup("DATA")]
        [ReadOnly]
        [SerializeField]
        private int currentLevelSeed = 0;

        // ========================================================================
        // --- PUBLIC PROPERTIES ACCESSORS ---
        // ========================================================================

        public int SelectedLevelIndex
        {
            get
            {
                return selectedLevelIndex;
            }
        }

        public int CurrentLevelSeed
        {
            get
            {
                return currentLevelSeed;
            }
        }

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void SetLevelData(int levelIndex)
        {
            selectedLevelIndex = Mathf.Clamp(levelIndex, 1, 9999);

            unchecked
            {
                currentLevelSeed = (selectedLevelIndex * 1234567) + masterSeed;
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Force calculate Seed for the start level (Level 1),
            // otherwise it will remain 0 until the first button press.
            SetLevelData(selectedLevelIndex);
        }
    }
}