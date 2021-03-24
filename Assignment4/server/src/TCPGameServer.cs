using System;
using System.Net.Sockets;
using System.Net;
using shared;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using shared.model;
using shared.protocol;
using shared.serialization;

namespace server {
    /**
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
    public class TCPGameServer {
        public static void Main() {
            new TCPGameServer().Run();
        }

        //we have 3 different rooms at the moment (aka simple but limited)

        private readonly LoginRoom loginRoom; //this is the room every new user joins
        private readonly LobbyRoom lobbyRoom; //this is the room a user moves to after a successful 'login'
        private readonly GameRoom gameRoom; //this is the room a user moves to when a game is successfully started

        //stores additional info for a player
        private readonly Dictionary<TcpMessageChannel, PlayerInfo> playerInfo = new Dictionary<TcpMessageChannel, PlayerInfo>();

        private TCPGameServer() {
            //we have only one instance of each room, this is especially limiting for the game room (since this means you can only have one game at a time).
            loginRoom = new LoginRoom(this);
            lobbyRoom = new LobbyRoom(this);
            gameRoom = new GameRoom(this);
        }

        private void Run() {
            Logger.Colored("Starting server on port 55555", ConsoleColor.Gray, this);

            //start listening for incoming connections (with max 50 in the queue)
            //we allow for a lot of incoming connections, so we can handle them
            //and tell them whether we will accept them or not instead of bluntly declining them
            var listener = new TcpListener(IPAddress.Any, 55555);
            listener.Start(50);

            while (true) {
                //check for new members	
                if (listener.Pending()) {
                    //get the waiting client
                    Logger.Colored("Accepting new client...", ConsoleColor.White, this);
                    var client = listener.AcceptTcpClient();
                    //and wrap the client in an easier to use communication channel
                    var channel = new TcpMessageChannel(client);
                    //and add it to the login room for further 'processing'
                    loginRoom.AddMember(channel);
                }

                //now update every single room
                loginRoom.Update();
                lobbyRoom.Update();
                gameRoom.Update();

                Thread.Sleep(50);
            }
        }

        //provide access to the different rooms on the server 
        public LoginRoom GetLoginRoom() {
            return loginRoom;
        }

        public LobbyRoom GetLobbyRoom() {
            return lobbyRoom;
        }

        public GameRoom GetGameRoom() {
            return gameRoom;
        }

        /**
		 * Returns a handle to the player info for the given client 
		 * (will create new player info if there was no info for the given client yet)
		 */
        public PlayerInfo GetPlayerInfo(TcpMessageChannel client) {
            if (!playerInfo.ContainsKey(client)) {
                playerInfo[client] = new PlayerInfo();
            }

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
    }
}