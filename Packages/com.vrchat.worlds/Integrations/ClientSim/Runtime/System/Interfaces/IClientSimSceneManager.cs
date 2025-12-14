using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimSceneManager
    {
        bool HasSceneDescriptor();
        Transform GetSpawnPoint(bool remote);
        Transform GetSpawnPoint(int index);
        float GetSpawnRadius();
        void SetupCamera(Camera camera);
        float GetRespawnHeight();
        bool ShouldObjectsDestroyAtRespawnHeight();
    }
}