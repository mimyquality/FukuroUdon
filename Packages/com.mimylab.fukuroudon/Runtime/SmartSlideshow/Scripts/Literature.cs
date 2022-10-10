/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
//using VRC.SDKBase;
//using VRC.Udon;

namespace MimyLab
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Literature : UdonSharpBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        Sprite[] slide;    // 表示するスライドのリスト

        Image _image = null; // スライドを表示するuGUI

        public int EndPage
        {
            get => slide.Length - 1;
        }

        [FieldChangeCallback(nameof(Page))]
        private int _page = 0;
        public int Page
        {
            get => _page;
            set => SetPage(value);
        }
        private void SetPage(int v)
        {
            _page = Mathf.Clamp(v, 0, EndPage);

            if (!_image) _image = GetComponent<Image>();
            if (_image)
            {
                _image.sprite = slide[_page];
            }
        }

        void Start()
        {
            Page = Page;
        }
    }
}
