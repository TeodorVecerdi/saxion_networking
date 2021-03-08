using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class WhisperCommandHandler : ICommandHandler {
        public string Name => "whisper";
        public string Syntax => "/whisper <target> <message>";
        public string Description => "Whisper <message> to <target>";
        public IReadOnlyList<string> Aliases { get; } = new List<string>{"w"}.AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            var split = command.Split(' ').ToList();
            split.RemoveAll(string.IsNullOrWhiteSpace);
            // command, target parameter, and at least one word
            return split.Count >= 3 && !string.IsNullOrWhiteSpace(split[1].Trim()) && !string.IsNullOrWhiteSpace(split[2].Trim());
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var split = command.Split(' ').ToList();
            split.RemoveAll(string.IsNullOrWhiteSpace);
            
            // Find target
            var targetNickname = split[1].Trim().ToLowerInvariant();
            if (server.Clients[sender].Name == targetNickname) {
                server.QueueMessage($"You cannot whisper to yourself!", sender);
                return;
            }
            
            var targets = server.Clients.Where(x => x.Value.Name == targetNickname).ToList();
            if (targets.Count < 1) {
                server.QueueMessage($"Target <b>{targetNickname}</b> does not exist.", sender);
                return;
            }
            var target = targets[0];
            
            // Join message
            var sb = new StringBuilder();
            for (int i = 2; i < split.Count; i++) {
                sb.Append($"{split[i]} ");
            }
            var message = sb.ToString().Trim();
            
            // Emit
            server.QueueMessage($"You whisper to <b>{targetNickname}</b> {message}", sender);
            server.QueueMessage($"<b>{server.Clients[sender].Name}</b> whispers {message}", target.Key);
        }
    }
}