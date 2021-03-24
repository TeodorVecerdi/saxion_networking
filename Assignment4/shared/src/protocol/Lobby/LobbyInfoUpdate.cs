using shared.serialization;
using shared.serialization.attr;

namespace shared.protocol {
    /**
	 * Send from SERVER to all CLIENTS to provide info on how many people are in the lobby
	 * and how many of them are ready.
	 */
    public class LobbyInfoUpdate : ASerializable {
        [Serialize] public int MemberCount;
        [Serialize] public int ReadyCount;
    }
}