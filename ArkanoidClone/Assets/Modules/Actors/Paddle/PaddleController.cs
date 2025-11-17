using UnityEngine;

// Убрали using UnityEngine.InputSystem; - он больше не нужен

[RequireComponent(typeof(SpriteRenderer))]
public class PaddleController : MonoBehaviour
{
    [Header("Настройки Платформы")]
    [Tooltip("Скорость, с которой платформа следует за пальцем/мышью")]
    [SerializeField] private float moveSpeed = 15f;

    private Camera _mainCamera;
    private float _yPosition;
    private float _minX;
    private float _maxX;
    private float _paddleHalfWidth;

    // Ссылка на якорь для мяча (нужна, чтобы мяч знал, где сидеть)
    private Transform _ballSpawnPoint;

    void Awake()
    {
        // Ищем или создаем точку крепления мяча, если её нет
        _ballSpawnPoint = transform.Find("BallSpawnPoint");
        if (_ballSpawnPoint == null)
        {
            GameObject point = new GameObject("BallSpawnPoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = new Vector3(0, 0.5f, 0);
            _ballSpawnPoint = point.transform;
        }
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _paddleHalfWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
        _yPosition = transform.position.y;

        // Рассчитываем границы экрана
        if (_mainCamera != null)
        {
            float camHeight = 2f * _mainCamera.orthographicSize;
            float camWidth = camHeight * _mainCamera.aspect;
            float camHalfWidth = camWidth / 2f;

            // 0,0,0 - центр камеры в World Space
            float camCenterX = _mainCamera.transform.position.x;

            _minX = camCenterX - camHalfWidth + _paddleHalfWidth;
            _maxX = camCenterX + camHalfWidth - _paddleHalfWidth;
        }
    }

    void Update()
    {
        // ИСПОЛЬЗУЕМ СТАРУЮ СИСТЕМУ ВВОДА
        // Input.GetMouseButton(0) работает и для мыши (левая кнопка), 
        // и для тача на мобильном (первое касание)
        if (Input.GetMouseButton(0))
        {
            MovePaddle(Input.mousePosition);
        }
        // Дополнительно: поддержка тачей (для надежности на Android)
        else if (Input.touchCount > 0)
        {
            MovePaddle(Input.GetTouch(0).position);
        }
    }

    private void MovePaddle(Vector3 inputScreenPosition)
    {
        if (_mainCamera == null) return;

        // 1. Конвертируем позицию экрана в мировые координаты
        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(inputScreenPosition);

        // 2. Ограничиваем по краям экрана (Clamp)
        float targetX = Mathf.Clamp(worldPosition.x, _minX, _maxX);

        // 3. Создаем целевую позицию
        Vector3 targetPosition = new Vector3(
            targetX,
            _yPosition,
            transform.position.z
        );

        // 4. Двигаем плавно
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}