using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace MiniIT.AUDIO
{
    public class MusicManager : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static MusicManager Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("AUDIO SOURCES")]
        [SerializeField, Required]
        private AudioSource musicSource = null;

        [BoxGroup("COLLECTIONS")]
        [Tooltip("Gameplay music tracks (shuffled).")]
        [SerializeField]
        private List<AudioClip> gameplayMusic = null;

        [BoxGroup("COLLECTIONS")]
        [Tooltip("Main menu music tracks.")]
        [SerializeField]
        private List<AudioClip> menuMusic = null;

        [BoxGroup("SINGLE CLIPS")]
        [SerializeField]
        private AudioClip victoryMusic = null;

        [BoxGroup("SINGLE CLIPS")]
        [SerializeField]
        private AudioClip gameOverMusic = null;

        [BoxGroup("SETTINGS")]
        [Range(0f, 1f)]
        [SerializeField]
        private float maxVolume = 0.5f;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private float fadeDuration = 1.0f;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private Coroutine currentFade = null;

        // Playlists for shuffle logic (to avoid repeats)
        private List<AudioClip> availableGameplayTracks = new List<AudioClip>();
        private List<AudioClip> availableMenuTracks = new List<AudioClip>();

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void PlayMenuMusic()
        {
            PlayRandomTrackFromList(menuMusic, availableMenuTracks, true);
        }

        public void PlayGameplayMusic()
        {
            // If a gameplay track is already playing, do not interrupt
            if (musicSource.isPlaying && gameplayMusic.Contains(musicSource.clip))
            {
                return;
            }

            PlayRandomTrackFromList(gameplayMusic, availableGameplayTracks, true);
        }

        public void PlayVictoryMusic()
        {
            PlaySingleClip(victoryMusic, false);
        }

        public void PlayGameOverMusic()
        {
            PlaySingleClip(gameOverMusic, false);
        }

        public void StopMusic()
        {
            if (currentFade != null)
            {
                StopCoroutine(currentFade);
            }

            currentFade = StartCoroutine(FadeOutAndStop());
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            RefillPlaylist(gameplayMusic, availableGameplayTracks);
            RefillPlaylist(menuMusic, availableMenuTracks);
        }

        private void PlayRandomTrackFromList(List<AudioClip> originalList, List<AudioClip> playlist, bool loop)
        {
            if (originalList.Count == 0)
            {
                return;
            }

            // If playlist is empty, refill it
            if (playlist.Count == 0)
            {
                RefillPlaylist(originalList, playlist);
            }

            // Grab-bag mechanics: take last, remove, play
            int lastIndex = playlist.Count - 1;
            AudioClip clip = playlist[lastIndex];
            playlist.RemoveAt(lastIndex);

            TransitionToClip(clip, loop);
        }

        private void PlaySingleClip(AudioClip clip, bool loop)
        {
            if (clip == null)
            {
                return;
            }

            TransitionToClip(clip, loop);
        }

        private void TransitionToClip(AudioClip clip, bool loop)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            if (currentFade != null)
            {
                StopCoroutine(currentFade);
            }

            currentFade = StartCoroutine(FadeMusic(clip, loop));
        }

        /// <summary>
        /// Shuffles the playlist using Fisher-Yates algorithm.
        /// </summary>
        private void RefillPlaylist(List<AudioClip> source, List<AudioClip> destination)
        {
            destination.Clear();
            destination.AddRange(source);

            System.Random rng = new System.Random();
            int n = destination.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                AudioClip value = destination[k];
                destination[k] = destination[n];
                destination[n] = value;
            }
        }

        private IEnumerator FadeMusic(AudioClip newClip, bool loop)
        {
            float startVolume = musicSource.volume;

            // 1. Fade out current
            if (musicSource.isPlaying)
            {
                for (float t = 0; t < fadeDuration; t += Time.deltaTime)
                {
                    musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                    yield return null;
                }
            }

            // 2. Switch track
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // 3. Fade in new
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, maxVolume, t / fadeDuration);
                yield return null;
            }

            musicSource.volume = maxVolume;
        }

        private IEnumerator FadeOutAndStop()
        {
            float startVolume = musicSource.volume;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }
    }
}