using JetBrains.Annotations;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

namespace VRC.Udon
{
    public abstract class AbstractSerializedUdonProgramAsset : ScriptableObject
    {
        [PublicAPI]
        public abstract void StoreProgram(IUdonProgram udonProgram);

        [PublicAPI]
        public abstract void StoreProgram(IUdonProgram udonProgram, NetworkCallingEntrypointMetadata[] networkCallingMetadata);

        [PublicAPI]
        public abstract IUdonProgram RetrieveProgram();

        [PublicAPI]
        public abstract ulong GetSerializedProgramSize();

        [PublicAPI]
        public abstract NetworkCallingEntrypointMetadata[] GetNetworkCallingMetadata();

        [PublicAPI]
        public abstract NetworkCallingEntrypointMetadata GetNetworkCallingMetadata(string entrypoint);

        [PublicAPI]
        public abstract bool TryGetEntrypointNameFromHash(uint hash, out string entrypoint);

        [PublicAPI]
        public abstract bool TryGetEntrypointHashFromName(string entrypoint, out uint hash);
    }
}
