using UnityEngine;
using System.Collections.Generic;

// --- НОВОЕ: Подключаем Editor-инструменты ---
#if UNITY_EDITOR
using UnityEditor;
#endif
// ----------------------------------------

public class BrickPool : MonoBehaviour
{
    // --- Singleton (только для Runtime) ---
    public static BrickPool Instance { get; private set; }
    // ----------------------------------------

    [Header("Настройки Пула")]
    [Tooltip("Префаб кирпича, который мы будем создавать")]
    [SerializeField] private Brick brickPrefab;

    [Header("Состояние Пула (Запоминается)")]
    [Tooltip("Список всех кирпичей, которыми управляет пул")]
    [SerializeField] private List<Brick> _allManagedBricks = new List<Brick>();

    // Геттер для LevelManager
    public Brick BrickPrefab => brickPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ВАЖНО: При старте игры, "знакомим" все кирпичи 
        foreach (Brick brick in _allManagedBricks)
        {
            if (brick == null) continue; // Пропускаем, если ссылка "потерялась"
            brick.Init(this);
            brick.gameObject.SetActive(false); // Прячем все для старта
        }
    }

    /// <summary>
    /// (RUNTIME) Берет кирпич из пула
    /// </summary>
    public Brick GetBrick()
    {
        foreach (Brick brick in _allManagedBricks)
        {
            // Если кирпич был удален вручную в редакторе
            if (brick == null) continue;

            if (!brick.gameObject.activeSelf)
            {
                brick.gameObject.SetActive(true);
                return brick;
            }
        }
        // Если "спящих" не нашли - создаем новый
        return CreateNewBrick(true);
    }

    /// <summary>
    // (RUNTIME) Возвращает кирпич в пул
    /// </summary>
    public void ReturnBrick(Brick brick)
    {
        brick.gameObject.SetActive(false);
    }

    /// <summary>
    /// (RUNTIME) Возвращает все активные кирпичи в пул
    /// </summary>
    public void ReturnAllActiveBricks()
    {
        foreach (Brick brick in _allManagedBricks)
        {
            if (brick == null) continue;
            if (brick.gameObject.activeSelf)
            {
                brick.gameObject.SetActive(false);
            }
        }
    }

    // --- МЕТОДЫ ДЛЯ РЕДАКТОРА ---

    /// <summary>
    /// (EDITOR) Создает новый кирпич, "запоминает" его
    /// </summary>
    public Brick GetBrickEditor()
    {
        return CreateNewBrick(false); // isRuntime = false
    }

    /// <summary>
    /// (EDITOR) Уничтожает все кирпичи, которые "запомнил" пул
    /// </summary>
    public void DestroyAllBricksEditor()
    {
        // 1. Сначала очищаем сам список (он нам больше не нужен для удаления)
        _allManagedBricks.Clear();

        // 2. "Ядерная чистка": Удаляем все дочерние объекты пула.
        // Мы используем while, потому что childCount меняется при каждом удалении.
        while (transform.childCount > 0)
        {
            // Берем первого ребенка
            Transform child = transform.GetChild(0);

            // Уничтожаем его немедленно
            DestroyImmediate(child.gameObject);
        }

        Debug.Log("BrickPool: Очистка в редакторе завершена (по иерархии).");
    }

    /// <summary>
    /// Внутренний метод для создания кирпича
    /// </summary>
    private Brick CreateNewBrick(bool isRuntime)
    {
        if (brickPrefab == null)
        {
            Debug.LogError("В BrickPool не назначен префаб!");
            return null;
        }

        Brick newBrick;

        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
        if (isRuntime)
        {
            // В РЕЖИМЕ ИГРЫ: Используем Instantiate
            newBrick = Instantiate(brickPrefab, transform);
        }
        else
        {
            // В РЕДАКТОРЕ: Используем PrefabUtility
#if UNITY_EDITOR
            newBrick = (Brick)PrefabUtility.InstantiatePrefab(brickPrefab, transform);
#else
                // Запасной вариант (не должен вызываться)
                newBrick = Instantiate(brickPrefab, transform);
#endif
        }
        // -------------------------

        // Общая настройка
        newBrick.gameObject.name = "Brick_" + _allManagedBricks.Count;
        newBrick.Init(this); // Знакомим с пулом
        _allManagedBricks.Add(newBrick); // "Запоминаем"

        if (isRuntime)
        {
            newBrick.gameObject.SetActive(true); // Активируем, если в игре
        }

        return newBrick;
    }
}