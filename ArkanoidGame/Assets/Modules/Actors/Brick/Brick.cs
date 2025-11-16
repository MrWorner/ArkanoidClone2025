using UnityEngine;

public class Brick : MonoBehaviour, IDamageable // Реализуем наш интерфейс
{
    // Ссылка на "дом" (пул), куда кирпич вернется
    private BrickPool _pool;

    /// <summary>
    /// Вызывается пулом, когда кирпич создается.
    /// Дает кирпичу ссылку на его "хозяина".
    /// </summary>
    public void Init(BrickPool ownerPool)
    {
        _pool = ownerPool;
    }

    /// <summary>
    /// Это - реализация метода из IDamageable.
    /// Вызывается, когда мяч попадает в кирпич.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        // (Позже здесь можно добавить здоровье: if (health > 0) health -= damageAmount;)
        // А пока любой урон "убивает" кирпич.

        // Вместо Destroy(gameObject), мы ПРОСИМ ПУЛ 
        // забрать нас обратно.
        if (_pool != null)
        {
            _pool.ReturnBrick(this);
        }
        else
        {
            // Запасной вариант, если что-то пошло не так
            gameObject.SetActive(false);
        }
    }
}