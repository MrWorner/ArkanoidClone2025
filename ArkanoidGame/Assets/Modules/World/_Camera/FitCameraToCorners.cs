using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Этот скрипт ОДИН РАЗ при старте игры
/// идеально выставляет ортографическую камеру так,
/// чтобы в нее помещалось поле, заданное двумя угловыми точками.
/// </summary>
public class FitCameraToCorners : MonoBehaviour
{
    [Header("ОБЯЗАТЕЛЬНЫЕ ССЫЛКИ")]
    [Tooltip("Объект (пустышка) в левом-верхнем углу игрового поля")]
    [SerializeField] private Transform _topLeftCorner;

    [Tooltip("Объект (пустышка) в правом-нижнем углу игрового поля")]
    [SerializeField] private Transform _bottomRightCorner;

    [Tooltip("Ссылка на игровую камеру. Если null, попытается найти Camera.main")]
    [SerializeField] private Camera _cam;

    [Header("НАСТРОЙКИ")]
    [Tooltip("Дополнительный отступ (в 'юнитах'), чтобы поле не прилипало к краям")]
    [SerializeField] private float _padding = 1f;

    void Start()
    {
        // 1. Проверка ссылок
        if (_cam == null)
        {
            _cam = Camera.main;
        }

        if (_cam == null)
        {
            Debug.LogError("FitCameraToCorners: Камера не найдена!", this);
            return;
        }

        if (!_cam.orthographic)
        {
            Debug.LogError("FitCameraToCorners: Камера должна быть ортографической (Orthographic)!", this);
            return;
        }

        if (_topLeftCorner == null || _bottomRightCorner == null)
        {
            Debug.LogError("FitCameraToCorners: Не назначены угловые точки (_topLeftCorner или _bottomRightCorner)!", this);
            return;
        }

        // 2. Мгновенная установка камеры
        SetCameraInstant();
    }

    [Button]
    public void SetCameraInstant()
    {
        // Мы используем ВАШИ методы из DynamicDuelCamera,
        // так как они написаны отлично.
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;

        UpdateBoundsWithTransform(_topLeftCorner, ref minX, ref maxX, ref minY, ref maxY);
        UpdateBoundsWithTransform(_bottomRightCorner, ref minX, ref maxX, ref minY, ref maxY);

        // --- Логика расчета ---

        // 1. Находим центр прямоугольника, который мы хотим видеть
        // Z-координату берем от камеры, чтобы она не сдвинулась
        Vector3 center = new Vector3(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f,
            _cam.transform.position.z
        );

        // 2. Считаем нужный ортографический размер
        float distanceX = (maxX - minX) + _padding;
        float distanceY = (maxY - minY) + _padding;

        // Отношение сторон экрана (ширина / высоту)
        float aspectRatio = _cam.aspect;

        // Считаем размер, нужный для охвата по ШИРИНЕ
        float sizeX = distanceX / aspectRatio / 2f;
        // Считаем размер, нужный для охвата по ВЫСОТЕ
        float sizeY = distanceY / 2f;

        // Ортографический размер камеры - это ПОЛОВИНА ее ВЫСОТЫ.
        // Мы берем МАКСИМАЛЬНОЕ из двух значений,
        // чтобы в камеру гарантированно влезло и по ширине, и по высоте.
        float targetSize = Mathf.Max(sizeX, sizeY);

        // 3. ПРИМЕНЯЕМ МГНОВЕННО
        _cam.transform.position = center;
        _cam.orthographicSize = targetSize;
    }

    // --- Методы из вашего скрипта DynamicDuelCamera ---
    // Они идеальны, просто копируем их.

    private void UpdateBoundsWithTransform(Transform t, ref float minX, ref float maxX, ref float minY, ref float maxY)
    {
        if (t == null) return;
        UpdateBoundsWithPosition(t.position, ref minX, ref maxX, ref minY, ref maxY);
    }

    private void UpdateBoundsWithPosition(Vector3 pos, ref float minX, ref float maxX, ref float minY, ref float maxY)
    {
        minX = Mathf.Min(minX, pos.x);
        maxX = Mathf.Max(maxX, pos.x);
        minY = Mathf.Min(minY, pos.y);
        maxY = Mathf.Max(maxY, pos.y);
    }
}