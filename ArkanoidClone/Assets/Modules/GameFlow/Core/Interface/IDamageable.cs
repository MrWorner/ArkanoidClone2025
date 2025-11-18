namespace MiniIT.CORE
{
    /// <summary>
    /// Interface for any object that can take damage (Bricks, Enemies, etc.).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage to the object.
        /// </summary>
        /// <param name="damageAmount">The amount of damage to apply.</param>
        void TakeDamage(int damageAmount);
    }
}