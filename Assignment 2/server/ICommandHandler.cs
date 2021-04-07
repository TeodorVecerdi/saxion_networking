using System.Collections.Generic;
using System.Net.Sockets;

public interface ICommandHandler {
    string Name { get; }
    string Syntax { get; }
    string Description { get; }
    IReadOnlyList<string> Aliases { get; }

    bool IsValidSyntax(string command);
    void HandleCommand(Server server, TcpClient sender, string command);
}