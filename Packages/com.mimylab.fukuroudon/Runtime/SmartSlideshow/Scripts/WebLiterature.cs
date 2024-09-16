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
    using VRC.SDKBase;
    using VRC.Udon.Common.Interfaces;
    using VRC.SDK3.Image;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Smart Slideshow/Web Literature")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WebLiterature : Literature
    {
        [Header("Reference")]
        [SerializeField]
        private VRCUrl[] imageUrl;    // 表示する画像リンクのリスト
        [SerializeField]
        private TextureInfo textureInfo;    // 読み込む画像の形式
        [SerializeField]
        private Texture2D textureLoading, textureLoadError; // 読み込むまでの表示用画像

        private VRCImageDownloader _imageDownloader;
        private Texture2D[] _dlTextures;

        private RawImage _rawImage = null;   // スライドを表示するuGUI

        public override int EndPage
        {
            get => imageUrl.Length - 1;
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _imageDownloader = new VRCImageDownloader();
            _dlTextures = new Texture2D[imageUrl.Length];
            for (int i = 0; i < imageUrl.Length; i++)
            {
                _imageDownloader.DownloadImage(imageUrl[i], null, (IUdonEventReceiver)this, textureInfo);
                _dlTextures[i] = textureLoading;
            }

            _initialized = true;
        }

        protected override void SetPage(int v)
        {
            Initialize();

            _page = Mathf.Clamp(v, 0, EndPage);

            if (!_rawImage) _rawImage = GetComponent<RawImage>();
            if (_rawImage)
            {
                _rawImage.texture = _dlTextures[_page];
            }
        }

        private void OnDestroy()
        {
            _imageDownloader.Dispose();
        }

        public override void OnImageLoadError(IVRCImageDownload result)
        {
            Debug.Log($"{result.Error} : {result.ErrorMessage}");

            for (int i = 0; i < imageUrl.Length; i++)
            {
                if (result.Url == imageUrl[i])
                {
                    _dlTextures[i] = textureLoadError;
                    Page = Page;
                }
            }
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            for (int i = 0; i < imageUrl.Length; i++)
            {
                if (result.Url == imageUrl[i])
                {
                    _dlTextures[i] = result.Result;
                    Page = Page;
                }
            }
        }
    }
}
