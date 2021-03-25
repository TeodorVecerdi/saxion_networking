using shared.serialization;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to indicate the move the client would like to make.
     * Since the board is just an array of cells, move is a simple index.
     */
    [System.Serializable]
    public class GameStarted : ASerializable {
        [Serialize] public int Order;
        [Serialize] public string OtherPlayerName;
    }
}