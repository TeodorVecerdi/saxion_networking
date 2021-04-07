using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class ListRoomCommandHandler : ICommandHandler {
        public string Name => "listroom";
        public string Syntax => "/listroom";
        public string Description => "Lists all connected users in the room you're currently in.";
        public IReadOnlyList<string> Aliases { get; } = new List<string>().AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            return true;
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var sb = new StringBuilder();
            sb.AppendLine($"Connected users in room <b>{server.Clients[sender].Room}</b>:");
            foreach (var client in server.Clients) {
                if(client.Value.Room != server.Clients[sender].Room) continue;
                
                var isSelf = client.Key == sender;
                sb.AppendLine($"{client.Value.Name}{(isSelf ? " <b>(You)</b>":"")}");
            }
            
            server.QueueMessage(sb.ToString().TrimEnd('\n', ' ', '\t'), sender);
        }
    }
}