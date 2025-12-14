using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimStackedVRCameraSystem : MonoBehaviour
    {
        [SerializeField]
        private ClientSimStackedCamera[] cameraStack;
        
        private bool _isCameraStackingEnabled;
        private Camera _mainSceneCamera;
        private bool _isInitialized;
        private bool _isReady;
        private List<Camera> _cameras;
        private ClientSimMenu _clientSimMenu;
        private IClientSimEventDispatcher _eventDispatcher;

        public IReadOnlyList<Camera> Cameras => _cameras;

        // all layers handled by this system - note that this does not change whether the system is active or not,
        // meaning it is valid to call this at any time, which `OnCameraSettingsChanged` may need to do
        public LayerMask RequestedStackLayers => cameraStack.Aggregate(0, (mask, cam) => mask | cam.RenderLayer);

        public void Initialize(Camera playerCamera, ClientSimMenu menu, IClientSimEventDispatcher eventDispatcher)
        {
            _mainSceneCamera = playerCamera;
            _clientSimMenu = menu;
            _eventDispatcher = eventDispatcher;
        }

        public void Ready()
        {
            _isReady = true;
        }
        
        void Update()
        {
            if(!_isReady) return;
            if (!_isInitialized) { InitializeStackedSystem(); }
        }
     
        void OnDisable()
        {
            if (_mainSceneCamera != null)
            {
                if (_isCameraStackingEnabled)
                    DestroyCameraStack();
            }
        }

        private void InitializeStackedSystem()
        {
            _cameras = new List<Camera>();
            if (_mainSceneCamera != null)
            {
                CreateCameraStack();
                _isInitialized = true;
                _eventDispatcher.SendEvent(new ClientSimStackedCameraReadyEvent());
            }
        }

        private void CreateCameraStack()
        {
            for (int i = 0; i < cameraStack.Length; i++)
            {
                AddCamera(i);
            }
            _isCameraStackingEnabled = true;
        }

        private void DestroyCameraStack()
        {
            for (int i = 0; i < _cameras.Count; i++)
            {
                DestroyCamera(i);
            }
            _isCameraStackingEnabled = false;
        }

        private void AddCamera(int index)
        {
            var cameraObj = new GameObject();
            cameraObj.transform.parent = _mainSceneCamera.transform;
            Camera cam = cameraObj.AddComponent<Camera>();
            XRDevice.DisableAutoXRCameraTracking(cam, true);

            cam.CopyFrom(_mainSceneCamera); // Start by copying all the settings from the main camera
#if VRC_VR_STEAM // We only want this on SteamVR.
            cameraObj.AddComponent<SteamVRCantedProjectionCullingFix>();
#endif
            _cameras.Add(cam);

            cameraObj.tag = "Untagged";
            cameraObj.name = $"StackedCamera : {cameraStack[index].CameraName}";
            cam.clearFlags = CameraClearFlags.Depth;
            cam.depth = 100 - index;
            cam.cullingMask = cameraStack[index].RenderLayer;
            cam.useOcclusionCulling = cameraStack[index].UseOcclusionCulling;

            //Remove this cameras layers from the base camera
            _mainSceneCamera.cullingMask = _mainSceneCamera.cullingMask ^ cameraStack[index].RenderLayer;

            // Set the ClientSim UI canvas to use this camera
            _clientSimMenu.SetCanvasCamera(cam);
        }

        private void DestroyCamera(int index)
        {
            Camera cam = _cameras[index];
            _cameras.RemoveAt(index);

            //Restore Layers from this camera to the main camera
            _mainSceneCamera.cullingMask = _mainSceneCamera.cullingMask | cameraStack[index].RenderLayer;
            Destroy(cam.gameObject);
        }
    }
}