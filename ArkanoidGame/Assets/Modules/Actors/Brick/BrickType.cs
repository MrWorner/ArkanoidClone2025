using UnityEngine;

[CreateAssetMenu(fileName = "NewBrickType", menuName = "Arkanoid/Brick Type")]
public class BrickType : ScriptableObject
{
    [Header("Визуал")]
    public Sprite sprite;

    // --- НОВОЕ ПОЛЕ ---
    [Tooltip("Цвет (tint), в который будет окрашен спрайт")]
    public Color color = Color.white; // По умолчанию - белый
    // -------------------

    [Header("Игровая логика")]
    [Tooltip("Сколько очков дается за этот кирпич")]
    public int points;

    [Tooltip("Неразрушаемый?")]
    public bool isIndestructible;

    [Tooltip("Сколько раз нужно ударить (1 = стандарт)")]
    public int health = 1;
}