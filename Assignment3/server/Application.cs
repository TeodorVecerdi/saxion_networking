using System.Threading;
using shared.protocol;
using shared.serialization;

namespace Server {
    public static class Application {
        public static void Main(string[] args) {
            SerializationHelper.Serialize(new Heartbeat());
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