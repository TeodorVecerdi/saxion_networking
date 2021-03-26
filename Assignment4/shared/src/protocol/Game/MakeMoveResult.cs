using shared.model;
using shared.serialization;

namespace shared {
    /**
     * Send from SERVER to all CLIENTS in response to a client's MakeMoveRequest
     */
    [System.Serializable]
    public class MakeMoveResult : Printable {
        [Serialized] public int Player;
        [Serialized] public int NextTurn;
        [Serialized] public TicTacToeBoardData BoardData;
    }
}