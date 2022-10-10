/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MimyLab
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SmartSlideshow : UdonSharpBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        Literature[] literatures = new Literature[0];   // 切替対象のスライドセット
        [SerializeField]
        SSs_Controller[] controllers = new SSs_Controller[0];   // 入力フィードバック用

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

                if (!_activeAutoSlide)
                {
                    _activeAutoSlide = true;
                    SendCustomEventDelayedSeconds(nameof(PageAutoIncrement), v);
                }
                // PageSelectでFeedbackするので省略
                // FeedbackController();
            }
            else { FeedbackController(); }
        }

        // 同期用変数
        [UdonSynced(UdonSyncMode.None)]
        int g_SelectedIndex = 0;
        [UdonSynced(UdonSyncMode.None)]
        int[] g_SelectedPage;

        // ローカル用変数
        int _selectedIndex = 0;
        int[] _selectedPage;
        int[] _endPage;
        bool _activeAutoSlide = false;

        void OnValidate()
        {
            if (AutoSlide > 0.0f) _pageLoop = true;
        }

        void Start()
        {
            _selectedPage = new int[literatures.Length];
            g_SelectedPage = new int[literatures.Length];
            _endPage = new int[literatures.Length];
            for (int i = 0; i < _endPage.Length; i++)
            {
                _endPage[i] = literatures[i].EndPage;
            }

            RefreshView();
            FeedbackController();

            // 自動ページ送り開始
            if (AutoSlide > 0.0f)
            {
                _activeAutoSlide = true;
                SendCustomEventDelayedSeconds(nameof(PageAutoIncrement), AutoSlide);
            }
        }

        /******************************
         Sync events
        ******************************/
        public override void OnPreSerialization()
        {
            g_SelectedIndex = _selectedIndex;
            for (int i = 0; i < literatures.Length; i++)
            {
                g_SelectedPage[i] = _selectedPage[i];
            }
        }

        public override void OnDeserialization()
        {
            if (!IsGlobal) { return; }

            _selectedIndex = g_SelectedIndex;
            for (int i = 0; i < literatures.Length; i++)
            {
                _selectedPage[i] = g_SelectedPage[i];
            }

            RefreshView();
            FeedbackController();
        }

        /******************************
         Public events
        ******************************/
        public void PageSelect(int num)
        {
            if (IsGlobal && !Networking.IsOwner(this.gameObject)) { Networking.SetOwner(Networking.LocalPlayer, this.gameObject); }

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
            if (IsGlobal && !Networking.IsOwner(this.gameObject)) { Networking.SetOwner(Networking.LocalPlayer, this.gameObject); }

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
            if (AutoSlide > 0.0f)
            {
                // 自動ページ送り継続
                _activeAutoSlide = true;
                SendCustomEventDelayedSeconds(nameof(PageAutoIncrement), AutoSlide);
            }
            else
            {
                // AutoSlide無効中なら停止
                _activeAutoSlide = false;
                return;
            }

            if (!IsGlobal || Networking.IsOwner(this.gameObject))
            {
                PageIncrement();
            }
        }

        /******************************
         Local events
        ******************************/
        void RefreshView()
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

        void FeedbackController()
        {
            bool next = (PageLoop) ? true : (_selectedPage[_selectedIndex] < _endPage[_selectedIndex]);
            bool prev = (PageLoop) ? true : (_selectedPage[_selectedIndex] > 0);
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
