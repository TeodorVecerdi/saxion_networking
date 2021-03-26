using shared.serialization;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to request joining the server.
     */
    [System.Serializable]
    public class PlayerJoinRequest : Printable {
        [Serialized] public string Name;
    }
}