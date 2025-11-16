using UnityEngine;
using UnityEngine.InputSystem; // 1. Подключаем НОВУЮ систему

[RequireComponent(typeof(SpriteRenderer))]
public class PaddleController : MonoBehaviour
{
    private Camera _mainCamera;
    private float _yPosition;
    private float _minX;
    private float _maxX;
    private float _paddleHalfWidth;

    // 2. Ссылка на "указатель" (мышь ИЛИ палец)
    private Pointer _pointer;

    // (В прошлый раз я забыл тут скобки '()' )
    void Start()
    {
        _mainCamera = Camera.main;
        _paddleHalfWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
        _yPosition = transform.position.y;

        _minX = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + _paddleHalfWidth;
        _maxX = _mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - _paddleHalfWidth;

        // 3. Получаем текущий активный "указатель"
        _pointer = Pointer.current;

        if (_pointer == null)
        {
            Debug.LogError("PaddleController: Не найден 'Pointer' (мышь или тачскрин).", this);
        }
    }

    void Update()
    {
        if (_pointer == null) return;

        // 4. ПРОВЕРКА НАЖАТИЯ (Это строка ~40)
        // Вместо: if (Input.GetMouseButton(0))
        if (_pointer.press.isPressed)
        {
            // 5. ПОЛУЧЕНИЕ ПОЗИЦИИ
            // Вместо: Vector3 screenPosition = Input.mousePosition;
            Vector2 screenPosition = _pointer.position.ReadValue();

            // 6. Конвертируем
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

            // 7. Ограничиваем
            float targetX = Mathf.Clamp(worldPosition.x, _minX, _maxX);

            // 8. Применяем
            transform.position = new Vector3(
                targetX,
                _yPosition,
                transform.position.z
            );
        }
    }
}