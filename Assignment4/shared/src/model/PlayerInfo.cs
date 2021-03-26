using shared.serialization;

namespace shared.model {
    /**
     * Empty placeholder class for the PlayerInfo object which is being tracked for each client by the server.
     * Add any data you want to store for the player here and make it extend ASerializable.
     */
    [System.Serializable]
    public class PlayerInfo : Printable {
        [Serialized] public string Name;
    }
}