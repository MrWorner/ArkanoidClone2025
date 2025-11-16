using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public BrickPool brickPool;

    [Header("Настройки Сетки Уровня")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 8;

    [Header("Настройки Финала")]
    [Tooltip("Сколько кирпичей должно остаться, чтобы включить 'Форсаж'")]
    [SerializeField] private int finalModeBrickCount = 10;

    // --- Переменные для логики ---
    private float _brickWidth;
    private float _brickHeight;
    private int _activeBrickCount;
    private bool _isFinalModeActive = false;
    private BallController _ball; // Ссылка на мяч

    // --- Ручная настройка (ваш код) ---
    [Header("Ручная Настройка Размеров (В ЮНИТАХ!)")]
    [SerializeField] private bool useManualSpacing = false;
    [SerializeField] private float manualBrickWidth = 0.64f;
    [SerializeField] private float manualBrickHeight = 0.32f;


    void Start()
    {
        // 1. Подписываемся на "сигналы" от кирпичей
        Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;

        // 2. Находим и кэшируем мяч
        _ball = FindObjectOfType<BallController>();

        // 3. Строим уровень (вызовется BuildLevel())
        BuildLevel();
    }

    /// <summary>
    /// Вызывается КАЖДЫЙ РАЗ, когда кирпич уничтожен
    /// </summary>
    private void HandleBrickDestroyed()
    {
        _activeBrickCount--;

        // 1. Активируем "Форсаж", если кирпичей <= 10
        if (!_isFinalModeActive && _activeBrickCount <= finalModeBrickCount)
        {
            if (_ball != null)
            {
                _ball.ActivateSpeedBoost();
                _isFinalModeActive = true;
            }
        }

        // 2. Активируем "Хоминг", если кирпич == 1
        if (_activeBrickCount == 1)
        {
            if (_ball != null && brickPool != null)
            {
                Transform lastBrick = brickPool.GetLastActiveBrickTransform();
                _ball.SetHomingTarget(lastBrick);
            }
        }

        // (Тут же можно проверять на победу)
        // if (_activeBrickCount <= 0) { /* Вы победили! */ }
    }


    [Button]
    public void BuildLevel()
    {
        // (Код по расчету размеров)
        if (useManualSpacing)
        {
            _brickWidth = manualBrickWidth;
            _brickHeight = manualBrickHeight;
        }
        else if (!CalculateBrickSize())
        { return; }

        if (_brickWidth == 0 || _brickHeight == 0)
        { return; }

        // (Код по очистке)
        if (brickPool == null) { return; }
        if (Application.isPlaying) { brickPool.ReturnAllActiveBricks(); }
        else { brickPool.DestroyAllBricksEditor(); }

        // (Код по расчету позиции)
        float totalGridWidth = (cols - 1) * _brickWidth;
        float totalGridHeight = (rows - 1) * _brickHeight;
        Vector2 currentCenter = transform.position;
        Vector2 startPos = new Vector2(
            currentCenter.x - (totalGridWidth / 2f),
            currentCenter.y + (totalGridHeight / 2f)
        );

        // (Код постройки)
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();
                float xPos = startPos.x + (c * _brickWidth);
                float yPos = startPos.y - (r * _brickHeight);
                newBrick.transform.position = new Vector2(xPos, yPos);
            }
        }

        // --- Сбрасываем счетчики для нового уровня ---
        _activeBrickCount = rows * cols;
        _isFinalModeActive = false;
        // (Также нужно сбросить мяч, если он уже "в форсаже")
        if (_ball != null) { _ball.ResetMode(); }
    }


    // --- (Метод CalculateBrickSize() остается у вас, как и был) ---
    private bool CalculateBrickSize()
    {
        if (brickPool == null) { return false; }
        Brick prefab = brickPool.BrickPrefab;
        if (prefab == null) { return false; }
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) { return false; }
        float baseWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        float baseHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        Vector3 prefabScale = prefab.transform.localScale;
        _brickWidth = baseWidth * prefabScale.x;
        _brickHeight = baseHeight * prefabScale.y;
        return (_brickWidth != 0 && _brickHeight != 0);
    }

    // ВАЖНО: отписаться от static event, чтобы избежать утечек
    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }
}