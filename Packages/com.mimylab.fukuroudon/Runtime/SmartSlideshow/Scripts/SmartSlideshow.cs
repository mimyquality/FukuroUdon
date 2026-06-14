/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Smart-Slideshow#smart-slideshow")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Smart Slideshow/Smart Slideshow")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SmartSlideshow : UdonSharpBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        private Literature[] literatures = new Literature[0];   // 切替対象のスライドセット
        [SerializeField]
        private SSs_Controller[] controllers = new SSs_Controller[0];   // 入力フィードバック用

        [Header("Settings")]
        [SerializeField]
        [FieldChangeCallback(nameof(IsGlobal))]
        private bool _isGlobal = false;   // プレイヤー間で同期するか
        public bool IsGlobal
        {
            get => _isGlobal;
            set => SetIsGlobal(value);
        }
        private void SetIsGlobal(bool v)
        {
            _isGlobal = v;
            if (v) { OnDeserialization(); }
            else { FeedbackController(); }
        }

        [SerializeField]
        [FieldChangeCallback(nameof(PageLink))]
        private bool _pageLink = false;   // スライド間でページを一致させるか
        public bool PageLink
        {
            get => _pageLink;
            set => SetPageLink(value);
        }
        private void SetPageLink(bool v)
        {
            _pageLink = v;
            // PageSelectでFeedbackするので省略
            // FeedbackController();
            PageSelect(_selectedPage[_selectedIndex]);
        }

        [Tooltip("When Auto Slide is enabled, this is forced to be enabled")]
        [SerializeField]
        [FieldChangeCallback(nameof(PageLoop))]
        private bool _pageLoop = false;   // ページの最初と最後をつなぐか
        public bool PageLoop
        {
            get => _pageLoop;
            set => SetPageLoop(value);
        }
        private void SetPageLoop(bool v)
        {
            _pageLoop = v;
            FeedbackController();
        }

        [Tooltip("If this value is greater than 0, Auto slide is enabled.")]
        [Range(0.0f, 9999.0f)]
        [SerializeField]
        [FieldChangeCallback(nameof(AutoSlide))]
        private float _autoSlide = 0.0f;  // 自動でページ送りするか　(0以上で有効、pageLoop強制的に有効化する)
        public float AutoSlide
        {
            get => _autoSlide;
            set => SetAutoSlide(value);
        }
        private void SetAutoSlide(float v)
        {
            _autoSlide = v;
            if (v > 0.0f)
            {
                PageLoop = true;

                _autoSlideHandle.SetDuration(v);
                if (!_autoSlideHandle.IsPlaying)
                {
                    _autoSlideHandle.Play();
                }

                // PageSelectでFeedbackするので省略
                // FeedbackController();
            }
            else
            {
                _autoSlideHandle.Pause();

                FeedbackController();
            }
        }

        // 同期用変数
        [UdonSynced(UdonSyncMode.None)]
        private int sync_selectedIndex = 0;
        [UdonSynced(UdonSyncMode.None)]
        private int[] sync_selectedPage;

        // ローカル用変数
        private int _selectedIndex = 0;
        private int[] _selectedPage;
        private int[] _endPage;
        private VRCTweenHandle _autoSlideHandle;

        private void OnValidate()
        {
            if (AutoSlide > 0.0f) _pageLoop = true;
        }

        private void Start()
        {
            _selectedPage = new int[literatures.Length];
            sync_selectedPage = new int[literatures.Length];
            _endPage = new int[literatures.Length];
            for (int i = 0; i < _endPage.Length; i++)
            {
                _endPage[i] = literatures[i].EndPage;
            }
            _autoSlideHandle = VRCTween.DelayedCall(this, nameof(PageAutoIncrement), 1.0f)
                .SetLoops(-1, VRCTweenLoopType.Restart);
            AutoSlide = _autoSlide;

            RefreshView();
            FeedbackController();
        }

        private void OnDestroy()
        {
            gameObject.KillAllTweens();
        }

        /******************************
         Sync events
        ******************************/
        public override void OnPreSerialization()
        {
            sync_selectedIndex = _selectedIndex;
            for (int i = 0; i < literatures.Length; i++)
            {
                sync_selectedPage[i] = _selectedPage[i];
            }
        }

        public override void OnDeserialization()
        {
            if (!IsGlobal) { return; }

            _selectedIndex = sync_selectedIndex;
            for (int i = 0; i < literatures.Length; i++)
            {
                _selectedPage[i] = sync_selectedPage[i];
            }

            RefreshView();
            FeedbackController();
        }

        /******************************
         Public events
        ******************************/
        public void PageSelect(int num)
        {
            if (IsGlobal && !Networking.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            if (PageLink)
            {
                for (int i = 0; i < _selectedPage.Length; i++)
                {
                    _selectedPage[i] = Mathf.Clamp(num, 0, _endPage[i]);
                }
            }
            else
            {
                _selectedPage[_selectedIndex] = Mathf.Clamp(num, 0, _endPage[_selectedIndex]);
            }
            if (IsGlobal) { RequestSerialization(); }
            RefreshView();
            FeedbackController();
        }

        public void PageIncrement()
        {
            if (_selectedPage[_selectedIndex] < _endPage[_selectedIndex])
            {
                PageSelect(_selectedPage[_selectedIndex] + 1);
            }
            else if (PageLoop)
            {
                PageSelect(0);
            }
        }

        public void PageDecrement()
        {
            if (_selectedPage[_selectedIndex] > 0)
            {
                PageSelect(_selectedPage[_selectedIndex] - 1);
            }
            else if (PageLoop)
            {
                PageSelect(_endPage[_selectedIndex]);
            }
        }

        public void IndexSelect(int num)
        {
            if (IsGlobal && !Networking.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            _selectedIndex = Mathf.Clamp(num, 0, literatures.Length - 1);

            if (IsGlobal) { RequestSerialization(); }
            RefreshView();
            FeedbackController();
        }

        public void IndexIncrement()
        {
            IndexSelect((_selectedIndex < literatures.Length - 1) ? _selectedIndex + 1 : 0);

        }

        public void IndexDecrement()
        {
            IndexSelect((_selectedIndex > 0) ? _selectedIndex - 1 : literatures.Length - 1);
        }

        public void PageAutoIncrement()
        {
            if (IsGlobal && !Networking.IsOwner(this.gameObject)) { return; }

            PageIncrement();
        }

        /******************************
         Local events
        ******************************/
        private void RefreshView()
        {
            for (int i = 0; i < literatures.Length; i++)
            {
                if (literatures[i])
                {
                    literatures[i].gameObject.SetActive(i == _selectedIndex);
                    literatures[i].Page = _selectedPage[i];
                }
            }
        }

        private void FeedbackController()
        {
            bool next = PageLoop || (_selectedPage[_selectedIndex] < _endPage[_selectedIndex]);
            bool prev = PageLoop || (_selectedPage[_selectedIndex] > 0);
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i])
                {
                    controllers[i].SetPageInteractable(next, prev);
                    controllers[i].SetPageSlider(_selectedPage[_selectedIndex], 0, _endPage[_selectedIndex]);
                    controllers[i].SetIndexSlider(_selectedIndex, 0, literatures.Length - 1);
                    controllers[i].SetSettingsToggle(IsGlobal, PageLink, PageLoop);
                    controllers[i].SetSettingsInteractable(true, true, !(AutoSlide > 0.0f));
                    controllers[i].SetAutoSlider(AutoSlide);
                }
            }
        }
    }
}
