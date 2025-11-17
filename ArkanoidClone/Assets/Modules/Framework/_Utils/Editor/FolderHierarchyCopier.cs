using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Этот скрипт создает окно редактора, в которое можно перетащить папку
/// для копирования ее структуры в буфер обмена.
/// ВАЖНО: Этот файл должен находиться в папке с названием "Editor".
/// </summary>
public class FolderHierarchyCopier : EditorWindow
{
    private DefaultAsset targetFolder = null;
    private Vector2 scrollPosition;
    private string hierarchyResult = "Перетащите сюда папку и нажмите одну из кнопок...";

    // Добавляем пункт меню в Unity Editor по пути "Tools/Folder Hierarchy Copier"
    [MenuItem("Tools/Folder Hierarchy Copier")]
    public static void ShowWindow()
    {
        // Эта команда находит существующее окно или создает новое.
        GetWindow<FolderHierarchyCopier>("Folder Hierarchy Copier").Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Копирование иерархии папок", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Перетащите папку из окна Project в поле ниже, чтобы скопировать ее структуру.", MessageType.Info);

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Целевая папка", targetFolder, typeof(DefaultAsset), false);

        // Кнопки будут активны только если выбрана папка
        GUI.enabled = IsValidFolder();

        // Горизонтальная группа для двух кнопок
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Скопировать (с файлами)", GUILayout.Height(40)))
        {
            CopyHierarchy(false);
        }

        if (GUILayout.Button("Скопировать (только папки)", GUILayout.Height(40)))
        {
            CopyHierarchy(true);
        }

        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        EditorGUILayout.LabelField("Предпросмотр:", EditorStyles.miniBoldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
        EditorGUILayout.TextArea(hierarchyResult, EditorStyles.miniLabel, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private bool IsValidFolder()
    {
        if (targetFolder == null) return false;
        string path = AssetDatabase.GetAssetPath(targetFolder);
        return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
    }

    private void CopyHierarchy(bool foldersOnly)
    {
        if (!IsValidFolder())
        {
            hierarchyResult = "Ошибка: Пожалуйста, выберите действительную папку.";
            this.ShowNotification(new GUIContent("Выбранный объект не является папкой!"));
            return;
        }

        string path = AssetDatabase.GetAssetPath(targetFolder);
        var stringBuilder = new StringBuilder();

        // Сначала добавляем корневую папку
        stringBuilder.AppendLine(Path.GetFileName(path));

        // Затем рекурсивно обрабатываем ее содержимое
        ProcessDirectoryContents(path, "", stringBuilder, foldersOnly);

        hierarchyResult = stringBuilder.ToString();
        EditorGUIUtility.systemCopyBuffer = hierarchyResult;

        string message = foldersOnly ? "Иерархия папок (без файлов) скопирована!" : "Иерархия (с файлами) скопирована!";
        Debug.Log($"Иерархия папки '{Path.GetFileName(path)}' скопирована в буфер обмена.");
        this.ShowNotification(new GUIContent(message));
    }

    /// <summary>
    /// ОБНОВЛЕННЫЙ МЕТОД: Рекурсивно обрабатывает содержимое директории,
    /// используя символы '├──' и '└──' для отрисовки дерева.
    /// </summary>
    private static void ProcessDirectoryContents(string path, string prefix, StringBuilder sb, bool foldersOnly)
    {
        try
        {
            // Получаем список всех дочерних папок
            List<string> directories = Directory.GetDirectories(path).ToList();

            // Получаем список файлов (если требуется) и отфильтровываем .meta файлы
            List<string> files = foldersOnly
                ? new List<string>()
                : Directory.GetFiles(path).Where(f => Path.GetExtension(f) != ".meta").ToList();

            // Объединяем папки и файлы в один список для корректного определения последнего элемента
            List<string> allEntries = directories.Concat(files).ToList();

            for (int i = 0; i < allEntries.Count; i++)
            {
                string currentEntryPath = allEntries[i];
                bool isLast = (i == allEntries.Count - 1);

                // Определяем префикс для текущего элемента ('├──' или '└──')
                string entryPrefix = isLast ? "└── " : "├── ";
                sb.AppendLine($"{prefix}{entryPrefix}{Path.GetFileName(currentEntryPath)}");

                // Если текущий элемент - это папка, запускаем рекурсию для нее
                if (Directory.Exists(currentEntryPath))
                {
                    // Создаем новый префикс для дочерних элементов
                    string childPrefix = prefix + (isLast ? "    " : "│   ");
                    ProcessDirectoryContents(currentEntryPath, childPrefix, sb, foldersOnly);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при обработке пути {path}: {ex.Message}");
        }
    }
}