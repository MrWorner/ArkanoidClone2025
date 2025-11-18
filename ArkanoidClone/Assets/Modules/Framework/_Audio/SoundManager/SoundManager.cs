using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

namespace MiniIT.AUDIO
{
    /// <summary>
    /// Defines types of sound effects available in the game.
    /// </summary>
    public enum SoundType
    {
        None,

        ButtonClick,

        PaddleHit,
        WallHit,
        BallLost,

        BrickHit,
        BrickDestroyed,
        IndestructibleHit,

        PowerUpPickup,
        LifeLost,
        LevelComplete,
        GameOver,

        ButtonClickStart,
    }

    [System.Serializable]
    public class SoundEffect
    {
        public SoundType type;

        [Tooltip("One of these clips will be selected randomly.")]
        public AudioClip[] clips;

        [Range(0f, 2f)]
        public float volumeMultiplier = 1f;
    }

    public class SoundManager : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static SoundManager Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Tooltip("Master volume for SFX.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float sfxVolume = 1f;

        [BoxGroup("SETTINGS")]
        [Tooltip("Initial size of the AudioSource pool.")]
        [SerializeField]
        private int oneShotPoolSize = 15;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private List<SoundEffect> soundEffects = new List<SoundEffect>();

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private List<AudioSource> oneShotSources = new List<AudioSource>();

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Plays a sound effect by type.
        /// </summary>
        public void PlayOneShot(SoundType type)
        {
            if (type == SoundType.None)
            {
                return;
            }

            SoundEffect effect = soundEffects.FirstOrDefault(e => e.type == type);

            if (effect == null)
            {
                Debug.LogWarning($"[SoundManager] Effect not found in settings: {type}");
                return;
            }

            if (effect.clips == null || effect.clips.Length == 0)
            {
                Debug.LogWarning($"[SoundManager] No clips assigned for: {type}");
                return;
            }

            // Select random clip for variation
            AudioClip clip = effect.clips[Random.Range(0, effect.clips.Length)];

            if (clip == null)
            {
                Debug.LogError($"[SoundManager] Clip is null for: {type}");
                return;
            }

            // Get free source
            AudioSource source = GetAvailableOneShotSource();

            if (source != null)
            {
                source.pitch = 1f;
                float finalVolume = sfxVolume * effect.volumeMultiplier;
                source.PlayOneShot(clip, finalVolume);
            }
            else
            {
                Debug.LogError("[SoundManager] Failed to get AudioSource!");
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitAudioSources();
        }

        private void InitAudioSources()
        {
            oneShotSources = new List<AudioSource>();

            for (int i = 0; i < oneShotPoolSize; i++)
            {
                CreateSource();
            }
        }

        private AudioSource CreateSource()
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            oneShotSources.Add(source);
            return source;
        }

        private AudioSource GetAvailableOneShotSource()
        {
            foreach (AudioSource source in oneShotSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            Debug.Log("[SoundManager] Pool exhausted. Expanding...");
            // Expand pool if all are busy
            return CreateSource();
        }
    }
}