using MiniIT.BRICK;
using MiniIT.CORE;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiniIT.LEVELS
{
    [CreateAssetMenu(fileName = "BrickTextMap", menuName = "Arkanoid/Generation/Brick Text Map")]
    public class BrickTextMapSO : ScriptableObject
    {
        [System.Serializable]
        public struct CharToBrickMapping
        {
            public char symbol;
            public BrickTypeSO brickType;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("MAPPINGS")]
        [ReorderableList]
        [SerializeField]
        public List<CharToBrickMapping> mappings;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Finds the BrickType associated with the specific character symbol.
        /// </summary>
        public BrickTypeSO GetBrickType(char symbol)
        {
            CharToBrickMapping map = mappings.FirstOrDefault(m => m.symbol == symbol);
            // If symbol not found (or it is whitespace), returns null
            return map.brickType;
        }
    }
}