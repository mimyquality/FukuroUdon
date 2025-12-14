#if UNITY_EDITOR
using UnityEngine;
using VRC.Dynamics;

namespace VRC.SDK3.Dynamics.PhysBone
{
    /// <summary>
    /// Internal component that makes it possible to grab and pose PhysBones for testing in an SDK project while it's
    /// in play mode.
    /// </summary>
    // This component is intentionally duplicated across SDKs instead of existing in the base SDK only, to address
    // compatibility issues with community made tooling.
    [AddComponentMenu("")]
    public class PhysBoneGrabHelper : MonoBehaviour
    {
        Camera currentCamera;

        void Update()
        {
            currentCamera = FindCamera();

            //Process mouse input
            SetMouseDown(Input.GetMouseButton(0));
            if (mouseIsDown && Input.GetMouseButtonDown(1))
            {
                if(grab != null)
                {
                    PhysBoneManager.Inst.ReleaseGrab(grab, true);
                    grab = null;
                }
            }
            UpdateGrab();
        }

        Camera FindCamera()
        {
            //Get target display
            int targetDisplay = Display.activeEditorGameViewTarget;

            //Find camera with matching target
            Camera result = null;
            var cameras = Camera.allCameras;
            foreach(var camera in cameras) //Get the last active camera
            {
                if (camera.isActiveAndEnabled &&
                    camera.cameraType == CameraType.Game &&
                    camera.targetDisplay == targetDisplay &&
                    camera.targetTexture == null
                   )
                    result = camera;
            }
            return result;
        }

        bool mouseIsDown = false;
        PhysBoneManager.Grab grab;
        Vector3 grabOriginLocal;
        void SetMouseDown(bool state)
        {
            if (state == mouseIsDown)
                return;

            mouseIsDown = state;

            if (mouseIsDown && currentCamera != null)
            {
                var localPlayer = VRC.SDKBase.Networking.LocalPlayer;
                var playerId = localPlayer != null && localPlayer.IsValid() ? localPlayer.playerId : -1;
                var ray = GetMouseRay();
                grab = PhysBoneManager.Inst.AttemptGrab(playerId, ray, out Vector3 grabOrigin);
                if (grab != null)
                {
                    grabOriginLocal = currentCamera.transform.InverseTransformPoint(grabOrigin);
                    #if VERBOSE_LOGGING
                    Debug.Log($"Grabbing - Chain:{grab.chainId} Bone:{grab.bone}");
                    #endif
                }
            }
            else
            {
                if (grab != null)
                {
                    PhysBoneManager.Inst.ReleaseGrab(grab);
                    grab = null;
                }
            }
        }
        Ray GetMouseRay()
        {
            return currentCamera.ScreenPointToRay(Input.mousePosition);
        }
        void UpdateGrab()
        {
            if(currentCamera == null)
                return;

            if (grab != null)
            {
                Vector3 curGrabOrigin = currentCamera.transform.TransformPoint(grabOriginLocal);
                var ray = GetMouseRay();
                Vector3 hit;
                if (PlaneLineIntersection(curGrabOrigin, -currentCamera.transform.forward, ray.origin, ray.origin + ray.direction * 1000f, out hit))
                {
                    grab.GlobalPosition = hit + grab.LocalOffset;
                }
            }
        }
        public static bool PlaneLineIntersection(Vector3 planeOrigin, Vector3 planeNormal, Vector3 lineA, Vector3 lineB, out Vector3 hit)
        {
            float delta;

            //Make sure the line is not parallel
            delta = Vector3.Dot(planeNormal, (lineB - lineA) - planeOrigin);
            if (delta == 0.0f)
            {
                hit = Vector3.zero;
                return false;
            }

            //Find the delta
            delta = Vector3.Dot(planeNormal, lineA - planeOrigin) / delta;
            hit = lineA + ((lineB - lineA) * -delta);
            return true;
        }
    }
}
#endif