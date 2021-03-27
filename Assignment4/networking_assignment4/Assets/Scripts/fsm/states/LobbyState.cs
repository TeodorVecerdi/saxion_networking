using shared.protocol;
using UnityEngine;

/**
 * 'Chat' state while you are waiting to start a game where you can signal that you are ready or not.
 */
public class LobbyState : ApplicationStateWithView<LobbyView> {
    [Tooltip("Should we enter the lobby in a ready state or not?")]
    [SerializeField] private bool AutoQueueForGame;

    public override void EnterState() {
        base.EnterState();

        view.SetLobbyHeading("Welcome to the Lobby...");
        view.ClearOutput();
        view.AddOutput($"Server settings: {fsm.channel.GetRemoteEndPoint()}");
        view.SetReadyToggle(AutoQueueForGame);

        view.OnChatTextEntered += OnTextEntered;
        view.OnReadyToggleClicked += OnReadyToggleClicked;

        if (AutoQueueForGame) {
            OnReadyToggleClicked(true);
        }
    }

    public override void ExitState() {
        base.ExitState();

        view.OnChatTextEntered -= OnTextEntered;
        view.OnReadyToggleClicked -= OnReadyToggleClicked;
    }

    /**
     * Called when you enter text and press enter.
     */
    private void OnTextEntered(string text) {
        view.ClearInput();
        var messageText = text.Trim();
        if(string.IsNullOrWhiteSpace(messageText)) return;
        var message = new ChatMessage {Message = messageText};
        fsm.channel.SendMessage(message);
    }

    /**
     * Called when you click on the ready checkbox
     */
    private void OnReadyToggleClicked(bool newValue) {
        var msg = new ChangeReadyStatusRequest {Ready = newValue};
        fsm.channel.SendMessage(msg);
    }

    private void AddOutput(string pInfo) {
        view.AddOutput(pInfo);
    }

    /// //////////////////////////////////////////////////////////////////
    ///                     NETWORK MESSAGE PROCESSING
    /// //////////////////////////////////////////////////////////////////
    private void Update() {
        ReceiveAndProcessNetworkMessages();
    }

    protected override void HandleNetworkMessage(object message) {
        if (message is ChatMessage chatMessage) HandleChatMessage(chatMessage);
        else if (message is RoomJoinedEvent roomJoinedEvent) HandleRoomJoinedEvent(roomJoinedEvent);
        else if (message is LobbyInfoUpdate lobbyInfoUpdate) HandleLobbyInfoUpdate(lobbyInfoUpdate);
    }

    private void HandleChatMessage(ChatMessage message) {
        //just show the message
        AddOutput(message.Message);
    }

    private void HandleRoomJoinedEvent(RoomJoinedEvent message) {
        //did we move to the game room?
        if (message.Room == RoomType.GAME_ROOM) {
            fsm.ChangeState<GameState>();
        }
    }

    private void HandleLobbyInfoUpdate(LobbyInfoUpdate message) {
        //update the lobby heading
        view.SetLobbyHeading($"Welcome to the Lobby ({message.MemberCount} people, {message.ReadyCount} ready)");
    }
}