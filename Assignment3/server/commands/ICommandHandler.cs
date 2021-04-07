using System.Collections.Generic;
using System.Net.Sockets;
using shared.protocol;

namespace Server {
    public interface ICommandHandler {
        string Name { get; }
        string Syntax { get; }
        string Description { get; }

        bool IsValidSyntax(Command command);
        void HandleCommand(Command command, TcpServer server, TcpClient sender);
    }
}