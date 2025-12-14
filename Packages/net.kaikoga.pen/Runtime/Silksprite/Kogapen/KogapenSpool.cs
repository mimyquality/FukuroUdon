using UdonSharp;
using UnityEngine;

namespace Silksprite.Kogapen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KogapenSpool : UdonSharpBehaviour
    {
        [SerializeField] internal KogapenStylus[] styli;
        [SerializeField] internal KogapenStylus[] dynamicStyli;

        public void Kogapen_RespawnAll()
        {
            foreach (var stylus in styli)
            {
                if (!stylus) continue;
                stylus.Kogapen_Respawn();
            }
        }
    }
}
