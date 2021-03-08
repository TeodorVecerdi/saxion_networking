using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class HelpCommandHandler : ICommandHandler {
        public string Name => "help";
        public string Syntax => "/help";
        public string Description => "Shows this help message";
        public IReadOnlyList<string> Aliases { get; } = new List<string>().AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            return true;
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            foreach (var commandHandler in server.commands) {
                sb.AppendLine($"[{commandHandler.Name}]: {commandHandler.Syntax}");
                sb.AppendLine($"    Description: {commandHandler.Description}");
                if(commandHandler.Aliases.Count > 0)
                    sb.AppendLine($"    Aliases: {string.Join(", ", commandHandler.Aliases.Select(alias => $"/{alias}"))}");
            }
            server.SendMessage(sb.ToString().TrimEnd('\n', ' ', '\t'), sender);
        }
    }
}