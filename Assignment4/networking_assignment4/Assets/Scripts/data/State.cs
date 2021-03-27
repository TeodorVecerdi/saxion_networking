using System;
using SerializationSystem.Logging;
using shared.model;
using shared.protocol;
using UnityCommons;
using UnityEditor;
using UnityEngine;

public class State : MonoSingleton<State> {
    public PlayerInfo SelfInfo => selfInfo;
    public GameResult LastGameResult => lastGameResult;

    private bool initialized;
    private PlayerInfo selfInfo;
    private float serverTimeout;
    private ApplicationFSM fsm;
    private IDisposable heartbeatCancelToken;
    private GameResult lastGameResult;

    public void Initialize(PlayerInfo selfInfo, ApplicationFSM fsm) {
        if (initialized) {
            Log.Warn("Attempting to initialize State when it is already initialized");
            return;
        }

        initialized = true;
        this.selfInfo = selfInfo;
        this.fsm = fsm;
    }

    public void InitializeHeartbeat(float timeout) {
        serverTimeout = timeout;
        BeginHeartbeat();
    }

    public void UpdateGameResults(int selfId, GameResults results) {
        lastGameResult = new GameResult {
            SelfID = selfId,
            IsTie = results.IsTie,
            WinnerID = results.WinnerID,
            WinnerName = results.WinnerName
        };
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