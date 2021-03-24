using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace shared {

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
    public class TcpMessageChannel
    {
        private TcpClient _client = null;                               //the underlying client connection
        private NetworkStream _stream = null;                           //the client's cached stream
        private IPEndPoint _remoteEndPoint = null;                      //cached endpoint info so we can still access it, even if the connection closes
        
        //stores all errors that occurred (can be used for debug info to get an idea of where and why the channel failed)
        private List<Exception> _errors = new List<Exception>();

        //quick cache thingy to avoid reserialization of objects when you have a lot of clients (only applies to the serverside)
        private static ASerializable _lastSerializedMessage = null;
        private static byte[] _lastSerializedBytes = null;

        /**
         * Creates a TcpMessageChannel based on an existing (and connected) TcpClient.
         * This is usually used on the server side after accepting a TcpClient from a TcpListener.
         */
        public TcpMessageChannel(TcpClient pTcpClient)
        {
            Log.LogInfo("TCPMessageChannel created around "+pTcpClient, this, ConsoleColor.Blue);

            _client = pTcpClient;
            _stream = _client.GetStream();
            _remoteEndPoint = _client.Client.RemoteEndPoint as IPEndPoint;
        }

        /**
         * Creates TcpMessageChannel which doesn't have an underlying connected TcpClient yet.
         * This is usually used on the client side, where you call Connect (..,..) on the TcpMessageChannel
         * after creating it. 
         */
        public TcpMessageChannel ()
        {
            Log.LogInfo("TCPMessageChannel created (not connected).", this, ConsoleColor.Blue);
        }

        /**
         * Try to (re)connect to the given server and port (blocks until connected or failed).
         * 
         * @return bool indicating connection status
         */
        public bool Connect (string pServerIP, int pServerPort)
        {
            Log.LogInfo("Connecting...", this, ConsoleColor.Blue);

            try
            {
                _client = new TcpClient();
                _client.Connect(pServerIP, pServerPort);
                _stream = _client.GetStream();
                _remoteEndPoint = _client.Client.RemoteEndPoint as IPEndPoint;
                _errors.Clear();
                Log.LogInfo("Connected.", this, ConsoleColor.Blue);
                return true;
            }
            catch (Exception e)
            {
                addError(e);
                return false;
            }
        }

        /**
         * Send the given message through the underlying TcpClient's NetStream.
         */
        public void SendMessage(ASerializable pMessage)
        {
            if (HasErrors())
            {
                Log.LogInfo("This channel has errors, cannot send.", this, ConsoleColor.Red);
                return;
            }

            //everything we log from now to the end of this method should be cyan
            Log.PushForegroundColor(ConsoleColor.Cyan);
            Log.LogInfo(pMessage, this);

            try
            {
                //grab the required bytes from either the packet or the cache
                if (_lastSerializedMessage != pMessage)
                {
                    Packet outPacket = new Packet();
                    outPacket.Write(pMessage);
                    _lastSerializedBytes = outPacket.GetBytes();
                }

                StreamUtil.Write(_stream, _lastSerializedBytes);
            }
            catch (Exception e)
            {
                addError(e);
            }

            Log.PopForegroundColor();
        }

        /**
         * Is there a message pending?
         */
        public bool HasMessage()
        {
            //we use an update StreamUtil.Available check instead of just Available > 0
            return Connected && StreamUtil.Available(_client);
        }

        /**
         * Block until a complete message is read over the underlying's TcpClient's NetStream.
         * If you don't want to block, check HasMessage first().
         */
        public ASerializable ReceiveMessage()
        {
            if (HasErrors())
            {
                Log.LogInfo("This channel has errors, cannot receive.", this, ConsoleColor.Red);
                return null;
            }

            try
            {
                Log.PushForegroundColor(ConsoleColor.Yellow);
                Log.LogInfo("Receiving message...", this);
                
                byte[] inBytes = StreamUtil.Read(_stream);
                Packet inPacket = new Packet(inBytes);
                ASerializable inObject = inPacket.ReadObject();
                Log.LogInfo("Received " + inObject, this);
                Log.PopForegroundColor();

                return inObject;
            }
            catch (Exception e)
            {
                addError(e);
                return null;
            }
        }

        /**
         * Similar to TcpClient connected, but also returns false if underlying client is null, or errors were detected.
         */
        public bool Connected 
        {
            get {
                    return !HasErrors() && _client != null && _client.Connected;
            }
        }

        public bool HasErrors()
        {
            return _errors.Count > 0;
        }

        public List<Exception> GetErrors()
        {
            return new List<Exception>(_errors);
        }

        private void addError(Exception pError)
        {
            Log.LogInfo("Error added:"+pError, this, ConsoleColor.Red);
            _errors.Add(pError);
            Close();
        }

        public IPEndPoint GetRemoteEndPoint() { return _remoteEndPoint; }

        public void Close ()
        {
            try
            {
                _client.Close();
            } catch {
            }
            finally
            {
                _client = null;
            }
        }

    }
}
