using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using shared;

public class Server {
    private const float defaultClientTimeout = 5.0f;

    private readonly float clientTimeout;
    private readonly TcpListener listener;
    private readonly Dictionary<TcpClient, User> clients;
    internal static bool verbose;

    private readonly List<TcpClient> faultyClients;

    public Server(float clientTimeout = defaultClientTimeout, bool verbose = false) {
        this.clientTimeout = clientTimeout;
        Server.verbose = verbose;
        listener = new TcpListener(IPAddress.Any, 55555);
        clients = new Dictionary<TcpClient, User>();
        faultyClients = new List<TcpClient>();
    }

    public void StartListening() {
        Logger.Info($"Server started listening on port {55555}");
        listener.Start();
    }

    public void StopListening() {
        Logger.Info("Server stopped listening.");
        listener.Stop();
    }

    public void AcceptClients() {
        while (listener.Pending()) {
            var client = listener.AcceptTcpClient();
            var user = new User($"guest{clients.Keys.Count}");
            clients.Add(client, user);
            
            Logger.Info($"Accepted new client {user}");
            StreamUtil.Write(client.GetStream(), Encoding.UTF8.GetBytes($"TIMEOUT:{clientTimeout:F4}"));
            StreamUtil.Write(client.GetStream(), Encoding.UTF8.GetBytes($"MSG:You joined the server as {user.Name}"));
            BroadcastMessage($"MSG:Client {user.Name} joined the server.", client);
        }
    }

    public void ProcessClients() {
        foreach (var client in clients) {
            if (client.Key.Available <= 0) continue;
            var stream = client.Key.GetStream();
            var inBytes = StreamUtil.Read(stream);
            var received = Encoding.UTF8.GetString(inBytes);
            if (verbose) 
                Logger.Info($"Received message [{received}] from client {client.Value}", "INFO-VERBOSE");
            
            client.Value.OnHeartbeat();
            if (!ProcessSpecial(client.Key, received)) {
                ProcessMessage(client.Key, received);
            }
        }
    }

    public void CleanupClients() {
        // Remove in case we have any faulty clients 
        RemoveFaultyClients();

        foreach (var client in clients) {
            // Kill if not connected
            if (!client.Key.Connected) {
                faultyClients.Add(client.Key);
                continue;
            }

            // Kill if didn't send a heartbeat
            if ((DateTime.Now - client.Value.LastHeartbeatTime).TotalSeconds > clientTimeout) {
                faultyClients.Add(client.Key);
                continue;
            }
        }

        // Remove found faulty clients
        RemoveFaultyClients();
    }

    private void RemoveFaultyClients() {
        foreach (var client in faultyClients) {
            Logger.Warn($"Removing faulty client. {clients[client]}");
            clients.Remove(client);
            try {
                client.Close();
            } catch {
                // ignored, might fail if already closed??? idk
            }
        }

        faultyClients.Clear();
    }

    private void BroadcastMessage(string message, TcpClient except = null) {
        foreach (var client in clients) {
            if (client.Key == except) continue;
            try {
                StreamUtil.Write(client.Key.GetStream(), Encoding.UTF8.GetBytes($"MSG:{message}"));
            } catch (IOException ioException) {
                Logger.Warn($"Client {client.Value} disconnected!");
                faultyClients.Add(client.Key);
            }
        }
    }

    private void ProcessMessage(TcpClient client, string message) {
        if (!message.StartsWith("MSG:")) {
            Logger.Warn($"Unexpected message `{message}` from client {clients[client]}. Kicking client.");
            faultyClients.Add(client);
            return;
        }
        var normalMessage = message.Substring(4); // get rid of MSG:
        BroadcastMessage($"{clients[client].Name}: {normalMessage}");
    }

    /// <summary>
    /// Executes command <paramref name="command"/> if it is a special command.
    /// Returns <value>true</value> if <paramref name="command"/> is a special command, and <value>false</value> otherwise.
    /// </summary>
    /// <returns><value>true</value> if <paramref name="command"/> is a special command, and <value>false</value> otherwise</returns>
    private bool ProcessSpecial(TcpClient client, string command) {
        if (string.Equals(command, "HEARTBEAT", StringComparison.Ordinal)) {
            return true;
        }
        
        var isChatCommand = command.StartsWith("/");
        if (isChatCommand) {
            if (verbose) Logger.Info($"Received chat command [{command}] from client {clients[client]}", "INFO-VERBOSE");
            ProcessChatCommand(client, command);
            return true;
        }

        return false;
    }

    private void ProcessChatCommand(TcpClient client, string command) {
        var chatCommand = command.Split(' ')[0].Substring(1);
        StreamUtil.Write(client.GetStream(), Encoding.UTF8.GetBytes($"MSG:Unknown chat command `/{chatCommand}`! To get a list of valid commands type `/help`."));
    }
}