using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class BrickChunkImporter : OdinEditorWindow
{
    [MenuItem("Tools/Arkanoid/Chunk Importer")]
    private static void OpenWindow() => GetWindow<BrickChunkImporter>().Show();

    [Title("Settings")]
    [SerializeField, Required] private TextAsset sourceFile;
    [SerializeField, Required] private BrickTextMapSO textMap;
    [SerializeField, FolderPath] private string outputPath = "Assets/Modules/Data/Chunks";

    [Button("Process TXT File", ButtonSizes.Large)]
    private void ProcessFile()
    {
        if (sourceFile == null || textMap == null) return;

        // --- ИСПРАВЛЕНИЕ ПУТИ ---
        // 1. Превращаем абсолютный путь (C:/...) в относительный (Assets/...)
        string relativePath = outputPath;
        if (relativePath.StartsWith(Application.dataPath))
        {
            relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
        }

        // 2. Убедимся, что папка существует, иначе CreateAsset выдаст ошибку
        if (!AssetDatabase.IsValidFolder(relativePath))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(relativePath);
            string newFolder = System.IO.Path.GetFileName(relativePath);
            // Если папки нет - это сложнее создать через AssetDatabase, 
            // поэтому просто используем System.IO для надежности, но CreateAsset любит существующие папки.
            // Самый простой способ для Editor-скрипта:
            System.IO.Directory.CreateDirectory(outputPath); // Создаем физически
            AssetDatabase.Refresh(); // Обновляем Unity, чтобы он увидел папку
        }
        // ------------------------

        string text = sourceFile.text;
        string[] blocks = text.Split(new[] { "\r\n\r\n", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        int count = 0;

        foreach (var block in blocks)
        {
            string[] lines = block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) continue;

            string rawName = lines[0].Trim();
            string safeName = Regex.Replace(rawName, @"[^a-zA-Z0-9_]", "");

            // ИСПРАВЛЕНО: Используем relativePath вместо outputPath
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
                string line = lines[y + 1];
                string cleanLine = line.Replace("[", "").Replace("]", "");

                int gridY = (dataLinesCount - 1) - y;

                for (int x = 0; x < cleanLine.Length; x++)
                {
                    // Защита от выхода за границы строки (если в файле ошибка)
                    if (x >= cleanLine.Length) break;

                    char symbol = cleanLine[x];
                    BrickTypeSO type = textMap.GetBrickType(symbol); // У вас BrickTypeSO теперь

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
        Debug.Log($"<b>[Importer]</b> Processed {count} chunks successfully at {relativePath}!");
    }
}