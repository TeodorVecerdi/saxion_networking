using shared;
using System;
using SerializationSystem.Logging;
using shared.net;
using shared.protocol;

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
        private bool isGameActive;

        //wraps the board to play on...
        private readonly TicTacToeBoard board = new TicTacToeBoard();
        private readonly TcpMessageChannel player1;
        private readonly TcpMessageChannel player2;

        public GameRoom(TCPGameServer owner, TcpMessageChannel player1, TcpMessageChannel player2) : base(owner) {
            this.player1 = player1;
            this.player2 = player2;
            
            // Swap players
            if (Rand.Bool) {
                this.player1 = player2;
                this.player2 = player1;
            }
            
            StartGame();
        }

        private void StartGame() {
            currentTurn = 0;
            isGameActive = true;
            AddMember(player1);
            AddMember(player2);

            var player1Info = Server.GetPlayerInfo(player1);
            var player2Info = Server.GetPlayerInfo(player2);

            player1.SendMessage(new GameStarted {Order = 0, OtherPlayerName = player2Info.Name});
            player2.SendMessage(new GameStarted {Order = 1, OtherPlayerName = player1Info.Name});
        }
        
        private void FinishGame() {
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

        protected internal override bool RemoveMember(TcpMessageChannel member) {
            if (!base.RemoveMember(member)) return false;
            if (!isGameActive) return true;
            
            Log.Info("Player left the game while it was running. Declaring other player as winner");
            DeclareWinner(member == player1 ? player2 : player1);
            return true;
        }

        private void DeclareWinner(TcpMessageChannel member) {
            var tie = member == null;
            var winnerName = (string)null;
            var winnerID = -1;
            
            if (!tie) {
                winnerName = Server.GetPlayerInfo(member).Name;
                winnerID = member == player1 ? 0 : 1;
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
                Log.Info("People left the game...", this, "ROOM-INFO");
            }
        }

        protected override void HandleNetworkMessage(object message, TcpMessageChannel sender) {
            if (message is MakeMoveRequest makeMoveRequest) HandleMakeMoveRequest(makeMoveRequest, sender);
            
            else if (message is LeaveRoomRequest leaveRoomRequest) {
                if (leaveRoomRequest.Room == RoomType) {
                    if (RemoveMember(sender)) 
                        Server.LobbyRoom.AddMember(sender);
                } else RemoveAndCloseMember(sender, $"Unexpected request: Leave room {leaveRoomRequest.Room} when in room {RoomType}");
            }
        }

        private void HandleMakeMoveRequest(MakeMoveRequest message, TcpMessageChannel sender) {
            var playerID = IndexOfMember(sender);
            var currentTurnPlayer = sender == player1 ? 0 : 1;
            if (currentTurnPlayer != currentTurn) {
                Log.Error($"Player with id {currentTurnPlayer} attempted to make a move when turn was {currentTurn}", this, "WARNING");
                return;
            }
            
            board.MakeMove(message.Move, playerID+1);
            
            var winner = board.GetWinner();
            if (winner != -1) {
                DeclareWinner(winner == 0 ? null : winner == 1 ? player1 : player2);
                return;
            }
            
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