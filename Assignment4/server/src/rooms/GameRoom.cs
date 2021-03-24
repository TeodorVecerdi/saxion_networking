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

        //wraps the board to play on...
        private readonly TicTacToeBoard board = new TicTacToeBoard();

        public GameRoom(TCPGameServer owner) : base(owner) {
        }

        public void StartGame(TcpMessageChannel player1, TcpMessageChannel player2) {
            if (IsGameInPlay) throw new Exception("Programmer error duuuude.");

            IsGameInPlay = true;
            AddMember(player1);
            AddMember(player2);
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
            //we have two players, so index of sender is 0 or 1, which means playerID becomes 1 or 2
            var playerID = IndexOfMember(sender) + 1;
            //make the requested move (0-8) on the board for the player
            board.MakeMove(message.Move, playerID);

            //and send the result of the board state back to all clients
            var makeMoveResult = new MakeMoveResult {
                Player = playerID,
                BoardData = board.GetBoardData()
            };
            SendToAll(makeMoveResult);
        }
    }
}