/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SCKeyInputManager : UdonSharpBehaviour
    {
        private SwivelChair _swivelChair;

        private void Start()
        {
            _swivelChair = GetComponent<SwivelChair>();
        }

        private void Update()
        {
            _swivelChair._OnUpdate();
        }
    }
}
