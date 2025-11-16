using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Настройки Сетки Уровня")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 8;
    [SerializeField] private float horizontalSpacing = 1.1f;
    [SerializeField] private float verticalSpacing = 0.5f;
    [SerializeField] private Vector2 gridStartPosition; // Точка старта (левый верх)

    void Start()
    {
        BuildLevel();
    }

    [ContextMenu("Build Level")] // Магия: добавит кнопку в инспекторе
    public void BuildLevel()
    {
        // 1. Получаем доступ к нашему Пулу
        BrickPool pool = BrickPool.Instance;
        if (pool == null)
        {
            Debug.LogError("BrickPool не найден на сцене!");
            return;
        }

        // 2. Убираем все старые кирпичи (если это 2+ уровень)
        pool.ReturnAllActiveBricks();

        // 3. Строим сетку
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 4. БЕРЕМ КИРПИЧ ИЗ ПУЛА
                Brick newBrick = pool.GetBrick();

                // 5. Считаем его позицию
                float xPos = gridStartPosition.x + (c * horizontalSpacing);
                float yPos = gridStartPosition.y - (r * verticalSpacing);

                // 6. Ставим его на место
                newBrick.transform.position = new Vector2(xPos, yPos);
            }
        }
    }
}