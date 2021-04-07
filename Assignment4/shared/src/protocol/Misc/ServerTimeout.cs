using SerializationSystem;

namespace shared.protocol {
    public class ServerTimeout : Printable {
        [Serialized] public float Timeout;
    }
}