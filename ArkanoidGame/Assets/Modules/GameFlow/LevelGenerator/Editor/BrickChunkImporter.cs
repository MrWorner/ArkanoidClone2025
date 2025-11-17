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

        string text = sourceFile.text;
        // Разделяем на блоки по двойному переносу строки
        string[] blocks = text.Split(new[] { "\r\n\r\n", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        int count = 0;

        foreach (var block in blocks)
        {
            string[] lines = block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) continue;

            // 1. Имя чанка (первая строка)
            string rawName = lines[0].Trim();
            // Убираем "Chunk XX" если есть, оставляем суть
            string safeName = Regex.Replace(rawName, @"[^a-zA-Z0-9_]", "");

            // 2. Создаем или загружаем SO
            string path = $"{outputPath}/Chunk_{safeName}.asset";
            BrickChunkSO chunk = AssetDatabase.LoadAssetAtPath<BrickChunkSO>(path);

            if (chunk == null)
            {
                chunk = ScriptableObject.CreateInstance<BrickChunkSO>();
                AssetDatabase.CreateAsset(chunk, path);
            }

            chunk.bricks.Clear();
            chunk.width = 6; // Жестко задаем, или вычисляем dynamic
            chunk.height = lines.Length - 1; // Минус заголовок

            // 3. Парсим сетку (читаем СНИЗУ ВВЕРХ, чтобы Y=0 был внизу, как в Unity Grid)
            // Но в текстовом файле визуально верх - это верх.
            // LevelManager обычно строит rows 0..N.
            // Давайте читать как есть: строка 1 файла = row (Max), последняя строка = row 0.

            int dataLinesCount = lines.Length - 1;

            for (int y = 0; y < dataLinesCount; y++)
            {
                string line = lines[y + 1]; // +1 пропускаем заголовок
                // Удаляем скобки []
                string cleanLine = line.Replace("[", "").Replace("]", "");

                // В Unity Y=0 это низ. В файле первая строка это верх.
                // Значит Y файла нужно инвертировать для Unity координат.
                int gridY = (dataLinesCount - 1) - y;

                for (int x = 0; x < cleanLine.Length; x++)
                {
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
        Debug.Log($"<b>[Importer]</b> Processed {count} chunks!");
    }
}