using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBrickPalette", menuName = "Arkanoid/Generation/Brick Palette")]
public class BrickPaletteSO : ScriptableObject
{
    [Tooltip("Список типов от самого слабого (0) до самого сильного")]
    public List<BrickTypeSO> tiers;

    public BrickTypeSO GetTier(int index)
    {
        if (tiers.Count == 0) return null;
        // Clamping: если индекс больше списка, берем самый последний (самый дорогой)
        return tiers[Mathf.Clamp(index, 0, tiers.Count - 1)];
    }

    public int Count => tiers.Count;
}