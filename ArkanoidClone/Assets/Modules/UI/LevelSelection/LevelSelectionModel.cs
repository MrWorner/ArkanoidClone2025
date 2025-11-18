// ДЕМОНСТРАЦИЯ: Простой класс данных
using System;

public class LevelSelectionModel
{
    public int CurrentLevel { get; private set; } = 1;
    public int MinLevel { get; } = 1;
    public int MaxLevel { get; } = 9999;

    public void SetLevel(int level)
    {
        // ДЕМОНСТРАЦИЯ: Math.Clamp (C# feature)
        CurrentLevel = Math.Clamp(level, MinLevel, MaxLevel);
    }
}