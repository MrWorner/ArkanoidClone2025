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
    }

    public void TakeDamage(int damageAmount)
    {
        if (_brickType == null) { gameObject.SetActive(false); return; }
        if (_brickType.isIndestructible) return;

        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_brickType.points);
            }

            // --- ИЗМЕНЕНИЕ 2: Передаем позицию при смерти ---
            OnAnyBrickDestroyed?.Invoke(transform.position);
            // ------------------------------------------------

            if (_pool != null) _pool.ReturnBrick(this);
            else gameObject.SetActive(false);
        }
    }
}