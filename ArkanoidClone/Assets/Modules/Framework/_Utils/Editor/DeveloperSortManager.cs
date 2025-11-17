using UnityEngine;
using UnityEditor;
using System.Linq;

// Скрипт для создания окна редактора, которое позволяет сортировать SpriteRenderers.
public class DeveloperSortManager : EditorWindow
{
    // Поле для хранения GameObject, который мы будем сортировать.
    private GameObject targetGameObject;

    // Этот метод создает пункт меню в Unity, чтобы открыть наше окно.
    [MenuItem("Tools/Developer Sprite Sorter")]
    public static void ShowWindow()
    {
        // Открываем существующее окно или создаем новое.
        GetWindow<DeveloperSortManager>("Developer Sprite Sorter");
    }

    // Метод, который вызывается для отрисовки графического интерфейса окна.
    private void OnGUI()
    {
        GUILayout.Label("Sprite Sorter Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // Поле для перетаскивания GameObject в окно.
        // Используем EditorGUILayout.ObjectField для удобного выбора объекта.
        targetGameObject = (GameObject)EditorGUILayout.ObjectField(
            "Target GameObject",
            targetGameObject,
            typeof(GameObject),
            true);

        EditorGUILayout.Space(20);

        // Кнопка для запуска сортировки. Она будет активна, только если targetGameObject не пуст.
        GUI.enabled = targetGameObject != null;
        if (GUILayout.Button("Start Sorting"))
        {
            StartSorting();
        }
        GUI.enabled = true;
    }

    // Метод, который выполняет фактическую сортировку.
    private void StartSorting()
    {
        if (targetGameObject == null)
        {
            Debug.LogError("Ошибка: Пожалуйста, выберите GameObject для сортировки.");
            return;
        }

        // Получаем все компоненты SpriteRenderer на выбранном объекте и его дочерних элементах.
        SpriteRenderer[] spriteRenderers = targetGameObject.GetComponentsInChildren<SpriteRenderer>();

        // Если не найдено спрайтов, выводим сообщение об ошибке.
        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning("Предупреждение: На объекте и его дочерних элементах не найдено компонентов SpriteRenderer.");
            return;
        }

        // Сортируем спрайты по их позиции по оси Y в возрастающем порядке.
        // SpriteRenderer с меньшим значением Y будет первым в списке,
        // что позволит нам назначить ему меньший sortingOrder.
        var sortedSprites = spriteRenderers.OrderBy(sr => sr.transform.position.y).ToList();

        // Переменная для присвоения нового порядка.
        int order = 0;

        // Применяем новый порядок сортировки.
        foreach (var sr in sortedSprites)
        {
            // Устанавливаем sortingOrder для каждого спрайта.
            sr.sortingOrder = 999999 + order;
            order--;
        }

        Debug.Log($"Успешно отсортировано {sortedSprites.Count} спрайтов на основе их позиции по оси Y.");
    }
}
