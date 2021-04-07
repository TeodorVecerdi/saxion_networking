using shared.protocol;

namespace shared.serialization {
    public class HelloWorldSerializer : Serializer<HelloWorld> {
        public override void Serialize(HelloWorld obj, Packet packet) {
            packet.Write(obj.Timeout);
            packet.Write(obj.SelfUserId);
        }

        public override HelloWorld Deserialize(Packet packet) {
            return new HelloWorld(packet.Read<float>(), packet.Read<int>());
        }
    }
}