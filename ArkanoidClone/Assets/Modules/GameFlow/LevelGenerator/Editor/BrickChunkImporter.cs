#if UNITY_EDITOR
using MiniIT.BRICK;
using MiniIT.CORE;
using MiniIT.LEVELS;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MiniIT.EDITOR
{
    public class BrickChunkImporter : EditorWindow
    {
        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private TextAsset sourceFile;
        private BrickTextMapSO textMap;
        private string outputPath = "Assets/Modules/Data/Chunks";

        // ========================================================================
        // --- MENU ITEMS ---
        // ========================================================================

        [MenuItem("Tools/Arkanoid/Chunk Importer")]
        private static void OpenWindow()
        {
            GetWindow<BrickChunkImporter>("Chunk Importer").Show();
        }

        // ========================================================================
        // --- GUI LOGIC ---
        // ========================================================================

        private void OnGUI()
        {
            // 1. Draw Header
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // 2. Draw Input Fields
            sourceFile = (TextAsset)EditorGUILayout.ObjectField("Source File (.txt)", sourceFile, typeof(TextAsset), false);
            textMap = (BrickTextMapSO)EditorGUILayout.ObjectField("Text Map SO", textMap, typeof(BrickTextMapSO), false);

            GUILayout.Space(5);

            // 3. Draw Folder Selection
            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert absolute path to relative Assets/...
                    if (path.StartsWith(Application.dataPath))
                    {
                        outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogWarning("Please select a folder inside the Assets folder.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 4. Draw Action Button
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Process TXT File", GUILayout.Height(40)))
            {
                ProcessFile();
            }

            GUI.backgroundColor = Color.white;
        }

        // ========================================================================
        // --- PROCESSING LOGIC ---
        // ========================================================================

        private void ProcessFile()
        {
            if (sourceFile == null || textMap == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign Source File and Text Map!", "OK");
                return;
            }

            // --- Path Validation ---
            string relativePath = outputPath;

            if (!relativePath.StartsWith("Assets") && relativePath.Contains(Application.dataPath))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            }

            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                Directory.CreateDirectory(relativePath);
                AssetDatabase.Refresh();
            }

            // --- Parsing ---
            string text = sourceFile.text;
            string[] blocks = text.Split(new[] { "\r\n\r\n", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            int count = 0;

            foreach (string block in blocks)
            {
                string[] lines = block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                {
                    continue;
                }

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
                    // Boundary check
                    if (y + 1 >= lines.Length)
                    {
                        continue;
                    }

                    string line = lines[y + 1];
                    string cleanLine = line.Replace("[", "").Replace("]", "");

                    // Reverse Y for grid coordinates (Bottom-Up)
                    int gridY = (dataLinesCount - 1) - y;

                    for (int x = 0; x < cleanLine.Length; x++)
                    {
                        if (x >= cleanLine.Length)
                        {
                            break;
                        }

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
            EditorUtility.DisplayDialog("Success", $"Processed {count} chunks successfully at:\n{relativePath}", "Cool");
        }
    }
}
#endif