using shared.protocol;

namespace shared.serialization {
    public class PositionChangedSerializer : Serializer<PositionChanged> {
        public override void Serialize(PositionChanged obj, Packet packet) {
            packet.Write(obj.UserId);
            packet.Write(obj.X);
            packet.Write(obj.Z);
        }

        public override PositionChanged Deserialize(Packet packet) {
            return new PositionChanged(packet.Read<int>(), packet.Read<float>(), packet.Read<float>());
        }
    }
}