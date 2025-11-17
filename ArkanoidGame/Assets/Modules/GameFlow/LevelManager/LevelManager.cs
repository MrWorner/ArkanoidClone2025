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
    [FolderPath] public string geometryPath = "Assets/Modules/Data/Chunks/Geometry";
    [SerializeField] private List<BrickChunkSO> geometryChunks;

    public enum SymmetryType { MirrorHorizontal, MirrorVertical, MirrorBoth, Chaos }
    [SerializeField] private SymmetryType symmetryMode = SymmetryType.MirrorHorizontal;

    [SerializeField] private BrickTypeSO defaultBrickType;

    // ========================================================================
    // PHASE 2: PAINTING
    // ========================================================================
    [Title("Phase 2: Painting")]
    [InfoBox("Внимание: В Палитре НЕ должно быть неубиваемых блоков!")]
    [SerializeField] private BrickPaletteSO palette;

    public enum PaintPattern { BottomToTop, LeftToRight, ZebraHorizontal, CenterOut, RandomNoise }
    [SerializeField] private PaintPattern paintPattern = PaintPattern.BottomToTop;

    // ========================================================================
    // PHASE 3: OBSTACLES
    // ========================================================================
    [Title("Phase 3: Obstacles")]

    [Tooltip("Если включено - накладывает слой препятствий поверх покраски.")]
    [SerializeField] private bool enableObstacles = true;

    [FolderPath] public string obstaclesPath = "Assets/Modules/Data/Chunks/Obstacles";
    [SerializeField] private List<BrickChunkSO> obstacleChunks;

    [SerializeField] private BrickTypeSO indestructibleType;

    // УБРАЛИ obstacleChance

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
        symmetryMode = SymmetryType.Chaos;

        var paintValues = System.Enum.GetValues(typeof(PaintPattern));
        paintPattern = (PaintPattern)paintValues.GetValue(Random.Range(0, paintValues.Length));

        // Рандомим включение препятствий (50/50)
        // = Random.value > 0.5f;

        BuildLevel();
    }

    [Button("Построить")]
    public void BuildLevel()
    {
        if (!ValidateReferences()) return;
        CleanupOldLevel();

        if (symmetryMode == SymmetryType.Chaos) GenerateChaosGeometry();
        else GenerateSymmetricGeometry();

        PaintBricksLayer();

        // Четкая логика: если галочка стоит - накладываем.
        if (enableObstacles)
        {
            OverlayObstaclesLayer();
        }

        ReportToGameManager();
    }

    // ... (GenerateChaosGeometry, GenerateSymmetricGeometry, SpawnQuadrant - БЕЗ ИЗМЕНЕНИЙ) ...
    // Я скопирую только измененные методы для краткости, остальное у вас есть.

    // ========================================================================
    // ЛОГИКА ГЕНЕРАЦИИ
    // ========================================================================

    private void GenerateChaosGeometry()
    {
        if (geometryChunks.Count < 2)
        {
            Debug.LogError("Not enough geometry chunks! Need at least 2.");
            return;
        }

        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        // 1. Выбираем 2 случайных шаблона
        BrickChunkSO chunkA = geometryChunks[Random.Range(0, geometryChunks.Count)];
        BrickChunkSO chunkB = geometryChunks[Random.Range(0, geometryChunks.Count)];

        // 2. Создаем список для распределения (2 раза А, 2 раза Б)
        List<BrickChunkSO> distribution = new List<BrickChunkSO> { chunkA, chunkA, chunkB, chunkB };

        // 3. Перемешиваем список (Shuffle)
        distribution = distribution.OrderBy(x => Random.value).ToList();

        // 4. Список углов
        List<Vector2Int> quadrants = new List<Vector2Int>
        {
            new Vector2Int(0, 0), // TL
            new Vector2Int(6, 0), // TR
            new Vector2Int(0, 6), // BL
            new Vector2Int(6, 6)  // BR
        };

        Debug.Log($"<b>[LevelGen]</b> CHAOS 2-Template Mode: <color=cyan>{chunkA.name}</color> & <color=cyan>{chunkB.name}</color>");

        // 5. Раздаем
        for (int i = 0; i < 4; i++)
        {
            BrickChunkSO selectedChunk = distribution[i];
            Vector2Int offset = quadrants[i];

            // Случайное вращение
            bool flipX = Random.value > 0.5f;
            bool flipY = Random.value > 0.5f;

            SpawnQuadrant(selectedChunk, offset.x, offset.y, flipX, flipY, startPos);

            Debug.Log($"-> Quad ({offset.x},{offset.y}): {selectedChunk.name} | Flip: {flipX}/{flipY}");
        }
    }

    private void GenerateSymmetricGeometry()
    {
        if (geometryChunks.Count == 0) return;

        BrickChunkSO chunkA = geometryChunks[Random.Range(0, geometryChunks.Count)];
        BrickChunkSO chunkB = geometryChunks[Random.Range(0, geometryChunks.Count)];

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

    private void SpawnQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY, Vector2 startPos)
    {
        if (chunk == null) return;
        foreach (var data in chunk.bricks)
        {
            int cx = data.position.x;
            int cy = data.position.y;
            if (flipX) cx = (chunk.width - 1) - cx;
            if (flipY) cy = (chunk.height - 1) - cy;

            int col = offsetX + cx;
            int row = offsetY + (chunk.height - 1 - cy);

            Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();
            newBrick.Setup(defaultBrickType);

            float xPos = startPos.x + (col * brickWidth);
            float yPos = startPos.y - (row * brickHeight);
            newBrick.transform.position = new Vector2(xPos, yPos);

            if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
                _spawnedGrid[col, row] = newBrick;
        }
    }

    // --- ИЗМЕНЕННЫЙ МЕТОД ---
    private void OverlayObstaclesLayer()
    {
        // Больше НЕТ проверки на obstacleChance
        if (obstacleChunks == null || obstacleChunks.Count == 0) return;

        BrickChunkSO obsChunk = obstacleChunks[Random.Range(0, obstacleChunks.Count)];

        // Накладываем препятствия
        ApplyObstacleQuadrant(obsChunk, 0, 0, false, false);
        ApplyObstacleQuadrant(obsChunk, 6, 0, true, false);
        ApplyObstacleQuadrant(obsChunk, 0, 6, false, true);
        ApplyObstacleQuadrant(obsChunk, 6, 6, true, true);
    }
    // ------------------------

    // ... (Остальные методы PaintBricksLayer, ApplyObstacleQuadrant, SetupSymmetry, ReportToGameManager, CleanupOldLevel, ValidateReferences, LoadAssets - БЕЗ ИЗМЕНЕНИЙ) ...

    // (Вставьте их сюда из предыдущего скрипта)

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
                    case PaintPattern.RandomNoise: t = Random.value; break;
                }
                int tierIndex = Mathf.RoundToInt(t * (palette.Count - 1));
                brick.Setup(palette.GetTier(tierIndex));
            }
        }
    }

    private void ApplyObstacleQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY)
    {
        foreach (var data in chunk.bricks)
        {
            int cx = data.position.x;
            int cy = data.position.y;
            if (flipX) cx = (chunk.width - 1) - cx;
            if (flipY) cy = (chunk.height - 1) - cy;
            int col = offsetX + cx;
            int row = offsetY + (chunk.height - 1 - cy);
            if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
            {
                Brick existingBrick = _spawnedGrid[col, row];
                if (existingBrick != null) existingBrick.Setup(indestructibleType);
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
            case SymmetryType.Chaos:
                break;
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