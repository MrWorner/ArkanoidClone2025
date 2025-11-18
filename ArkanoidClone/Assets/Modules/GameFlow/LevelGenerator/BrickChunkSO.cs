using UnityEngine;
using System.Collections.Generic;
using MiniIT.BRICK;

[CreateAssetMenu(fileName = "NewBrickChunk", menuName = "Arkanoid/Generation/Brick Chunk")]
public class BrickChunkSO : ScriptableObject
{
    [System.Serializable]
    public struct BrickData
    {
        public Vector2Int position; // Локальная позиция внутри чанка (0..5, 0..5)
        public BrickTypeSO type;
    }

    public int width = 6;
    public int height = 6;
    public List<BrickData> bricks = new List<BrickData>();
}