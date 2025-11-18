using DG.Tweening;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace MiniIT.UI
{
    /// <summary>
    /// Base abstract class for all UI Views with DOTween animations.
    /// </summary>
    public abstract class BaseView : MonoBehaviour, IAnimatedView
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public bool IsVisible
        {
            get
            {
                return canvas != null && canvas.enabled && canvasGroup.alpha > 0;
            }
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("REFERENCES")]
        [Tooltip("Requirement: Assign manually in Inspector.")]
        [SerializeField, Required]
        private Canvas canvas = null;

        [BoxGroup("REFERENCES")]
        [Tooltip("Requirement: Assign manually in Inspector.")]
        [SerializeField, Required]
        private CanvasGroup canvasGroup = null;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        protected float defaultFadeDuration = 0.3f;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        [Button("Show Default")]
        public void Show()
        {
            Show(defaultFadeDuration);
        }

        [Button("Hide Default")]
        public void Hide()
        {
            Hide(defaultFadeDuration);
        }

        public void Show(float duration, Action onComplete = null)
        {
            // FIX: We enable the GameObject itself where the Canvas resides.
            // This ensures it works even if the Canvas is on a disabled child object.
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
                canvas.enabled = true;
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                // If alpha is already 1 (e.g., after quick toggle), reset to 0 for effect
                if (canvasGroup.alpha >= 0.99f)
                {
                    canvasGroup.alpha = 0f;
                }

                canvasGroup.DOFade(1f, duration).OnComplete(() =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke();
                    }
                });
            }
        }

        public void Hide(float duration, Action onComplete = null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                canvasGroup.DOFade(0f, duration).OnComplete(() =>
                {
                    // FIX: Disable the entire GameObject to guarantee no rendering cost
                    if (canvas != null)
                    {
                        canvas.gameObject.SetActive(false);
                    }

                    if (onComplete != null)
                    {
                        onComplete.Invoke();
                    }
                });
            }
        }

        // ========================================================================
        // --- PROTECTED & PRIVATE METHODS ---
        // ========================================================================

        protected virtual void Awake()
        {
            // Force hide immediately on startup (no animation)
            ForceHide();
        }

        /// <summary>
        /// Instantly hides the view without events or animations.
        /// </summary>
        private void ForceHide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (canvas != null)
            {
                canvas.enabled = false;
                // Also good practice to ensure GO is inactive if that's the logic
                canvas.gameObject.SetActive(false);
            }
        }
    }
}