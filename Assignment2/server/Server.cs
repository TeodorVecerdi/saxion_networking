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
    internal const string defaultRoomName = "general";

    private readonly float clientTimeout;
    private readonly TcpListener listener;

    internal readonly List<ICommandHandler> Commands;
    internal readonly Dictionary<TcpClient, User> Clients;
    internal readonly HashSet<string> Rooms;
    internal static bool Verbose;

    private readonly List<TcpClient> faultyClients;
    private readonly Queue<QueuedMessage> messageQueue;
    private readonly Queue<QueuedMessage> broadcastQueue;
    private readonly int port;

    public Server(float clientTimeout = defaultClientTimeout, bool verbose = false, int port = 55555) {
        this.clientTimeout = clientTimeout;
        faultyClients = new List<TcpClient>();
        messageQueue = new Queue<QueuedMessage>();
        broadcastQueue = new Queue<QueuedMessage>();
        this.port = port;
        
        listener = new TcpListener(IPAddress.Any, port);
        Clients = new Dictionary<TcpClient, User>();
        Rooms = new HashSet<string> {defaultRoomName};
        Verbose = verbose;

        Commands = new List<ICommandHandler> {
            new HelpCommandHandler(),
            new ListCommandHandler(),
            new NicknameCommandHandler(),
            new WhisperCommandHandler(),
            new ListRoomCommandHandler(),
            new ListRoomsCommandHandler(),
            new JoinRoomCommandHandler(),
            new RoomCommandHandler()
        };
    }

    public void StartListening() {
        Logger.Info($"Server started listening on port {port}");
        listener.Start();
    }

    public void StopListening() {
        Logger.Info("Server stopped listening.");
        listener.Stop();
    }

    public void AcceptClients() {
        while (listener.Pending()) {
            var client = listener.AcceptTcpClient();
            var user = new User($"guest{Clients.Keys.Count}", defaultRoomName);
            Clients.Add(client, user);

            Logger.Info($"Accepted new client {user}");
            SendMessage($"TIMEOUT:{clientTimeout:F4}", client);
            
            QueueMessage($"<i>You joined the server as {user.Name}</i>", client);
            QueueBroadcast($"<i>Client {user.Name} joined the server.</i>", client);
        }
    }

    public void ProcessQueue() {
        while (broadcastQueue.Count > 0) {
            var message = broadcastQueue.Dequeue();
            BroadcastMessage(message.Message, message.Client, message.Predicate);
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

    internal void QueueBroadcast(string message, TcpClient except = null, Func<TcpClient, bool> predicate = null) {
        broadcastQueue.Enqueue(new QueuedMessage(message, except, predicate));
    }

    internal void QueueMessage(string message, TcpClient target) {
        messageQueue.Enqueue(new QueuedMessage(message, target, null));
    }

    internal void BroadcastMessage(string message, TcpClient except = null, Func<TcpClient, bool> predicate = null) {
        foreach (var client in Clients) {
            if (predicate != null && !predicate(client.Key)) continue;
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

    internal void JoinOrCreateRoom(string roomName, TcpClient client) {
        var oldRoom = Clients[client].Room;
        if (oldRoom == roomName) {
            QueueMessage($"<i>You are already in room <b>{roomName}</b></i>", client);
            return;
        }
        Rooms.Add(roomName);
        Clients[client].UpdateRoom(roomName);
        
        // Check to see if oldRoom still has people
        var count = Clients.Values.Count(user => string.Equals(user.Room, oldRoom, StringComparison.Ordinal));
        var deleted = false;
        if (count == 0 && oldRoom != defaultRoomName) {
            Rooms.Remove(oldRoom);
            deleted = true;
            if (Verbose) Logger.Info($"Room {oldRoom} was deleted.", "INFO-VERBOSE");
        } else {
            if (Verbose) Logger.Info($"Room {oldRoom} was not deleted because it has {count} people still in it. People: {string.Join(", ", Clients.Values.Where(user => user.Room == oldRoom).Select(user => user.Name))}", "INFO-VERBOSE");
        }
        
        // Tell client that they joined a new room
        QueueMessage($"<i>You are now in room <b>{roomName}</b></i>", client);
        QueueBroadcast($"<i><b>{Clients[client].Name}</b> joined the room.</i>", client, tcpClient => Clients[tcpClient].Room == roomName);
        if(!deleted) QueueBroadcast($"<i><b>{Clients[client].Name}</b> left the room.</i>", client, tcpClient => Clients[tcpClient].Room == oldRoom);
    }

    private void ProcessMessage(TcpClient client, string message) {
        if (!message.StartsWith("MSG:")) {
            Logger.Error($"Unexpected message `{message}` from client {Clients[client]}. Kicking client.");
            faultyClients.Add(client);
            return;
        }

        var normalMessage = message.Substring(4); // get rid of MSG:
        BroadcastMessage($"<b>{Clients[client].Name}</b>: {normalMessage}", predicate: tcpClient => Clients[tcpClient].Room == Clients[client].Room);
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
            SendMessage($"<i>Unknown chat command `/{chatCommand}`. To get a list of valid commands type `/help`.</i>", client);
            return;
        }

        if (!commandHandler.IsValidSyntax(command)) {
            SendMessage($"<i>Invalid syntax for command `/{chatCommand}`. Syntax: `{commandHandler.Syntax}`.</i>", client);
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
        internal readonly Func<TcpClient, bool> Predicate;

        public QueuedMessage(string message, TcpClient client, Func<TcpClient, bool> predicate) {
            Message = message;
            Client = client;
            Predicate = predicate;
        }
    }
}