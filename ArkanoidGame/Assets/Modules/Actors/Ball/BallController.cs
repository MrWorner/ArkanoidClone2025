using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащите сюда объект 'Paddle' (нужен только для главного мяча)")]
    [SerializeField] private Transform paddleTransform;

    [Tooltip("Позиция мяча ОТНОСИТЕЛЬНО центра платформы")]
    [SerializeField] private Vector3 paddleOffset = new Vector3(0, 0.6f, 0);

    [Header("Настройки Запуска")]
    [SerializeField] private float initialSpeed = 7f;
    [SerializeField] private float launchDelay = 3f;

    [Header("Физика")]
    [Tooltip("Минимальная вертикальная скорость (защита от застревания по горизонтали)")]
    [SerializeField] private float minVerticalVelocity = 0.5f;

    [Header("Финальный Режим / Бонусы")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    [SerializeField] private float homingStrength = 0.1f;

    // --- Внутренние переменные ---
    private Rigidbody2D rb;
    private bool isLaunched = false;
    private float _currentSpeed;
    private Transform _homingTarget = null;
    private Coroutine _launchCoroutine;
    private float _launchTime; // Время запуска для защиты от двойной коллизии

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

        // 1. ЛОГИКА "ХОМИНГА" (Умный мяч)
        if (_homingTarget != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            // Направляем вектор скорости в сторону цели
            Vector2 targetDirection = (_homingTarget.position - transform.position).normalized;
            Vector2 newDirection = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.fixedDeltaTime);

            rb.linearVelocity = newDirection * _currentSpeed;
        }

        // 2. ЛОГИКА "АНТИ-ЗАСТРЕВАНИЯ" (Всегда активна)
        Vector2 velocity = rb.linearVelocity;
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            // Если Y почти 0, принудительно добавляем вертикальную скорость
            velocity.y = (velocity.y >= 0) ? minVerticalVelocity : -minVerticalVelocity;
            rb.linearVelocity = velocity;
        }
    }

    // ========================================================================
    // МЕТОДЫ ЗАПУСКА
    // ========================================================================

    /// <summary>
    /// Метод для ГЛАВНОГО мяча (старт уровня, потеря жизни).
    /// Приклеивает мяч к ракетке и запускает таймер.
    /// </summary>
    public void ResetToPaddle()
    {
        isLaunched = false;
        _currentSpeed = initialSpeed;
        _homingTarget = null;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // Ищем ракетку, если ссылка потерялась (для надежности)
        if (paddleTransform == null)
        {
            GameObject paddleObj = GameObject.FindGameObjectWithTag("Paddle");
            if (paddleObj != null) paddleTransform = paddleObj.transform;
        }

        if (paddleTransform != null)
        {
            transform.SetParent(paddleTransform);
            transform.localPosition = paddleOffset;
        }

        // Перезапуск корутины
        if (_launchCoroutine != null) StopCoroutine(_launchCoroutine);
        _launchCoroutine = StartCoroutine(LaunchDelayCoroutine());
    }

    /// <summary>
    /// Метод для КЛОНОВ (PowerUp).
    /// Запускает мяч сразу из указанной точки с указанной скоростью.
    /// </summary>
    public void SpawnAsClone(Vector2 position, Vector2 velocity)
    {
        transform.SetParent(null); // Клоны сами по себе
        transform.position = position;

        // Настраиваем состояние
        isLaunched = true;
        _currentSpeed = velocity.magnitude; // Наследуем скорость родителя
        _homingTarget = null;
        _launchTime = Time.time; // Запоминаем время для защиты от коллизий

        // Физика
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;
    }

    private IEnumerator LaunchDelayCoroutine()
    {
        yield return new WaitForSeconds(launchDelay);
        LaunchMainBall();
    }

    private void LaunchMainBall()
    {
        // Если ракетки нет или уже запущен - выходим
        if (isLaunched) return;

        isLaunched = true;
        transform.SetParent(null);
        rb.bodyType = RigidbodyType2D.Dynamic;

        _launchTime = Time.time; // Запоминаем время

        // Случайный угол старта
        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;
        rb.linearVelocity = direction * _currentSpeed;
    }

    // ========================================================================
    // КОЛЛИЗИИ
    // ========================================================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Столкновение с РАКЕТКОЙ
        if (isLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            // ЗАЩИТА: Игнорируем удары первые 0.2 сек после спавна,
            // чтобы мяч не "зажевало" внутри ракетки
            if (Time.time - _launchTime < 0.2f) return;

            CalculateRebound(collision);
            return;
        }

        // Столкновение с КИРПИЧОМ (через интерфейс)
        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(1);
        }
    }

    private void CalculateRebound(Collision2D collision)
    {
        Vector3 paddleCenter = collision.transform.position;
        float paddleWidth = collision.collider.bounds.size.x;

        // Берем первую точку контакта
        Vector3 hitPoint = collision.contacts[0].point;

        float xOffset = hitPoint.x - paddleCenter.x;
        // Нормализуем от -1 (левый край) до 1 (правый край)
        float normalizedX = Mathf.Clamp(xOffset / (paddleWidth / 2f), -1f, 1f);

        // Новый вектор: X зависит от удара, Y всегда вверх
        Vector2 newDirection = new Vector2(normalizedX, 1f).normalized;

        rb.linearVelocity = newDirection * _currentSpeed;
    }

    // ========================================================================
    // ПУБЛИЧНЫЕ МЕТОДЫ (БОНУСЫ)
    // ========================================================================

    public void ActivateSpeedBoost()
    {
        _currentSpeed = initialSpeed * speedBoostMultiplier;
        // Обновляем текущую скорость, сохраняя направление
        if (isLaunched)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * _currentSpeed;
        }
    }

    public void SetHomingTarget(Transform target)
    {
        _homingTarget = target;
    }
}