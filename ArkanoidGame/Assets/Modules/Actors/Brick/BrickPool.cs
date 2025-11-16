using UnityEngine;
using System.Collections.Generic;

public class BrickPool : MonoBehaviour
{
    // ---- Singleton ----
    public static BrickPool Instance { get; private set; }
    // -------------------

    [Header("Настройки Пула")]
    [Tooltip("Префаб кирпича, который мы будем создавать")]
    [SerializeField] private Brick brickPrefab;

    [Tooltip("Сколько кирпичей создать при старте")]
    [SerializeField] private int initialPoolSize = 50;

    // "Склад" неактивных кирпичей
    private Stack<Brick> _inactivePool = new Stack<Brick>();
    // Список активных (для очистки уровня)
    private List<Brick> _activeBricks = new List<Brick>();

    void Awake()
    {
        // Настраиваем Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 1. Заполняем "склад" при запуске игры
        PopulatePool();
    }

    /// <summary>
    /// Создает стартовое кол-во кирпичей и прячет их
    /// </summary>
    private void PopulatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateAndPoolBrick();
        }
    }

    /// <summary>
    /// Вспомогательный метод: создает 1 кирпич и кладет на склад
    /// </summary>
    private Brick CreateAndPoolBrick()
    {
        // Instantiate создает копию префаба
        // (transform) делает этот объект дочерним к [BrickPool]
        Brick newBrick = Instantiate(brickPrefab, transform);

        newBrick.Init(this); // "Знакомим" кирпич с пулом
        newBrick.gameObject.SetActive(false); // Прячем его
        _inactivePool.Push(newBrick); // Кладем на "склад"
        return newBrick;
    }

    /// <summary>
    /// ГЛАВНЫЙ МЕТОД: Взять кирпич со склада
    /// </summary>
    public Brick GetBrick()
    {
        Brick brickToGet;

        if (_inactivePool.Count > 0)
        {
            // Берем со склада, если там что-то есть
            brickToGet = _inactivePool.Pop();
        }
        else
        {
            // Если склад пуст - создаем новый (расширяем пул)
            Debug.LogWarning("Пул пуст. Создаем новый кирпич...");
            brickToGet = CreateAndPoolBrick();
            _inactivePool.Pop(); // Сразу забираем его со склада
        }

        brickToGet.gameObject.SetActive(true); // "Показываем" кирпич
        _activeBricks.Add(brickToGet); // Добавляем в список "на уровне"
        return brickToGet;
    }

    /// <summary>
    /// ГЛАВНЫЙ МЕТОД: Вернуть кирпич на склад
    /// </summary>
    public void ReturnBrick(Brick brick)
    {
        if (brick.gameObject.activeInHierarchy)
        {
            brick.gameObject.SetActive(false); // Прячем
            _activeBricks.Remove(brick); // Убираем из "активных"
            _inactivePool.Push(brick); // Кладем на "склад"
        }
    }

    /// <summary>
    /// Вызывается перед постройкой нового уровня
    /// </summary>
    public void ReturnAllActiveBricks()
    {
        // Важно: луп в обратную сторону, т.к. мы меняем список
        for (int i = _activeBricks.Count - 1; i >= 0; i--)
        {
            ReturnBrick(_activeBricks[i]);
        }
    }
}