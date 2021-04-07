using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using shared;
using shared.protocol;
using shared.serialization;

namespace Server {
    public static class Application {
        public static void Main(string[] args) {
            var users = new List<UserModel> {new User().ToUserModel(), new User().ToUserModel(), new User().ToUserModel()};
            var conn = new ConnectedClients(users);
            var s = SerializationHelper.Serialize(conn);
            var ds = SerializationHelper.Deserialize(s);
            Console.WriteLine($"is ds null? {ds == null}");
            Console.WriteLine($"is ds ConnectedClients? {ds is ConnectedClients}");
            Console.WriteLine($"correct count? {(ds as ConnectedClients).Users.Count}");

            var server = new TcpServer(verbose: true);
            
            server.StartListening();
            while (true) {
                server.AcceptClients(); // accept new clients
                server.ProcessQueue(); // process queued messages
                server.ProcessClients(); // process clients for new messages
                server.CleanupClients(); // remove faulty/disconnected clients

                Thread.Sleep(25);
            }
        }
    }
}