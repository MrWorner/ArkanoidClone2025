using UnityEngine;

public class Brick : MonoBehaviour, IDamageable
{
    public static event System.Action OnAnyBrickDestroyed;

    private BrickPool _pool;
    [SerializeField] private BrickType _brickType;
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
        // ЗАЩИТА: Если тип потерялся, просто уничтожаем кирпич, чтобы не ломать игру
        if (_brickType == null)
        {
            Debug.LogError($"Brick {name}: BrickType потерян! Уничтожаю объект.");
            gameObject.SetActive(false);
            return;
        }

        if (_brickType.isIndestructible)
        {
            return;
        }

        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            // Проверка на Singleton GameManager (на случай если вы тестируете без него)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_brickType.points);
            }

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