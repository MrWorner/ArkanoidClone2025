using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Размещает стены по краям экрана и растягивает их, 
/// СОХРАНЯЯ настроенную в инспекторе толщину.
/// 
/// ТРЕБОВАНИЯ:
/// 1. На стенах SpriteRenderer.DrawMode должен быть 'Sliced' или 'Tiled'.
/// 2. На стенах BoxCollider2D.AutoTiling должен быть 'true'.
/// </summary>
public class BoundaryManager : MonoBehaviour
{
    [Header("Ссылки на СПРАЙТЫ стен")]
    [Tooltip("Ссылка на SpriteRenderer левой стены")]
    [SerializeField] private SpriteRenderer leftWall;

    [Tooltip("Ссылка на SpriteRenderer правой стены")]
    [SerializeField] private SpriteRenderer rightWall;

    [Tooltip("Ссылка на SpriteRenderer верхней стены")]
    [SerializeField] private SpriteRenderer topWall;

    [Tooltip("Ссылка на SpriteRenderer нижней стены")]
    [SerializeField] private SpriteRenderer bottomWall;

    void Start()
    {
        Execute();
    }

    [Button]
    public void Execute()
    {
        Camera mainCamera = Camera.main;

        // --- Получаем размеры экрана в игровых юнитах ---
        float screenHeight = mainCamera.orthographicSize * 2;
        // (camera.aspect = ширина / высота)
        float screenWidth = screenHeight * mainCamera.aspect;

        // Получаем Z-координату (глубину) для позиционирования
        // (10f - это "глубже" камеры. Замените, если нужно)
        float zPos = 10f;

        // --- 1. Позиционируем стены по краям ---
        // (Используем .transform, т.к. ссылка у нас на SpriteRenderer)

        // (0, 0.5) - центр левой границы экрана
        leftWall.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, zPos));

        // (1, 0.5) - центр правой границы
        rightWall.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, zPos));

        // (0.5, 1) - центр верхней границы
        topWall.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, zPos));

        // (0.5, 0) - центр нижней границы
        bottomWall.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, zPos));


        // --- 2. Растягиваем, СОХРАНЯЯ ТОЛЩИНУ ---
        // Мы меняем .size у SpriteRenderer. 
        // BoxCollider2D с 'Auto Tiling' = true подстроится АВТОМАТИЧЕСКИ.

        // Левая стена: Сохраняем X (толщину), меняем Y (длину)
        leftWall.size = new Vector2(leftWall.size.x, screenHeight);

        // Правая стена: Сохраняем X (толщину), меняем Y (длину)
        rightWall.size = new Vector2(rightWall.size.x, screenHeight);

        // Верхняя стена: Сохраняем Y (толщину), меняем X (длину)
        topWall.size = new Vector2(screenWidth, topWall.size.y);

        // Нижняя стена: Сохраняем Y (толщину), меняем X (длину)
        bottomWall.size = new Vector2(screenWidth, bottomWall.size.y);
    }
}