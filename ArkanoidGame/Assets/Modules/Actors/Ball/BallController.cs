using UnityEngine;

// Требуем, чтобы на этом объекте был Rigidbody2D, 
// чтобы скрипт не сломался
[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Настройки Мяча")]
    [Tooltip("Начальная скорость мяча")]
    [SerializeField] private float initialSpeed = 5f;

    // Ссылка на компонент Rigidbody2D
    private Rigidbody2D rb;

    void Start()
    {
        // 1. Получаем ссылку на наш Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        // 2. Запускаем мяч!
        LaunchBall();
    }

    /// <summary>
    /// Дает мячу начальный импульс
    /// </summary>
    private void LaunchBall()
    {
        // 1. Создаем случайное начальное направление
        // (x будет случайным числом между -1 и 1, но не 0)
        float startX = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        // (y всегда будет -1, чтобы мяч летел ВНИЗ к платформе)
        float startY = -1f;

        // 2. Создаем вектор направления и "нормализуем" его
        // (чтобы его длина была равна 1)
        Vector2 direction = new Vector2(startX, startY).normalized;

        // 3. Применяем силу к Rigidbody
        // (Умножаем направление на скорость, чтобы получить вектор силы)
        rb.linearVelocity = direction * initialSpeed;
    }
}