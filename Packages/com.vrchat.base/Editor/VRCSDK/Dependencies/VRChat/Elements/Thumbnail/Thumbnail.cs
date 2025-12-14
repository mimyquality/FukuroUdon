using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase.Editor.Api;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class Thumbnail: VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Thumbnail, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlFloatAttributeDescription _width = new UxmlFloatAttributeDescription { name = "width" };
            private readonly UxmlFloatAttributeDescription _height = new UxmlFloatAttributeDescription { name = "height" };
            private readonly UxmlStringAttributeDescription _placeholder = new UxmlStringAttributeDescription { name = "placeholder" };
            private readonly UxmlStringAttributeDescription _loadingText = new UxmlStringAttributeDescription { name = "loading-text" };
            private readonly UxmlStringAttributeDescription _hoverText = new UxmlStringAttributeDescription { name = "hover-text" };
            private readonly UxmlStringAttributeDescription _imageUrl = new UxmlStringAttributeDescription { name = "image-url" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var thumbnail = (Thumbnail) ve;
                var width = 0f;
                if (_width.TryGetValueFromBag(bag, cc, ref width))
                {
                    thumbnail._width = width;
                }
                var height = 0f;
                if (_height.TryGetValueFromBag(bag, cc, ref height))
                {
                    thumbnail._height = height;
                }

                var placeholder = "";
                if (_placeholder.TryGetValueFromBag(bag, cc, ref placeholder))
                {
                    thumbnail._placeholder = placeholder;
                }
                var loadingText = "";
                if (_loadingText.TryGetValueFromBag(bag, cc, ref loadingText))
                {
                    thumbnail._loadingText = loadingText;
                }
                var hoverText = "";
                if (_hoverText.TryGetValueFromBag(bag, cc, ref hoverText))
                {
                    thumbnail._hoverText = hoverText;
                }
                thumbnail.UpdateProps();
                var imageUrl = "";
                if (_imageUrl.TryGetValueFromBag(bag, cc, ref imageUrl))
                {
                    thumbnail.SetImageUrl(imageUrl).ConfigureAwait(false);
                }
            }
        }

        private float _width = 192f;
        private float _height = 144f;
        private string _placeholder = "No Image";
        private string _loadingText = "Loading...";
        private string _hoverText = "Image Size\n(1200 x 900)";
        private readonly VisualElement _container;
        private readonly Label _placeholderText;
        private readonly Label _hoverTextElement;
        private readonly VisualElement _imageElement;
        private string _imageUrl;
        private Texture2D _imageTexture;
        private Texture2D _transparentPlaceholder;

        public string CurrentImage => _imageUrl;
        public Texture2D CurrentImageTexture => _imageTexture;

        private bool _loading;

        public bool Loading
        {
            get => _loading;
            set
            {
                _loading = value;
                _placeholderText.text = _loading ? _loadingText : _placeholder;
            }
        }

        public Thumbnail()
        {
            Resources.Load<VisualTreeAsset>("Thumbnail").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("ThumbnailStyles"));
            
            _container = this.Q("thumbnail-container");
            _placeholderText = this.Q<Label>("thumbnail-placeholder-text");
            _hoverTextElement = this.Q<Label>("thumbnail-hover-text");
            _imageElement = this.Q("thumbnail-image");

            _container.style.width = _width;
            _container.style.height = _height;
            _container.style.minWidth = _width;
            
            _placeholderText.text = _placeholder;
            _hoverTextElement.text = _hoverText;

            _transparentPlaceholder = new Texture2D(1, 1);
            _transparentPlaceholder.SetPixel(0, 0, new Color(0,0,0,0));
            _transparentPlaceholder.Apply();
        }

        private void UpdateProps()
        {
            _container.style.width = _width;
            _container.style.height = _height;
            _container.style.minWidth = _width;
            
            _placeholderText.text = _placeholder;
            _hoverTextElement.text = _hoverText;
        }

        [PublicAPI]
        public async Task SetImageUrl(string url, CancellationToken cancellationToken = default, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (url == _imageUrl) return;
            _imageUrl = url;
            _imageTexture = null;
            Loading = true;
            try
            {
                _imageElement.style.backgroundImage = await VRCApi.GetImage(url, forceRefresh, cancellationToken: cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _imageUrl = null;
                _imageElement.style.backgroundImage = null;
            }
        }

        [PublicAPI]
        public void SetImage(Texture2D image)
        {
            _imageUrl = null;
            _imageTexture = image;
            _imageElement.style.backgroundImage = image;
        }

        [PublicAPI]
        public void SetImage(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var newThumbnail = new Texture2D(2, 2);
            newThumbnail.LoadImage(bytes);
            SetImage(newThumbnail);
        }

        [PublicAPI]
        public void ClearImage()
        {
            _imageUrl = null;
            _imageTexture = null;
            _imageElement.style.backgroundImage = _transparentPlaceholder;
        }
    }
}