using System.Collections;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox.runtime
{
    [AddComponentMenu("/")]
    internal class CoroutineHandler : EditorOnlyBehaviour
    {
        private static CoroutineHandler m_Instance;
        private static CoroutineHandler Instance => m_Instance ? m_Instance : m_Instance = new GameObject("CoroutineHandler"){hideFlags = HideFlags.HideAndDontSave}.AddComponent<CoroutineHandler>();
        private void OnDisable(){if(m_Instance) Destroy(m_Instance.gameObject);}
        internal static Coroutine StartStaticCoroutine(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);
    }
}
