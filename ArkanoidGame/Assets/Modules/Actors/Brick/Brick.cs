using UnityEngine;

public class Brick : MonoBehaviour, IDamageable
{
    public static event System.Action OnAnyBrickDestroyed;

    private BrickPool _pool;
    private BrickType _brickType;
    private SpriteRenderer _spriteRenderer;
    private int _currentHealth;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(BrickPool ownerPool)
    {
        _pool = ownerPool;
    }

    /// <summary>
    /// "Настраивает" кирпич, когда его берут из пула.
    /// </summary>
    public void Setup(BrickType type)
    {
        _brickType = type;

        // 1. Устанавливаем спрайт
        _spriteRenderer.sprite = _brickType.sprite;

        // --- НОВАЯ СТРОКА ---
        // 2. Устанавливаем цвет (tint)
        _spriteRenderer.color = _brickType.color;
        // --------------------

        // 3. Устанавливаем "здоровье"
        _currentHealth = _brickType.health;
    }

    /// <summary>
    /// Вызывается, когда мяч попадает в кирпич.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (_brickType.isIndestructible)
        {
            return;
        }

        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            GameManager.Instance.AddScore(_brickType.points);
            OnAnyBrickDestroyed?.Invoke();

            if (_pool != null)
            {
                _pool.ReturnBrick(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}