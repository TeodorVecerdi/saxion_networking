using System.Collections.Generic;
using shared.protocol;

namespace shared.serialization {
    public class CommandSerializer : Serializer<Command> {
        public override void Serialize(Command obj, Packet packet) {
            packet.Write(obj.CommandName);
            packet.Write(obj.Parameters.Count);
            foreach (var parameter in obj.Parameters) {
                packet.Write(parameter);
            }
        }

        public override Command Deserialize(Packet packet) {
            var commandName = packet.Read<string>();
            var argumentCount = packet.Read<int>();
            var arguments = new List<string>();
            for (var i = 0; i < argumentCount; i++) {
                arguments.Add(packet.Read<string>());
            }

            return new Command(commandName, arguments);
        }
    }
}