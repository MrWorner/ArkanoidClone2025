using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащите сюда объект 'Paddle'")]
    [SerializeField] private Transform paddleTransform;

    [Tooltip("ВАЖНО: Y должен быть достаточно большим (0.6+), чтобы мяч не касался ракетки при старте!")]
    [SerializeField] private Vector3 paddleOffset = new Vector3(0, 0.6f, 0); // <-- Увеличил до 0.6 для безопасности

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
    private Coroutine _launchCoroutine;

    // --- НОВАЯ ПЕРЕМЕННАЯ ---
    private float _launchTime; // Время, когда мяч был запущен
    // ------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // ResetMode вызывается из GameManager, но для теста в сцене оставим здесь
        if (isLaunched == false)
        {
            ResetMode();
            StartCoroutine(LaunchDelayCoroutine());
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

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

        // --- ЗАПОМИНАЕМ ВРЕМЯ ЗАПУСКА ---
        _launchTime = Time.time;
        // --------------------------------

        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;
        rb.linearVelocity = direction * _currentSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            // --- ЗАЩИТА ОТ БАГА "ВНИЗ" ---
            // Если с момента запуска прошло меньше 0.2 секунды,
            // мы ИГНОРИРУЕМ расчет отскока от ракетки.
            // Мяч полетит туда, куда его толкнул LaunchBall (то есть ВВЕРХ).
            if (Time.time - _launchTime < 0.2f)
            {
                return;
            }
            // -----------------------------

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

    public void ResetMode()
    {
        _currentSpeed = initialSpeed;
        _homingTarget = null;
        isLaunched = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        if (paddleTransform != null)
        {
            transform.SetParent(paddleTransform);
            transform.localPosition = paddleOffset;
        }

        if (_launchCoroutine != null) StopCoroutine(_launchCoroutine);
        _launchCoroutine = StartCoroutine(LaunchDelayCoroutine());
    }
}