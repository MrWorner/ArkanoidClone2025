using UnityEngine;

public class Brick : MonoBehaviour, IDamageable
{
    public static event System.Action OnAnyBrickDestroyed;

    private BrickPool _pool;
    [SerializeField] private BrickTypeSO _brickType;
    private SpriteRenderer _spriteRenderer;
    private int _currentHealth;
    public bool IsIndestructible => _brickType != null && _brickType.isIndestructible;

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
    public void Setup(BrickTypeSO type)
    {
        _brickType = type;

        // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
        // Если мы в Редакторе и Awake еще не сработал, _spriteRenderer будет null.
        // Мы находим его вручную.
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        // -------------------------

        // Теперь безопасно используем
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = _brickType.sprite;
            _spriteRenderer.color = _brickType.color;
        }

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
            Debug.LogError($"Brick {name}: BrickTypeSO потерян! Уничтожаю объект.");
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