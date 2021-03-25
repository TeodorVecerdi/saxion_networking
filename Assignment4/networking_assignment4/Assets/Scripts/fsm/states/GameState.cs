using shared;
using shared.protocol;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView> {
    private bool receivedGameStarted;
    private int playerOrder;
    private int currentOrder;
    private string otherPlayerName;

    public override void EnterState() {
        base.EnterState();
        view.playerLabel1.text = "Waiting...";
        view.playerLabel2.text = "Waiting...";
        view.gameBoard.OnCellClicked += OnCellClicked;
    }

    private void OnCellClicked(int cellIndex) {
        if (!receivedGameStarted || playerOrder != currentOrder) return;

        var makeMoveRequest = new MakeMoveRequest {Move = cellIndex};
        fsm.channel.SendMessage(makeMoveRequest);
    }

    public override void ExitState() {
        base.ExitState();
        receivedGameStarted = false;
        view.gameBoard.OnCellClicked -= OnCellClicked;
    }

    private void Update() {
        ReceiveAndProcessNetworkMessages();
    }

    protected override void HandleNetworkMessage(object message) {
        if (message is MakeMoveResult makeMoveResult) HandleMakeMoveResult(makeMoveResult);
        else if (message is GameStarted gameStarted) HandleGameStarted(gameStarted);
    }

    private void HandleMakeMoveResult(MakeMoveResult makeMoveResult) {
        view.gameBoard.SetBoardData(makeMoveResult.BoardData);
        currentOrder = makeMoveResult.NextTurn;

        UpdateLabels();
    }

    private void HandleGameStarted(GameStarted gameStarted) {
        playerOrder = gameStarted.Order;
        currentOrder = 0;
        otherPlayerName = gameStarted.OtherPlayerName;
        receivedGameStarted = true;

        UpdateLabels();
    }

    private void UpdateLabels() {
        var selfLabel = playerOrder == 0 ? view.playerLabel1 : view.playerLabel2;
        var otherLabel = playerOrder == 0 ? view.playerLabel2 : view.playerLabel1;
        if (currentOrder == playerOrder) {
            selfLabel.text = $"<b>{State.Instance.SelfInfo.Name}</b>";
            otherLabel.text = $"{otherPlayerName}";
        } else {
            selfLabel.text = $"{State.Instance.SelfInfo.Name}";
            otherLabel.text = $"<b>{otherPlayerName}</b>";
        }
    }
}