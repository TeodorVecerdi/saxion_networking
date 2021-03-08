using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Commands;
using Shared;

public class Server {
    private const float defaultClientTimeout = 5.0f;

    private readonly float clientTimeout;
    private readonly TcpListener listener;

    internal readonly List<ICommandHandler> Commands;
    internal readonly Dictionary<TcpClient, User> Clients;
    internal static bool Verbose;

    private readonly List<TcpClient> faultyClients;
    private readonly Queue<QueuedMessage> messageQueue;
    private readonly Queue<QueuedMessage> broadcastQueue;

    public Server(float clientTimeout = defaultClientTimeout, bool verbose = false) {
        this.clientTimeout = clientTimeout;
        Verbose = verbose;
        listener = new TcpListener(IPAddress.Any, 55555);
        Clients = new Dictionary<TcpClient, User>();
        faultyClients = new List<TcpClient>();
        messageQueue = new Queue<QueuedMessage>();
        broadcastQueue = new Queue<QueuedMessage>();

        Commands = new List<ICommandHandler> {
            new HelpCommandHandler(),
            new ListCommandHandler(),
            new NicknameCommandHandler(),
            new WhisperCommandHandler()
        };
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
            var user = new User($"guest{Clients.Keys.Count}");
            Clients.Add(client, user);

            Logger.Info($"Accepted new client {user}");
            SendMessage($"TIMEOUT:{clientTimeout:F4}", client);
            
            QueueMessage($"You joined the server as {user.Name}", client);
            QueueBroadcast($"Client {user.Name} joined the server.", client);
        }
    }

    public void ProcessQueue() {
        while (broadcastQueue.Count > 0) {
            var message = broadcastQueue.Dequeue();
            BroadcastMessage(message.Message, message.Client);
        }

        while (messageQueue.Count > 0) {
            var message = messageQueue.Dequeue();
            SendMessage(message.Message, message.Client);
        }
    }

    public void ProcessClients() {
        foreach (var client in Clients) {
            if (client.Key.Available <= 0) continue;
            var stream = client.Key.GetStream();
            var inBytes = StreamUtil.Read(stream);
            var received = Encoding.UTF8.GetString(inBytes);
            if (Verbose)
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

        foreach (var client in Clients) {
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
            Logger.Warn($"Removing invalid client {Clients[client]} (could be a disconnected client)");
            Clients.Remove(client);
            try {
                client.Close();
            } catch {
                // ignored, might fail if already closed??? idk
            }
        }

        faultyClients.Clear();
    }

    internal void QueueBroadcast(string message, TcpClient except = null) {
        broadcastQueue.Enqueue(new QueuedMessage(message, except));
    }

    internal void QueueMessage(string message, TcpClient target) {
        messageQueue.Enqueue(new QueuedMessage(message, target));
    }

    internal void BroadcastMessage(string message, TcpClient except = null) {
        foreach (var client in Clients) {
            if (client.Key == except) continue;
            SendMessage(message, client.Key);
        }
    }

    internal void SendMessage(string message, TcpClient client) {
        try {
            var encodedMessage = ServerUtility.EncodeMessageAsBytes(message);
            if (Verbose) Logger.Info($"Sent [{ServerUtility.EncodeMessage(message)}] to client {Clients[client]}", "INFO-VERBOSE");
            StreamUtil.Write(client.GetStream(), encodedMessage);
        } catch (IOException) {
            Logger.Warn($"Client {Clients[client]} disconnected!");
            faultyClients.Add(client);
        }
    }

    private void ProcessMessage(TcpClient client, string message) {
        if (!message.StartsWith("MSG:")) {
            Logger.Warn($"Unexpected message `{message}` from client {Clients[client]}. Kicking client.");
            faultyClients.Add(client);
            return;
        }

        var normalMessage = message.Substring(4); // get rid of MSG:
        BroadcastMessage($"<b>{Clients[client].Name}</b>: {normalMessage}");
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

        if (command.StartsWith("/")) {
            if (Verbose) Logger.Info($"Received chat command [{command}] from client {Clients[client]}", "INFO-VERBOSE");
            ProcessChatCommand(client, command);
            return true;
        }

        return false;
    }

    private void ProcessChatCommand(TcpClient client, string command) {
        var chatCommand = command.Split(' ')[0].Substring(1);
        var commandHandler = GetCommandHandler(chatCommand);
        if (commandHandler == null) {
            SendMessage($"Unknown chat command `/{chatCommand}`. To get a list of valid commands type `/help`.", client);
            return;
        }

        if (!commandHandler.IsValidSyntax(command)) {
            SendMessage($"Invalid syntax for command `/{chatCommand}`. Syntax: `{commandHandler.Syntax}`.", client);
            return;
        }

        commandHandler.HandleCommand(this, client, command);
        if (Verbose) Logger.Info($"Command `/{chatCommand}` was handled successfully by `{commandHandler.GetType().Name}`", "INFO-VERBOSE");
    }

    private ICommandHandler GetCommandHandler(string command) {
        return Commands.Find(handler => handler.Name == command || handler.Aliases.Contains(command));
    }

    private readonly struct QueuedMessage {
        internal readonly string Message;
        internal readonly TcpClient Client;

        public QueuedMessage(string message, TcpClient client) {
            Message = message;
            Client = client;
        }
    }
}