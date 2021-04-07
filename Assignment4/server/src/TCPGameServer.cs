using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using SerializationSystem;
using SerializationSystem.Logging;
using shared;
using shared.model;
using shared.net;
using shared.protocol;

namespace server {/**
	 * Basic TCPGameServer that runs our game.
	 * 
	 * Server is made up out of different rooms that can hold different members.
	 * Each member is identified by a TcpMessageChannel, which can also be used for communication.
	 * In this setup each client is only member of ONE room, but you could change that of course.
	 * 
	 * Each room is responsible for cleaning up faulty clients (since it might involve gameplay, status changes etc).
	 * 
	 * As you can see this setup is limited/lacking:
	 * - only 1 game can be played at a time
	 */

    class SerializationError : Printable {
        [Serialized] public string ErrorType;
        [Serialized] public string Message;

        public SerializationError(string errorType, string message) {
            ErrorType = errorType;
            Message = message;
        }
    }

    class TestHandler : SerializationExceptionHandler {
        public override byte[] HandleSerializationException(Exception exception) {
            Log.Except(exception, includeStackTrace: true, includeTimestamp: false, includeFileInfo: false);
            ReplaceSerializationResult(Serializer.Serialize(new SerializationError(exception.GetType().FullName, exception.Message)));
            return new byte[0];
        }

        public override object HandleDeserializationException(Exception exception) {
            Log.Except(exception, includeStackTrace: true, includeTimestamp: false, includeFileInfo: false);
            ReplaceDeserializationResult(new SerializationError(exception.GetType().FullName, exception.Message));
            return null;
        }
    }

    public class TCPGameServer {
        public static void Main() {
            // LogOptions.LogSerialization = false;
            // LogOptions.LogSerializationReplacements = false;
            // LogOptions.LogSerializationWrite = false;
            // LogOptions.LogSerializationRead = false;
            Serializer.ExceptionHandler = new TestHandler();
            new TCPGameServer(5f).Run();
        }

        private readonly LoginRoom loginRoom; //this is the room every new user joins
        private readonly LobbyRoom lobbyRoom; //this is the room a user moves to after a successful 'login'
        private readonly GameResultsRoom gameResultsRoom;
        private readonly List<GameRoom> gameRooms;

        //stores additional info for a player
        private readonly Dictionary<TcpMessageChannel, PlayerInfo> playerInfo = new Dictionary<TcpMessageChannel, PlayerInfo>();

        private readonly float timeout;
        public float Timeout => timeout;

        //provide access to the different rooms on the server 
        public LoginRoom LoginRoom => loginRoom;
        public LobbyRoom LobbyRoom => lobbyRoom;
        public GameResultsRoom GameResultsRoom => gameResultsRoom;

        private TCPGameServer(float timeout) {
            this.timeout = timeout;
            //we have only one instance of each room, this is especially limiting for the game room (since this means you can only have one game at a time).
            loginRoom = new LoginRoom(this);
            lobbyRoom = new LobbyRoom(this);
            gameResultsRoom = new GameResultsRoom(this, 60.0f);
            gameRooms = new List<GameRoom>();
        }

        private void Run() {
            Log.Message("Starting server on port 55555", this);

            //start listening for incoming connections (with max 50 in the queue)
            //we allow for a lot of incoming connections, so we can handle them
            //and tell them whether we will accept them or not instead of bluntly declining them
            var listener = new TcpListener(IPAddress.Any, 55555);
            listener.Start(50);

            while (true) {
                //check for new members	
                if (listener.Pending()) {
                    //get the waiting client
                    Log.Message("Accepting new client...", this, ConsoleColor.White);

                    var client = new TcpMessageChannel(listener.AcceptTcpClient());
                    playerInfo[client] = new PlayerInfo {LastHeartbeat = DateTime.Now};
                    client.SendMessage(new ServerTimeout {Timeout = timeout});
                    loginRoom.AddMember(client);
                }

                //now update every single room
                loginRoom.Update();
                lobbyRoom.Update();
                gameResultsRoom.Update();
                gameRooms.SafeForEach(room => room.Update());
                
                // Remove timeout users again in case some weird stuff happened and a client isn't in any room  
                foreach (var client in playerInfo.Keys.Where(client => !IsHeartbeatValid(client)).ToList()) {
                    RemoveClient(client, "Timeout");
                }

                Thread.Sleep(50);
            }
        }

        /**
		 * Returns a handle to the player info for the given client 
		 * (will create new player info if there was no info for the given client yet)
		 */
        public PlayerInfo GetPlayerInfo(TcpMessageChannel client) {
            return playerInfo[client];
        }

        /**
		 * Returns a list of all players that match the predicate, e.g. to get a list of 
		 * all players named bob, you would do:
		 *	GetPlayerInfo((playerInfo) => playerInfo.name == "bob");
		 */
        public List<PlayerInfo> GetPlayerInfo(Func<PlayerInfo, bool> predicate) {
            return playerInfo.Values.Where(predicate).ToList();
        }

        /**
		 * Should be called by a room when a member is closed and removed.
		 */
        public void RemovePlayerInfo(TcpMessageChannel client) {
            playerInfo.Remove(client);
        }

        private void RemoveClient(TcpMessageChannel client, string reason) {
            loginRoom.RemoveMember(client);
            lobbyRoom.RemoveMember(client);
            gameResultsRoom.RemoveMember(client);
            gameRooms.ForEach(room => room.RemoveMember(client));
            RemovePlayerInfo(client);
            client.Close();
            Log.Info($"Removed client at {client.GetRemoteEndPoint()}{(string.IsNullOrEmpty(reason) ? "" : $" [Reason: {reason}]")}", this, "ROOM-INFO");
        }

        public void StartGame(TcpMessageChannel player1, TcpMessageChannel player2) {
            var newGame = new GameRoom(this, player1, player2);
            gameRooms.Add(newGame);
            Log.Info("Created new game room");
        }

        public void RemoveGameRoom(GameRoom gameRoom) {
            gameRooms.Remove(gameRoom);
            Log.Info("Destroyed finished game room");
        }

        public void UpdateHeartbeat(TcpMessageChannel client) {
            playerInfo[client].LastHeartbeat = DateTime.Now;
        }

        public bool IsHeartbeatValid(TcpMessageChannel client) => (DateTime.Now - playerInfo[client].LastHeartbeat).TotalSeconds <= timeout;

        public void UpdateRoomType(TcpMessageChannel client, RoomType newRoom) {
            playerInfo[client].CurrentRoom = newRoom;
        }

        public RoomType GetRoomType(TcpMessageChannel client) => playerInfo[client].CurrentRoom;
    }
}