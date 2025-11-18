using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader _instance;

    #region Поля: Required
    [BoxGroup("Required"), Required, SerializeField] private CanvasGroup _loadingCanvasGroup;
    [BoxGroup("Required"), Required, SerializeField] private Transform _hourglassIconTransform;
    [BoxGroup("Required"), Required, SerializeField] private TextMeshProUGUI _loadingText;
    #endregion

    #region Поля
    [BoxGroup("SETTINGS"), SerializeField, Range(0.1f, 3.0f)] private float _fadeDuration = 0.5f;
    [BoxGroup("SETTINGS"), SerializeField, Range(0.1f, 2.0f)] private float _rotationDuration = 0.5f;
    [BoxGroup("SETTINGS"), SerializeField, Range(0.0f, 1.0f)] private float _rotationPause = 0.1f;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Sequence _hourglassSequence;
    #endregion

    #region Свойства
    public static ScreenFader Instance { get => _instance; }
    #endregion

    #region Методы UNITY
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            ///DebugUtils.LogInstanceAlreadyExists(this);
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        DOTween.Init();

        if (_loadingCanvasGroup != null)
        {
            _loadingCanvasGroup.alpha = 0f;
            _loadingCanvasGroup.interactable = false;
            _loadingCanvasGroup.blocksRaycasts = false;
            _loadingCanvasGroup.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Публичные методы
    [Button]
    public void ShowLoadingScreen()
    {
        ShowLoadingScreen(_fadeDuration);
    }

    public void ShowLoadingScreen(float duration)
    {
        if (_loadingCanvasGroup == null) return;
        _hourglassIconTransform.rotation = new Quaternion();
        _loadingCanvasGroup.DOKill();
        _loadingCanvasGroup.interactable = true;
        _loadingCanvasGroup.blocksRaycasts = true;
        _loadingCanvasGroup.DOFade(1f, duration).OnComplete(StartHourglassAnimation);
    }

    [Button]
    public void HideLoadingScreen()
    {
        HideLoadingScreen(_fadeDuration);
    }

    public void HideLoadingScreen(float duration)
    {
        if (_loadingCanvasGroup == null) return;
        StopHourglassAnimation();
        _loadingCanvasGroup.DOKill();
        _loadingCanvasGroup.interactable = false;
        _loadingCanvasGroup.blocksRaycasts = false;
        _loadingCanvasGroup.DOFade(0f, duration);
    }
    #endregion

    #region Личные методы
    private void StartHourglassAnimation()
    {
        if (_hourglassIconTransform == null) return;
        StopHourglassAnimation();
        _hourglassIconTransform.localRotation = Quaternion.identity;
        _hourglassSequence = DOTween.Sequence();
        _hourglassSequence.Append(_hourglassIconTransform.DOLocalRotate(new Vector3(0, 0, -180), _rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .AppendInterval(_rotationPause)
            .Append(_hourglassIconTransform.DOLocalRotate(new Vector3(0, 0, -180), _rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .AppendInterval(_rotationPause)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopHourglassAnimation()
    {
        if (_hourglassSequence == null || !_hourglassSequence.IsActive()) return;
        _hourglassSequence?.Kill();
        _hourglassIconTransform?.DOKill();
    }
    #endregion
}