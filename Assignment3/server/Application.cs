﻿using System.Threading;
using shared;

namespace Server {
    /**
     * This class implements a simple tcp echo server.
     * Read carefully through the comments below.
     * Note that the server does not contain any sort of error handling.
     */
    public static class Application {
        public static void Main(string[] args) {
            var serverTimeout = new ServerTimeout(75f);
            var packet = new Packet();
            SerializationHelper.Serialize(serverTimeout, packet);
            var readPacket = new Packet(packet.GetBytes());
            var deserialized = SerializationHelper.Deserialize(readPacket);
            Logger.Info($"Is deserialized null? {deserialized == null}");
            Logger.Info($"Is deserialized type ServerTimeout? {deserialized is ServerTimeout}");
            return;
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