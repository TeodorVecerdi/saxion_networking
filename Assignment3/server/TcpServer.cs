using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using shared;
using shared.protocol;
using shared.serialization;

namespace Server {
    public class TcpServer {
        private const float defaultClientTimeout = 5.0f;
        private const int defaultPort = 55555;
        internal static bool Verbose;

        private readonly float clientTimeout;
        private readonly int port;
        private readonly TcpListener listener;
        private readonly Dictionary<TcpClient, User> clients;
        private readonly List<TcpClient> faultyClients;
        private readonly Queue<QueuedMessage> messageQueue;
        private readonly Queue<QueuedMessage> broadcastQueue;
        private readonly List<ICommandHandler> commands;

        public TcpServer(float clientTimeout = defaultClientTimeout, int port = defaultPort, bool verbose = false) {
            this.clientTimeout = clientTimeout;
            this.port = port;
            Verbose = verbose;

            clients = new Dictionary<TcpClient, User>();
            faultyClients = new List<TcpClient>();
            listener = new TcpListener(IPAddress.Any, port);
            messageQueue = new Queue<QueuedMessage>();
            broadcastQueue = new Queue<QueuedMessage>();

            commands = new List<ICommandHandler> {
                new WhisperCommandHandler(),
                new ReskinCommandHandler()
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
                var user = new User();
                clients.Add(client, user);

                Logger.Info($"Accepted new client {user}");
                SendMessage(new HelloWorld(clientTimeout, user.Id), client);
                var connectedClients = new ConnectedClients(clients.Where(x => x.Key != client)
                                       .Select(x => new UserModel(x.Value.Id, x.Value.SkinId, x.Value.PositionX, x.Value.PositionY, x.Value.PositionZ)));
                QueueMessage(connectedClients, client);
                QueueBroadcast(new ClientJoined(user.Id, user.SkinId, user.PositionX, user.PositionY, user.PositionZ));
            }
        }

        public void ProcessQueue() {
            while (broadcastQueue.Count > 0) {
                var message = broadcastQueue.Dequeue();
                BroadcastMessage(message.Message, message.Client, message.Predicate, message.TypeId);
            }

            while (messageQueue.Count > 0) {
                var message = messageQueue.Dequeue();
                SendMessage(message.Message, message.Client, message.TypeId);
            }
        }

        public void ProcessClients() {
            foreach (var client in clients) {
                if (client.Key.Available <= 0) continue;
                var stream = client.Key.GetStream();
                byte[] inBytes = StreamUtil.Read(stream);
                client.Value.LastHeartbeat = DateTime.Now;

                var obj = SerializationHelper.Deserialize(inBytes);
                if (Verbose)
                    Logger.Info($"Received object [{obj.GetType().Name}] from client {client.Value.Id}", "INFO-VERBOSE");
                ProcessObject(obj, client.Key);
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
                if ((DateTime.Now - client.Value.LastHeartbeat).TotalSeconds > clientTimeout) {
                    faultyClients.Add(client.Key);
                    continue;
                }
            }

            // Remove found faulty clients
            RemoveFaultyClients();
        }

        private void ProcessObject(object obj, TcpClient sender) {
            switch (obj) {
                case Message message:
                    QueueBroadcast(message);
                    return;
                case Command command:
                    HandleCommand(sender, command);
                    return;
                case PositionChangeRequest positionChangeRequest:
                    // check if position is valid
                    var deltaCenter = (positionChangeRequest.X * positionChangeRequest.X + positionChangeRequest.Z * positionChangeRequest.Z);
                    var valid = Constants.SpawnRange * Constants.SpawnRange - deltaCenter > 0;
                    if (!valid) return;

                    clients[sender].UpdatePosition(positionChangeRequest.X, positionChangeRequest.Y, positionChangeRequest.Z);
                    var positionChanged = new PositionChanged(positionChangeRequest.UserId, positionChangeRequest.X, positionChangeRequest.Y, positionChangeRequest.Z);
                    QueueBroadcast(positionChanged);
                    return;
            }
        }

        internal void ReskinUser(TcpClient client) {
            clients[client].Reskin();
            var newSkin = clients[client].SkinId;
            QueueBroadcast(new SkinChanged(clients[client].Id, newSkin));
        }

        internal void WhisperMessage(string message, TcpClient client) {
            var sender = clients[client];
            var messagePacket = new Message(clients[client].Id, message);
            foreach (var clientPair in clients) {
                var dx = clientPair.Value.PositionX - sender.PositionX;
                var dz = clientPair.Value.PositionZ - sender.PositionZ;
                // distance = 2 
                if (dx * dx + dz * dz > 4.0f) continue;

                QueueMessage(messagePacket, clientPair.Key);
            }
        }

        internal void QueueMessage<T>(T message, TcpClient target) {
            messageQueue.Enqueue(new QueuedMessage(TypeIdCache.Get(typeof(T)), message, target, null));
        }

        internal void QueueBroadcast<T>(T message, TcpClient except = null, Func<TcpClient, bool> predicate = null) {
            broadcastQueue.Enqueue(new QueuedMessage(TypeIdCache.Get(typeof(T)), message, except, predicate));
        }

        internal void SendMessage<T>(T message, TcpClient client, int? typeId = null) {
            try {
                var outBytes = typeId.HasValue ? SerializationHelper.Serialize(message, typeId.Value) : SerializationHelper.Serialize(message);
                StreamUtil.Write(client.GetStream(), outBytes);
                if (Verbose) Logger.Info($"Sent object [{message.GetType().Name}] to client {clients[client].Id}", "INFO-VERBOSE");
            } catch (IOException e) {
                Logger.Warn($"Client {clients[client].Id} disconnected!");
                faultyClients.Add(client);
            } catch (Exception e) {
                Logger.Except(e, true);
            } 
        }

        internal void BroadcastMessage<T>(T message, TcpClient except = null, Func<TcpClient, bool> predicate = null, int? typeId = null) {
            if (Verbose) Logger.Info($"Broadcasting object [{message.GetType().Name}]", "INFO-VERBOSE");
            foreach (var client in clients) {
                if (predicate != null && !predicate(client.Key)) continue;
                if (client.Key == except) continue;
                SendMessage(message, client.Key, typeId);
            }
        }

        private void RemoveFaultyClients() {
            foreach (var client in faultyClients) {
                Logger.Warn($"Removing invalid client {clients[client].Id} (could be a disconnected client)");
                QueueBroadcast(new ClientLeft(clients[client].Id), client);
                clients.Remove(client);

                try {
                    client.Close();
                } catch {
                    // ignored, might fail if already closed??? idk
                }
            }

            faultyClients.Clear();
        }

        private void HandleCommand(TcpClient client, Command command) {
            var commandHandler = GetCommandHandler(command.CommandName);
            if (commandHandler == null || !commandHandler.IsValidSyntax(command)) {
                return;
            }

            commandHandler.HandleCommand(command, this, client);
            if (Verbose) Logger.Info($"Command `/{command.CommandName}` was handled successfully by `{commandHandler.GetType().Name}`", "INFO-VERBOSE");
        }

        private ICommandHandler GetCommandHandler(string command) {
            return commands.Find(handler => handler.Name == command);
        }

        private readonly struct QueuedMessage {
            internal readonly int TypeId;
            internal readonly object Message;
            internal readonly TcpClient Client;
            internal readonly Func<TcpClient, bool> Predicate;

            public QueuedMessage(int typeId, object message, TcpClient client, Func<TcpClient, bool> predicate) {
                TypeId = typeId;
                Message = message;
                Client = client;
                Predicate = predicate;
            }
        }
    }
}