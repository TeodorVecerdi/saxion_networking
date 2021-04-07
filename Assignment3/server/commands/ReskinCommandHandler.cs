using System.Net.Sockets;
using shared.protocol;

namespace Server {
    public class ReskinCommandHandler : ICommandHandler {
        public string Name => "reskin";
        public string Syntax => "/reskin";
        public string Description => "Changes your skin to a random skin";

        public bool IsValidSyntax(Command command) {
            return true;
        }

        public void HandleCommand(Command command, TcpServer server, TcpClient sender) {
            server.ReskinUser(sender);
        }
    }
}