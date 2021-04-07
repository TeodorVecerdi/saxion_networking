using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class JoinRoomCommandHandler : ICommandHandler {
        public string Name => "joinroom";
        public string Syntax => "/joinroom <room_name>";
        public string Description => "Joins room <b><room_name></b> or creates it if it doesn't exist";
        public IReadOnlyList<string> Aliases { get; } = new List<string>().AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            var split = command.Split(' ');
            // command & one parameter
            return split.Length == 2 && !string.IsNullOrWhiteSpace(split[1].Trim());
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var roomName = command.Split(' ')[1].Trim().ToLowerInvariant();
            server.JoinOrCreateRoom(roomName, sender);
        }
    }
}