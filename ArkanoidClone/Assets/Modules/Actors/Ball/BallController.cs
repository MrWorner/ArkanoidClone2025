using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float initialSpeed = 7f;
    [SerializeField] private float minTotalSpeed = 7f;
    [SerializeField] private float launchDelay = 3f;

    // --- ИСПРАВЛЕНИЕ: Минимальная скорость по обеим осям ---
    [SerializeField] private float minVerticalVelocity = 0.5f;   // Чтобы не застревал горизонтально
    [SerializeField] private float minHorizontalVelocity = 0.5f; // Чтобы не застревал вертикально
    // -------------------------------------------------------

    [Header("Бонусы")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    [SerializeField] private float homingStrength = 0.1f;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private float _currentSpeed;
    private Transform _homingTarget = null;
    private Coroutine _launchCoroutine;
    private float _launchTime;

    private Transform _anchorPoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isLaunched && _anchorPoint != null)
        {
            transform.position = _anchorPoint.position;
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

        // 1. Логика Хоминга (без изменений)
        if (_homingTarget != null)
        {
            Vector2 currentVelocity = rb.velocity;
            Vector2 targetDirection = (_homingTarget.position - transform.position).normalized;
            Vector2 newDirection = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.fixedDeltaTime);
            rb.velocity = newDirection * _currentSpeed;
        }

        // --- ИСПРАВЛЕНИЕ ЗАСТРЕВАНИЙ ---
        Vector2 velocity = rb.velocity;
        bool velocityChanged = false;

        // Проблема 1: Мяч летает чисто горизонтально (Y слишком мал)
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            // Если скорость 0, выбираем случайное направление, иначе сохраняем текущее
            float sign = (velocity.y == 0) ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(velocity.y);
            velocity.y = sign * minVerticalVelocity;
            velocityChanged = true;
        }

        // Проблема 2: Мяч летает чисто вертикально (X слишком мал)
        if (Mathf.Abs(velocity.x) < minHorizontalVelocity)
        {
            // Если скорость 0, выбираем случайное направление
            float sign = (velocity.x == 0) ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(velocity.x);
            velocity.x = sign * minHorizontalVelocity;
            velocityChanged = true;
        }

        // Применяем изменения и нормализуем скорость
        if (velocityChanged)
        {
            rb.velocity = velocity.normalized * _currentSpeed;
        }
        // -------------------------------

        // Контроль общей скорости (чтобы не замедлялся)
        if (rb.velocity.magnitude < minTotalSpeed)
        {
            rb.velocity = rb.velocity.normalized * minTotalSpeed;
        }
    }

    public void ResetToPaddle()
    {
        isLaunched = false;
        _currentSpeed = initialSpeed;
        _homingTarget = null;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;

        if (_anchorPoint == null)
        {
            GameObject paddle = GameObject.FindGameObjectWithTag("Paddle");
            if (paddle != null)
            {
                Transform spawnPoint = paddle.transform.Find("BallSpawnPoint");
                if (spawnPoint != null) _anchorPoint = spawnPoint;
                else _anchorPoint = paddle.transform;
            }
        }

        if (_anchorPoint != null)
        {
            transform.position = _anchorPoint.position;
        }

        if (_launchCoroutine != null) StopCoroutine(_launchCoroutine);
        _launchCoroutine = StartCoroutine(LaunchDelayCoroutine());
    }

    public void SpawnAsClone(Vector2 position, Vector2 velocity)
    {
        _anchorPoint = null;
        transform.position = position;

        isLaunched = true;
        _currentSpeed = velocity.magnitude;
        if (_currentSpeed < minTotalSpeed) _currentSpeed = minTotalSpeed;

        _homingTarget = null;
        _launchTime = Time.time;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = velocity.normalized * _currentSpeed;
    }

    private IEnumerator LaunchDelayCoroutine()
    {
        yield return new WaitForSeconds(launchDelay);
        LaunchMainBall();
    }

    private void LaunchMainBall()
    {
        if (isLaunched) return;

        isLaunched = true;
        _anchorPoint = null;

        rb.bodyType = RigidbodyType2D.Dynamic;
        _launchTime = Time.time;

        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;
        rb.velocity = direction * _currentSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Если ударились о ракетку - рассчитываем отскок
        if (isLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            SoundManager.Instance.PlayOneShot(SoundType.PaddleHit);
            if (Time.time - _launchTime < 0.2f) return;
            CalculateRebound(collision);
            return;
        }

        // --- ИСПРАВЛЕНИЕ: ЛОМАЕМ ИДЕАЛЬНЫЕ ЦИКЛЫ ---
        // Если ударились обо что угодно КРОМЕ ракетки (стена, кирпич)
        else
        {
            // Добавляем микроскопическое отклонение, чтобы разбить "бесконечный цикл" отскоков
            Vector2 randomTweak = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            rb.velocity += randomTweak;

            // Восстанавливаем скорость (так как tweak мог ее изменить)
            rb.velocity = rb.velocity.normalized * _currentSpeed;
        }
        // -------------------------------------------

        if (collision.gameObject.CompareTag("Wall"))
        {
            SoundManager.Instance.PlayOneShot(SoundType.WallHit);
        }

        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            // Звук перенесен в сам кирпич или оставлен тут - на ваше усмотрение, 
            // но лучше не дублировать.
            // SoundManager.Instance.PlayOneShot(SoundType.WallHit); 
            damageable.TakeDamage(1);
            return;
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
        rb.velocity = newDirection * _currentSpeed;
    }

    public void ActivateSpeedBoost()
    {
        _currentSpeed = initialSpeed * speedBoostMultiplier;
        if (isLaunched) rb.velocity = rb.velocity.normalized * _currentSpeed;
    }

    public void SetHomingTarget(Transform target)
    {
        _homingTarget = target;
    }
}