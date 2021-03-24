using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace shared.serialization {
    /**
     * TcpMessageChannel is sort of a facade around a TcpClient, Packets & the StreamUtil class.
     * It abstracts communication to a single bidirectional channel which you can use to pass 
     * ASerializable objects back and forth, and check to see whether everything is still peachy 
     * where the underlying connection is concerned.
     * 
     * Basically after the initial setup, 'all' you have to worry about is having a channel and
     * being able to push objects through it.
     * 
     * If you want to implement your own serialization mechanism, this is the place to do it.
     */
    public class TcpMessageChannel {
        private TcpClient client; //the underlying client connection
        private NetworkStream stream; //the client's cached stream
        private IPEndPoint remoteEndPoint; //cached endpoint info so we can still access it, even if the connection closes

        //stores all errors that occurred (can be used for debug info to get an idea of where and why the channel failed)
        private readonly List<Exception> errors = new List<Exception>();

        //quick cache thingy to avoid re-serialization of objects when you have a lot of clients (only applies to the serverside)
        private static object lastSerializedMessage;
        private static byte[] lastSerializedBytes;

        /**
         * Creates a TcpMessageChannel based on an existing (and connected) TcpClient.
         * This is usually used on the server side after accepting a TcpClient from a TcpListener.
         */
        public TcpMessageChannel(TcpClient tcpClient) {
            Log.LogInfo("TCPMessageChannel created around " + tcpClient, this, ConsoleColor.Blue);

            client = tcpClient;
            stream = client.GetStream();
            remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        }

        /**
         * Creates TcpMessageChannel which doesn't have an underlying connected TcpClient yet.
         * This is usually used on the client side, where you call Connect (..,..) on the TcpMessageChannel
         * after creating it. 
         */
        public TcpMessageChannel() {
            Log.LogInfo("TCPMessageChannel created (not connected).", this, ConsoleColor.Blue);
        }

        /**
         * Try to (re)connect to the given server and port (blocks until connected or failed).
         * 
         * @return bool indicating connection status
         */
        public bool Connect(string serverIP, int serverPort) {
            Log.LogInfo("Connecting...", this, ConsoleColor.Blue);

            try {
                client = new TcpClient();
                client.Connect(serverIP, serverPort);
                stream = client.GetStream();
                remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                errors.Clear();
                Log.LogInfo("Connected.", this, ConsoleColor.Blue);
                return true;
            } catch (Exception e) {
                AddError(e);
                return false;
            }
        }

        /**
         * Send the given message through the underlying TcpClient's NetStream.
         */
        public void SendMessage(object message) {
            if (HasErrors()) {
                Log.LogInfo("This channel has errors, cannot send.", this, ConsoleColor.Red);
                return;
            }

            //everything we log from now to the end of this method should be cyan
            Log.PushForegroundColor(ConsoleColor.Cyan);
            Log.LogInfo(message, this);

            try {
                //grab the required bytes from either the packet or the cache
                if (lastSerializedMessage != message) {
                    lastSerializedBytes = message.Serialize();
                    lastSerializedMessage = message;
                }

                StreamUtil.Write(stream, lastSerializedBytes);
            } catch (Exception e) {
                AddError(e);
            }

            Log.PopForegroundColor();
        }

        /**
         * Is there a message pending?
         */
        public bool HasMessage() {
            //we use an update StreamUtil.Available check instead of just Available > 0
            return Connected && StreamUtil.Available(client);
        }

        /**
         * Block until a complete message is read over the underlying's TcpClient's NetStream.
         * If you don't want to block, check HasMessage first().
         */
        public object ReceiveMessage() {
            if (HasErrors()) {
                Log.LogInfo("This channel has errors, cannot receive.", this, ConsoleColor.Red);
                return null;
            }

            try {
                Log.PushForegroundColor(ConsoleColor.Yellow);
                Log.LogInfo("Receiving message...", this);

                byte[] inBytes = StreamUtil.Read(stream);
                var message = inBytes.Deserialize();
                Log.LogInfo("Received " + message, this);
                Log.PopForegroundColor();

                return message;
            } catch (Exception e) {
                AddError(e);
                return null;
            }
        }

        /**
         * Similar to TcpClient connected, but also returns false if underlying client is null, or errors were detected.
         */
        public bool Connected => !HasErrors() && client != null && client.Connected;

        public bool HasErrors() {
            return errors.Count > 0;
        }

        public List<Exception> GetErrors() {
            return new List<Exception>(errors);
        }

        private void AddError(Exception error) {
            Log.LogInfo("Error added:" + error, this, ConsoleColor.Red);
            errors.Add(error);
            Close();
        }

        public IPEndPoint GetRemoteEndPoint() {
            return remoteEndPoint;
        }

        public void Close() {
            try {
                client.Close();
            } catch {
                // ignore
            } finally {
                client = null;
            }
        }
    }
}