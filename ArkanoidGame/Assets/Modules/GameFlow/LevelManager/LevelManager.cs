using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Ссылка на пул (вы назначаете ее в инспекторе)
    public BrickPool brickPool;

    [Header("Настройки Сетки Уровня")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 8;

    // --- Ручная настройка (ваш код) ---
    [Header("Ручная Настройка Размеров (В ЮНИТАХ!)")]
    [SerializeField] private bool useManualSpacing = false;
    [Tooltip("Ручная ширина кирпича (например, 0.64)")]
    [SerializeField] private float manualBrickWidth = 0.64f;
    [Tooltip("Ручная высота кирпича (например, 0.32)")]
    [SerializeField] private float manualBrickHeight = 0.32f;

    // --- Внутренние переменные ---
    private float _brickWidth;
    private float _brickHeight;

    void Start()
    {
        // GameManager теперь отвечает за вызов BuildLevel()
        // при старте, поэтому здесь можно ничего не делать.
        // Оставим Start() пустым.
    }

    /// <sSummary>
    /// Вычисляет РЕАЛЬНЫЙ размер кирпича (АВТОМАТИЧЕСКИ)
    /// </summary>
    private bool CalculateBrickSize()
    {
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
    /// Главный метод постройки уровня.
    /// Теперь он вызывается из GameManager.
    /// </summary>
    [Button]
    public void BuildLevel()
    {
        if (brickPool == null)
        {
            Debug.LogError("BrickPool не назначен в LevelManager!");
            return;
        }

        // 1. Расчет размера кирпича
        if (useManualSpacing)
        {
            _brickWidth = manualBrickWidth;
            _brickHeight = manualBrickHeight;
        }
        else if (!CalculateBrickSize())
        {
            return; // Ошибка расчета
        }

        if (_brickWidth == 0 || _brickHeight == 0)
        {
            Debug.LogError("Размер кирпича равен 0. Проверьте 'Manual Spacing'.");
            return;
        }

        // 2. Чистим старые кирпичи
        if (Application.isPlaying)
        {
            brickPool.ReturnAllActiveBricks();
        }
        else
        {
            brickPool.DestroyAllBricksEditor();
        }

        // 3. Расчет центрирования (по позиции [LevelManager])
        float totalGridWidth = (cols - 1) * _brickWidth;
        float totalGridHeight = (rows - 1) * _brickHeight;
        Vector2 currentCenter = transform.position;

        Vector2 startPos = new Vector2(
            currentCenter.x - (totalGridWidth / 2f),
            currentCenter.y + (totalGridHeight / 2f)
        );

        // 4. Строим сетку
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Brick newBrick;
                if (Application.isPlaying)
                {
                    newBrick = brickPool.GetBrick();
                }
                else
                {
                    newBrick = brickPool.GetBrickEditor();
                }

                float xPos = startPos.x + (c * _brickWidth);
                float yPos = startPos.y - (r * _brickHeight);
                newBrick.transform.position = new Vector2(xPos, yPos);
            }
        }

        // 5. --- НОВОЕ ---
        // Сообщаем "Мозгу", сколько кирпичей мы построили
        if (Application.isPlaying)
        {
            // Убедимся, что GameManager существует, 
            // прежде чем его вызывать
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetBrickCount(rows * cols);
            }
        }
    }
}