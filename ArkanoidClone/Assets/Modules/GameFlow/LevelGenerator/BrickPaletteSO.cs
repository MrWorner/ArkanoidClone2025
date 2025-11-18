using MiniIT.BRICK;
using MiniIT.CORE;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.LEVELS
{
    [CreateAssetMenu(fileName = "NewBrickPalette", menuName = "Arkanoid/Generation/Brick Palette")]
    public class BrickPaletteSO : ScriptableObject
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Tooltip("List of brick types from weakest (0) to strongest.")]
        [SerializeField]
        public List<BrickTypeSO> tiers;

        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public int Count
        {
            get
            {
                return tiers.Count;
            }
        }

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Returns a brick type by index (tier). Clamps to the last element if index exceeds count.
        /// </summary>
        public BrickTypeSO GetTier(int index)
        {
            if (tiers.Count == 0)
            {
                return null;
            }

            // Clamping: if index is larger than list, take the last one (most expensive)
            return tiers[Mathf.Clamp(index, 0, tiers.Count - 1)];
        }
    }
}