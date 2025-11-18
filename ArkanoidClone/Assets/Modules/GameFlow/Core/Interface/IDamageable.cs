/// <summary>
/// Интерфейс для всего, что может получать урон (кирпичи, враги и т.д.)
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Метод для получения урона
    /// </summary>
    /// <param name="damageAmount">Количество урона</param>
    void TakeDamage(int damageAmount);
}