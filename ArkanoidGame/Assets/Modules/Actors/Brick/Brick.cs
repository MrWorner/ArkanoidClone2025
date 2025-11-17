using UnityEngine;
using System; // Нужно для Action<>

public class Brick : MonoBehaviour, IDamageable
{
    // --- ИЗМЕНЕНИЕ 1: Событие теперь передает позицию (Vector3) ---
    public static event Action<Vector3> OnAnyBrickDestroyed;
    // -------------------------------------------------------------

    [SerializeField] private BrickTypeSO _brickType;
    private BrickPool _pool;
    private SpriteRenderer _spriteRenderer;
    private int _currentHealth;
    private bool _isDestroyed = false;

    public BrickTypeSO BrickType { get => _brickType; set => _brickType = value; }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(BrickPool ownerPool)
    {
        _pool = ownerPool;
    }

    public void Setup(BrickTypeSO type)
    {
        _brickType = type;
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null && _brickType != null)
        {
            _spriteRenderer.sprite = _brickType.sprite;
            _spriteRenderer.color = _brickType.color;
        }
        _currentHealth = _brickType != null ? _brickType.health : 1;
        _isDestroyed = false; // Сбрасываем флаг при респавне из пула!
    }

    public void TakeDamage(int damageAmount)
    {
        // 1. Если уже уничтожен или нет типа - выходим
        if (_isDestroyed || _brickType == null) return;

        if (_brickType.isIndestructible) return;

        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            // 2. Ставим флаг, чтобы второй мяч не мог зайти сюда
            _isDestroyed = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_brickType.points);
            }

            OnAnyBrickDestroyed?.Invoke(transform.position);

            if (_pool != null) _pool.ReturnBrick(this);
            else gameObject.SetActive(false);
        }
    }
}