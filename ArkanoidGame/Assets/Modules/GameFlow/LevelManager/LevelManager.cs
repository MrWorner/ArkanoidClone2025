using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public BrickPool brickPool;

    [Header("Настройки Сетки Уровня")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 8;

    // [Tooltip("Позиция ЦЕНТРА...")]  // <-- УДАЛЕНО
    // [SerializeField] private Vector2 gridCenterPosition; // <-- УДАЛЕНО

    [Header("Ручная Настройка Размеров (В ЮНИТАХ!)")]
    [SerializeField] private bool useManualSpacing = false;
    [SerializeField] private float manualBrickWidth = 0.64f;
    [SerializeField] private float manualBrickHeight = 0.32f;

    private float _brickWidth;
    private float _brickHeight;

    void Start()
    {
        // Просто вызываем BuildLevel. Вся логика теперь внутри него.
        BuildLevel();
    }

    /// <summary>
    /// Вычисляет РЕАЛЬНЫЙ размер кирпича (АВТОМАТИЧЕСКИ)
    /// </summary>
    private bool CalculateBrickSize()
    {
        if (brickPool == null) { Debug.LogError("BrickPool не назначен!"); return false; }

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


    [Button]
    public void BuildLevel()
    {
        if (brickPool == null)
        {
            Debug.LogError("BrickPool не назначен в LevelManager!");
            return;
        }

        // --- ИСПРАВЛЕНО: Расчет размера ДОЛЖЕН БЫТЬ ЗДЕСЬ ---
        // (Чтобы кнопка работала в редакторе без запуска Start)
        if (useManualSpacing)
        {
            _brickWidth = manualBrickWidth;
            _brickHeight = manualBrickHeight;
        }
        else if (!CalculateBrickSize())
        {
            // Если автоматика не сработала, выходим
            return;
        }
        // ----------------------------------------------------

        if (_brickWidth == 0 || _brickHeight == 0)
        {
            Debug.LogError("Размер кирпича равен 0. Проверьте 'Manual Spacing' или настройки префаба.");
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

        // 3. Расчет центрирования
        float totalGridWidth = (cols - 1) * _brickWidth;
        float totalGridHeight = (rows - 1) * _brickHeight;

        // --- ГЛАВНЫЙ ФИКС: Используем transform.position ---
        Vector2 currentCenter = transform.position; // Берем позицию этого объекта!

        Vector2 startPos = new Vector2(
            currentCenter.x - (totalGridWidth / 2f),
            currentCenter.y + (totalGridHeight / 2f)
        );
        // -------------------------------------------------

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
    }
}