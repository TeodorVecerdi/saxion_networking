using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Assignment_1 {
    public static class Program {
        public static void Main(string[] args) { }
        
        public static void Server() {
            var listener = new TcpListener(IPAddress.Any, 55555);
            listener.Start();
            Console.WriteLine($"Server started on {listener.LocalEndpoint}");
            while (true) {
                var client = listener.AcceptTcpClient();
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                Console.WriteLine($"Client connected from {endpoint.Address}:{endpoint.Port}");

                var stream = client.GetStream();

                while (true) {
                    try {
                        var buffer = new byte[1024];
                        var byteCount = stream.Read(buffer, 0, buffer.Length);
                        var str = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        
                        Console.WriteLine($"Received {str} ({byteCount} bytes)");
                        stream.Write(buffer, 0, byteCount);
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        break; //yeet
                    }
                }
            }
        }

        public static void Client() {
            var client = new TcpClient();
            client.Connect("localhost", 55555);
            Console.WriteLine($"Connected to server {client.Client.RemoteEndPoint} from {client.Client.LocalEndPoint}\n");
            
            var stream = client.GetStream();
            while (true) {
                Console.WriteLine("Enter message:");
                var outString = Console.ReadLine();
                var sendBytes = Encoding.UTF8.GetBytes(outString);
                stream.Write(sendBytes, 0, sendBytes.Length);

                while (true) {
                    var buffer = new byte[1024];
                    var byteCount = stream.Read(buffer, 0, buffer.Length);
                    var recvString = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine($"Received message from server: {recvString}");
                    break;
                }
            }
        }
    }
}