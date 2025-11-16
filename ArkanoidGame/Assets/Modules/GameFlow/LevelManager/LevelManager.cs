using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Ваша публичная ссылка. Убедитесь, что перетащили 
    // [BrickPool] в этот слот в инспекторе!
    public BrickPool brickPool;

    [Header("Настройки Сетки Уровня")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 8;

    [Tooltip("Позиция ЦЕНТРА всей сетки кирпичей")]
    [SerializeField] private Vector2 gridCenterPosition;

    [Header("Ручная Настройка Размеров (В ЮНИТАХ!)")]
    [Tooltip("Включить, если авто-расчет размера работает неверно")]
    [SerializeField] private bool useManualSpacing = false;

    [Tooltip("Ручная ширина кирпича (например, 0.64)")]
    [SerializeField] private float manualBrickWidth = 0.64f;

    [Tooltip("Ручная высота кирпича (например, 0.32)")]
    [SerializeField] private float manualBrickHeight = 0.32f;

    private float _brickWidth;
    private float _brickHeight;

    /// <summary>
    /// Вызывается при запуске игры
    /// </summary>
    void Start()
    {
        // В режиме игры мы НЕ ПЕРЕСТРАИВАЕМ уровень,
        // а просто просим пул "показать" кирпичи, 
        // которые мы уже создали в редакторе.

        // (Но если вы хотите перестраивать уровень при 
        // каждом Start, раскомментируйте код ниже)

        // Debug.Log("Уровень построен из пула (Runtime)");
        // BuildLevel(); 
    }

    /// <summary>
    /// Вычисляет размер кирпича
    /// </summary>
    private bool CalculateBrickSize()
    {
        // ИСПРАВЛЕНО: Используем public-ссылку, а не Singleton
        if (brickPool == null)
        {
            Debug.LogError("BrickPool не назначен в LevelManager!");
            return false;
        }

        Brick prefab = brickPool.BrickPrefab;
        if (prefab == null) { Debug.LogError("В BrickPool не назначен префаб!"); return false; }

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("На префабе кирпича нет SpriteRenderer или самого спрайта!");
            return false;
        }

        float baseWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        float baseHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

        Vector3 prefabScale = prefab.transform.localScale;
        _brickWidth = baseWidth * prefabScale.x;
        _brickHeight = baseHeight * prefabScale.y;

        if (_brickWidth == 0 || _brickHeight == 0)
        {
            Debug.LogError("Рассчитанный размер кирпича равен 0.");
            return false;
        }
        return true;
    }


    /// <summary>
    /// Кнопка для постройки уровня в РЕДАКТОРЕ
    /// </summary>
    [Button]
    public void BuildLevel()
    {
        if (brickPool == null)
        {
            Debug.LogError("BrickPool не назначен в LevelManager!");
            return;
        }

        // Проверяем, что мы в редакторе.
        // (Этот метод не предназначен для Runtime)
        if (Application.isPlaying)
        {
            Debug.LogWarning("BuildLevel() вызван в Runtime. Используется Runtime-логика пула.");
        }

        // 1. Считаем размеры
        if (useManualSpacing)
        {
            _brickWidth = manualBrickWidth;
            _brickHeight = manualBrickHeight;
        }
        else if (!CalculateBrickSize())
        {
            return; // Ошибка расчета
        }

        // 2. ИСПРАВЛЕНО: Говорим пулу УНИЧТОЖИТЬ старые кирпичи
        if (Application.isPlaying)
        {
            brickPool.ReturnAllActiveBricks();
        }
        else
        {
            brickPool.DestroyAllBricksEditor(); // Метод для редактора
        }

        // 3. Расчет центрирования (как у вас и было)
        float totalGridWidth = (cols - 1) * _brickWidth;
        float totalGridHeight = (rows - 1) * _brickHeight;

        Vector2 startPos = new Vector2(
            gridCenterPosition.x - (totalGridWidth / 2f),
            gridCenterPosition.y + (totalGridHeight / 2f)
        );

        // 4. Строим сетку
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 5. ИСПРАВЛЕНО: Берем кирпич через метод Редактора
                Brick newBrick;
                if (Application.isPlaying)
                {
                    newBrick = brickPool.GetBrick();
                }
                else
                {
                    newBrick = brickPool.GetBrickEditor(); // Метод для редактора
                }

                // 6. Ставим на место
                float xPos = startPos.x + (c * _brickWidth);
                float yPos = startPos.y - (r * _brickHeight);
                newBrick.transform.position = new Vector2(xPos, yPos);
            }
        }
    }
}