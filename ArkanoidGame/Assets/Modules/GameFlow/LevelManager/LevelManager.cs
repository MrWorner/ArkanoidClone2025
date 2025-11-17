using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Required]
    public BrickPool brickPool;

    [Header("Настройки Сетки (Grid Settings)")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 12; // Ваше новое число колонок

    [Header("Размеры Кирпича (В Юнитах)")]
    [Tooltip("Ширина кирпича (шаг по X). Для 12 колонок вы считали: ~0.69333")]
    [SerializeField] private float brickWidth = 0.69333f;

    [Tooltip("Высота кирпича (шаг по Y). Для сохранения пропорций: ~0.34666")]
    [SerializeField] private float brickHeight = 0.34666f;

    [Header("Типы Кирпичей (Brick Types)")]
    [Tooltip("Перетащите сюда ваши ассеты BrickType (Blue, Red, Steel)")]
    [SerializeField] private List<BrickType> levelBrickTypes;

    void Start()
    {
        // Пусто, так как GameManager сам вызывает BuildLevel() при старте игры
    }

    [Button("Построить Уровень")]
    public void BuildLevel()
    {
        // --- 1. Проверки ---
        if (brickPool == null)
        {
            Debug.LogError("LevelManager: Не назначен BrickPool!");
            return;
        }
        if (levelBrickTypes == null || levelBrickTypes.Count == 0)
        {
            Debug.LogError("LevelManager: Список типов кирпичей пуст!");
            return;
        }
        if (brickWidth == 0 || brickHeight == 0)
        {
            Debug.LogError("LevelManager: Размеры кирпича не могут быть 0!");
            return;
        }

        // --- 2. Очистка старого уровня ---
        if (Application.isPlaying)
        {
            brickPool.ReturnAllActiveBricks();
        }
        else
        {
            brickPool.DestroyAllBricksEditor();
        }

        // --- 3. Расчет начальной позиции (Центрирование) ---
        // Используем transform.position самого LevelManager как центр
        Vector2 currentCenter = transform.position;

        float totalGridWidth = (cols - 1) * brickWidth;
        float totalGridHeight = (rows - 1) * brickHeight;

        Vector2 startPos = new Vector2(
            currentCenter.x - (totalGridWidth / 2f),
            currentCenter.y + (totalGridHeight / 2f)
        );

        int destroyableBrickCount = 0;

        // --- 4. Генерация сетки ---
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // А. Получаем кирпич
                Brick newBrick;
                if (Application.isPlaying)
                {
                    newBrick = brickPool.GetBrick();
                }
                else
                {
                    newBrick = brickPool.GetBrickEditor();
                }

                // Б. Выбираем случайный тип
                BrickType randomType = levelBrickTypes[Random.Range(0, levelBrickTypes.Count)];

                // В. Настраиваем кирпич (Спрайт, Цвет, HP)
                newBrick.Setup(randomType);

                // Г. Считаем только разрушаемые (для условия победы)
                if (!randomType.isIndestructible)
                {
                    destroyableBrickCount++;
                }

                // Д. Устанавливаем позицию
                float xPos = startPos.x + (c * brickWidth);
                float yPos = startPos.y - (r * brickHeight);
                newBrick.transform.position = new Vector2(xPos, yPos);
            }
        }

        // --- 5. Сообщаем GameManager кол-во кирпичей ---
        if (Application.isPlaying)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetBrickCount(destroyableBrickCount);
            }
        }
    }
}