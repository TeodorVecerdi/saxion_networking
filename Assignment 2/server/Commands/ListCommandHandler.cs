using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class ListCommandHandler : ICommandHandler {
        public string Name => "list";
        public string Syntax => "/list";
        public string Description => "Lists all connected users";
        public IReadOnlyList<string> Aliases { get; } = new List<string>().AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            return true;
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var sb = new StringBuilder();
            sb.AppendLine("Connected users:");
            foreach (var client in server.Clients) {
                var isSelf = client.Key == sender;
                sb.AppendLine($"{client.Value.Name}{(isSelf ? " <b>(You)</b>":"")}");
            }
            
            server.QueueMessage(sb.ToString().TrimEnd('\n', ' ', '\t'), sender);
        }
    }
}