using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using static BrickChunkSO;
using miniit.CORE;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniIT.LEVELS
{
    public class LevelManager : MonoBehaviour
    {
        // 1. Constants & Enums
        private const int COLS = 12;
        private const int ROWS = 12;

        public enum SymmetryType
        {
            MirrorHorizontal,
            MirrorVertical,
            MirrorBoth,
            Chaos
        }

        public enum PaintPattern
        {
            BottomToTop,
            LeftToRight,
            ZebraHorizontal,
            CenterOut
        }

        // 2. Serialized Fields
        [BoxGroup("DEPENDENCIES")]
        [Required]
        [SerializeField]
        public BrickPool brickPool = null;

        [BoxGroup("GRID SETTINGS")]
        [SerializeField]
        private float brickWidth = 0.69333f;

        [BoxGroup("GRID SETTINGS")]
        [SerializeField]
        private float brickHeight = 0.34666f;

        [BoxGroup("PHASE 1: GEOMETRY")]
        [SerializeField]
        public string geometryPath = "Assets/Modules/Data/Chunks/Geometry";

        [BoxGroup("PHASE 1: GEOMETRY")]
        [SerializeField]
        private System.Collections.Generic.List<BrickChunkSO> geometryChunks = null;

        [BoxGroup("PHASE 1: GEOMETRY")]
        [Range(1, 4)]
        [SerializeField]
        private int geometryTemplateCount = 2;

        [BoxGroup("PHASE 1: GEOMETRY")]
        [SerializeField]
        private SymmetryType symmetryMode = SymmetryType.MirrorHorizontal;

        [BoxGroup("PHASE 1: GEOMETRY")]
        [SerializeField]
        private BrickTypeSO defaultBrickType = null;

        [BoxGroup("PHASE 2: PAINTING")]
        [SerializeField]
        private BrickPaletteSO palette = null;

        [BoxGroup("PHASE 2: PAINTING")]
        [SerializeField]
        private PaintPattern paintPattern = PaintPattern.BottomToTop;

        [BoxGroup("PHASE 3: OBSTACLES")]
        [SerializeField]
        private bool enableObstacles = true;

        [BoxGroup("PHASE 3: OBSTACLES")]
        [Range(1, 4)]
        [SerializeField]
        private int obstacleTemplateCount = 1;

        [BoxGroup("PHASE 3: OBSTACLES")]
        [SerializeField]
        public string obstaclesPath = "Assets/Modules/Data/Chunks/Obstacles";

        [BoxGroup("PHASE 3: OBSTACLES")]
        [SerializeField]
        private System.Collections.Generic.List<BrickChunkSO> obstacleChunks = null;

        [BoxGroup("PHASE 3: OBSTACLES")]
        [SerializeField]
        private BrickTypeSO indestructibleType = null;

        // 3. Non-Serialized Privates
        private System.Random prng = null;
        private Brick[,] spawnedGrid = new Brick[COLS, ROWS];

        // 4. Properties
        public static LevelManager Instance
        {
            get;
            private set;
        }

        // 5. Unity Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            // Sorting is critical for determinism
            if (geometryChunks != null)
            {
                geometryChunks.Sort((a, b) => a.name.CompareTo(b.name));
            }

            if (obstacleChunks != null)
            {
                obstacleChunks.Sort((a, b) => a.name.CompareTo(b.name));
            }
        }

        // 6. Public Methods
        public void GenerateLevelBySeed(int seed)
        {
            // LOG 1: Input data check
            int gCount = geometryChunks != null ? geometryChunks.Count : 0;
            int oCount = obstacleChunks != null ? obstacleChunks.Count : 0;

            prng = new System.Random(seed);

            BuildChaosLevel();
        }

        public void SetLevelVisibility(bool isVisible)
        {
            foreach (Brick brick in spawnedGrid)
            {
                if (brick != null)
                {
                    brick.gameObject.SetActive(isVisible);
                }
            }
        }

        [Button("Build Chaos Level")]
        public void BuildChaosLevel()
        {
            symmetryMode = SymmetryType.Chaos;
            BuildLevel();
        }

        [Button("Build Level")]
        public void BuildLevel()
        {
            if (!ValidateReferences())
            {
                return;
            }

            CleanupOldLevel();

            if (symmetryMode == SymmetryType.Chaos)
            {
                System.Array paintValues = System.Enum.GetValues(typeof(PaintPattern));
                paintPattern = (PaintPattern)paintValues.GetValue(GetPRNG().Next(0, paintValues.Length));

                // LOG 2: Selected pattern
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
            // LOG: Finish
        }

        // 7. Private Methods
        private System.Random GetPRNG()
        {
            if (prng == null)
            {
                int seed = (GameInstance.Instance != null) ? GameInstance.Instance.CurrentLevelSeed : 12345;
                prng = new System.Random(seed);
                Debug.LogWarning($"[LevelManager] prng auto-init with seed: {seed}");
            }
            return prng;
        }

        [Button]
        private void LoadChunksFromFolders()
        {
#if UNITY_EDITOR
            geometryChunks = LoadAssets<BrickChunkSO>(geometryPath);
            obstacleChunks = LoadAssets<BrickChunkSO>(obstaclesPath);
            EditorUtility.SetDirty(this);
#endif
        }

        // --- GENERATION LOGIC ---

        private System.Collections.Generic.List<BrickChunkSO> GetDistributedTemplates(System.Collections.Generic.List<BrickChunkSO> sourceList, int count)
        {
            int safeCount = Mathf.Min(count, sourceList.Count);

            // Logging the selection process to see the random "shift"
            // Using GetPRNG()
            System.Collections.Generic.List<BrickChunkSO> uniqueSelection = sourceList.OrderBy(x => GetPRNG().Next()).Take(safeCount).ToList();

            // LOG 3: Selected templates from pool
            string selectedNames = string.Join(", ", uniqueSelection.Select(c => c.name));

            System.Collections.Generic.List<BrickChunkSO> finalDistribution = new System.Collections.Generic.List<BrickChunkSO>();

            for (int i = 0; i < 4; i++)
            {
                finalDistribution.Add(uniqueSelection[i % uniqueSelection.Count]);
            }

            return finalDistribution.OrderBy(x => GetPRNG().Next()).ToList();
        }

        private void GenerateChaosGeometry()
        {
            if (geometryChunks.Count == 0)
            {
                return;
            }

            System.Collections.Generic.List<BrickChunkSO> templates = GetDistributedTemplates(geometryChunks, geometryTemplateCount);

            Vector2 currentCenter = transform.position;
            float totalW = COLS * brickWidth;
            float totalH = ROWS * brickHeight;
            Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

            System.Collections.Generic.List<Vector2Int> quadrants = new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(0, 0),
                new Vector2Int(6, 0),
                new Vector2Int(0, 6),
                new Vector2Int(6, 6)
            };

            for (int i = 0; i < 4; i++)
            {
                BrickChunkSO chunk = templates[i];
                Vector2Int offset = quadrants[i];

                bool flipX = GetPRNG().NextDouble() > 0.5;
                bool flipY = GetPRNG().NextDouble() > 0.5;

                // LOG 4: Detailed info for each quadrant
                SpawnQuadrant(chunk, offset.x, offset.y, flipX, flipY, startPos);
            }
        }

        private void OverlayObstaclesLayer()
        {
            if (obstacleChunks == null || obstacleChunks.Count == 0)
            {
                return;
            }

            System.Collections.Generic.List<BrickChunkSO> templates = GetDistributedTemplates(obstacleChunks, obstacleTemplateCount);

            Vector2 currentCenter = transform.position;
            float totalW = COLS * brickWidth;
            float totalH = ROWS * brickHeight;
            Vector2 startPos = new Vector2(currentCenter.x - (totalW / 2f), currentCenter.y + (totalH / 2f));

            System.Collections.Generic.List<Vector2Int> quadrants = new System.Collections.Generic.List<Vector2Int>
            {
                new Vector2Int(0, 0),
                new Vector2Int(6, 0),
                new Vector2Int(0, 6),
                new Vector2Int(6, 6)
            };

            for (int i = 0; i < 4; i++)
            {
                BrickChunkSO chunk = templates[i];
                Vector2Int offset = quadrants[i];

                bool flipX = GetPRNG().NextDouble() > 0.5;
                bool flipY = GetPRNG().NextDouble() > 0.5;

                ApplyObstacleQuadrant(chunk, offset.x, offset.y, flipX, flipY, startPos);
            }
        }

        // --- SYMMETRIC GENERATION ---

        private void GenerateSymmetricGeometry()
        {
            if (geometryChunks.Count == 0)
            {
                return;
            }

            BrickChunkSO chunkA = geometryChunks[GetPRNG().Next(0, geometryChunks.Count)];
            BrickChunkSO chunkB = geometryChunks[GetPRNG().Next(0, geometryChunks.Count)];

            if (geometryTemplateCount == 1)
            {
                chunkB = chunkA;
            }

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

        // --- HELPERS ---

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

        private void SpawnQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY, Vector2 startPos)
        {
            if (chunk == null)
            {
                return;
            }

            foreach (BrickData data in chunk.bricks)
            {
                int cx = data.position.x;
                int cy = data.position.y;

                if (flipX)
                {
                    cx = (chunk.width - 1) - cx;
                }

                if (flipY)
                {
                    cy = (chunk.height - 1) - cy;
                }

                int col = offsetX + cx;
                int row = offsetY + (chunk.height - 1 - cy);

                Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();
                newBrick.Setup(defaultBrickType);

                float xPos = startPos.x + (col * brickWidth);
                float yPos = startPos.y - (row * brickHeight);

                newBrick.transform.position = new Vector2(xPos, yPos);

                if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
                {
                    spawnedGrid[col, row] = newBrick;
                }
            }
        }

        private void ApplyObstacleQuadrant(BrickChunkSO chunk, int offsetX, int offsetY, bool flipX, bool flipY, Vector2 startPos)
        {
            foreach (BrickData data in chunk.bricks)
            {
                int cx = data.position.x;
                int cy = data.position.y;

                if (flipX)
                {
                    cx = (chunk.width - 1) - cx;
                }

                if (flipY)
                {
                    cy = (chunk.height - 1) - cy;
                }

                int col = offsetX + cx;
                int row = offsetY + (chunk.height - 1 - cy);

                if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
                {
                    Brick existingBrick = spawnedGrid[col, row];

                    if (existingBrick != null)
                    {
                        existingBrick.Setup(indestructibleType);
                    }
                    else
                    {
                        Brick newBrick = Application.isPlaying ? brickPool.GetBrick() : brickPool.GetBrickEditor();
                        newBrick.Setup(indestructibleType);

                        float xPos = startPos.x + (col * brickWidth);
                        float yPos = startPos.y - (row * brickHeight);

                        newBrick.transform.position = new Vector2(xPos, yPos);
                        spawnedGrid[col, row] = newBrick;
                    }
                }
            }
        }

        private void PaintBricksLayer()
        {
            if (palette == null || palette.Count == 0)
            {
                return;
            }

            for (int y = 0; y < ROWS; y++)
            {
                for (int x = 0; x < COLS; x++)
                {
                    Brick brick = spawnedGrid[x, y];

                    if (brick == null)
                    {
                        continue;
                    }

                    float t = 0f;
                    switch (paintPattern)
                    {
                        case PaintPattern.BottomToTop:
                            t = (float)(ROWS - 1 - y) / (ROWS - 1);
                            break;
                        case PaintPattern.LeftToRight:
                            t = (float)x / (COLS - 1);
                            break;
                        case PaintPattern.ZebraHorizontal:
                            t = (y % 2 == 0) ? 0f : 1f;
                            break;
                        case PaintPattern.CenterOut:
                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(COLS / 2f, ROWS / 2f));
                            float maxDist = Vector2.Distance(Vector2.zero, new Vector2(COLS / 2f, ROWS / 2f));
                            t = 1f - (dist / maxDist);
                            break;
                    }

                    int tierIndex = Mathf.RoundToInt(t * (palette.Count - 1));
                    brick.Setup(palette.GetTier(tierIndex));
                }
            }
        }

        private void ReportToGameManager()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            int destroyableCount = 0;
            foreach (Brick brick in spawnedGrid)
            {
                if (brick != null && !brick.BrickType.isIndestructible)
                {
                    destroyableCount++;
                }
            }

            GameManager.Instance.SetBrickCount(destroyableCount);
        }

        private void CleanupOldLevel()
        {
            System.Array.Clear(spawnedGrid, 0, spawnedGrid.Length);

            if (Application.isPlaying)
            {
                brickPool.ReturnAllActiveBricks();
            }
            else
            {
                brickPool.DestroyAllBricksEditor();
            }
        }

        private bool ValidateReferences()
        {
            if (brickPool == null || palette == null || defaultBrickType == null)
            {
                Debug.LogError("Missing references!");
                return false;
            }
            return true;
        }

#if UNITY_EDITOR
        private System.Collections.Generic.List<T> LoadAssets<T>(string folderPath) where T : Object
        {
            System.Collections.Generic.List<T> assets = new System.Collections.Generic.List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }
#endif
    }
}