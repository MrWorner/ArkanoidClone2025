using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelManager : MonoBehaviour
{
    [Required] public BrickPool brickPool;

    [Header("Grid Settings")]
    private const int COLS = 12;
    private const int ROWS = 12;
    [SerializeField] private float brickWidth = 0.69333f;
    [SerializeField] private float brickHeight = 0.34666f;

    // ========================================================================
    // PHASE 1: GEOMETRY
    // ========================================================================
    [Title("Phase 1: Geometry")]

    [FolderPath]
    public string geometryPath = "Assets/Modules/Data/Chunks/Geometry";

    [SerializeField] private List<BrickChunkSO> geometryChunks;

    // --- НОВАЯ ОПЦИЯ ---
    [Range(1, 4)]
    [Tooltip("Сколько разных шаблонов использовать для построения (1 = симметрия, 4 = хаос)")]
    [SerializeField] private int geometryTemplateCount = 2;
    // -------------------

    public enum SymmetryType { MirrorHorizontal, MirrorVertical, MirrorBoth, Chaos }
    [SerializeField] private SymmetryType symmetryMode = SymmetryType.MirrorHorizontal;

    [SerializeField] private BrickTypeSO defaultBrickType;

    // ========================================================================
    // PHASE 2: PAINTING
    // ========================================================================
    [Title("Phase 2: Painting")]

    [SerializeField] private BrickPaletteSO palette;
    public enum PaintPattern { BottomToTop, LeftToRight, ZebraHorizontal, CenterOut }
    [SerializeField] private PaintPattern paintPattern = PaintPattern.BottomToTop;

    // ========================================================================
    // PHASE 3: OBSTACLES
    // ========================================================================
    [Title("Phase 3: Obstacles")]

    [SerializeField] private bool enableObstacles = true;

    // --- НОВАЯ ОПЦИЯ ---
    [Range(1, 4)]
    [Tooltip("Сколько разных шаблонов препятствий смешивать")]
    [SerializeField] private int obstacleTemplateCount = 1;
    // -------------------

    [FolderPath]
    public string obstaclesPath = "Assets/Modules/Data/Chunks/Obstacles";

    [SerializeField] private List<BrickChunkSO> obstacleChunks;

    [SerializeField] private BrickTypeSO indestructibleType;

    // --- Internal ---
    private Brick[,] _spawnedGrid = new Brick[COLS, ROWS];

    void Start() { }

    // ========================================================================
    // TOOLS
    // ========================================================================
    [Title("Tools")]

    [Button("📂 Загрузить Чанки", ButtonSizes.Large), GUIColor(0.6f, 1f, 0.6f)]
    private void LoadChunksFromFolders()
    {
#if UNITY_EDITOR
        geometryChunks = LoadAssets<BrickChunkSO>(geometryPath);
        obstacleChunks = LoadAssets<BrickChunkSO>(obstaclesPath);
        EditorUtility.SetDirty(this);
        Debug.Log($"[LevelManager] Loaded {geometryChunks.Count} Geometry & {obstacleChunks.Count} Obstacles.");
#endif
    }

    [Button("🎲 Полный Рандом (Chaos)", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
    public void BuildChaosLevel()
    {
        // Принудительно ставим Хаос режим (чтобы каждый угол вертелся)
        symmetryMode = SymmetryType.Chaos;

        // Случайное кол-во шаблонов (от 1 до 4)
        //geometryTemplateCount = Random.Range(1, 5);
        //obstacleTemplateCount = Random.Range(1, 3); // Для препятствий лучше 1-2, чтобы не перегружать

        var paintValues = System.Enum.GetValues(typeof(PaintPattern));
        paintPattern = (PaintPattern)paintValues.GetValue(Random.Range(0, paintValues.Length));

        // ВНИМАНИЕ: enableObstacles НЕ трогаем, как вы просили!

        BuildLevel();
    }

    [Button("Построить")]
    public void BuildLevel()
    {
        if (!ValidateReferences()) return;
        CleanupOldLevel();

        // Генерация
        if (symmetryMode == SymmetryType.Chaos)
        {
            var paintValues = System.Enum.GetValues(typeof(PaintPattern));
            paintPattern = (PaintPattern)paintValues.GetValue(Random.Range(0, paintValues.Length));

            GenerateChaosGeometry();
        }
        else
        {
            GenerateSymmetricGeometry();
        }

        PaintBricksLayer();

        if (enableObstacles)
        {
            OverlayObstaclesLayer();
        }

        ReportToGameManager();
    }

    // ========================================================================
    // GENERATION LOGIC
    // ========================================================================

    // --- ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ РАСПРЕДЕЛЕНИЯ ШАБЛОНОВ ---
    private List<BrickChunkSO> GetDistributedTemplates(List<BrickChunkSO> sourceList, int count)
    {
        // 1. Перемешиваем исходный список и берем N уникальных
        // (Если count > кол-ва доступных чанков, берем сколько есть)
        int safeCount = Mathf.Min(count, sourceList.Count);
        List<BrickChunkSO> uniqueSelection = sourceList.OrderBy(x => Random.value).Take(safeCount).ToList();

        // 2. Создаем список на 4 слота (для 4-х квадрантов)
        List<BrickChunkSO> finalDistribution = new List<BrickChunkSO>();

        // 3. Заполняем 4 слота, циклично проходясь по уникальным
        // Пример: count=2 (A, B). Result: A, B, A, B.
        // Пример: count=3 (A, B, C). Result: A, B, C, A.
        for (int i = 0; i < 4; i++)
        {
            finalDistribution.Add(uniqueSelection[i % uniqueSelection.Count]);
        }

        // 4. Перемешиваем итоговый результат, чтобы порядок не был предсказуемым
        return finalDistribution.OrderBy(x => Random.value).ToList();
    }
    // ----------------------------------------------------------

    private void GenerateChaosGeometry()
    {
        if (geometryChunks.Count == 0) return;

        // 1. Получаем распределенный список из 4 шаблонов
        List<BrickChunkSO> templates = GetDistributedTemplates(geometryChunks, geometryTemplateCount);

        // Логи
        string names = string.Join(", ", templates.Select(c => c.name).Distinct());
        Debug.Log($"<b>[LevelGen]</b> Geo [{geometryTemplateCount}]: {names}");

        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        List<Vector2Int> quadrants = new List<Vector2Int>
        { new Vector2Int(0, 0), new Vector2Int(6, 0), new Vector2Int(0, 6), new Vector2Int(6, 6) };

        // Перемешиваем порядок квадрантов, чтобы шаблоны ложились в случайные места
        // (хотя templates уже перемешан, это двойная гарантия)

        for (int i = 0; i < 4; i++)
        {
            BrickChunkSO chunk = templates[i];
            Vector2Int offset = quadrants[i];

            // В Chaos режиме флипы всегда случайны
            bool flipX = Random.value > 0.5f;
            bool flipY = Random.value > 0.5f;

            SpawnQuadrant(chunk, offset.x, offset.y, flipX, flipY, startPos);
        }
    }

    private void OverlayObstaclesLayer()
    {
        if (obstacleChunks == null || obstacleChunks.Count == 0) return;

        // 1. Тоже используем распределение для препятствий!
        // Теперь препятствия тоже могут быть разными в разных углах
        List<BrickChunkSO> templates = GetDistributedTemplates(obstacleChunks, obstacleTemplateCount);

        Debug.Log($"<b>[LevelGen]</b> Obs [{obstacleTemplateCount}]: {string.Join(", ", templates.Select(c => c.name).Distinct())}");

        // Квадранты (TL, TR, BL, BR)
        List<Vector2Int> quadrants = new List<Vector2Int>
        { new Vector2Int(0, 0), new Vector2Int(6, 0), new Vector2Int(0, 6), new Vector2Int(6, 6) };

        for (int i = 0; i < 4; i++)
        {
            BrickChunkSO chunk = templates[i];
            Vector2Int offset = quadrants[i];

            // Для препятствий тоже включим случайные флипы, раз у нас Chaos-подход
            // Или можно сделать фиксированно, если хотите порядка. 
            // Сделаем случайно для веселья:
            bool flipX = Random.value > 0.5f;
            bool flipY = Random.value > 0.5f;

            ApplyObstacleQuadrant(chunk, offset.x, offset.y, flipX, flipY);
        }
    }

    // --- СТАРЫЕ МЕТОДЫ (Остаются для совместимости) ---

    private void GenerateSymmetricGeometry()
    {
        // Тут логика старая: берем A и B и расставляем по правилам
        if (geometryChunks.Count == 0) return;

        BrickChunkSO chunkA = geometryChunks[Random.Range(0, geometryChunks.Count)];
        BrickChunkSO chunkB = geometryChunks[Random.Range(0, geometryChunks.Count)];

        // Если пользователь выбрал Count=1, заставляем B быть равным A
        if (geometryTemplateCount == 1) chunkB = chunkA;

        BrickChunkSO[] quads = new BrickChunkSO[4];
        bool[] fx = new bool[4];
        bool[] fy = new bool[4];

        SetupSymmetry(chunkA, chunkB, ref quads, ref fx, ref fy);

        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        SpawnQuadrant(quads[0], 0, 0, fx[0], fy[0], startPos);
        SpawnQuadrant(quads[1], 6, 0, fx[1], fy[1], startPos);
        SpawnQuadrant(quads[2], 0, 6, fx[2], fy[2], startPos);
        SpawnQuadrant(quads[3], 6, 6, fx[3], fy[3], startPos);
    }

    // ... (Методы SpawnQuadrant, ApplyObstacleQuadrant, PaintBricksLayer, SetupSymmetry, ReportToGameManager, CleanupOldLevel, ValidateReferences) ...
    // Они не меняются, просто скопируйте их.

    private void SpawnQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY, Vector2 startPos)
    {
        if (chunk == null) return;
        foreach (var data in chunk.bricks)
        {
            int cx = data.position.x; int cy = data.position.y;
            if (flipX) cx = (chunk.width - 1) - cx;
            if (flipY) cy = (chunk.height - 1) - cy;
            int col = offsetX + cx; int row = offsetY + (chunk.height - 1 - cy);

            Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();
            newBrick.Setup(defaultBrickType);
            float xPos = startPos.x + (col * brickWidth);
            float yPos = startPos.y - (row * brickHeight);
            newBrick.transform.position = new Vector2(xPos, yPos);

            if (col >= 0 && col < COLS && row >= 0 && row < ROWS) _spawnedGrid[col, row] = newBrick;
        }
    }

    private void ApplyObstacleQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY)
    {
        foreach (var data in chunk.bricks)
        {
            int cx = data.position.x; int cy = data.position.y;
            if (flipX) cx = (chunk.width - 1) - cx;
            if (flipY) cy = (chunk.height - 1) - cy;
            int col = offsetX + cx; int row = offsetY + (chunk.height - 1 - cy);
            if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
            {
                Brick existingBrick = _spawnedGrid[col, row];
                if (existingBrick != null) existingBrick.Setup(indestructibleType);
            }
        }
    }

    private void PaintBricksLayer()
    {
        if (palette == null || palette.Count == 0) return;
        for (int y = 0; y < ROWS; y++)
        {
            for (int x = 0; x < COLS; x++)
            {
                Brick brick = _spawnedGrid[x, y];
                if (brick == null) continue;
                float t = 0f;
                switch (paintPattern)
                {
                    case PaintPattern.BottomToTop: t = (float)(ROWS - 1 - y) / (ROWS - 1); break;
                    case PaintPattern.LeftToRight: t = (float)x / (COLS - 1); break;
                    case PaintPattern.ZebraHorizontal: t = (y % 2 == 0) ? 0f : 1f; break;
                    case PaintPattern.CenterOut:
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(COLS / 2f, ROWS / 2f));
                        float maxDist = Vector2.Distance(Vector2.zero, new Vector2(COLS / 2f, ROWS / 2f));
                        t = 1f - (dist / maxDist); break;
                }
                int tierIndex = Mathf.RoundToInt(t * (palette.Count - 1));
                brick.Setup(palette.GetTier(tierIndex));
            }
        }
    }

    private void SetupSymmetry(BrickChunkSO A, BrickChunkSO B, ref BrickChunkSO[] tmpl, ref bool[] fx, ref bool[] fy)
    {
        switch (symmetryMode)
        {
            case SymmetryType.MirrorHorizontal:
                tmpl[0] = A; fx[0] = false; fy[0] = false;
                tmpl[1] = A; fx[1] = true; fy[1] = false;
                tmpl[2] = B; fx[2] = false; fy[2] = false;
                tmpl[3] = B; fx[3] = true; fy[3] = false;
                break;
            case SymmetryType.MirrorVertical:
                tmpl[0] = A; fx[0] = false; fy[0] = false;
                tmpl[1] = B; fx[1] = false; fy[1] = false;
                tmpl[2] = A; fx[2] = false; fy[2] = true; 
                tmpl[3] = B; fx[3] = false; fy[3] = true; 
                break;
            case SymmetryType.MirrorBoth:
                tmpl[0] = A; fx[0] = false; fy[0] = false;
                tmpl[1] = A; fx[1] = true; fy[1] = false;
                tmpl[2] = A; fx[2] = false; fy[2] = true;
                tmpl[3] = A; fx[3] = true; fy[3] = true; 
                break;
            case SymmetryType.Chaos: break;
        }
    }

    private void ReportToGameManager()
    {
        if (!Application.isPlaying) return;
        if (GameManager.Instance == null) return;
        int destroyableCount = 0;
        foreach (var brick in _spawnedGrid)
        {
            if (brick != null && !brick.IsIndestructible) destroyableCount++;
        }
        GameManager.Instance.SetBrickCount(destroyableCount);
    }

    private void CleanupOldLevel()
    {
        System.Array.Clear(_spawnedGrid, 0, _spawnedGrid.Length);
        if (Application.isPlaying) brickPool.ReturnAllActiveBricks();
        else brickPool.DestroyAllBricksEditor();
    }

    private bool ValidateReferences()
    {
        if (brickPool == null || palette == null || defaultBrickType == null)
        {
            Debug.LogError("LevelManager: Missing references!");
            return false;
        }
        return true;
    }

#if UNITY_EDITOR
    private List<T> LoadAssets<T>(string folderPath) where T : Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) assets.Add(asset);
        }
        return assets;
    }
#endif
}