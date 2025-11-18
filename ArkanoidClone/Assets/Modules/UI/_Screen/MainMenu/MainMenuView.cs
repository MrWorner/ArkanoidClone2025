using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using System;
using MiniIT.UI;

public class MainMenuView : BaseView
{
    #region Main Buttons
    [BoxGroup("Main Menu UI"), Required]
    [SerializeField] public Button NewGameButton;

    // Новая кнопка выхода
    [BoxGroup("Main Menu UI"), Required]
    [SerializeField] public Button QuitButton;
    #endregion

    #region Confirmation Popup
    // Ссылка на объект панели (Panel_Confirmation), внутри которой текст и 2 кнопки
    [BoxGroup("Confirmation Popup"), Required]
    [SerializeField] private GameObject _confirmationPanel;

    [BoxGroup("Confirmation Popup"), Required]
    [SerializeField] public Button ConfirmYesButton;

    [BoxGroup("Confirmation Popup"), Required]
    [SerializeField] public Button ConfirmNoButton;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        // При старте гарантированно скрываем попап
        SetConfirmationActive(false);
    }

    public void SetConfirmationActive(bool isActive)
    {
        if (_confirmationPanel != null)
            _confirmationPanel.SetActive(isActive);
    }
}