/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using UnityEditor;
using UdonSharpEditor;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(DynamicsParameterTransfer))]
    public class DynamicsParameterTransferEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            var dpt = (DynamicsParameterTransfer)target;
            if (GUILayout.Button("Setup Contacts in Children"))
            {
                SetupContactsInChildren(dpt.gameObject);
            }
        }

        private void SetupContactsInChildren(GameObject dptObject)
        {
            var receivers = dptObject.GetComponentsInChildren<VRCContactReceiver>(true);
            foreach (var receiver in receivers)
            {
                if (!receiver.TryGetComponent<ContactReceiverInfomation>(out var alreadyAdded))
                {
                    UdonSharpUndo.AddComponent<ContactReceiverInfomation>(receiver.gameObject);
                }
            }
        }
    }
}
