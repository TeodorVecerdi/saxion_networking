using shared.model;
using shared.serialization;

namespace shared {
    /**
     * Send from SERVER to all CLIENTS in response to a client's MakeMoveRequest
     */
    [System.Serializable]
    public class MakeMoveResult : ASerializable {
        [Serialize] public int Player;
        [Serialize] public int NextTurn;
        [Serialize] public TicTacToeBoardData BoardData;
    }
}