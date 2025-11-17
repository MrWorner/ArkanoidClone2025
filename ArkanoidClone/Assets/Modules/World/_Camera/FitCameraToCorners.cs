using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Этот скрипт выставляет ортографическую камеру так,
/// чтобы _topLeftCorner был в левом-верхнем углу экрана,
/// а _bottomRightCorner.x - на правом краю экрана.
/// </summary>
public class FitCameraToCorners : MonoBehaviour
{
    [Header("ОБЯЗАТЕЛЬНЫЕ ССЫЛКИ")]
    [Tooltip("Объект (пустышка) в левом-верхнем углу игрового поля")]
    [SerializeField] private Transform _topLeftCorner;

    [Tooltip("Объект (пустышка) на правом краю игрового поля")]
    [SerializeField] private Transform _bottomRightCorner;

    [Tooltip("Ссылка на игровую камеру. Если null, попытается найти Camera.main")]
    [SerializeField] private Camera _cam;

    void Start()
    {
        // --- (Вся ваша логика Start() остается прежней) ---
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
            Debug.LogError("FitCameraToCorners: Не назначены угловые точки!", this);
            return;
        }

        SetCameraInstant();
    }

    [Button]
    public void SetCameraInstant()
    {
        // --- НОВАЯ ЛОГИКА РАСЧЕТА ---

        // 1. Получаем позиции наших "якорей"
        Vector3 tl_pos = _topLeftCorner.position;
        Vector3 dr_pos = _bottomRightCorner.position;

        // 2. Получаем соотношение сторон экрана (ширина / высота)
        float aspectRatio = _cam.aspect;

        // 3. Вычисляем ШИРИНУ мира, которую мы хотим видеть
        // (от левого края до правого)
        float worldWidth = dr_pos.x - tl_pos.x;

        // 4. Вычисляем ОРТО-РАЗМЕР
        // Орто-размер = (ШиринаМира / СоотношениеСторон) / 2
        // Это ГАРАНТИРУЕТ, что в экран влезет 
        // ровно от tl_pos.x до dr_pos.x
        float newOrthoSize = worldWidth / aspectRatio / 2f;

        // 5. Вычисляем ПОЗИЦИЮ камеры

        // Камера должна быть по центру между левым и правым краем
        float newCamPosX = (tl_pos.x + dr_pos.x) / 2f;

        // Камера должна быть СМЕЩЕНА ВНИЗ от верхнего края
        // ровно на свой (новый) орто-размер
        float newCamPosY = tl_pos.y - newOrthoSize;

        // 6. ПРИМЕНЯЕМ МГНОВЕННО
        _cam.transform.position = new Vector3(
            newCamPosX,
            newCamPosY,
            _cam.transform.position.z
        );
        _cam.orthographicSize = newOrthoSize;
    }
}