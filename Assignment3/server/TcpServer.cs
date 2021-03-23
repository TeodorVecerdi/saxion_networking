using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using shared;

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

        public TcpServer(float clientTimeout = defaultClientTimeout, int port = defaultPort, bool verbose = false) {
            this.clientTimeout = clientTimeout;
            this.port = port;
            Verbose = verbose;

            faultyClients = new List<TcpClient>();
            Clients = new Dictionary<TcpClient, User>();
            listener = new TcpListener(IPAddress.Any, port);
            
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
            throw new NotImplementedException();
        }

        public void ProcessClients() {
            foreach (var client in Clients) {
                if (client.Key.Available <= 0) continue;
                var stream = client.Key.GetStream();
                byte[] inBytes = StreamUtil.Read(stream);
                
                // TODO: Deserialize bytes
                // var received = Encoding.UTF8.GetString(inBytes);
                // if (Verbose) Logger.Info($"Received message [{received}] from client {client.Value}", "INFO-VERBOSE");

                client.Value.LastHeartbeat = DateTime.Now;
                // TODO: Process request
                // if (!ProcessSpecial(client.Key, received)) {
                    // ProcessMessage(client.Key, received);
                // }
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

        private void SendMessage<T>(T message, TcpClient client) {
            /*var typeId = typeof(T).GUID.GetHashCode();
            if (!serializers.ContainsKey(typeId)) {
                Logger.Error($"Could not find serializer for type {typeof(T).FullName}");
                return;
            }

            var serializerType = serializers[typeId];
            var inst =  Activator.CreateInstance();*/
        }
    }
}