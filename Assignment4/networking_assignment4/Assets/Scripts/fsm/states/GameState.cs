using shared;
using shared.model;
using shared.protocol;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView> {
    private bool receivedGameStarted;
    private int selfId;
    private int currentOrder;
    private string otherPlayerName;

    public override void EnterState() {
        base.EnterState();
        view.PlayerLabel1.text = "Waiting for server...";
        view.PlayerLabel2.text = "";
        
        view.GameBoard.SetBoardData(new TicTacToeBoardData()); // reset board
        view.GameBoard.OnCellClicked += OnCellClicked;
        view.LeaveButton.onClick.AddListener(() => {
            fsm.channel.SendMessage(new LeaveRoomRequest{Room = RoomType.GAME_ROOM});
        });
    }

    private void OnCellClicked(int cellIndex) {
        if (!receivedGameStarted || selfId != currentOrder) return;

        var makeMoveRequest = new MakeMoveRequest {Move = cellIndex};
        fsm.channel.SendMessage(makeMoveRequest);
    }

    public override void ExitState() {
        base.ExitState();
        receivedGameStarted = false;
        view.GameBoard.OnCellClicked -= OnCellClicked;
        view.LeaveButton.onClick.RemoveAllListeners();
    }

    private void Update() {
        ReceiveAndProcessNetworkMessages();
    }

    protected override void HandleNetworkMessage(object message) {
        if (message is MakeMoveResult makeMoveResult) HandleMakeMoveResult(makeMoveResult);
        else if (message is GameStarted gameStarted) HandleGameStarted(gameStarted);
        else if (message is GameResults gameResults) State.Instance.UpdateGameResults(selfId, gameResults);
        else if (message is RoomJoinedEvent roomJoinedEvent) {
            if (roomJoinedEvent.Room == RoomType.GAME_RESULTS_ROOM) {
                fsm.ChangeState<GameResultsState>();
            } else if (roomJoinedEvent.Room == RoomType.LOBBY_ROOM) {
                fsm.ChangeState<LobbyState>();
            }
        }
    }

    private void HandleMakeMoveResult(MakeMoveResult makeMoveResult) {
        view.GameBoard.SetBoardData(makeMoveResult.BoardData);
        currentOrder = makeMoveResult.NextTurn;

        UpdateLabels();
    }

    private void HandleGameStarted(GameStarted gameStarted) {
        selfId = gameStarted.Order;
        currentOrder = 0;
        otherPlayerName = gameStarted.OtherPlayerName;
        receivedGameStarted = true;

        UpdateLabels();
    }

    private void UpdateLabels() {
        var selfLabel = selfId == 0 ? view.PlayerLabel1 : view.PlayerLabel2;
        var otherLabel = selfId == 0 ? view.PlayerLabel2 : view.PlayerLabel1;
        if (currentOrder == selfId) {
            selfLabel.text = $"<b>{State.Instance.SelfInfo.Name} (You)</b>";
            otherLabel.text = $"{otherPlayerName}";
        } else {
            selfLabel.text = $"{State.Instance.SelfInfo.Name} (You)";
            otherLabel.text = $"<b>{otherPlayerName}</b>";
        }
    }
}