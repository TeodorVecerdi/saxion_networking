using shared.serialization;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to request joining the server.
     */
    [System.Serializable]
    public class PlayerJoinRequest : ASerializable {
        [Serialized] public string Name;
    }
}