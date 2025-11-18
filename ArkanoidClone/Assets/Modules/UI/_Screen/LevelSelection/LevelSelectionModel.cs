using System;

namespace MiniIT.UI
{
    public class LevelSelectionModel
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public int CurrentLevel { get; private set; } = 1;
        public int MinLevel { get; } = 1;
        public int MaxLevel { get; } = 9999;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void SetLevel(int level)
        {
            CurrentLevel = Math.Clamp(level, MinLevel, MaxLevel);
        }
    }
}