using System.Linq;
using System.Net.Sockets;
using shared.protocol;

namespace Server {
    public class WhisperCommandHandler : ICommandHandler {
        public string Name => "whisper";
        public string Syntax => "/whisper";
        public string Description => "Whispers to nearby players";

        public bool IsValidSyntax(Command command) {
            return true;
        }

        public void HandleCommand(Command command, TcpServer server, TcpClient sender) {
            var message = string.Join(" ", command.Parameters.Select(s => s.Trim()));
            server.WhisperMessage(message, sender);
        }
    }
}