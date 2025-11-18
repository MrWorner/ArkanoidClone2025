using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Типы звуковых эффектов для Arkanoid.
/// </summary>
public enum SoundType
{
    None,

    // UI
    ButtonClick,

    // Геймплей - Мяч и Физика
    PaddleHit,      // Удар о ракетку
    WallHit,        // Удар о стену (опционально)
    BallLost,       // Мяч улетел в дно

    // Геймплей - Кирпичи
    BrickHit,       // Удар по кирпичу (если он не разрушился, а треснул)
    BrickDestroyed, // Кирпич уничтожен
    IndestructibleHit, // Удар по неубиваемому (дзынь!)

    // Геймплей - События
    PowerUpPickup,  // Взятие бонуса
    LifeLost,       // Потеря жизни
    LevelComplete,  // Победа
    GameOver,        // Поражение
    ButtonClickStart,
}

[System.Serializable]
public class SoundEffect
{
    public SoundType type;
    [Tooltip("Можно назначить несколько клипов, будет выбираться случайный")]
    public AudioClip[] clips;
    [Range(0f, 2f)] public float volumeMultiplier = 1f;
}

public class SoundManager : MonoBehaviour
{
    #region Поля
    [BoxGroup("SETTINGS"), Tooltip("Основная громкость для всех SFX."), Range(0f, 1f), SerializeField]
    private float _sfxVolume = 1f;

    [BoxGroup("SETTINGS"), Tooltip("Размер пула для одновременных звуков"), SerializeField]
    private int _oneShotPoolSize = 15;

    [BoxGroup("SETTINGS/Sound List"), SerializeField]
    private List<SoundEffect> _soundEffects = new List<SoundEffect>();

    [BoxGroup("DEBUG"), SerializeField] protected bool _ColoredDebug;

    private static SoundManager _instance;
    private List<AudioSource> _oneShotSources;
    #endregion Поля

    #region Свойства
    public static SoundManager Instance { get => _instance; }
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        if (_instance != null)
        {
            ColoredDebug.CLog(gameObject, "<color=orange>[SYSTEM]</color> Найден дубликат SoundManager. Удаляю: <color=yellow>{0}</color>", _ColoredDebug, gameObject.name);
            Destroy(gameObject); // Если менеджер уже есть, удаляем этот объект
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // Чтобы музыка не прерывалась при смене сцен

        ColoredDebug.CLog(gameObject, "<color=cyan>[INFO]</color> Инициализация SoundManager...", _ColoredDebug);
        InitAudioSources();
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Воспроизводит звуковой эффект.
    /// </summary>
    public void PlayOneShot(SoundType type)
    {
        ColoredDebug.CLog(gameObject, "<color=lime>[ACTION]</color> Запрос на проигрывание звука: <color=yellow>{0}</color>", _ColoredDebug, type);

        if (type == SoundType.None)
        {
            ColoredDebug.CLog(gameObject, "<color=grey>[DEBUG]</color> Тип звука 'None', пропускаем.", _ColoredDebug);
            return;
        }

        SoundEffect effect = _soundEffects.FirstOrDefault(e => e.type == type);

        if (effect == null)
        {
            ColoredDebug.CLog(gameObject, "<color=red>[ERROR]</color> Звуковой эффект <color=yellow>{0}</color> не найден в списке настроек!", _ColoredDebug, type);
            return;
        }

        if (effect.clips == null || effect.clips.Length == 0)
        {
            ColoredDebug.CLog(gameObject, "<color=red>[ERROR]</color> Для эффекта <color=yellow>{0}</color> не назначены аудиоклипы!", _ColoredDebug, type);
            return;
        }

        // Выбираем случайный клип (вариативность)
        AudioClip clip = effect.clips[Random.Range(0, effect.clips.Length)];

        if (clip == null)
        {
            ColoredDebug.CLog(gameObject, "<color=red>[ERROR]</color> Один из клипов для <color=yellow>{0}</color> пустой (null)!", _ColoredDebug, type);
            return;
        }

        // Берем свободный источник
        AudioSource source = GetAvailableOneShotSource();

        if (source != null)
        {
            // Расчет питча (опционально можно добавить вариативность pitch)
            source.pitch = 1f;
            float finalVolume = _sfxVolume * effect.volumeMultiplier;

            ColoredDebug.CLog(gameObject, "<color=cyan>[INFO]</color> Проигрываю клип: <color=white>{0}</color>. Громкость: <color=yellow>{1}</color>", _ColoredDebug, clip.name, finalVolume);

            source.PlayOneShot(clip, finalVolume);
        }
        else
        {
            ColoredDebug.CLog(gameObject, "<color=red>[ERROR]</color> Не удалось получить AudioSource для проигрывания!", _ColoredDebug);
        }
    }
    #endregion Публичные методы

    #region Личные методы
    private void InitAudioSources()
    {
        _oneShotSources = new List<AudioSource>();
        for (int i = 0; i < _oneShotPoolSize; i++)
        {
            CreateSource();
        }
        ColoredDebug.CLog(gameObject, "<color=orange>[SYSTEM]</color> Создан пул аудио источников. Размер: <color=yellow>{0}</color>", _ColoredDebug, _oneShotPoolSize);
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        _oneShotSources.Add(source);
        return source;
    }

    private AudioSource GetAvailableOneShotSource()
    {
        foreach (var source in _oneShotSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        ColoredDebug.CLog(gameObject, "<color=orange>[SYSTEM]</color> Все источники заняты. Расширяю пул (+1).", _ColoredDebug);
        // Если все заняты - создаем новый (расширяем пул)
        return CreateSource();
    }
    #endregion
}