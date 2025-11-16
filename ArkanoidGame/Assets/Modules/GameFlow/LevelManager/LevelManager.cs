using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic; // Нужно для List<>

public class LevelManager : MonoBehaviour
{
    public BrickPool brickPool;

    [Header("Настройки Сетки Уровня")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 8;

    // --- НОВОЕ ПОЛЕ ---
    [Header("Типы Кирпичей")]
    [Tooltip("Перетащите сюда все BrickType ассеты, которые вы создали")]
    [SerializeField] private List<BrickType> levelBrickTypes;
    // -------------------

    [Header("Ручная Настройка Размеров")]
    [SerializeField] private bool useManualSpacing = false;
    [SerializeField] private float manualBrickWidth = 0.64f;
    [SerializeField] private float manualBrickHeight = 0.32f;

    private float _brickWidth;
    private float _brickHeight;

    // (Start() остается пустым)
    void Start() { }

    // (CalculateBrickSize() остается без изменений)
    private bool CalculateBrickSize()
    {
        if (brickPool == null) { /*...*/ return false; }
        Brick prefab = brickPool.BrickPrefab;
        if (prefab == null) { /*...*/ return false; }
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) { /*...*/ return false; }
        float baseWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        float baseHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        Vector3 prefabScale = prefab.transform.localScale;
        _brickWidth = baseWidth * prefabScale.x;
        _brickHeight = baseHeight * prefabScale.y;
        if (_brickWidth == 0 || _brickHeight == 0) { /*...*/ return false; }
        return true;
    }


    [Button]
    public void BuildLevel()
    {
        if (brickPool == null) { /*...*/ return; }
        if (levelBrickTypes == null || levelBrickTypes.Count == 0)
        {
            Debug.LogError("LevelManager: Список 'levelBrickTypes' пуст! Не могу построить уровень.");
            return;
        }

        // 1. Расчет размера
        if (useManualSpacing) { /*...*/ }
        else if (!CalculateBrickSize()) { /*...*/ }
        if (_brickWidth == 0 || _brickHeight == 0) { /*...*/ return; }

        // 2. Чистка
        if (Application.isPlaying) { brickPool.ReturnAllActiveBricks(); }
        else { brickPool.DestroyAllBricksEditor(); }

        // 3. Расчет позиции
        float totalGridWidth = (cols - 1) * _brickWidth;
        float totalGridHeight = (rows - 1) * _brickHeight;
        Vector2 currentCenter = transform.position;
        Vector2 startPos = new Vector2(
            currentCenter.x - (totalGridWidth / 2f),
            currentCenter.y + (totalGridHeight / 2f)
        );

        // --- НОВОЕ: Счетчик "победных" кирпичей ---
        int destroyableBrickCount = 0;
        // ------------------------------------------

        // 4. Строим сетку
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 4a. Берем "пустой" кирпич из пула
                Brick newBrick;
                if (Application.isPlaying) { newBrick = brickPool.GetBrick(); }
                else { newBrick = brickPool.GetBrickEditor(); }

                // --- ГЛАВНЫЕ ИЗМЕНЕНИЯ ---

                // 4b. Выбираем случайный "шаблон"
                BrickType randomType = levelBrickTypes[Random.Range(0, levelBrickTypes.Count)];

                // 4c. "Настраиваем" кирпич
                newBrick.Setup(randomType);

                // 4d. Считаем, важен ли он для победы (Запрос #2)
                if (!randomType.isIndestructible)
                {
                    destroyableBrickCount++;
                }
                // -------------------------

                // 4e. Ставим на место
                float xPos = startPos.x + (c * _brickWidth);
                float yPos = startPos.y - (r * _brickHeight);
                newBrick.transform.position = new Vector2(xPos, yPos);
            }
        }

        // 5. Сообщаем "Мозгу", сколько кирпичей НУЖНО уничтожить
        if (Application.isPlaying)
        {
            if (GameManager.Instance != null)
            {
                // Отправляем ТОЛЬКО число разрушаемых
                GameManager.Instance.SetBrickCount(destroyableBrickCount);
            }
        }
    }
}