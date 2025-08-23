/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;
    //using VRC.SDKBase;
    //using VRC.Udon;
    using TMPro;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Smart-Slideshow#sss_controller")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Smart Slideshow/SSs Controller")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SSs_Controller : UdonSharpBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        SmartSlideshow target;    // 操作対象のスライドショー
        [SerializeField]
        SmartSlideshow[] subTarget = new SmartSlideshow[0];    // 同時操作対象のスライドショー

        [Header("Paging UI")]
        [SerializeField]
        Button pageNext;  // ページ送り操作用
        [SerializeField]
        Button pagePrev;  // ページ戻し操作用
        [SerializeField]
        Slider pageSlider;  // ページ直指定用
        [SerializeField]
        Text pageNumber;    // ページ数表示用
        [SerializeField]
        TextMeshProUGUI pageNumberTMP;    // ページ数表示用

        [Header("Indexing UI")]
        [SerializeField]
        Button indexNext;   // 巻送り操作用
        [SerializeField]
        Button indexPrev;    // 巻戻し操作用
        [SerializeField]
        Slider indexSlider; // 巻の直指定用
        [SerializeField]
        Text indexNumber;   // 巻数表示用
        [SerializeField]
        TextMeshProUGUI indexNumberTMP;   // 巻数表示用

        [Header("Setting UI")]
        [SerializeField]
        Toggle isGlobal;    // isGlobal変更用
        [SerializeField]
        Toggle pageLink;    // pageLink変更用
        [SerializeField]
        Toggle pageLoop;    // pageLoop変更用
        [SerializeField]
        Slider autoSlideSlider; // AutoSlide値変更用
        [SerializeField]
        Text autoSlideCount;    // AutoSlide秒数表示用
        [SerializeField]
        TextMeshProUGUI autoSlideCountTMP;  // AutoSlide秒数表示用

        [Header("List of SendCustomEvent (For copy and paste)")]
        [SerializeField]
        [TextArea(11, 15)]
        string events;
        string _events = nameof(OnPageNext)
                + "\n" + nameof(OnPagePrev)
                + "\n" + nameof(OnPageSlide)
                + "\n" + nameof(OnIndexNext)
                + "\n" + nameof(OnIndexPrev)
                + "\n" + nameof(OnIndexSlide)
                + "\n" + nameof(OnIsGlobal)
                + "\n" + nameof(OnPageLink)
                + "\n" + nameof(OnPageLoop)
                + "\n" + nameof(OnAutoSlide);

        private void OnValidate()
        {
            events = _events;
        }

        /******************************
         UI input events
        ******************************/
        public void OnPageNext()
        {
            target.PageIncrement();
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].PageIncrement();
            }
        }

        public void OnPagePrev()
        {
            target.PageDecrement();
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].PageDecrement();
            }
        }

        public void OnPageSlide()
        {
            target.PageSelect((int)pageSlider.value);
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].PageSelect((int)pageSlider.value);
            }
        }

        public void OnIndexNext()
        {
            target.IndexIncrement();
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].IndexIncrement();
            }
        }

        public void OnIndexPrev()
        {
            target.IndexDecrement();
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].IndexDecrement();
            }
        }

        public void OnIndexSlide()
        {
            target.IndexSelect((int)indexSlider.value);
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].IndexSelect((int)indexSlider.value);
            }
        }

        public void OnIsGlobal()
        {
            target.IsGlobal = isGlobal.isOn;
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].IsGlobal = isGlobal.isOn;
            }
        }

        public void OnPageLink()
        {
            target.PageLink = pageLink.isOn;
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].PageLink = pageLink.isOn;
            }
        }

        public void OnPageLoop()
        {
            target.PageLoop = pageLoop.isOn;
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].PageLoop = pageLoop.isOn;
            }
        }

        public void OnAutoSlide()
        {
            target.AutoSlide = autoSlideSlider.value;
            for (int i = 0; i < subTarget.Length; i++)
            {
                subTarget[i].AutoSlide = autoSlideSlider.value;
            }
        }

        /******************************
         Feedback events
        ******************************/
        public void SetPageInteractable(bool next, bool prev)
        {
            if (pageNext) { pageNext.interactable = next; }
            if (pagePrev) { pagePrev.interactable = prev; }
        }

        public void SetPageSlider(int num, int min, int max)
        {
            if (pageSlider)
            {
                pageSlider.minValue = (float)min;
                if (max < pageSlider.value)
                {
                    pageSlider.SetValueWithoutNotify((float)num);
                    pageSlider.maxValue = (float)max;
                }
                else
                {
                    pageSlider.maxValue = (float)max;
                    pageSlider.SetValueWithoutNotify((float)num);
                }
            }
            if (pageNumber) { pageNumber.text = num.ToString(); }
            if (pageNumberTMP) { pageNumberTMP.text = num.ToString(); }
        }

        public void SetIndexInteractable(bool next, bool prev)
        {
            if (indexNext) { indexNext.interactable = next; }
            if (indexPrev) { indexPrev.interactable = prev; }
        }

        public void SetIndexSlider(int num, int min, int max)
        {
            if (indexSlider)
            {
                indexSlider.minValue = (float)min;
                if (max < indexSlider.value)
                {
                    indexSlider.SetValueWithoutNotify((float)num);
                    indexSlider.maxValue = (float)max;
                }
                else
                {
                    indexSlider.maxValue = (float)max;
                    indexSlider.SetValueWithoutNotify((float)num);

                }
            }
            if (indexNumber) { indexNumber.text = num.ToString(); }
            if (indexNumberTMP) { indexNumberTMP.text = num.ToString(); }
        }

        public void SetSettingsInteractable(bool global, bool link, bool loop)
        {
            if (isGlobal) { isGlobal.interactable = global; }
            if (pageLink) { pageLink.interactable = link; }
            if (pageLoop) { pageLoop.interactable = loop; }
        }

        public void SetSettingsToggle(bool global, bool link, bool loop)
        {
            if (isGlobal) { isGlobal.SetIsOnWithoutNotify(global); }
            if (pageLink) { pageLink.SetIsOnWithoutNotify(link); }
            if (pageLoop) { pageLoop.SetIsOnWithoutNotify(loop); }
        }

        public void SetAutoSlider(float auto)
        {
            if (autoSlideSlider) { autoSlideSlider.SetValueWithoutNotify(auto); }
            if (autoSlideCount) { autoSlideCount.text = (auto > 0.0f) ? auto.ToString("f2") + " sec" : "Stop"; }
            if (autoSlideCountTMP) { autoSlideCountTMP.text = (auto > 0.0f) ? auto.ToString("f2") + " sec" : "Stop"; }
        }
    }
}
