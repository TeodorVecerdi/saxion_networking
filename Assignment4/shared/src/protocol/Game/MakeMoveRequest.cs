using shared.serialization;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to indicate the move the client would like to make.
     * Since the board is just an array of cells, move is a simple index.
     */
    [System.Serializable]
    public class MakeMoveRequest : ASerializable {
        [Serialized] public int Move;
    }
}