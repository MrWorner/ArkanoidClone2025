using UnityEngine;

// Повесьте этот скрипт на ваш 'bottomWall' (зону поражения)
// Убедитесь, что на нем есть BoxCollider2D (с Auto Tiling)
// и SpriteRenderer (с Draw Mode = Sliced)
public class AnchorToBottom : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("AnchorToBottom: Main Camera не найдена!");
            return;
        }

        // --- 1. Позиционируем стену ---

        // Находим позицию НИЖНЕГО ЦЕНТРА экрана (Viewport 0.5, 0)
        Vector3 bottomEdgePos = _mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0, _mainCamera.nearClipPlane)
        );

        // Ставим стену туда
        transform.position = new Vector3(
            0, // Ставим по центру X
            bottomEdgePos.y, // Ставим на дно
            transform.position.z
        );

        // --- 2. Растягиваем стену ---

        // Считаем полную ширину экрана в "юнитах"
        float screenWidth = _mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x -
                            _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

        // Применяем к SpriteRenderer (BoxCollider подтянется сам)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.size = new Vector2(screenWidth, sr.size.y);
        }
        else
        {
            // Если нет спрайта, меняем коллайдер напрямую
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                bc.size = new Vector2(screenWidth, bc.size.y);
            }
        }
    }
}