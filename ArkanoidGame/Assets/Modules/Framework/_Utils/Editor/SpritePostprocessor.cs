using UnityEngine;
using UnityEditor;

public class SpritePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Получаем импортер ассета
        TextureImporter textureImporter = (TextureImporter)assetImporter;

        // Ваши стандартные настройки
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        textureImporter.filterMode = FilterMode.Point;

        // 1. Получаем оригинальную ширину и высоту текстуры.
        int width, height;
        textureImporter.GetSourceTextureWidthAndHeight(out width, out height);

        // 2. Находим наибольшую сторону (ширину или высоту).
        int maxDimension = Mathf.Max(width, height);

        // 3. Вычисляем ближайшую сверху степень двойки (например, для 600 это будет 1024).
        int potSize = Mathf.NextPowerOfTwo(maxDimension);

        // 4. Устанавливаем это оптимальное значение как Max Size.
        textureImporter.maxTextureSize = potSize;
    }
}