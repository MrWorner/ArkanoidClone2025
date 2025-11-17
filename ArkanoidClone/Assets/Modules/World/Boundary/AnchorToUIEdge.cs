using UnityEngine;

/// <summary>
/// Прикрепляет этот объект (стену) к НИЖНЕМУ краю 
/// указанного UI-элемента (RectTransform).
/// </summary>
public class AnchorToUIEdge : MonoBehaviour
{
    [Header("Элемент UI для 'прилипания'")]
    [Tooltip("Перетащите сюда RectTransform вашего 'Background' UI")]
    [SerializeField] private RectTransform uiElement;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("AnchorToUIEdge: Камера не найдена!");
            return;
        }
        if (uiElement == null)
        {
            Debug.LogError("AnchorToUIEdge: UI Element не назначен в инспекторе!");
            return;
        }

        ApplyPosition();
    }

    private void ApplyPosition()
    {
        // 1. Получаем углы UI в пикселях (Screen Space)
        Vector3[] corners = new Vector3[4];
        uiElement.GetWorldCorners(corners);
        // corners[0] = bottom-left
        // corners[1] = top-left

        // 2. Нам нужна Y-координата НИЖНЕЙ границы UI
        float uiEdgeY_Screen = corners[0].y;

        // 3. Нам нужен X-центр камеры (в пикселях)
        float screenCenterX = Screen.width / 2f;

        // 4. Считаем Z-дистанцию от камеры до нашей стены 
        // (Обычно 10, если камера в -10, а стена в 0)
        float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);

        // 5. Конвертируем "Точку под UI" из Screen Space -> World Space
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenCenterX, uiEdgeY_Screen, zDistance));

        // 6. Устанавливаем позицию стены
        // Y-берем из 'worldPos'
        // X-берем из 'worldPos' (он будет по центру камеры)
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);

        // 7. Растягиваем стену по ширине (как в AnchorToBottom)
        float screenWidth = _mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x -
                            _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.size = new Vector2(screenWidth, sr.size.y);
        }
        else
        {
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            if (bc != null) { bc.size = new Vector2(screenWidth, bc.size.y); }
        }
    }
}