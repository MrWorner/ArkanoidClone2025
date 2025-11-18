using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float initialSpeed = 7f;
    [SerializeField] private float minTotalSpeed = 7f;
    [SerializeField] private float launchDelay = 3f;
    [SerializeField] private float minVerticalVelocity = 0.5f;

    [Header("Бонусы")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    [SerializeField] private float homingStrength = 0.1f;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private float _currentSpeed;
    private Transform _homingTarget = null;
    private Coroutine _launchCoroutine;
    private float _launchTime;

    // --- НОВОЕ: Точка привязки ---
    private Transform _anchorPoint; // Ссылка на BallSpawnPoint внутри ракетки
    // ----------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() // Используем Update для визуального следования
    {
        // Если мяч НЕ запущен и у нас есть "якорь" -> следуем за ним
        if (!isLaunched && _anchorPoint != null)
        {
            transform.position = _anchorPoint.position;
            // Мы НЕ меняем parent, поэтому scale не ломается!
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

        // (Логика Хоминга и Анти-застревания осталась прежней)
        if (_homingTarget != null)
        {
            Vector2 currentVelocity = rb.velocity;
            Vector2 targetDirection = (_homingTarget.position - transform.position).normalized;
            Vector2 newDirection = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.fixedDeltaTime);
            rb.velocity = newDirection * _currentSpeed;
        }

        Vector2 velocity = rb.velocity;
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            velocity.y = (velocity.y >= 0) ? minVerticalVelocity : -minVerticalVelocity;
            rb.velocity = velocity.normalized * velocity.magnitude;
            velocity = rb.velocity;
        }

        float currentMagnitude = velocity.magnitude;
        if (currentMagnitude < minTotalSpeed)
        {
            rb.velocity = velocity.normalized * minTotalSpeed;
        }
    }

    public void ResetToPaddle()
    {
        isLaunched = false;
        _currentSpeed = initialSpeed;
        _homingTarget = null;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;

        // --- НОВАЯ ЛОГИКА ПОИСКА ТОЧКИ ---
        if (_anchorPoint == null)
        {
            GameObject paddle = GameObject.FindGameObjectWithTag("Paddle");
            if (paddle != null)
            {
                // Ищем дочерний объект с именем "BallSpawnPoint"
                Transform spawnPoint = paddle.transform.Find("BallSpawnPoint");
                if (spawnPoint != null)
                {
                    _anchorPoint = spawnPoint;
                }
                else
                {
                    // Если забыли создать точку, используем центр ракетки + отступ
                    _anchorPoint = paddle.transform;
                    Debug.LogWarning("BallController: BallSpawnPoint не найден в Paddle! Использую центр.");
                }
            }
        }
        // ----------------------------------

        // Сразу ставим на позицию
        if (_anchorPoint != null)
        {
            transform.position = _anchorPoint.position;
        }

        if (_launchCoroutine != null) StopCoroutine(_launchCoroutine);
        _launchCoroutine = StartCoroutine(LaunchDelayCoroutine());
    }

    public void SpawnAsClone(Vector2 position, Vector2 velocity)
    {
        _anchorPoint = null; // Клоны не привязаны
        transform.position = position;

        isLaunched = true;
        _currentSpeed = velocity.magnitude;
        if (_currentSpeed < minTotalSpeed) _currentSpeed = minTotalSpeed;

        _homingTarget = null;
        _launchTime = Time.time;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = velocity.normalized * _currentSpeed;
    }

    // (Методы LaunchDelayCoroutine, LaunchMainBall, OnCollisionEnter2D... без изменений)

    private IEnumerator LaunchDelayCoroutine()
    {
        yield return new WaitForSeconds(launchDelay);
        LaunchMainBall();
    }

    private void LaunchMainBall()
    {
        if (isLaunched) return;

        isLaunched = true;
        _anchorPoint = null; // Перестаем следовать

        rb.bodyType = RigidbodyType2D.Dynamic;
        _launchTime = Time.time;

        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;
        rb.velocity = direction * _currentSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            SoundManager.Instance.PlayOneShot(SoundType.PaddleHit);
            if (Time.time - _launchTime < 0.2f) return;
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