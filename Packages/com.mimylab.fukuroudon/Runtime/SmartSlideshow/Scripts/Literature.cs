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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Smart-Slideshow#literature")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Smart Slideshow/Literature")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Literature : UdonSharpBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        private Sprite[] slide;    // 表示するスライドのリスト

        private protected Image _image = null; // スライドを表示するuGUI

        public virtual int EndPage
        {
            get => slide.Length - 1;
        }

        [FieldChangeCallback(nameof(Page))]
        private protected int _page = 0;
        public int Page
        {
            get => _page;
            set => SetPage(value);
        }
        private protected virtual void SetPage(int v)
        {
            _page = Mathf.Clamp(v, 0, EndPage);

            if (!_image) _image = GetComponent<Image>();
            if (_image)
            {
                _image.sprite = slide[_page];
            }
        }

        private void Start()
        {
            Page = Page;
        }
    }
}
