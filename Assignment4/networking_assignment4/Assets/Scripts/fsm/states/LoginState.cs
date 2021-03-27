using System.Collections.Generic;
using shared.protocol;
using UnityEngine;

/**
 * Starting state where you can connect to the server.
 */
public class LoginState : ApplicationStateWithView<LoginView> {
    [SerializeField] private string ServerIP;
    [SerializeField] private int ServerPort;
    [Tooltip("To avoid long iteration times, set this to true while testing.")]
    [SerializeField] private bool AutoConnectWithRandomName;

    public override void EnterState() {
        base.EnterState();

        //listen to a connect click from our view
        view.ButtonConnect.onClick.AddListener(Connect);

        //If flagged, generate a random name and connect automatically
        if (!AutoConnectWithRandomName) return;

        var names = new List<string> {"Pergu", "Korgulg", "Xaguk", "Rodagog", "Kodagog", "Dular", "Buggug", "Gruumsh"};
        view.UserName = names[Random.Range(0, names.Count)];
        Connect();
    }

    public override void ExitState() {
        base.ExitState();

        //stop listening to button clicks
        view.ButtonConnect.onClick.RemoveAllListeners();
    }

    /**
     * Connect to the server (with some client side validation)
     */
    private void Connect() {
        if (view.UserName == "") {
            view.TextConnectResults = "Please enter a name first";
            return;
        }
        view.TextConnectResults = "";

        //connect to the server and on success try to join the lobby
        if (fsm.channel.Connect(ServerIP, ServerPort)) {
            TryToJoinLobby();
        } else {
            view.TextConnectResults = "Oops, couldn't connect:" + string.Join("\n", fsm.channel.GetErrors());
        }
    }

    private void TryToJoinLobby() {
        //Construct a player join request based on the user name 
        var playerJoinRequest = new PlayerJoinRequest {Name = view.UserName};
        fsm.channel.SendMessage(playerJoinRequest);
    }

    /// //////////////////////////////////////////////////////////////////
    ///                     NETWORK MESSAGE PROCESSING
    /// //////////////////////////////////////////////////////////////////
    private void Update() {
        //if we are connected, start processing messages
        if (fsm.channel.Connected) ReceiveAndProcessNetworkMessages();
    }

    protected override void HandleNetworkMessage(object message) {
        if (message is PlayerJoinResponse playerJoinResponse) HandlePlayerJoinResponse(playerJoinResponse);
        else if (message is RoomJoinedEvent roomJoinedEvent) HandleRoomJoinedEvent(roomJoinedEvent);
    }

    private void HandlePlayerJoinResponse(PlayerJoinResponse message) {
        //Dont do anything with this info at the moment, just leave it to the RoomJoinedEvent
        //We could handle duplicate name messages, get player info etc here
        if (message.Result == PlayerJoinResponse.RequestResult.ACCEPTED) {
            State.Instance.Initialize(message.PlayerInfo, message.ServerTimeout, fsm);
        } else if (message.Result == PlayerJoinResponse.RequestResult.CONFLICT) {
            view.TextConnectResults = "Name is already taken. Please choose another one.";
        }
    }

    private void HandleRoomJoinedEvent(RoomJoinedEvent message) {
        if (message.Room == RoomType.LOBBY_ROOM) {
            fsm.ChangeState<LobbyState>();
        }
    }
}