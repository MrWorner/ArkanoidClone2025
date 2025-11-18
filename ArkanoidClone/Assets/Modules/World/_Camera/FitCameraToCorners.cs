
using NaughtyAttributes;
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
        Vector3 tl_pos = _topLeftCorner.position;
        Vector3 dr_pos = _bottomRightCorner.position;
        float aspectRatio = _cam.aspect;

        // --- 1. Требуемая Ширина и Высота в мире ---
        float requiredWorldWidth = dr_pos.x - tl_pos.x;
        float requiredWorldHeight = tl_pos.y - dr_pos.y; // Высота = Верхний Y - Нижний Y (14.976 - 0.000 = 14.976)

        // --- 2. Ортографический размер, необходимый для ФИКСАЦИИ ШИРИНЫ ---
        float orthoSizeForWidth = (requiredWorldWidth / aspectRatio) / 2f;

        // --- 3. Ортографический размер, необходимый для ФИКСАЦИИ ВЫСОТЫ ---
        float orthoSizeForHeight = requiredWorldHeight / 2f; // Орто-размер - это половина высоты

        // --- 4. Принцип "Fit-or-Expand": Выбираем БОЛЬШИЙ размер ---
        // Чтобы гарантированно увидеть ВСЕ точки, мы должны выбрать МАКСИМАЛЬНЫЙ размер.
        // Это гарантирует, что ни одна точка не будет обрезана.
        float finalOrthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);

        // --- 5. Вычисляем Позицию Камеры (X, Y) ---
        float newCamPosX = (tl_pos.x + dr_pos.x) / 2f; // Центр по X (остается прежним)

        // Центр по Y должен быть ровно посередине между TL.y и DR.y
        float newCamPosY = (tl_pos.y + dr_pos.y) / 2f;

        // ПРИМЕЧАНИЕ: Если вы хотите, чтобы верхний край всегда был ТОЧНО по tl_pos.y,
        // используйте: newCamPosY = tl_pos.y - finalOrthoSize; 
        // НО ЭТО СНОВА СДВИНЕТ НИЖНЮЮ ГРАНИЦУ! 
        // Поэтому используем центр:
        // float newCamPosY = tl_pos.y - finalOrthoSize;

        // --- 6. ПРИМЕНЯЕМ ---
        _cam.transform.position = new Vector3(
            newCamPosX,
            newCamPosY, // (14.976 + 0.000) / 2 = 7.488
            _cam.transform.position.z
        );
        _cam.orthographicSize = finalOrthoSize;
    }
}