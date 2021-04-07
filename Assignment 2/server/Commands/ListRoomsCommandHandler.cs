using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class ListRoomsCommandHandler : ICommandHandler {
        public string Name => "listrooms";
        public string Syntax => "/listrooms";
        public string Description => "Lists all rooms";
        public IReadOnlyList<string> Aliases { get; } = new List<string>().AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            return true;
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var sb = new StringBuilder();
            sb.AppendLine("Rooms:");
            var rooms = new List<string> {$"{Server.defaultRoomName} <b>(Default Room)</b>"};
            
            foreach (var room in server.Rooms) {
                if (room == Server.defaultRoomName) continue;
                rooms.Add(room);
            }

            sb.AppendLine(string.Join(", ", rooms));
            server.QueueMessage(sb.ToString().TrimEnd('\n', ' ', '\t'), sender);
        }
    }
}