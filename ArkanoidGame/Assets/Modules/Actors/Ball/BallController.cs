using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащите сюда объект 'Paddle'")]
    [SerializeField] private Transform paddleTransform;
    [Tooltip("Позиция мяча ОТНОСИТЕЛЬНО центра платформы")]
    [SerializeField] private Vector3 paddleOffset = new Vector3(0, 0.5f, 0);

    [Header("Настройки Запуска")]
    [SerializeField] private float initialSpeed = 7f;
    [SerializeField] private float launchDelay = 3f;

    [Header("Физика")]
    [SerializeField] private float minVerticalVelocity = 0.5f;

    [Header("Финальный Режим")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    [SerializeField] private float homingStrength = 0.1f;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private float _currentSpeed;
    private Transform _homingTarget = null;
    private Coroutine _launchCoroutine; // Ссылка на таймер

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Start() теперь НЕ вызывает ResetMode.
        // GameManager полностью управляет мячом.
        // Когда GameManager.Start() вызовет LoadLevel(), 
        // он вызовет ResetMode() за нас.
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

        // 1. Хоминг
        if (_homingTarget != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 targetDirection = (_homingTarget.position - transform.position).normalized;
            Vector2 newDirection = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.fixedDeltaTime);
            rb.linearVelocity = newDirection * _currentSpeed;
        }

        // 2. Анти-застревание
        Vector2 velocity = rb.linearVelocity;
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            velocity.y = (velocity.y >= 0) ? minVerticalVelocity : -minVerticalVelocity;
            rb.linearVelocity = velocity;
        }
    }

    private IEnumerator LaunchDelayCoroutine()
    {
        yield return new WaitForSeconds(launchDelay);
        LaunchBall();
    }

    private void LaunchBall()
    {
        if (isLaunched || paddleTransform == null) return;
        isLaunched = true;
        transform.SetParent(null);
        rb.bodyType = RigidbodyType2D.Dynamic;
        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;
        rb.linearVelocity = direction * _currentSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            CalculateRebound(collision);
            return;
        }
        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(1);
        }
    }

    private void CalculateRebound(Collision2D collision)
    {
        Vector3 paddleCenter = collision.transform.position;
        float paddleWidth = collision.collider.bounds.size.x;
        Vector3 hitPoint = collision.contacts[0].point;
        float xOffset = hitPoint.x - paddleCenter.x;
        float normalizedX = Mathf.Clamp(xOffset / (paddleWidth / 2f), -1f, 1f);
        Vector2 newDirection = new Vector2(normalizedX, 1f).normalized;
        rb.linearVelocity = newDirection * _currentSpeed;
    }

    public void ActivateSpeedBoost()
    {
        _currentSpeed = initialSpeed * speedBoostMultiplier;
        rb.linearVelocity = rb.linearVelocity.normalized * _currentSpeed;
    }

    public void SetHomingTarget(Transform target)
    {
        _homingTarget = target;
    }

    /// <summary>
    /// Сбрасывает все режимы И ЗАПУСКАЕТ ТАЙМЕР
    /// </summary>
    public void ResetMode()
    {
        _currentSpeed = initialSpeed;
        _homingTarget = null;
        isLaunched = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero; // Останавливаем мяч

        if (paddleTransform != null)
        {
            transform.SetParent(paddleTransform);
            transform.localPosition = paddleOffset;
        }

        // --- ГЛАВНЫЙ ФИКС ---
        // 1. Останавливаем старый таймер (если он был)
        if (_launchCoroutine != null)
        {
            StopCoroutine(_launchCoroutine);
        }
        // 2. Запускаем новый 3-секундный таймер
        _launchCoroutine = StartCoroutine(LaunchDelayCoroutine());
        // ------------------
    }
}