using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Нужен RigidBody для получения скорости
public class BallSpriteAnimator : MonoBehaviour
{
    [Header("Настройки Вращения")]
    [Tooltip("Насколько быстро мяч вращается относительно его скорости. Больше значение = быстрее вращается.")]
    [SerializeField] private float rotationSpeedMultiplier = 100f; // Умножитель скорости вращения

    [Tooltip("Направление вращения по оси Z. 1 = по часовой, -1 = против часовой.")]
    [SerializeField] private float rotationDirectionZ = -1f; // По умолчанию -1 для вращения против часовой стрелки при движении вправо

    private Rigidbody2D _rb;
    private Transform _ballTransform; // Ссылка на сам transform мяча

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ballTransform = transform; // Кешируем transform
    }

    void FixedUpdate() // FixedUpdate для работы с физикой
    {
        // Получаем горизонтальную скорость
        float horizontalVelocity = _rb.velocity.x;

        // Если мяч движется, вращаем его
        if (Mathf.Abs(horizontalVelocity) > 0.01f) // Проверяем, что мяч движется хоть немного
        {
            // Рассчитываем угол вращения за этот FixedUpdate
            // horizontalVelocity * rotationSpeedMultiplier: чем быстрее движется, тем быстрее вращается
            // Time.fixedDeltaTime: чтобы вращение было независимым от частоты кадров
            // rotationDirectionZ: чтобы контролировать направление вращения
            float rotationAngle = horizontalVelocity * rotationSpeedMultiplier * Time.fixedDeltaTime * rotationDirectionZ;

            // Применяем вращение вокруг оси Z
            _ballTransform.Rotate(0, 0, rotationAngle);
        }
    }
}