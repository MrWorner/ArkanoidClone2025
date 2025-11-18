using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    [Header("Collections")]
    [Tooltip("Музыка для геймплея (будет выбираться случайно)")]
    public List<AudioClip> gameplayMusic;

    [Tooltip("Музыка для главного меню")]
    public List<AudioClip> menuMusic;

    [Header("Single Clips")]
    public AudioClip victoryMusic;
    public AudioClip gameOverMusic;

    [Header("Settings")]
    [Range(0f, 1f)] public float maxVolume = 0.5f; // Чуть тише по дефолту
    public float fadeDuration = 1.0f;

    private Coroutine currentFade;

    // Плейлисты для перемешивания (чтобы треки не повторялись подряд)
    private List<AudioClip> _availableGameplayTracks = new List<AudioClip>();
    private List<AudioClip> _availableMenuTracks = new List<AudioClip>();

    void Awake()
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

    void Start()
    {
        // Инициализируем плейлисты
        RefillPlaylist(gameplayMusic, _availableGameplayTracks);
        RefillPlaylist(menuMusic, _availableMenuTracks);
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ---

    public void PlayMenuMusic()
    {
        PlayRandomTrackFromList(menuMusic, _availableMenuTracks, true);
    }

    public void PlayGameplayMusic()
    {
        // Если уже играет трек из геймплея - не прерываем его
        if (musicSource.isPlaying && gameplayMusic.Contains(musicSource.clip)) return;

        PlayRandomTrackFromList(gameplayMusic, _availableGameplayTracks, true);
    }

    public void PlayVictoryMusic()
    {
        PlaySingleClip(victoryMusic, false); // false = не зацикливать победную мелодию
    }

    public void PlayGameOverMusic()
    {
        PlaySingleClip(gameOverMusic, false);
    }

    public void StopMusic()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeOutAndStop());
    }

    // --- ВНУТРЕННЯЯ ЛОГИКА ---

    private void PlayRandomTrackFromList(List<AudioClip> originalList, List<AudioClip> playlist, bool loop)
    {
        if (originalList.Count == 0) return;

        // Если плейлист пуст - наполняем заново
        if (playlist.Count == 0)
        {
            RefillPlaylist(originalList, playlist);
        }

        // Берем последний трек и удаляем его из доступных (механика "Мешка")
        int lastIndex = playlist.Count - 1;
        AudioClip clip = playlist[lastIndex];
        playlist.RemoveAt(lastIndex);

        TransitionToClip(clip, loop);
    }

    private void PlaySingleClip(AudioClip clip, bool loop)
    {
        if (clip == null) return;
        TransitionToClip(clip, loop);
    }

    private void TransitionToClip(AudioClip clip, bool loop)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeMusic(clip, loop));
    }

    // Перемешивание списка (Shuffle)
    private void RefillPlaylist(List<AudioClip> source, List<AudioClip> destination)
    {
        destination.Clear();
        destination.AddRange(source);

        // Алгоритм Фишера-Йетса для перемешивания
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

        // 1. Затухание текущего
        if (musicSource.isPlaying)
        {
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }
        }

        // 2. Смена трека
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // 3. Нарастание нового
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
        musicSource.volume = startVolume; // Возвращаем громкость для следующего раза
    }
}