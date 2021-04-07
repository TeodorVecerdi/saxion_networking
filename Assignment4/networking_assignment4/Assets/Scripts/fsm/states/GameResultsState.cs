using shared.protocol;

public class GameResultsState : ApplicationStateWithView<GameResultsView> {
    
    public override void EnterState() {
        base.EnterState();

        view.Load(State.Instance.LastGameResult);
        view.ReturnToLobbyButton.onClick.AddListener(() => {
            fsm.channel.SendMessage(new LeaveRoomRequest {Room = RoomType.GAME_RESULTS_ROOM});
        });
    }

    public override void ExitState() {
        base.ExitState();
        view.ReturnToLobbyButton.onClick.RemoveAllListeners();
    }

    private void Update() {
        ReceiveAndProcessNetworkMessages();
    }

    protected override void HandleNetworkMessage(object message) {
        if (message is RoomJoinedEvent {Room: RoomType.LOBBY_ROOM}) {
            fsm.ChangeState<LobbyState>();
        }
    }
}