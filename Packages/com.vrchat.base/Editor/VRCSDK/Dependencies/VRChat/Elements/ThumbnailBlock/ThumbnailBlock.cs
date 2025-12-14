using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#if POST_PROCESSING_INCLUDED
using UnityEngine.Rendering.PostProcessing;
#endif

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class ThumbnailBlock: VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ThumbnailBlock, UxmlTraits> {}
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        public Thumbnail Thumbnail { get; private set; }
        public Foldout Foldout { get; private set; }
        public EventHandler<string> OnNewThumbnailSelected { get; set; }

        private VisualElement _selectorBlock;
        private Button _selectThumbnailButton;
        private Button _captureThumbnailButton;
        private VisualElement _modalMountPoint;
        
        private VisualElement _captureBlock;
        private IMGUIContainer _previewContainer;
        private Toggle _fillBackground;
        private ColorField _backgroundColor;
        private Toggle _usePostProcessing;
        private Toggle _useCustomCamera;
        private ObjectField _customCamera;
        private VisualElement _captureConfirmBlock;
        private Button _captureConfirmButton;
        private Button _captureCancelButton;
        private Modal _captureModal;

        private string _oldThumbnail;
        private Texture2D _oldThumbnailTexture;
        private Camera _captureCamera;
        private Texture2D _bufferTexture;
        private RenderTexture _targetTexture;

        private bool _capturing;
        private bool Capturing
        {
            get => _capturing;
            set
            {
                _capturing = value;
                if (value)
                {
                    _captureModal.Open();
                    _targetTexture = Resources.Load<RenderTexture>("ThumbnailCapture");
                    _captureCamera =
                        VRC_EditorTools.CreateThumbnailCaptureCamera(_targetTexture, _fillBackground.value, _backgroundColor.value, _usePostProcessing.value);
                    _captureCamera.enabled = true;
                    return;
                }
                else
                {
                    _captureModal.Close();
                }
                if (_captureCamera != null)
                {
                    Object.DestroyImmediate(_captureCamera.gameObject);
                    _captureCamera = null;
                }

                if (_customCamera.value != null)
                {
                    ((Camera) _customCamera.value).targetTexture = null;
                }

                if (_bufferTexture != null)
                {
                    Object.DestroyImmediate(_bufferTexture);
                    _bufferTexture = null;
                }

                _targetTexture = null;
            }
        }

        private async void HandleCaptureCancel()
        {
            Capturing = false;
            Thumbnail.ClearImage();
            if (!string.IsNullOrWhiteSpace(_oldThumbnail))
            {
                await Thumbnail.SetImageUrl(_oldThumbnail);
            }

            if (_oldThumbnailTexture != null)
            {
                Thumbnail.SetImage(_oldThumbnailTexture);
            }
            
            _oldThumbnail = null;
            _oldThumbnailTexture = null;
        }

        public ThumbnailBlock()
        {
            Resources.Load<VisualTreeAsset>("ThumbnailBlock").CloneTree(this);

            Thumbnail = this.Q<Thumbnail>("content-thumbnail");
            
            _selectorBlock = this.Q<VisualElement>("thumbnail-selector-block");
            _selectThumbnailButton = this.Q<Button>("select-new-thumbnail-btn");
            _captureThumbnailButton = this.Q<Button>("capture-thumbnail-from-scene-btn");

            _captureModal = this.Q<Modal>();
            _captureModal.OnClose += (_, _) =>
            {
                if (!_capturing) return;
                HandleCaptureCancel();
            };
            
            // We cannot traverse the tree until the component is mounted
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                // the panel is the root of the window tree, so we can see all elements from here
                _captureModal.SetAnchor(evt.destinationPanel.visualTree.Q("content-info"));
            });
           
            _previewContainer = this.Q<IMGUIContainer>("thumbnail-capture-preview");

            _fillBackground = this.Q<Toggle>("thumbnail-fill-background-toggle");
            var backgroundBlock = this.Q("thumbnail-background-block");
            _backgroundColor = this.Q<ColorField>("thumbnail-background-color-field");
            _usePostProcessing = this.Q<Toggle>("thumbnail-use-post-processing-toggle");
            var customCameraBlock = this.Q("thumbnail-custom-camera-block");
            _useCustomCamera = this.Q<Toggle>("thumbnail-use-custom-camera-toggle");
            _customCamera = this.Q<ObjectField>("thumbnail-custom-camera-ref");
            _captureConfirmBlock = this.Q<VisualElement>("thumbnail-capture-confirm-block");
            _captureConfirmButton = this.Q<Button>("thumbnail-capture-confirm-btn");
            _captureCancelButton = this.Q<Button>("thumbnail-capture-cancel-btn");

#if POST_PROCESSING_INCLUDED
            _usePostProcessing.RemoveFromClassList("d-none");
#endif

            _selectThumbnailButton.clicked += () =>
            {
                var imagePath = EditorUtility.OpenFilePanel("Select thumbnail", "", "png");
                if (string.IsNullOrWhiteSpace(imagePath)) return;
                OnNewThumbnailSelected?.Invoke(this, imagePath);
            };

            _captureThumbnailButton.clicked += () =>
            {
                _oldThumbnail = Thumbnail.CurrentImage;
                _oldThumbnailTexture = Thumbnail.CurrentImageTexture;
                Capturing = true;
            };

            _captureConfirmButton.clicked += () =>
            {
                Capturing = false;
                var capturedPicture = VRC_EditorTools.CaptureSceneImage(1200, 900, _fillBackground.value,
                    _backgroundColor.value, _usePostProcessing.value,
                    _useCustomCamera.value ? (Camera) _customCamera.value : null);
                OnNewThumbnailSelected?.Invoke(this, capturedPicture);
            };

            _fillBackground.RegisterValueChangedCallback(evt =>
            {
                if (!Capturing || _captureCamera == null) return;
                backgroundBlock.EnableInClassList("d-none", !evt.newValue);
                if (evt.newValue)
                {
                    _captureCamera.clearFlags = CameraClearFlags.SolidColor;
                    _captureCamera.backgroundColor = _backgroundColor.value;
                }
                else
                {
                    _captureCamera.clearFlags = CameraClearFlags.Skybox;
                }
            });

            _backgroundColor.RegisterValueChangedCallback(evt =>
            {
                if (!Capturing || _captureCamera == null) return;
                // Enforce alpha to 1
                _backgroundColor.SetValueWithoutNotify(new Color(evt.newValue.r, evt.newValue.g, evt.newValue.b, 1f));
                _captureCamera.backgroundColor = _backgroundColor.value;
            });

            _usePostProcessing.RegisterValueChangedCallback(evt =>
            {
#if POST_PROCESSING_INCLUDED
                if (!Capturing || _captureCamera == null) return;
                if (_captureCamera.TryGetComponent<PostProcessLayer>(out var layer))
                {
                    layer.enabled = evt.newValue;
                }
                else if (evt.newValue)
                {
                    var postProcessLayer = _captureCamera.gameObject.AddComponent<PostProcessLayer>();
                    postProcessLayer.volumeLayer = int.MaxValue;
                    postProcessLayer.volumeTrigger = _captureCamera.transform;
                }
#endif
            });

            _useCustomCamera.RegisterValueChangedCallback(evt =>
            {
                if (!Capturing) return;
                customCameraBlock.EnableInClassList("d-none", !evt.newValue);
            });

            _customCamera.objectType = typeof(Camera);
            _customCamera.allowSceneObjects = true;

            _previewContainer.style.width = 192;
            _previewContainer.style.height = 144;
            
             _previewContainer.onGUIHandler = () =>
            {
                if (!Capturing) return;
                if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive) return;
                
                var customCameraValue = (Camera) _customCamera.value;
                if (customCameraValue != null && _useCustomCamera.value)
                {
                    if (_useCustomCamera.value)
                    {
                        customCameraValue.targetTexture = _targetTexture;
                        _captureCamera.targetTexture = null;
                    }
                    else
                    {
                        customCameraValue.targetTexture = null;
                    }
                }
                else
                {
                    _captureCamera.targetTexture = _targetTexture;
                    var copyFrom = SceneView.lastActiveSceneView.camera;
                    _captureCamera.transform.SetPositionAndRotation(copyFrom.transform.position,
                        copyFrom.transform.rotation);
                }
                
                if (!Capturing) return;
                if (_targetTexture == null) return;
                
                var rect = EditorGUILayout.GetControlRect();
                var aspect = _targetTexture.width / (float) _targetTexture.height;
                var previewRect = new Rect(rect.x, rect.y, rect.width, rect.width / aspect);
                GUI.DrawTexture(previewRect, _targetTexture);
                // Enforce repaint immediately
                _previewContainer.MarkDirtyRepaint();
            };
        }
    }
}