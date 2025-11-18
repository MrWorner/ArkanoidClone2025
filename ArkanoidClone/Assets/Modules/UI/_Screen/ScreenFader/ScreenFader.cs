using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MiniIT.UI
{
    public class ScreenFader : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static ScreenFader Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("REQUIRED")]
        [SerializeField, Required]
        private CanvasGroup loadingCanvasGroup = null;

        [BoxGroup("REQUIRED")]
        [SerializeField, Required]
        private Transform hourglassIconTransform = null;

        [BoxGroup("REQUIRED")]
        [SerializeField, Required]
        private TextMeshProUGUI loadingText = null;

        [BoxGroup("SETTINGS")]
        [SerializeField, Range(0.1f, 3.0f)]
        private float fadeDuration = 0.5f;

        [BoxGroup("SETTINGS")]
        [SerializeField, Range(0.1f, 2.0f)]
        private float rotationDuration = 0.5f;

        [BoxGroup("SETTINGS")]
        [SerializeField, Range(0.0f, 1.0f)]
        private float rotationPause = 0.1f;

        [BoxGroup("DEBUG")]
        [SerializeField, ReadOnly]
        private Sequence hourglassSequence = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        [Button]
        public void ShowLoadingScreen()
        {
            ShowLoadingScreen(fadeDuration);
        }

        public void ShowLoadingScreen(float duration)
        {
            if (loadingCanvasGroup == null)
            {
                return;
            }

            hourglassIconTransform.rotation = new Quaternion();

            loadingCanvasGroup.DOKill();
            loadingCanvasGroup.interactable = true;
            loadingCanvasGroup.blocksRaycasts = true;

            loadingCanvasGroup.DOFade(1f, duration).OnComplete(StartHourglassAnimation);
        }

        [Button]
        public void HideLoadingScreen()
        {
            HideLoadingScreen(fadeDuration);
        }

        public void HideLoadingScreen(float duration)
        {
            if (loadingCanvasGroup == null)
            {
                return;
            }

            StopHourglassAnimation();

            loadingCanvasGroup.DOKill();
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;

            loadingCanvasGroup.DOFade(0f, duration);
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            DOTween.Init();

            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = 0f;
                loadingCanvasGroup.interactable = false;
                loadingCanvasGroup.blocksRaycasts = false;
                loadingCanvasGroup.gameObject.SetActive(true);
            }
        }

        private void StartHourglassAnimation()
        {
            if (hourglassIconTransform == null)
            {
                return;
            }

            StopHourglassAnimation();

            hourglassIconTransform.localRotation = Quaternion.identity;
            hourglassSequence = DOTween.Sequence();

            hourglassSequence.Append(hourglassIconTransform.DOLocalRotate(new Vector3(0, 0, -180), rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
                .AppendInterval(rotationPause)
                .Append(hourglassIconTransform.DOLocalRotate(new Vector3(0, 0, -180), rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
                .AppendInterval(rotationPause)
                .SetLoops(-1, LoopType.Restart);
        }

        private void StopHourglassAnimation()
        {
            if (hourglassSequence == null || !hourglassSequence.IsActive())
            {
                return;
            }

            hourglassSequence.Kill();

            if (hourglassIconTransform != null)
            {
                hourglassIconTransform.DOKill();
            }
        }
    }
}