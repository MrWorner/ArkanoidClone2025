using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащите сюда объект 'Paddle'")]
    [SerializeField] private Transform paddleTransform;

    [Header("Настройки Запуска")]
    [Tooltip("Начальная скорость мяча")]
    [SerializeField] private float initialSpeed = 7f;
    [Tooltip("Задержка перед авто-запуском в секундах")]
    [SerializeField] private float launchDelay = 3f;

    // --- НОВОЕ ПОЛЕ ---
    [Header("Физика")]
    [Tooltip("Минимальная вертикальная скорость. Спасает от 'горизонтальных' застреваний.")]
    [SerializeField] private float minVerticalVelocity = 0.5f;
    // ------------------

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Vector3 paddleOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (paddleTransform != null)
        {
            transform.SetParent(paddleTransform);
            paddleOffset = transform.localPosition;
        }

        StartCoroutine(LaunchDelayCoroutine());
    }

    // --- НОВЫЙ МЕТОД ---
    /// <summary>
    /// FixedUpdate вызывается в том же ритме, что и физический движок
    /// </summary>
    void FixedUpdate()
    {
        // 1. Ничего не делаем, если мяч еще не запущен
        if (!isLaunched)
        {
            return;
        }

        // 2. Получаем текущую скорость
        Vector2 velocity = rb.linearVelocity;

        // 3. ПРОВЕРКА: Если вертикальная скорость СЛИШКОМ маленькая...
        if (Mathf.Abs(velocity.y) < minVerticalVelocity)
        {
            // 4. ...мы ее "подталкиваем", сохраняя направление

            // Если Y был 0.1 (почти 0, вверх), он станет 0.5
            // Если Y был -0.1 (почти 0, вниз), он станет -0.5
            // Если Y был 0, он станет 0.5 (вверх)
            velocity.y = (velocity.y >= 0) ? minVerticalVelocity : -minVerticalVelocity;

            // 5. Применяем "исправленную" скорость
            rb.linearVelocity = velocity;
        }
    }
    // ------------------

    void Update()
    {
        // (Update остается без изменений)
        if (!isLaunched && paddleTransform != null)
        {
            // Позиция обновляется сама, т.к. мы дочерний объект
        }
    }

    private IEnumerator LaunchDelayCoroutine()
    {
        yield return new WaitForSeconds(launchDelay);
        LaunchBall();
    }

    private void LaunchBall()
    {
        if (isLaunched || paddleTransform == null)
            return;

        isLaunched = true;
        transform.SetParent(null);
        rb.bodyType = RigidbodyType2D.Dynamic;

        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;
        rb.linearVelocity = direction * initialSpeed;
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

        rb.linearVelocity = newDirection * initialSpeed;
    }
}