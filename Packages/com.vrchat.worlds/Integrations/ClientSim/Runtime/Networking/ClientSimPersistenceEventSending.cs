using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using VRC.SDK3.ClientSim.Persistence;
using VRC.Udon;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimPersistenceEventSending
    {
#if VRC_ENABLE_PLAYER_PERSISTENCE
        private static ClientSimPersistenceEventSending _instance;
        
        public static ClientSimPersistenceEventSending Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClientSimPersistenceEventSending();
                }

                return _instance;
            }
        }
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private Dictionary<ClientSimPlayerObjectStorage,List<UdonBehaviour>> _RequestedObjects = new Dictionary<ClientSimPlayerObjectStorage,List<UdonBehaviour>>();
        
        private const int TIME_BETWEEN_EVENTS = 50;
        
        private ClientSimPersistenceEventSending()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            SendNetworkEvents().Forget();
        }
        
        ~ClientSimPersistenceEventSending()
        {
            _cancellationTokenSource.Cancel();
        }
        
        public void QueueRequest(UdonBehaviour udonBehaviour, ClientSimPlayerObjectStorage playerObjectStorage)
        {
            if(!_RequestedObjects.ContainsKey(playerObjectStorage)){
                List<UdonBehaviour> udonBehaviours = new List<UdonBehaviour>();
                udonBehaviours.Add(udonBehaviour);
                _RequestedObjects[playerObjectStorage] = udonBehaviours;
            }
            else
            {
                if(!_RequestedObjects[playerObjectStorage].Contains(udonBehaviour))
                    _RequestedObjects[playerObjectStorage].Add(udonBehaviour);
            }
        }

        private async UniTask SendNetworkEvents()
        {
            await UniTask.SwitchToMainThread();
            
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await UniTask.Delay(TIME_BETWEEN_EVENTS, cancellationToken: _cancellationTokenSource.Token);

                List<ClientSimPlayerObjectStorage> keys = _RequestedObjects.Keys.ToList();
                
                for (int i = keys.Count-1; i >= 0; i--)
                {
                    ClientSimPlayerObjectStorage storage = keys[i];
                    List<UdonBehaviour> udonBehaviours = _RequestedObjects[storage];
                    for (int j = udonBehaviours.Count-1; j >= 0; j--)
                    {
                        storage.Encode(udonBehaviours[j].gameObject);
                    }
                }
                
                _RequestedObjects.Clear();
            }
        }
#endif
    }
}