using shared.serialization;

namespace shared.protocol {
    /**
	 * Send from SERVER to all CLIENTS to provide info on how many people are in the lobby
	 * and how many of them are ready.
	 */
    [System.Serializable]
    public class LobbyInfoUpdate : ASerializable {
        [Serialized] public int MemberCount;
        [Serialized] public int ReadyCount;
    }
}