using Sirenix.OdinInspector;
using UnityEngine;

// Повесьте этот скрипт на ваш 'bottomWall' (зону поражения)
public class AnchorToBottom : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        Apply();
    }

    [Button]
    private void Apply()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("AnchorToBottom: Main Camera не найдена!");
            return;
        }

        // --- 1. Позиционируем стену ---

        // Находим позицию НИЖНЕГО ЦЕНТРА экрана (Viewport 0.5, 0)
        // bottomEdgePos.x будет равен X-координате центра камеры
        // bottomEdgePos.y будет равен Y-координате дна камеры
        Vector3 bottomEdgePos = _mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0, _mainCamera.nearClipPlane)
        );

        // Ставим стену туда
        transform.position = new Vector3(
            bottomEdgePos.x, // ИСПРАВЛЕНО: Используем X из 'bottomEdgePos', а не 0
            bottomEdgePos.y, // Ставим на дно
            transform.position.z
        );

        // --- 2. Растягиваем стену ---

        // (Этот код у вас уже был правильный)
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
            if (bc != null)
            {
                bc.size = new Vector2(screenWidth, bc.size.y);
            }
        }
    }
}