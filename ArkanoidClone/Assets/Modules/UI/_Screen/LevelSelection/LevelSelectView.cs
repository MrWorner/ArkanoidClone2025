using DG.Tweening;
using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.UI
{
    public class LevelSelectView : BaseView, IDataView<int>
    {
        // ========================================================================
        // --- EVENTS ---
        // ========================================================================

        public event Action OnBackClicked;
        public event Action OnStartClicked;
        public event Action OnNextClicked;
        public event Action OnPrevClicked;
        public event Action OnNextBigClicked;
        public event Action OnPrevBigClicked;

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("UI REFERENCES")]
        [SerializeField, Required]
        private TextMeshProUGUI levelText = null;

        [BoxGroup("BUTTONS")]
        [SerializeField, Required]
        private Button btnBack = null;

        [BoxGroup("BUTTONS")]
        [SerializeField, Required]
        private Button btnStart = null;

        [BoxGroup("NAVIGATION")]
        [SerializeField, Required]
        private Button btnPrevBig = null;  // <<

        [BoxGroup("NAVIGATION")]
        [SerializeField, Required]
        private Button btnPrev = null;     // <

        [BoxGroup("NAVIGATION")]
        [SerializeField, Required]
        private Button btnNext = null;     // >

        [BoxGroup("NAVIGATION")]
        [SerializeField, Required]
        private Button btnNextBig = null;  // >>

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void UpdateView(int currentLevel)
        {
            if (levelText == null)
            {
                return;
            }

            // DOTween punch animation for text update
            levelText.transform.DOKill();
            levelText.transform.localScale = Vector3.one;
            levelText.text = $"Level {currentLevel}";
            levelText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }

        // ========================================================================
        // --- PROTECTED & PRIVATE METHODS ---
        // ========================================================================

        protected override void Awake()
        {
            base.Awake();
            BindButtons();
        }

        private void BindButtons()
        {
            // Using lambdas to invoke events safely
            btnBack.onClick.AddListener(() =>
            {
                if (OnBackClicked != null) OnBackClicked.Invoke();
            });

            btnStart.onClick.AddListener(() =>
            {
                if (OnStartClicked != null) OnStartClicked.Invoke();
            });

            btnPrev.onClick.AddListener(() =>
            {
                if (OnPrevClicked != null) OnPrevClicked.Invoke();
            });

            btnNext.onClick.AddListener(() =>
            {
                if (OnNextClicked != null) OnNextClicked.Invoke();
            });

            btnPrevBig.onClick.AddListener(() =>
            {
                if (OnPrevBigClicked != null) OnPrevBigClicked.Invoke();
            });

            btnNextBig.onClick.AddListener(() =>
            {
                if (OnNextBigClicked != null) OnNextBigClicked.Invoke();
            });
        }
    }
}