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
    [Tooltip("Начальная скорость мяча")]
    [SerializeField] private float initialSpeed = 7f;
    [Tooltip("Задержка перед авто-запуском в секундах")]
    [SerializeField] private float launchDelay = 3f;

    [Header("Физика")]
    [Tooltip("Минимальная вертикальная скорость. Спасает от 'горизонтальных' застреваний.")]
    [SerializeField] private float minVerticalVelocity = 0.5f;

    [Header("Финальный Режим")]
    [Tooltip("Множитель скорости в режиме 'Форсаж'")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    [Tooltip("Сила 'подруливания' к последнему кирпичу")]
    [SerializeField] private float homingStrength = 0.1f;

    // --- Внутренние переменные ---
    private Rigidbody2D rb;
    private bool isLaunched = false;
    private float _currentSpeed;
    private Transform _homingTarget = null;

    /// <summary>
    /// Awake() вызывается раньше Start(). 
    /// Идеально для получения ссылок на компоненты.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("BallController: Rigidbody2D не найден!", this);
        }
    }

    /// <summary>
    /// Start() вызывается после Awake().
    /// Идеально для инициализации состояния.
    /// </summary>
    void Start()
    {
        // 'rb' уже 100% присвоен, 
        // 'paddleOffset' задан в инспекторе.
        // Вызов ResetMode() теперь безопасен.
        ResetMode();
        StartCoroutine(LaunchDelayCoroutine());
    }

    /// <summary>
    /// FixedUpdate() для всех физических расчетов.
    /// </summary>
    void FixedUpdate()
    {
        if (!isLaunched) return;

        // 1. ЛОГИКА "ХОМИНГА" (Решение 2)
        if (_homingTarget != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 targetDirection = (_homingTarget.position - transform.position).normalized;

            Vector2 newDirection = Vector2.Lerp(
                currentVelocity.normalized,
                targetDirection,
                homingStrength * Time.fixedDeltaTime
            );

            rb.linearVelocity = newDirection * _currentSpeed;
        }

        // 2. ЛОГИКА "АНТИ-ЗАСТРЕВАНИЯ"
        // (Работает независимо от хоминга)
        Vector2 velocity = rb.linearVelocity;
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            velocity.y = (velocity.y >= 0) ? minVerticalVelocity : -minVerticalVelocity;
            rb.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// Корутина для 3-секундной задержки
    /// </summary>
    private IEnumerator LaunchDelayCoroutine()
    {
        yield return new WaitForSeconds(launchDelay);
        LaunchBall();
    }

    /// <summary>
    /// Отсоединяет мяч и "выстреливает" им
    /// </summary>
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

    /// <summary>
    /// Обработка всех столкновений
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Столкновение с Платформой
        if (isLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            CalculateRebound(collision);
            return;
        }

        // 2. Столкновение с Кирпичом (через интерфейс)
        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(1);
        }
    }

    /// <summary>
    /// Рассчитывает угол отскока от платформы
    /// </summary>
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

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ LevelManager ---

    /// <summary>
    /// Включает "Форсаж" (Решение 1)
    /// </summary>
    public void ActivateSpeedBoost()
    {
        _currentSpeed = initialSpeed * speedBoostMultiplier;
        rb.linearVelocity = rb.linearVelocity.normalized * _currentSpeed;
    }

    /// <summary>
    /// Включает "Хоминг" (Решение 2)
    /// </summary>
    public void SetHomingTarget(Transform target)
    {
        _homingTarget = target;
    }

    /// <summary>
    /// Сбрасывает все режимы (для старта уровня)
    /// </summary>
    public void ResetMode()
    {
        _currentSpeed = initialSpeed;
        _homingTarget = null;
        isLaunched = false;

        rb.bodyType = RigidbodyType2D.Kinematic;

        if (paddleTransform != null)
        {
            transform.SetParent(paddleTransform);
            // Используем 'paddleOffset' из инспектора
            transform.localPosition = paddleOffset;
        }
    }
}