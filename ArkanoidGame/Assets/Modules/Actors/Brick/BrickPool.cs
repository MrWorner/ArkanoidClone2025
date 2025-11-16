using UnityEngine;
using System.Collections.Generic;

public class BrickPool : MonoBehaviour
{
    // ---- Singleton (только для Runtime) ----
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
        // Настраиваем Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ВАЖНО: При старте игры, "знакомим" все кирпичи 
        // (созданные в редакторе) с этим пулом
        foreach (Brick brick in _allManagedBricks)
        {
            brick.Init(this);
            brick.gameObject.SetActive(false); // Прячем все для старта
        }
    }

    /// <summary>
    /// (RUNTIME) Берет кирпич из пула
    /// </summary>
    public Brick GetBrick()
    {
        // Ищем в списке "спящий" кирпич
        foreach (Brick brick in _allManagedBricks)
        {
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
        return CreateNewBrick(false);
    }

    /// <summary>
    /// (EDITOR) Уничтожает все кирпичи, которые "запомнил" пул
    /// </summary>
    public void DestroyAllBricksEditor()
    {
        // Уничтожаем объекты
        foreach (Brick brick in _allManagedBricks)
        {
            if (brick != null)
            {
                // Используем DestroyImmediate, т.к. мы в редакторе
                DestroyImmediate(brick.gameObject);
            }
        }

        // Чистим список
        _allManagedBricks.Clear();
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

        // Создаем
        Brick newBrick = Instantiate(brickPrefab, transform);
        newBrick.gameObject.name = "Brick_" + _allManagedBricks.Count;

        // Знакомим с пулом
        newBrick.Init(this);

        // "Запоминаем"
        _allManagedBricks.Add(newBrick);

        // Если это runtime, сразу активируем
        if (isRuntime)
        {
            newBrick.gameObject.SetActive(true);
        }

        return newBrick;
    }

    public Transform GetLastActiveBrickTransform()
    {
        foreach (Brick brick in _allManagedBricks)
        {
            if (brick.gameObject.activeSelf)
            {
                return brick.transform;
            }
        }
        return null; // Не найдено
    }
}