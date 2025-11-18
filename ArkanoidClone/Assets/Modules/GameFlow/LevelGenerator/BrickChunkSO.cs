using MiniIT.BRICK;
using MiniIT.CORE;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.LEVELS
{
    [CreateAssetMenu(fileName = "NewBrickChunk", menuName = "Arkanoid/Generation/Brick Chunk")]
    public class BrickChunkSO : ScriptableObject
    {
        [System.Serializable]
        public struct BrickData
        {
            [Tooltip("Local position within the chunk (0..5, 0..5).")]
            public Vector2Int position;

            [Tooltip("The type of brick at this position.")]
            public BrickTypeSO type;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [SerializeField]
        public int width = 6;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        public int height = 6;

        [BoxGroup("DATA")]
        [SerializeField]
        public List<BrickData> bricks = new List<BrickData>();
    }
}