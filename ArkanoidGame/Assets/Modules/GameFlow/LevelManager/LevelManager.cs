using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Required] public BrickPool brickPool;

    [Header("Grid Settings")]
    private const int COLS = 12;
    private const int ROWS = 12;
    [SerializeField] private float brickWidth = 0.69333f;
    [SerializeField] private float brickHeight = 0.34666f;

    [Header("Phase 1: Geometry (Base)")]
    [Tooltip("Обычные шаблоны для формы уровня")]
    [SerializeField] private List<BrickChunkSO> geometryChunks;
    public enum SymmetryType { MirrorHorizontal, MirrorVertical, MirrorBoth, Chaos }
    [SerializeField] private SymmetryType symmetryMode = SymmetryType.MirrorHorizontal;

    [Tooltip("Тип кирпича 'по умолчанию' (заглушка перед покраской)")]
    [SerializeField] private BrickTypeSO defaultBrickType;

    [Header("Phase 2: Painting (Colors)")]
    [SerializeField] private BrickPaletteSO palette;
    public enum PaintPattern { BottomToTop, LeftToRight, ZebraHorizontal, CenterOut, RandomNoise }
    [SerializeField] private PaintPattern paintPattern = PaintPattern.BottomToTop;

    [Header("Phase 3: Obstacles (Hard Layer)")]
    [Tooltip("Шаблоны ТОЛЬКО для неубиваемых блоков (щадящие)")]
    [SerializeField] private List<BrickChunkSO> obstacleChunks;
    [SerializeField] private BrickTypeSO indestructibleType;
    [Range(0f, 1f)][SerializeField] private float obstacleChance = 0.5f; // Шанс появления слоя препятствий

    // --- Внутренняя сетка для доступа к кирпичам по координатам ---
    private Brick[,] _spawnedGrid = new Brick[COLS, ROWS];
    // -------------------------------------------------------------

    [Button("Сгенерировать Уровень (Pipeline)")]
    public void BuildLevel()
    {
        if (!ValidateReferences()) return;

        // 0. Очистка
        CleanupOldLevel();

        // --- ФАЗА 1: ГЕОМЕТРИЯ ---
        GenerateGeometryLayer();

        // --- ФАЗА 2: ПОКРАСКА ---
        PaintBricksLayer();

        // --- ФАЗА 3: ПРЕПЯТСТВИЯ ---
        OverlayObstaclesLayer();

        // Финал: Отчет GameManager'у
        ReportToGameManager();
    }

    // ========================================================================
    // ФАЗА 1: ГЕОМЕТРИЯ (Создание объектов)
    // ========================================================================
    private void GenerateGeometryLayer()
    {
        BrickChunkSO chunkA = geometryChunks[Random.Range(0, geometryChunks.Count)];
        BrickChunkSO chunkB = geometryChunks[Random.Range(0, geometryChunks.Count)];

        // Расчет симметрии (Квадранты)
        BrickChunkSO[] quads = new BrickChunkSO[4];
        bool[] fx = new bool[4];
        bool[] fy = new bool[4];
        SetupSymmetry(chunkA, chunkB, ref quads, ref fx, ref fy);

        // Расчет центра
        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        // Создаем кирпичи
        SpawnQuadrant(quads[0], 0, 0, fx[0], fy[0], startPos); // Top-Left
        SpawnQuadrant(quads[1], 6, 0, fx[1], fy[1], startPos); // Top-Right
        SpawnQuadrant(quads[2], 0, 6, fx[2], fy[2], startPos); // Bottom-Left
        SpawnQuadrant(quads[3], 6, 6, fx[3], fy[3], startPos); // Bottom-Right
    }

    private void SpawnQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY, Vector2 startPos)
    {
        if (chunk == null) return;

        foreach (var data in chunk.bricks)
        {
            // 1. Локальные координаты в чанке 6x6
            int cx = data.position.x;
            int cy = data.position.y;

            // 2. Зеркалирование
            if (flipX) cx = (chunk.width - 1) - cx;
            if (flipY) cy = (chunk.height - 1) - cy;

            // 3. Глобальные координаты
            int col = offsetX + cx;
            int row = offsetY + (chunk.height - 1 - cy);

            // 4. Создаем кирпич
            Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();

            // 5. СТАВИМ ДЕФОЛТНЫЙ ТИП (Покрасим позже)
            newBrick.Setup(defaultBrickType);

            // 6. Позиционируем
            float xPos = startPos.x + (col * brickWidth);
            float yPos = startPos.y - (row * brickHeight);
            newBrick.transform.position = new Vector2(xPos, yPos);

            // 7. ЗАПИСЫВАЕМ В МАССИВ (ВАЖНО!)
            if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
            {
                _spawnedGrid[col, row] = newBrick;
            }
        }
    }

    // ========================================================================
    // ФАЗА 2: ПОКРАСКА (Изменение типов)
    // ========================================================================
    private void PaintBricksLayer()
    {
        for (int y = 0; y < ROWS; y++)
        {
            for (int x = 0; x < COLS; x++)
            {
                Brick brick = _spawnedGrid[x, y];
                if (brick == null) continue; // Тут пусто

                // Вычисляем индекс палитры (0.0 to 1.0)
                float t = 0f;

                switch (paintPattern)
                {
                    case PaintPattern.BottomToTop:
                        // y=0 (верх), y=11 (низ). Нам нужно снизу(0) вверх(1)
                        // значит: (MaxRow - y) / MaxRow
                        t = (float)(ROWS - 1 - y) / (ROWS - 1);
                        break;

                    case PaintPattern.LeftToRight:
                        t = (float)x / (COLS - 1);
                        break;

                    case PaintPattern.ZebraHorizontal:
                        // Четные ряды = 0, Нечетные = 1 (или наоборот)
                        t = (y % 2 == 0) ? 0f : 1f;
                        // Или можно map'ить на середину и конец палитры
                        break;

                    case PaintPattern.CenterOut:
                        // Дистанция от центра (6, 6)
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(COLS / 2f, ROWS / 2f));
                        float maxDist = Vector2.Distance(Vector2.zero, new Vector2(COLS / 2f, ROWS / 2f));
                        t = 1f - (dist / maxDist); // 1 в центре, 0 с краю
                        break;

                    case PaintPattern.RandomNoise:
                        t = Random.value;
                        break;
                }

                // Конвертируем 0..1 в Индекс Палитры
                int tierIndex = Mathf.RoundToInt(t * (palette.Count - 1));

                // Применяем тип из палитры
                brick.Setup(palette.GetTier(tierIndex));
            }
        }
    }

    // ========================================================================
    // ФАЗА 3: ПРЕПЯТСТВИЯ (Наложение Hard Layer)
    // ========================================================================
    private void OverlayObstaclesLayer()
    {
        // Шанс пропустить этот шаг
        if (Random.value > obstacleChance) return;
        if (obstacleChunks.Count == 0) return;

        // Выбираем случайные шаблоны препятствий (для симметрии можно так же 2 взять)
        BrickChunkSO obsChunk = obstacleChunks[Random.Range(0, obstacleChunks.Count)];

        // Мы просто "накладываем" их. Логика похожа на SpawnQuadrant, 
        // но вместо создания мы ИЩЕМ существующий кирпич.

        // Пример: накладываем зеркально во все 4 угла
        ApplyObstacleQuadrant(obsChunk, 0, 0, false, false);
        ApplyObstacleQuadrant(obsChunk, 6, 0, true, false);
        ApplyObstacleQuadrant(obsChunk, 0, 6, false, true);
        ApplyObstacleQuadrant(obsChunk, 6, 6, true, true);
    }

    private void ApplyObstacleQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY)
    {
        foreach (var data in chunk.bricks)
        {
            // В шаблоне препятствий нас интересуют только те кирпичи, 
            // которые помечены как "Indestructible" или специальные.
            // Если в шаблоне "обычный" кирпич - мы его игнорируем (прозрачность).
            // НО: В вашей системе BrickChunk хранит BrickType. 
            // Допустим, мы договорились: если в шаблоне Obstacle есть кирпич - мы заменяем на Indestructible.

            int cx = data.position.x;
            int cy = data.position.y;
            if (flipX) cx = (chunk.width - 1) - cx;
            if (flipY) cy = (chunk.height - 1) - cy;

            int col = offsetX + cx;
            int row = offsetY + (chunk.height - 1 - cy);

            if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
            {
                Brick existingBrick = _spawnedGrid[col, row];

                // Если там уже есть кирпич -> заменяем его на Неубиваемый
                if (existingBrick != null)
                {
                    existingBrick.Setup(indestructibleType);
                }
            }
        }
    }


    // ========================================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ========================================================================

    private void ReportToGameManager()
    {
        if (!Application.isPlaying) return;
        if (GameManager.Instance == null) return;

        int destroyableCount = 0;
        foreach (var brick in _spawnedGrid)
        {
            if (brick != null && !brick.IsIndestructible) // Нужно добавить свойство IsIndestructible в Brick
            {
                destroyableCount++;
            }
        }
        GameManager.Instance.SetBrickCount(destroyableCount);
    }

    private void CleanupOldLevel()
    {
        System.Array.Clear(_spawnedGrid, 0, _spawnedGrid.Length); // Чистим массив ссылок
        if (Application.isPlaying) brickPool.ReturnAllActiveBricks();
        else brickPool.DestroyAllBricksEditor();
    }

    private bool ValidateReferences()
    {
        if (brickPool == null || palette == null || defaultBrickType == null || geometryChunks.Count == 0)
        {
            Debug.LogError("LevelManager: Missing references!");
            return false;
        }
        return true;
    }

    // (Метод SetupSymmetry такой же, как в прошлом ответе, скопируйте его сюда)
    private void SetupSymmetry(BrickChunkSO A, BrickChunkSO B, ref BrickChunkSO[] templates, ref bool[] fx, ref bool[] fy)
    {
        switch (symmetryMode)
        {
            case SymmetryType.MirrorHorizontal:
                templates[0] = A; fx[0] = false; fy[0] = false;
                templates[1] = A; fx[1] = true; fy[1] = false;
                templates[2] = B; fx[2] = false; fy[2] = false;
                templates[3] = B; fx[3] = true; fy[3] = false;
                break;
            case SymmetryType.MirrorVertical:
                templates[0] = A; fx[0] = false; fy[0] = false;
                templates[1] = B; fx[1] = false; fy[1] = false;
                templates[2] = A; fx[2] = false; fy[2] = true;
                templates[3] = B; fx[3] = false; fy[3] = true;
                break;
            case SymmetryType.MirrorBoth:
                templates[0] = A; fx[0] = false; fy[0] = false;
                templates[1] = A; fx[1] = true; fy[1] = false;
                templates[2] = A; fx[2] = false; fy[2] = true;
                templates[3] = A; fx[3] = true; fy[3] = true;
                break;
            case SymmetryType.Chaos:
                for (int i = 0; i < 4; i++)
                {
                    templates[i] = geometryChunks[Random.Range(0, geometryChunks.Count)];
                    fx[i] = Random.value > 0.5f;
                    fy[i] = Random.value > 0.5f;
                }
                break;
        }
    }
}