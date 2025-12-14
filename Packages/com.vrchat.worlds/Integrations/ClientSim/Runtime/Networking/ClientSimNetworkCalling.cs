
using System;
using UnityEngine;
using VRC.SDK3.Network;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace VRC.SDK3.ClientSim
{
    public static class ClientSimNetworkCalling
    {
        // this is fine (insert funny dog with fire meme)
        internal static int GetAllQueuedEventsProxy() => 0;
        internal static int GetQueuedEventsProxy(IUdonEventReceiver udonBehaviour, string eventName) => 0;

        // magic goes here, though note we don't really encode/decode parameters here, just pass them through
        // in the real client, even local events are passed through binary serialization
        internal static void SendCustomNetworkEventProxy(IUdonEventReceiver udonBehaviour, NetworkEventTarget target, string eventName, Memory<object> parameters)
        {
            // "Others" is not relevant for ClientSim, we're forever alone :(
            // "All", "Owner" and "Self" pass through all the same
            if (target == NetworkEventTarget.Others)
                return;

            if (udonBehaviour is not UdonBehaviour behaviour)
            {
                // ClientSim only warning
                Debug.LogWarning($"Attempted to send network event {eventName}, but the IUdonEventReceiver was null or the wrong type.");
                return;
            }

            if (behaviour.SyncMethod == Networking.SyncType.None)
            {
                Debug.LogError($"Unable to send network event to UdonBehaviour '{behaviour.name}' with SyncType '{behaviour.SyncMethod}'.", behaviour);
                return;
            }

            var hasEventHash = behaviour.TryGetEntrypointHashFromName(eventName, out var _);
            if (!hasEventHash)
            {
                Debug.LogError($"Unable to send network event '{eventName}' as it does not exist on the target Udon behaviour '{behaviour.name}'.");
                return;
            }

            var metadata = behaviour.GetNetworkCallingMetadata(eventName);

            if (metadata == null)
            {
                // legacy event, match client checks and errors
                if (eventName.StartsWith("_"))
                {
                    Debug.LogError($"Unable to send local event '{eventName}' as an RPC. Events starting with an underscore may not be run remotely.");
                    return;
                }

                if (parameters.Length > 0)
                {
                    throw new ArgumentException($"Network call to '{eventName}' had {parameters.Length} parameters, but no metadata was found. Are you missing a [NetworkCallable] attribute?");
                }

                // GameObject targeting for legacy events
                foreach (var component in behaviour.gameObject.GetComponents<UdonBehaviour>())
                {
                    if (component == null || component.SyncMethod == SDKBase.Networking.SyncType.None)
                        continue;

                    RunEventSafe(component, eventName, null, null);
                }
            }
            else
            {
                // TODO: add size limits

                // modern style, once again match client validation errors here
                metadata.ValidateOnce();

                if (metadata.Parameters.Length != parameters.Length)
                {
                    throw new ArgumentException($"Parameter count mismatch for network call to '{metadata.Name}'. Expected {metadata.Parameters.Length}, got {parameters.Length}.");
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters.Span[i];
                    if (parameter == null)
                        continue;
                    var paramMetadata = metadata.Parameters[i];
                    var parameterType = parameter.GetType();
                    if (VRCUdonSyncTypeConverter.TypeToUdonType(parameterType) != paramMetadata.Type)
                    {
                        throw new ArgumentException($"Parameter '{paramMetadata.Name}' in network call to '{metadata.Name}' had incorrect type '{parameterType}'.");
                    }
                }

                // component targeting for modern events
                RunEventSafe(behaviour, eventName, metadata, parameters);
            }
        }

        private static void RunEventSafe(UdonBehaviour udonBehaviour, string eventName, NetworkCallingEntrypointMetadata metadata, Memory<object> parameters)
        {
            // client does a try/catch, so we do it too
            try
            {
                NetworkCalling.WithNetworkCallingContext(Networking.LocalPlayer, () => {
                    RunEvent(udonBehaviour, eventName, metadata, parameters.Span);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error running Udon network event '{eventName}' on {udonBehaviour.name}: {e}");
            }
        }

        private static void RunEvent(UdonBehaviour udonBehaviour, string eventName, NetworkCallingEntrypointMetadata metadata, Span<object> parameters)
        {
            var parameterCount = metadata?.Parameters != null ? metadata.Parameters.Length : 0;
            switch (parameterCount)
            {
                case 0:
                    // includes legacy events
                    udonBehaviour.RunEventAdvanced(eventName, canRunBeforeStart: true);
                    break;
                case 1:
                    udonBehaviour.RunEventAdvanced<object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null)
                    );
                    break;
                case 2:
                    udonBehaviour.RunEventAdvanced<object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null)
                    );
                    break;
                case 3:
                    udonBehaviour.RunEventAdvanced<object, object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null),
                        (metadata.Parameters[2].Name, parameters.Length >= 3 ? parameters[2] : null)
                    );
                    break;
                case 4:
                    udonBehaviour.RunEventAdvanced<object, object, object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null),
                        (metadata.Parameters[2].Name, parameters.Length >= 3 ? parameters[2] : null),
                        (metadata.Parameters[3].Name, parameters.Length >= 4 ? parameters[3] : null)
                    );
                    break;
                case 5:
                    udonBehaviour.RunEventAdvanced<object, object, object, object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null),
                        (metadata.Parameters[2].Name, parameters.Length >= 3 ? parameters[2] : null),
                        (metadata.Parameters[3].Name, parameters.Length >= 4 ? parameters[3] : null),
                        (metadata.Parameters[4].Name, parameters.Length >= 5 ? parameters[4] : null)
                    );
                    break;
                // look I don't like "vibe-coding" and "AI programmers", but you'd be mistaken if you think I didn't just tell copilot to copy these at some point
                case 6:
                    udonBehaviour.RunEventAdvanced<object, object, object, object, object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null),
                        (metadata.Parameters[2].Name, parameters.Length >= 3 ? parameters[2] : null),
                        (metadata.Parameters[3].Name, parameters.Length >= 4 ? parameters[3] : null),
                        (metadata.Parameters[4].Name, parameters.Length >= 5 ? parameters[4] : null),
                        (metadata.Parameters[5].Name, parameters.Length >= 6 ? parameters[5] : null)
                    );
                    break;
                case 7:
                    udonBehaviour.RunEventAdvanced<object, object, object, object, object, object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null),
                        (metadata.Parameters[2].Name, parameters.Length >= 3 ? parameters[2] : null),
                        (metadata.Parameters[3].Name, parameters.Length >= 4 ? parameters[3] : null),
                        (metadata.Parameters[4].Name, parameters.Length >= 5 ? parameters[4] : null),
                        (metadata.Parameters[5].Name, parameters.Length >= 6 ? parameters[5] : null),
                        (metadata.Parameters[6].Name, parameters.Length >= 7 ? parameters[6] : null)
                    );
                    break;
                case 8:
                    udonBehaviour.RunEventAdvanced<object, object, object, object, object, object, object, object>(eventName, mangleParameterNames: false, canRunBeforeStart: true,
                        (metadata.Parameters[0].Name, parameters.Length >= 1 ? parameters[0] : null),
                        (metadata.Parameters[1].Name, parameters.Length >= 2 ? parameters[1] : null),
                        (metadata.Parameters[2].Name, parameters.Length >= 3 ? parameters[2] : null),
                        (metadata.Parameters[3].Name, parameters.Length >= 4 ? parameters[3] : null),
                        (metadata.Parameters[4].Name, parameters.Length >= 5 ? parameters[4] : null),
                        (metadata.Parameters[5].Name, parameters.Length >= 6 ? parameters[5] : null),
                        (metadata.Parameters[6].Name, parameters.Length >= 7 ? parameters[6] : null),
                        (metadata.Parameters[7].Name, parameters.Length >= 8 ? parameters[7] : null)
                    );
                    break;
            }
        }
    }
}