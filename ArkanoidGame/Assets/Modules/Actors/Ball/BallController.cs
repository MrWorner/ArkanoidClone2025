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

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Vector3 paddleOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // "Выключаем" физику, пока мяч не запущен
        // rb.isKinematic = true; // <-- УСТАРЕВШЕЕ
        rb.bodyType = RigidbodyType2D.Kinematic; // ИСПРАВЛЕНО

        if (paddleTransform != null)
        {
            transform.SetParent(paddleTransform);
            paddleOffset = transform.localPosition;
        }

        StartCoroutine(LaunchDelayCoroutine());
    }

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

        // "Включаем" физику
        // rb.isKinematic = false; // <-- УСТАРЕВШЕЕ
        rb.bodyType = RigidbodyType2D.Dynamic; // ИСПРАВЛЕНО

        // Задаем начальный импульс
        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        Vector2 direction = new Vector2(startX, 1f).normalized;

        // rb.velocity = direction * initialSpeed; // <-- УСТАРЕВШЕЕ
        rb.linearVelocity = direction * initialSpeed; // ИСПРАВЛЕНО
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

        // Применяем новую скорость
        // rb.velocity = newDirection * initialSpeed; // <-- УСТАРЕВШЕЕ
        rb.linearVelocity = newDirection * initialSpeed; // ИСПРАВЛЕНО
    }
}