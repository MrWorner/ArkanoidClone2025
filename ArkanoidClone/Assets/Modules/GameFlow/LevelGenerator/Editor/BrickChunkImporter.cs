#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
// Убрали using Sirenix...

// Наследуемся от стандартного EditorWindow вместо OdinEditorWindow
public class BrickChunkImporter : EditorWindow
{
    // Поля теперь просто приватные переменные, атрибуты [Required] тут не сработают автоматически
    private TextAsset sourceFile;
    private BrickTextMapSO textMap;
    private string outputPath = "Assets/Modules/Data/Chunks";

    [MenuItem("Tools/Arkanoid/Chunk Importer")]
    private static void OpenWindow()
    {
        // Создаем и показываем стандартное окно
        GetWindow<BrickChunkImporter>("Chunk Importer").Show();
    }

    // ЭТО ГЛАВНОЕ ИЗМЕНЕНИЕ:
    // Вместо магии атрибутов мы вручную говорим Unity, как рисовать интерфейс
    private void OnGUI()
    {
        // 1. Рисуем заголовок (аналог [Title])
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // 2. Рисуем поля ввода (аналог [SerializeField])
        // ObjectField позволяет перетаскивать файлы
        sourceFile = (TextAsset)EditorGUILayout.ObjectField("Source File (.txt)", sourceFile, typeof(TextAsset), false);
        textMap = (BrickTextMapSO)EditorGUILayout.ObjectField("Text Map SO", textMap, typeof(BrickTextMapSO), false);

        GUILayout.Space(5);

        // 3. Рисуем выбор папки (аналог [FolderPath])
        // Делаем поле текстовым + кнопку "Выбрать"
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Превращаем абсолютный путь в относительный Assets/...
                if (path.StartsWith(Application.dataPath))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    // Если выбрали папку внутри проекта, Unity вернет абсолютный путь, нужно обрезать
                    Debug.LogWarning("Please select a folder inside the Assets folder.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);

        // 4. Рисуем кнопку (аналог [Button])
        // GUI.backgroundColor делает кнопку цветной (опционально)
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Process TXT File", GUILayout.Height(40)))
        {
            ProcessFile();
        }
        GUI.backgroundColor = Color.white; // Возвращаем цвет обратно
    }

    // Логика осталась вашей, только убрали атрибут [Button]
    private void ProcessFile()
    {
        if (sourceFile == null || textMap == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign Source File and Text Map!", "OK");
            return;
        }

        // --- Ваша логика обработки путей ---
        string relativePath = outputPath;
        // Небольшая страховка, если путь уже относительный
        if (!relativePath.StartsWith("Assets") && relativePath.Contains(Application.dataPath))
        {
            relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
        }

        if (!AssetDatabase.IsValidFolder(relativePath))
        {
            Directory.CreateDirectory(relativePath);
            AssetDatabase.Refresh();
        }
        // ------------------------

        string text = sourceFile.text;
        // Добавил System.StringSplitOptions для надежности
        string[] blocks = text.Split(new[] { "\r\n\r\n", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        int count = 0;

        foreach (var block in blocks)
        {
            string[] lines = block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) continue;

            string rawName = lines[0].Trim();
            string safeName = Regex.Replace(rawName, @"[^a-zA-Z0-9_]", "");

            string path = $"{relativePath}/Chunk_{safeName}.asset";

            BrickChunkSO chunk = AssetDatabase.LoadAssetAtPath<BrickChunkSO>(path);

            if (chunk == null)
            {
                chunk = ScriptableObject.CreateInstance<BrickChunkSO>();
                AssetDatabase.CreateAsset(chunk, path);
            }

            chunk.bricks.Clear();
            chunk.width = 6;
            chunk.height = lines.Length - 1;

            int dataLinesCount = lines.Length - 1;

            for (int y = 0; y < dataLinesCount; y++)
            {
                // Проверка на выход за границы массива
                if (y + 1 >= lines.Length) continue;

                string line = lines[y + 1];
                string cleanLine = line.Replace("[", "").Replace("]", "");

                int gridY = (dataLinesCount - 1) - y;

                for (int x = 0; x < cleanLine.Length; x++)
                {
                    if (x >= cleanLine.Length) break;

                    char symbol = cleanLine[x];
                    BrickTypeSO type = textMap.GetBrickType(symbol);

                    if (type != null)
                    {
                        chunk.bricks.Add(new BrickChunkSO.BrickData
                        {
                            position = new Vector2Int(x, gridY),
                            type = type
                        });
                    }
                }
            }

            EditorUtility.SetDirty(chunk);
            count++;
        }

        AssetDatabase.SaveAssets();
        // Используем DisplayDialog вместо простого лога, чтобы видеть результат явно
        EditorUtility.DisplayDialog("Success", $"Processed {count} chunks successfully at:\n{relativePath}", "Cool");
    }
}
#endif