using NaughtyAttributes;
using TMPro;
using UnityEngine;


public class FPSCounter : MonoBehaviour
{
    #region Поля: Required
    [BoxGroup("Required"), Required, SerializeField] private TextMeshProUGUI _fpsText;
    #endregion Поля: Required

    #region Поля
    [BoxGroup("SETTINGS"), SerializeField] private float _updateInterval = 0.5f;
    #endregion Поля

    #region Свойства
    private float _accumulatedFrames = 0;
    private float _timeLeft;
    private int _lastFPS;
    #endregion Свойства

    #region Методы UNITY
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (_fpsText == null)
        {
            enabled = false;
            return;
        }

        _timeLeft = _updateInterval;
    }

    private void Update()
    {
        _timeLeft -= Time.unscaledDeltaTime;
        _accumulatedFrames++;

        if (_timeLeft <= 0.0f)
        {
            _lastFPS = (int)(_accumulatedFrames / _updateInterval);
            _fpsText.text = "FPS: " + _lastFPS;

            ///ColoredDebug.CLog(gameObject, "<color=cyan>FPSCounter:</color> Обновление значения. Текущий FPS: <color=yellow>{0}</color>.", _ColoredDebug, _lastFPS);

            _timeLeft = _updateInterval;
            _accumulatedFrames = 0;
        }
    }
    #endregion Методы UNITY
}