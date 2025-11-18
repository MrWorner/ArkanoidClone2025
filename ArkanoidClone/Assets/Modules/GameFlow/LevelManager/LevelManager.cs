using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
// ВАЖНО: Нам нужен System для System.Random, но чтобы не было конфликта с UnityEngine.Random
// мы будем использовать полное имя System.Random или псевдоним, но тут проще так:
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // --- ГЛАВНОЕ ИЗМЕНЕНИЕ: ЛИЧНЫЙ ГЕНЕРАТОР ---
    // Никакой другой скрипт в Unity не сможет повлиять на этот генератор.
    private System.Random _prng;
    // ------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        if (geometryChunks != null) geometryChunks.Sort((a, b) => a.name.CompareTo(b.name));
        if (obstacleChunks != null) obstacleChunks.Sort((a, b) => a.name.CompareTo(b.name));
    }

    [Required] public BrickPool brickPool;

    [Header("Grid Settings")]
    private const int COLS = 12;
    private const int ROWS = 12;
    [SerializeField] private float brickWidth = 0.69333f;
    [SerializeField] private float brickHeight = 0.34666f;

    // ========================================================================
    // PHASE 1: GEOMETRY
    // ========================================================================
    public string geometryPath = "Assets/Modules/Data/Chunks/Geometry";
    [SerializeField] private List<BrickChunkSO> geometryChunks;

    [Range(1, 4)]
    [SerializeField] private int geometryTemplateCount = 2;

    public enum SymmetryType { MirrorHorizontal, MirrorVertical, MirrorBoth, Chaos }
    [SerializeField] private SymmetryType symmetryMode = SymmetryType.MirrorHorizontal;

    [SerializeField] private BrickTypeSO defaultBrickType;

    // ========================================================================
    // PHASE 2: PAINTING
    // ========================================================================
    [SerializeField] private BrickPaletteSO palette;
    public enum PaintPattern { BottomToTop, LeftToRight, ZebraHorizontal, CenterOut }
    [SerializeField] private PaintPattern paintPattern = PaintPattern.BottomToTop;

    // ========================================================================
    // PHASE 3: OBSTACLES
    // ========================================================================
    [SerializeField] private bool enableObstacles = true;
    [Range(1, 4)]
    [SerializeField] private int obstacleTemplateCount = 1;
    public string obstaclesPath = "Assets/Modules/Data/Chunks/Obstacles";
    [SerializeField] private List<BrickChunkSO> obstacleChunks;
    [SerializeField] private BrickTypeSO indestructibleType;

    private Brick[,] _spawnedGrid = new Brick[COLS, ROWS];

    // ========================================================================
    // INITIALIZATION & PUBLIC METHODS
    // ========================================================================

    public void GenerateLevelBySeed(int seed)
    {
        Debug.Log($"<b>[LevelManager]</b> Generating with System.Random Seed: {seed}");

        // Инициализируем наш ЛИЧНЫЙ генератор. 
        // Теперь глобальный UnityEngine.Random.InitState нам не нужен и не помешает.
        _prng = new System.Random(seed);

        BuildChaosLevel();
    }

    public void SetLevelVisibility(bool isVisible)
    {
        foreach (var brick in _spawnedGrid)
        {
            if (brick != null) brick.gameObject.SetActive(isVisible);
        }
    }

    // ========================================================================
    // BUTTONS & EDITOR
    // ========================================================================

    [Button]
    private void LoadChunksFromFolders()
    {
#if UNITY_EDITOR
        geometryChunks = LoadAssets<BrickChunkSO>(geometryPath);
        obstacleChunks = LoadAssets<BrickChunkSO>(obstaclesPath);
        EditorUtility.SetDirty(this);
        Debug.Log($"[LevelManager] Loaded {geometryChunks.Count} Geometry & {obstacleChunks.Count} Obstacles.");
#endif
    }

    [Button]
    public void BuildChaosLevel()
    {
        symmetryMode = SymmetryType.Chaos;
        BuildLevel();
    }

    [Button("Построить")]
    public void BuildLevel()
    {
        if (!ValidateReferences()) return;

        // Если запускаем кнопку из редактора (не через GenerateLevelBySeed), 
        // _prng может быть null. Создадим временный рандомный.
        if (_prng == null) _prng = new System.Random();

        CleanupOldLevel();

        if (symmetryMode == SymmetryType.Chaos)
        {
            var paintValues = System.Enum.GetValues(typeof(PaintPattern));
            // ЗАМЕНА: Random.Range -> _prng.Next
            paintPattern = (PaintPattern)paintValues.GetValue(_prng.Next(0, paintValues.Length));

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
    // GENERATION LOGIC (Using _prng)
    // ========================================================================

    private List<BrickChunkSO> GetDistributedTemplates(List<BrickChunkSO> sourceList, int count)
    {
        int safeCount = Mathf.Min(count, sourceList.Count);

        // ЗАМЕНА: Random.value -> _prng.Next() (для сортировки это даже надежнее)
        List<BrickChunkSO> uniqueSelection = sourceList.OrderBy(x => _prng.Next()).Take(safeCount).ToList();

        List<BrickChunkSO> finalDistribution = new List<BrickChunkSO>();

        for (int i = 0; i < 4; i++)
        {
            finalDistribution.Add(uniqueSelection[i % uniqueSelection.Count]);
        }

        // ЗАМЕНА: Random.value -> _prng.Next()
        return finalDistribution.OrderBy(x => _prng.Next()).ToList();
    }

    private void GenerateChaosGeometry()
    {
        if (geometryChunks.Count == 0) return;

        List<BrickChunkSO> templates = GetDistributedTemplates(geometryChunks, geometryTemplateCount);

        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        List<Vector2Int> quadrants = new List<Vector2Int>
        { new Vector2Int(0, 0), new Vector2Int(6, 0), new Vector2Int(0, 6), new Vector2Int(6, 6) };

        for (int i = 0; i < 4; i++)
        {
            BrickChunkSO chunk = templates[i];
            Vector2Int offset = quadrants[i];

            // ЗАМЕНА: Random.value -> _prng.NextDouble()
            bool flipX = _prng.NextDouble() > 0.5;
            bool flipY = _prng.NextDouble() > 0.5;

            SpawnQuadrant(chunk, offset.x, offset.y, flipX, flipY, startPos);
        }
    }

    private void OverlayObstaclesLayer()
    {
        if (obstacleChunks == null || obstacleChunks.Count == 0) return;

        List<BrickChunkSO> templates = GetDistributedTemplates(obstacleChunks, obstacleTemplateCount);

        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        List<Vector2Int> quadrants = new List<Vector2Int>
        { new Vector2Int(0, 0), new Vector2Int(6, 0), new Vector2Int(0, 6), new Vector2Int(6, 6) };

        for (int i = 0; i < 4; i++)
        {
            BrickChunkSO chunk = templates[i];
            Vector2Int offset = quadrants[i];

            // ЗАМЕНА: Random.value -> _prng.NextDouble()
            bool flipX = _prng.NextDouble() > 0.5;
            bool flipY = _prng.NextDouble() > 0.5;

            ApplyObstacleQuadrant(chunk, offset.x, offset.y, flipX, flipY, startPos);
        }
    }

    private void GenerateSymmetricGeometry()
    {
        if (geometryChunks.Count == 0) return;

        // ЗАМЕНА: Random.Range -> _prng.Next
        BrickChunkSO chunkA = geometryChunks[_prng.Next(0, geometryChunks.Count)];
        BrickChunkSO chunkB = geometryChunks[_prng.Next(0, geometryChunks.Count)];

        if (geometryTemplateCount == 1) chunkB = chunkA;

        BrickChunkSO[] quads = new BrickChunkSO[4];
        bool[] fx = new bool[4];
        bool[] fy = new bool[4];

        SetupSymmetry(chunkA, chunkB, ref quads, ref fx, ref fy);
        // ... спавн квадрантов (код без изменений) ...
        Vector2 currentCenter = transform.position;
        float totalW = COLS * brickWidth;
        float totalH = ROWS * brickHeight;
        Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

        SpawnQuadrant(quads[0], 0, 0, fx[0], fy[0], startPos);
        SpawnQuadrant(quads[1], 6, 0, fx[1], fy[1], startPos);
        SpawnQuadrant(quads[2], 0, 6, fx[2], fy[2], startPos);
        SpawnQuadrant(quads[3], 6, 6, fx[3], fy[3], startPos);
    }

    // ... Остальные методы (SpawnQuadrant, PaintBricksLayer и т.д.) 
    // НЕ ИСПОЛЬЗУЮТ Random, поэтому их можно оставить без изменений,
    // либо скопировать из прошлого файла. Главное - методы выше.

    // ВНИМАНИЕ: Вставь сюда методы SpawnQuadrant, ApplyObstacleQuadrant, PaintBricksLayer, SetupSymmetry, ReportToGameManager, CleanupOldLevel, ValidateReferences 
    // из твоего предыдущего рабочего кода. Я их сократил, чтобы не дублировать простыню текста.

    // --- Вспомогательные методы (копипаст) ---
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

    private void ApplyObstacleQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY, Vector2 startPos)
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
                else
                {
                    Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();
                    newBrick.Setup(indestructibleType);
                    float xPos = startPos.x + (col * brickWidth);
                    float yPos = startPos.y - (row * brickHeight);
                    newBrick.transform.position = new Vector2(xPos, yPos);
                    _spawnedGrid[col, row] = newBrick;
                }
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
            case SymmetryType.MirrorHorizontal: tmpl[0] = A; fx[0] = false; fy[0] = false; tmpl[1] = A; fx[1] = true; fy[1] = false; tmpl[2] = B; fx[2] = false; fy[2] = false; tmpl[3] = B; fx[3] = true; fy[3] = false; break;
            case SymmetryType.MirrorVertical: tmpl[0] = A; fx[0] = false; fy[0] = false; tmpl[1] = B; fx[1] = false; fy[1] = false; tmpl[2] = A; fx[2] = false; fy[2] = true; tmpl[3] = B; fx[3] = false; fy[3] = true; break;
            case SymmetryType.MirrorBoth: tmpl[0] = A; fx[0] = false; fy[0] = false; tmpl[1] = A; fx[1] = true; fy[1] = false; tmpl[2] = A; fx[2] = false; fy[2] = true; tmpl[3] = A; fx[3] = true; fy[3] = true; break;
            case SymmetryType.Chaos: break;
        }
    }

    private void ReportToGameManager()
    {
        if (!Application.isPlaying) return;
        if (GameManager.Instance == null) return;
        int destroyableCount = 0;
        foreach (var brick in _spawnedGrid) if (brick != null && !brick.BrickType.isIndestructible) destroyableCount++;
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
        if (brickPool == null || palette == null || defaultBrickType == null) { Debug.LogError("Missing references!"); return false; }
        return true;
    }

#if UNITY_EDITOR
    private List<T> LoadAssets<T>(string folderPath) where T : Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });
        foreach (string guid in guids) { string path = AssetDatabase.GUIDToAssetPath(guid); T asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) assets.Add(asset); }
        return assets;
    }
#endif
}