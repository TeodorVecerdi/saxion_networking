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
        protected override RoomType RoomType => RoomType.GAME_ROOM;

        private int currentTurn;
        private int playerIdTurnOffset;
        private bool isGameActive;

        //wraps the board to play on...
        private readonly TicTacToeBoard board = new TicTacToeBoard();
        private readonly TcpMessageChannel player1;
        private readonly TcpMessageChannel player2;

        public GameRoom(TCPGameServer owner, TcpMessageChannel player1, TcpMessageChannel player2) : base(owner) {
            this.player1 = player1;
            this.player2 = player2;
            StartGame();
        }

        private void StartGame() {
            currentTurn = 0;
            isGameActive = true;
            AddMember(player1);
            AddMember(player2);

            var player1Info = Server.GetPlayerInfo(player1);
            var player2Info = Server.GetPlayerInfo(player2);

            playerIdTurnOffset = Rand.Bool ? 0 : 1;
            player1.SendMessage(new GameStarted {Order = playerIdTurnOffset, OtherPlayerName = player2Info.Name});
            player2.SendMessage(new GameStarted {Order = 1-playerIdTurnOffset, OtherPlayerName = player1Info.Name});
        }
        
        private void FinishGame() {
            Server.GetPlayerInfo(player1).GameRoom = null;
            Server.GetPlayerInfo(player2).GameRoom = null;

            isGameActive = false;
            if (RemoveMember(player1)) {
                Server.GameResultsRoom.AddMember(player1);
            }
            if (RemoveMember(player2)) {
                Server.GameResultsRoom.AddMember(player2);
            }
            
            Server.RemoveGameRoom(this);
        }


        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);

            //notify client he has joined a game room 
            var roomJoinedEvent = new RoomJoinedEvent {Room = RoomType};
            member.SendMessage(roomJoinedEvent);
        }

        protected override bool RemoveMember(TcpMessageChannel member) {
            if (!base.RemoveMember(member)) return false;
            if (!isGameActive) return true;
            
            Logger.Info("Player left the game while it was running. Declaring other player as winner");
            DeclareWinner(member == player1 ? player2 : player1);
            return true;
        }

        private void DeclareWinner(TcpMessageChannel member) {
            var tie = member == null;
            var winnerName = (string)null;
            var winnerID = -1;
            if (!tie) {
                winnerName = Server.GetPlayerInfo(member).Name;
                winnerID = member == player1 ? playerIdTurnOffset : 1 - playerIdTurnOffset;
            }

            var gameResults = new GameResults {IsTie = tie, WinnerName = winnerName, WinnerID = winnerID};
            Members.ForEach(channel => channel.SendMessage(gameResults));
            FinishGame();
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
            if (message is MakeMoveRequest makeMoveRequest) HandleMakeMoveRequest(makeMoveRequest, sender);
            else if (message is LeaveGameRequest) {
                RemoveMember(sender);
                Server.LobbyRoom.AddMember(sender);
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