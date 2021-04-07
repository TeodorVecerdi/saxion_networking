using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class RoomCommandHandler : ICommandHandler {
        public string Name => "room";
        public string Syntax => "/room";
        public string Description => "Shows the room you're currently in.";
        public IReadOnlyList<string> Aliases { get; } = new List<string>().AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            return true;
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            server.QueueMessage($"You are currently in room <b>{server.Clients[sender].Room}</b>", sender);
        }
    }
}