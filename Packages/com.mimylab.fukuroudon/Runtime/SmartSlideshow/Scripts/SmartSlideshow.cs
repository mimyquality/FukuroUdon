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
            PageSelect(selectedPage[selectedIndex]);
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

                if (!activeAutoSlide)
                {
                    activeAutoSlide = true;
                    SendCustomEventDelayedSeconds(nameof(PageAutoIncrement), v);
                }
                // PageSelectでFeedbackするので省略
                // FeedbackController();
            }
            else { FeedbackController(); }
        }

        // 同期用変数
        [UdonSynced(UdonSyncMode.None)]
        int gSelectedIndex = 0;
        [UdonSynced(UdonSyncMode.None)]
        int[] gSelectedPage;

        // ローカル用変数
        int selectedIndex = 0;
        int[] selectedPage;
        int[] endPage;
        bool activeAutoSlide = false;

        void OnValidate()
        {
            if (AutoSlide > 0.0f) _pageLoop = true;
        }

        void Start()
        {
            selectedPage = new int[literatures.Length];
            gSelectedPage = new int[literatures.Length];
            endPage = new int[literatures.Length];
            for (int i = 0; i < endPage.Length; i++)
            {
                endPage[i] = literatures[i].EndPage;
            }

            RefreshView();
            FeedbackController();

            // 自動ページ送り開始
            if (AutoSlide > 0.0f)
            {
                activeAutoSlide = true;
                SendCustomEventDelayedSeconds(nameof(PageAutoIncrement), AutoSlide);
            }
        }

        /******************************
         Sync events
        ******************************/
        public override void OnPreSerialization()
        {
            gSelectedIndex = selectedIndex;
            for (int i = 0; i < literatures.Length; i++)
            {
                gSelectedPage[i] = selectedPage[i];
            }
        }

        public override void OnDeserialization()
        {
            if (!IsGlobal) { return; }

            selectedIndex = gSelectedIndex;
            for (int i = 0; i < literatures.Length; i++)
            {
                selectedPage[i] = gSelectedPage[i];
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
                for (int i = 0; i < selectedPage.Length; i++)
                {
                    selectedPage[i] = Mathf.Clamp(num, 0, endPage[i]);
                }
            }
            else
            {
                selectedPage[selectedIndex] = Mathf.Clamp(num, 0, endPage[selectedIndex]);
            }
            if (IsGlobal) { RequestSerialization(); }
            RefreshView();
            FeedbackController();
        }
        public void PageIncrement()
        {
            if (selectedPage[selectedIndex] < endPage[selectedIndex])
            {
                PageSelect(selectedPage[selectedIndex] + 1);
            }
            else if (PageLoop)
            {
                PageSelect(0);
            }
        }
        public void PageDecrement()
        {
            if (selectedPage[selectedIndex] > 0)
            {
                PageSelect(selectedPage[selectedIndex] - 1);
            }
            else if (PageLoop)
            {
                PageSelect(endPage[selectedIndex]);
            }
        }

        public void IndexSelect(int num)
        {
            if (IsGlobal && !Networking.IsOwner(this.gameObject)) { Networking.SetOwner(Networking.LocalPlayer, this.gameObject); }

            selectedIndex = Mathf.Clamp(num, 0, literatures.Length - 1);

            if (IsGlobal) { RequestSerialization(); }
            RefreshView();
            FeedbackController();
        }
        public void IndexIncrement()
        {
            IndexSelect((selectedIndex < literatures.Length - 1) ? selectedIndex + 1 : 0);

        }
        public void IndexDecrement()
        {
            IndexSelect((selectedIndex > 0) ? selectedIndex - 1 : literatures.Length - 1);
        }

        public void PageAutoIncrement()
        {
            if (AutoSlide > 0.0f)
            {
                // 自動ページ送り継続
                activeAutoSlide = true;
                SendCustomEventDelayedSeconds(nameof(PageAutoIncrement), AutoSlide);
            }
            else
            {
                // AutoSlide無効中なら停止
                activeAutoSlide = false;
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
                    literatures[i].gameObject.SetActive(i == selectedIndex);
                    literatures[i].Page = selectedPage[i];
                }
            }
        }

        void FeedbackController()
        {
            bool next = (PageLoop) ? true : (selectedPage[selectedIndex] < endPage[selectedIndex]);
            bool prev = (PageLoop) ? true : (selectedPage[selectedIndex] > 0);
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i])
                {
                    controllers[i].SetPageInteractable(next, prev);
                    controllers[i].SetPageSlider(selectedPage[selectedIndex], 0, endPage[selectedIndex]);
                    controllers[i].SetIndexSlider(selectedIndex, 0, literatures.Length - 1);
                    controllers[i].SetSettingsToggle(IsGlobal, PageLink, PageLoop);
                    controllers[i].SetSettingsInteractable(true, true, !(AutoSlide > 0.0f));
                    controllers[i].SetAutoSlider(AutoSlide);
                }
            }
        }
    }
}
