using System;
using SerializationSystem.Logging;
using shared.model;
using shared.protocol;
using UnityCommons;

public class State : MonoSingleton<State> {
    public PlayerInfo SelfInfo => selfInfo;

    private bool initialized;
    private PlayerInfo selfInfo;
    private float serverTimeout;
    private ApplicationFSM fsm;
    private IDisposable heartbeatCancelToken;

    public void Initialize(PlayerInfo selfInfo, float serverTimeout, ApplicationFSM fsm) {
        if (initialized) {
            Log.Warn("Attempting to initialize State when it is already initialized");
            return;
        }

        initialized = true;
        this.selfInfo = selfInfo;
        this.serverTimeout = serverTimeout;
        this.fsm = fsm;
        BeginHeartbeat();
    }

    private void BeginHeartbeat() {
        heartbeatCancelToken?.Dispose();
        heartbeatCancelToken = Run.Every(serverTimeout / 2.0f, () => {
            fsm.channel.SendMessage(new Heartbeat());
        });
    }

    private void OnDestroy() {
        heartbeatCancelToken?.Dispose();
    }
}