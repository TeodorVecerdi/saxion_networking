using shared.serialization;
using shared.serialization.attr;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to indicate the move the client would like to make.
     * Since the board is just an array of cells, move is a simple index.
     */
    public class MakeMoveRequest : ASerializable {
        [Serialize] public int Move;
    }
}