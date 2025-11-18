using UnityEngine;
using System;
using DG.Tweening;
using MiniIT.CORE;

public class Brick : MonoBehaviour, IDamageable
{
    public static event Action<Vector3> OnAnyBrickDestroyed;

    [SerializeField] private BrickTypeSO _brickType;
    private BrickPool _pool;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider; // Кешируем коллайдер
    private int _currentHealth;
    private bool _isDestroyed = false;

    public BrickTypeSO BrickType { get => _brickType; }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    public void Init(BrickPool ownerPool)
    {
        _pool = ownerPool;
    }

    public void Setup(BrickTypeSO type)
    {
        _brickType = type;

        // --- СБРОС СОСТОЯНИЯ ДЛЯ ПУЛА (ВАЖНО!) ---
        // Возвращаем нормальный размер и цвет, так как прошлая анимация их изменила
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity; // На всякий случай

        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _brickType != null ? _brickType.color : Color.white;
            if (_brickType != null) _spriteRenderer.sprite = _brickType.sprite;
            // Сбрасываем альфа-канал, если анимация его меняла
            Color c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }

        // Включаем коллайдер обратно
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_collider != null) _collider.enabled = true;
        // ------------------------------------------

        _currentHealth = _brickType != null ? _brickType.health : 1;
        _isDestroyed = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (_isDestroyed || _brickType == null) return;
        if (_brickType.isIndestructible)
        {
            SoundManager.Instance.PlayOneShot(SoundType.IndestructibleHit);
            // Бонус: Анимация "дрожания" для неубиваемого блока
            transform.DOShakePosition(0.2f, 0.1f, 10, 90, false, true);
            return;
        }

        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            SoundManager.Instance.PlayOneShot(SoundType.BrickDestroyed);
            _isDestroyed = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_brickType.points);
            }

            OnAnyBrickDestroyed?.Invoke(transform.position);

            // --- ANIMATION MAGIC (DOTween) ---

            // 1. Сразу отключаем физику, чтобы мяч пролетал сквозь
            if (_collider != null) _collider.enabled = false;

            // 2. Создаем последовательность анимации
            Sequence seq = DOTween.Sequence();

            // Уменьшаем до 0 за 0.2 секунды (эффект схлопывания)
            seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));

            // Одновременно слегка вращаем
            seq.Join(transform.DORotate(new Vector3(0, 0, 45), 0.2f));

            // 3. Когда анимация закончилась -> возвращаем в пул
            seq.OnComplete(() => {
                if (_pool != null) _pool.ReturnBrick(this);
                else gameObject.SetActive(false);
            });
            // ---------------------------------
        }
        else
        {
            SoundManager.Instance.PlayOneShot(SoundType.BrickHit);
            // Если кирпич ранен, но не убит - тоже трясем
            transform.DOShakeScale(0.15f, 0.2f);
        }
    }
}