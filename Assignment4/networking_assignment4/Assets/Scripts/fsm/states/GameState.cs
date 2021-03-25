using shared;
using shared.protocol;
using shared.serialization;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView> {
    //just for fun we keep track of how many times a player clicked the board
    //note that in the current application you have no idea whether you are player 1 or 2
    //normally it would be better to maintain this sort of info on the server if it is actually important information
    private int player1MoveCount = 0;
    private int player2MoveCount = 0;

    public override void EnterState() {
        base.EnterState();

        view.gameBoard.OnCellClicked += _onCellClicked;
    }

    private void _onCellClicked(int cellIndex) {
        var makeMoveRequest = new MakeMoveRequest {Move = cellIndex};

        fsm.channel.SendMessage(makeMoveRequest);
    }

    public override void ExitState() {
        base.ExitState();
        view.gameBoard.OnCellClicked -= _onCellClicked;
    }

    private void Update() {
        ReceiveAndProcessNetworkMessages();
    }

    protected override void HandleNetworkMessage(object message) {
        if (message is MakeMoveResult makeMoveResult) {
            HandleMakeMoveResult(makeMoveResult);
        }
    }

    private void HandleMakeMoveResult(MakeMoveResult makeMoveResult) {
        view.gameBoard.SetBoardData(makeMoveResult.BoardData);

        //some label display
        if (makeMoveResult.Player == 1) {
            player1MoveCount++;
            view.playerLabel1.text = $"Player 1 (Movecount: {player1MoveCount})";
        }

        if (makeMoveResult.Player == 2) {
            player2MoveCount++;
            view.playerLabel2.text = $"Player 2 (Movecount: {player2MoveCount})";
        }
    }
}