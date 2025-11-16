using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PaddleController : MonoBehaviour
{
    [Header("Настройки Платформы")]
    [Tooltip("Скорость, с которой платформа следует за пальцем/мышью")]
    [SerializeField] private float moveSpeed = 15f; // Вот новая переменная!

    private Camera _mainCamera;
    private float _yPosition;
    private float _minX;
    private float _maxX;
    private float _paddleHalfWidth;
    private Pointer _pointer;

    void Start()
    {
        _mainCamera = Camera.main;
        _paddleHalfWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
        _yPosition = transform.position.y;

        _minX = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + _paddleHalfWidth;
        _maxX = _mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - _paddleHalfWidth;

        _pointer = Pointer.current;

        if (_pointer == null)
        {
            Debug.LogError("PaddleController: Не найден 'Pointer' (мышь или тачскрин).", this);
        }
    }

    void Update()
    {
        if (_pointer == null) return;

        if (_pointer.press.isPressed)
        {
            // --- ЭТА ЧАСТЬ ОСТАЛАСЬ ПРЕЖНЕЙ ---

            // 1. Получаем позицию пальца/мыши
            Vector2 screenPosition = _pointer.position.ReadValue();
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

            // 2. Вычисляем целевую X-координату
            float targetX = Mathf.Clamp(worldPosition.x, _minX, _maxX);

            // --- А ЭТА ЧАСТЬ ТЕПЕРЬ НОВАЯ ---

            // 3. Создаем ПОЛНУЮ целевую позицию
            Vector3 targetPosition = new Vector3(
                targetX,
                _yPosition,
                transform.position.z
            );

            // 4. Плавно ДВИГАЕМСЯ к цели, а не телепортируемся
            // (Time.deltaTime делает движение плавным 
            //  независимо от FPS)
            transform.position = Vector3.MoveTowards(
                transform.position, // Откуда
                targetPosition,     // Куда
                moveSpeed * Time.deltaTime // С какой скоростью
            );
        }
    }
}