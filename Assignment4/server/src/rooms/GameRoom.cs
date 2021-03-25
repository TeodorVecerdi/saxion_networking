using shared;
using System;
using shared.protocol;
using shared.serialization;

namespace server {
    /**
	 * This room runs a single Game (at a time). 
	 * 
	 * The 'Game' is very simple at the moment:
	 *	- all client moves are broadcasted to all clients
	 *	
	 * The game has no end yet (that is up to you), in other words:
	 * all players that are added to this room, stay in here indefinitely.
	 */
    public class GameRoom : Room {
        public bool IsGameInPlay { get; private set; }
        private int currentTurn;
        private int playerIdTurnOffset;

        //wraps the board to play on...
        private readonly TicTacToeBoard board = new TicTacToeBoard();

        public GameRoom(TCPGameServer owner) : base(owner) {
        }

        public void StartGame(TcpMessageChannel player1, TcpMessageChannel player2) {
            if (IsGameInPlay) throw new Exception("Programmer error duuuude.");

            currentTurn = 0;
            IsGameInPlay = true;
            AddMember(player1);
            AddMember(player2);

            var player1Info = Server.GetPlayerInfo(player1);
            var player2Info = Server.GetPlayerInfo(player2);

            var player1Turn = Rand.Bool ? 0 : 1;
            playerIdTurnOffset = player1Turn;
            player1.SendMessage(new GameStarted {Order = player1Turn, OtherPlayerName = player2Info.Name});
            player2.SendMessage(new GameStarted {Order = 1-player1Turn, OtherPlayerName = player1Info.Name});
        }

        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);

            //notify client he has joined a game room 
            var roomJoinedEvent = new RoomJoinedEvent {Room = RoomJoinedEvent.RoomType.GAME_ROOM};
            member.SendMessage(roomJoinedEvent);
        }

        public override void Update() {
            //demo of how we can tell people have left the game...
            var oldMemberCount = MemberCount;
            base.Update();
            var newMemberCount = MemberCount;

            if (oldMemberCount != newMemberCount) {
                Logger.Info("People left the game...", this, "ROOM-INFO");
            }
        }

        protected override void HandleNetworkMessage(object message, TcpMessageChannel sender) {
            if (message is MakeMoveRequest makeMoveRequest) {
                HandleMakeMoveRequest(makeMoveRequest, sender);
            }
        }

        private void HandleMakeMoveRequest(MakeMoveRequest message, TcpMessageChannel sender) {
            var playerID = IndexOfMember(sender);
            if ((playerID + playerIdTurnOffset) % 2 != currentTurn) {
                var turn = (playerID + playerIdTurnOffset) % 2;
                Logger.Error($"Player with id {playerID} and turnIndex {turn} attempted to make a move when turn was {currentTurn}", this, "WARNING");
                return;
            }
            
            board.MakeMove(message.Move, playerID+1);
            currentTurn = 1 - playerID; // next player 

            //and send the result of the board state back to all clients
            var makeMoveResult = new MakeMoveResult {
                Player = playerID,
                NextTurn = currentTurn,
                BoardData = board.GetBoardData()
            };
            SendToAll(makeMoveResult);
        }
    }
}