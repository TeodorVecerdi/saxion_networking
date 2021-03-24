using shared.model;
using shared.serialization;

namespace shared {
    /**
     * Send from SERVER to all CLIENTS in response to a client's MakeMoveRequest
     */
    public class MakeMoveResult : ASerializable {
        [Serialize] public int Player;
        [Serialize] public TicTacToeBoardData BoardData;
    }
}