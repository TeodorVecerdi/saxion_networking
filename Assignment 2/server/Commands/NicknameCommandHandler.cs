using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Commands {
    public class NicknameCommandHandler : ICommandHandler {
        public string Name => "setname";
        public string Syntax => "/setname <new_name>";
        public string Description => "Changes your display name to <new_name> if it is valid. A nickname is valid if it is unique, and if it contains at least one character.";
        public IReadOnlyList<string> Aliases { get; } = new List<string>{"sn"}.AsReadOnly();
        
        public bool IsValidSyntax(string command) {
            var split = command.Split(' ');
            // command & one parameter, the name
            return split.Length == 2 && !string.IsNullOrWhiteSpace(split[1].Trim());
        }

        public void HandleCommand(Server server, TcpClient sender, string command) {
            var newNickname = command.Split(' ')[1].Trim();
            var count = server.Clients.Count(pair => string.Equals(newNickname.ToLowerInvariant(), pair.Value.Name));
            if (count != 0) {
                server.QueueMessage($"<i>This nickname is already taken.</i>", sender);
                return;
            }

            var oldNickname = server.Clients[sender].Name;
            server.Clients[sender].UpdateNickname(newNickname);
            server.QueueMessage($"<i>Your nickname is now <b>{newNickname}</b></i>", sender);
            server.QueueBroadcast($"<i>User <b>{oldNickname}</b> changed their nickname to <b>{newNickname}</b></i>", sender);
        }
    }
}