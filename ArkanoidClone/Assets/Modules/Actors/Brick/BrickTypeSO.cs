using UnityEngine;
using NaughtyAttributes;

namespace MiniIT.BRICK
{
    [CreateAssetMenu(fileName = "NewBrickType", menuName = "Arkanoid/Brick Type")]
    public class BrickTypeSO : ScriptableObject
    {
        // ========================================================================
        // --- VISUAL SETTINGS ---
        // ========================================================================

        [BoxGroup("VISUAL")]
        [Tooltip("The sprite used for this brick.")]
        [SerializeField]
        public Sprite sprite;

        [BoxGroup("VISUAL")]
        [Tooltip("Tint color for the sprite.")]
        [SerializeField]
        public Color color = Color.white;

        // ========================================================================
        // --- GAMEPLAY SETTINGS ---
        // ========================================================================

        [BoxGroup("GAMEPLAY")]
        [Tooltip("Score points awarded for destroying this brick.")]
        [SerializeField]
        public int points;

        [BoxGroup("GAMEPLAY")]
        [Tooltip("If true, the brick cannot be destroyed.")]
        [SerializeField]
        public bool isIndestructible;

        [BoxGroup("GAMEPLAY")]
        [Tooltip("Hit points required to destroy (Default = 1).")]
        [SerializeField]
        public int health = 1;
    }
}