using System.Collections.Generic;
using shared.protocol;

namespace shared.serialization {
    public class ConnectedClientsSerializer : Serializer<ConnectedClients> {
        public override void Serialize(ConnectedClients obj, Packet packet) {
            packet.Write(obj.Users.Count);
            foreach (var userModel in obj.Users) {
                packet.Write(userModel);
            }
        }

        public override ConnectedClients Deserialize(Packet packet) {
            var users = new List<UserModel>();
            var count = packet.Read<int>();
            for (var i = 0; i < count; i++) {
                users.Add(SerializationHelper.Deserialize(packet) as UserModel);
            }

            return new ConnectedClients(users);
        }
    }
}