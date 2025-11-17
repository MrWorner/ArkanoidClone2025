using UnityEditor;
using UnityEngine;
using System.Text;
using System.Linq;

public class HierarchyCopier : EditorWindow
{
    // --- ОПЦИЯ 1: Копировать иерархию только с компонентами ---
    // Горячая клавиша: Ctrl+Alt+C
    [MenuItem("GameObject/Copy Hierarchy with Components %#c", false, 40)]
    public static void CopyHierarchyWithComponents()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Не выбран ни один GameObject для копирования иерархии.");
            return;
        }

        GameObject selectedGo = Selection.activeGameObject;
        var stringBuilder = new StringBuilder();

        // Начинаем рекурсивный обход с выбранного объекта
        ProcessTransform(selectedGo.transform, "", stringBuilder);

        EditorGUIUtility.systemCopyBuffer = stringBuilder.ToString();
        Debug.Log($"Иерархия объекта '{selectedGo.name}' скопирована в буфер обмена.");
    }

    // --- ОПЦИЯ 2: Копировать иерархию с компонентами и позициями ---
    // Горячая клавиша: Ctrl+Alt+P
    [MenuItem("GameObject/Copy Hierarchy with Components & Positions %#p", false, 41)]
    public static void CopyHierarchyWithComponentsAndPositions()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Не выбран ни один GameObject для копирования иерархии.");
            return;
        }

        GameObject selectedGo = Selection.activeGameObject;
        var stringBuilder = new StringBuilder();

        // Начинаем рекурсивный обход, но используем новую функцию, которая включает позиции
        ProcessTransformWithPosition(selectedGo.transform, "", stringBuilder);

        EditorGUIUtility.systemCopyBuffer = stringBuilder.ToString();
        Debug.Log($"Иерархия объекта '{selectedGo.name}' с позициями скопирована в буфер обмена.");
    }


    /// <summary>
    /// Рекурсивный метод для обхода иерархии и сбора информации о компонентах.
    /// </summary>
    private static void ProcessTransform(Transform target, string prefix, StringBuilder sb)
    {
        string componentsInfo = string.Join(", ", target.GetComponents<Component>().Where(c => c != null).Select(component =>
        {
            string componentName = component.GetType().Name;
            if (component is Behaviour behaviour)
            {
                string status = behaviour.enabled ? "enabled" : "disabled";
                return $"{componentName} ({status})";
            }
            return componentName;
        }));

        string objectStatus = target.gameObject.activeSelf ? "" : " (inactive)";

        // Формируем строку БЕЗ позиции
        sb.AppendLine($"{prefix}{target.name}{objectStatus} (Компоненты: '{componentsInfo}')");

        string childIndent = prefix.Replace("├── ", "│   ").Replace("└── ", "    ");

        for (int i = 0; i < target.childCount; i++)
        {
            Transform child = target.GetChild(i);
            bool isLast = (i == target.childCount - 1);
            string childPrefix = isLast ? "└── " : "├── ";
            ProcessTransform(child, childIndent + childPrefix, sb);
        }
    }

    /// <summary>
    /// НОВАЯ ВЕРСИЯ: Рекурсивный метод, который также включает transform.position.
    /// </summary>
    private static void ProcessTransformWithPosition(Transform target, string prefix, StringBuilder sb)
    {
        string componentsInfo = string.Join(", ", target.GetComponents<Component>().Where(c => c != null).Select(component =>
        {
            string componentName = component.GetType().Name;
            if (component is Behaviour behaviour)
            {
                string status = behaviour.enabled ? "enabled" : "disabled";
                return $"{componentName} ({status})";
            }
            return componentName;
        }));

        string objectStatus = target.gameObject.activeSelf ? "" : " (inactive)";

        // Получаем позицию и форматируем ее для лучшей читаемости (3 знака после запятой)
        string positionInfo = $"Position: {target.position.ToString("F3")}";

        // Формируем строку, ВКЛЮЧАЯ позицию
        sb.AppendLine($"{prefix}{target.name}{objectStatus} [{positionInfo}] (Компоненты: '{componentsInfo}')");

        string childIndent = prefix.Replace("├── ", "│   ").Replace("└── ", "    ");

        for (int i = 0; i < target.childCount; i++)
        {
            Transform child = target.GetChild(i);
            bool isLast = (i == target.childCount - 1);
            string childPrefix = isLast ? "└── " : "├── ";
            // Рекурсивно вызываем эту же версию метода для дочерних объектов
            ProcessTransformWithPosition(child, childIndent + childPrefix, sb);
        }
    }
}