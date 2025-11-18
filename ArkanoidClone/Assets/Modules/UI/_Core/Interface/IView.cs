using System;

// Базовые интерфейсы (как вы и просили)
public interface IView
{
    void Show();
    void Hide();
    bool IsVisible { get; }
}