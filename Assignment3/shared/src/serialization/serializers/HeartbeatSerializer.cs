using shared.protocol;

namespace shared.serialization {
    public class HeartbeatSerializer : Serializer<Heartbeat> {
        public override void Serialize(Heartbeat obj, Packet packet) {
            // does nothing
        }

        public override Heartbeat Deserialize(Packet packet) {
            return new Heartbeat();
        }
    }
}