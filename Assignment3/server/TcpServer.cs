using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using shared;
using shared.protocol;
using shared.serialization;

namespace Server {
    public class TcpServer {
        private const float defaultClientTimeout = 5.0f;
        private const int defaultPort = 55555;
        internal static bool Verbose;

        internal readonly Dictionary<TcpClient, User> Clients;

        private readonly float clientTimeout;
        private readonly int port;
        private readonly TcpListener listener;
        private readonly List<TcpClient> faultyClients;
        private readonly Queue<QueuedMessage> messageQueue;
        private readonly Queue<QueuedMessage> broadcastQueue;

        public TcpServer(float clientTimeout = defaultClientTimeout, int port = defaultPort, bool verbose = false) {
            this.clientTimeout = clientTimeout;
            this.port = port;
            Verbose = verbose;

            faultyClients = new List<TcpClient>();
            Clients = new Dictionary<TcpClient, User>();
            listener = new TcpListener(IPAddress.Any, port);
            messageQueue = new Queue<QueuedMessage>();
            broadcastQueue = new Queue<QueuedMessage>();
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
                Clients.Add(client, user);

                Logger.Info($"Accepted new client {user}");
                // TODO: Send timeout
                SendMessage(new ServerTimeout(clientTimeout), client);

                // TODO: Send user joined
                // QueueMessage($"<i>You joined the server as {user.Name}</i>", client);
                // QueueBroadcast($"<i>Client {user.Name} joined the server.</i>", client);
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
                byte[] inBytes = StreamUtil.Read(stream);
                
                client.Value.LastHeartbeat = DateTime.Now;
                var obj = SerializationHelper.Deserialize(inBytes);
                ProcessObject(obj, client.Key);
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
                if ((DateTime.Now - client.Value.LastHeartbeat).TotalSeconds > clientTimeout) {
                    faultyClients.Add(client.Key);
                    continue;
                }
            }

            // Remove found faulty clients
            RemoveFaultyClients();
        }

        private void ProcessObject(object obj, TcpClient sender) {
            if (obj is Message message) {
                return;
            }

            if (obj is Command command) {
                HandleCommand(command);
                return;
            }
        }

        internal void QueueMessage<T>(T message, TcpClient target) {
            messageQueue.Enqueue(new QueuedMessage(message, target, null));
        }

        internal void QueueBroadcast<T>(T message, TcpClient except = null, Func<TcpClient, bool> predicate = null) {
            broadcastQueue.Enqueue(new QueuedMessage(message, except, predicate));
        }

        internal void SendMessage<T>(T message, TcpClient client) {
            try {
                var outBytes = SerializationHelper.Serialize(message);
                StreamUtil.Write(client.GetStream(), outBytes);
            } catch (SerializationException e) {
                Logger.Except(e);
            } catch (IOException e) {
                Logger.Warn($"Client {Clients[client]} disconnected!");
                faultyClients.Add(client);
            }
        }

        internal void BroadcastMessage<T>(T message, TcpClient except = null, Func<TcpClient, bool> predicate = null) {
            foreach (var client in Clients) {
                if (predicate != null && !predicate(client.Key)) continue;
                if (client.Key == except) continue;
                SendMessage(message, client.Key);
            }
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

        private readonly struct QueuedMessage {
            internal readonly object Message;
            internal readonly TcpClient Client;
            internal readonly Func<TcpClient, bool> Predicate;

            public QueuedMessage(object message, TcpClient client, Func<TcpClient, bool> predicate) {
                Message = message;
                Client = client;
                Predicate = predicate;
            }
        }
    }
}