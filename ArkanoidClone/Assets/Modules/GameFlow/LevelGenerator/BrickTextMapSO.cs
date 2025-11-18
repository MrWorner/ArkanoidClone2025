using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MiniIT.BRICK;

[CreateAssetMenu(fileName = "BrickTextMap", menuName = "Arkanoid/Generation/Brick Text Map")]
public class BrickTextMapSO : ScriptableObject
{
    [System.Serializable]
    public struct CharToBrickMapping
    {
        public char symbol;
        public BrickTypeSO brickType;
    }

    public List<CharToBrickMapping> mappings;

    public BrickTypeSO GetBrickType(char symbol)
    {
        var map = mappings.FirstOrDefault(m => m.symbol == symbol);
        // Если символ не найден (или это пробел/пустота), вернем null
        return map.brickType;
    }
}