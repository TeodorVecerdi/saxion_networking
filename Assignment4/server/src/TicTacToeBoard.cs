using System.Linq;
using SerializationSystem.Logging;
using shared;
using shared.model;

namespace server {
    /**
     * This class wraps the actual board we are playing on.
     * 
     * Normally both logic and data would be in a single class (e.g. something like TicTacToeModel).
     * In this case I chose to split them up: we have one class TicTacToeBoard that wraps an instance
     * of the actual board data. This way we have an object we can serialize to clients (the board data) 
     * and one class that actually manages the logic of manipulating the board.
     * 
     * In this specific instance that logic is almost non existent, because I tried to keep the demo
     * as simple as possible, so we can only make a move. In an actual game, this class would implement
     * methods such as GetValidMoves(), HasWon(), GetCurrentPlayer() etc.
     */
    public class TicTacToeBoard {
        private readonly TicTacToeBoardData board = new TicTacToeBoardData();

        /**
         * @param pMove     a number from 0-8 that indicates the cell we want to change
         * @param pPlayer   1 or 2 to indicate which player made the move
         */
        public void MakeMove(int move, int player) {
            board[move] = player;

            //we could also check which row and column if we wanted to:
            var columns = 3;
            var row = move / columns;
            var column = move % columns;
            Log.Info($"Player {player} made a move in cell ({column},{row})", this, "BOARD");
        }

        public int GetWinner() {
            if (Find(1)) return 1;
            if (Find(2)) return 2;
            if (board.Board.Any(i => i == 0)) return -1;
            return 0; // tie
        }

        private bool Find(int player) {
            if (board[0] == player && board[1] == player && board[2] == player) return true; //Row 0
            if (board[3] == player && board[4] == player && board[5] == player) return true; //Row 1
            if (board[6] == player && board[7] == player && board[8] == player) return true; //Row 2
            if (board[0] == player && board[3] == player && board[6] == player) return true; //Col 0
            if (board[1] == player && board[4] == player && board[7] == player) return true; //Col 1
            if (board[2] == player && board[5] == player && board[8] == player) return true; //Col 2
            if (board[0] == player && board[4] == player && board[8] == player) return true; //Diag
            if (board[2] == player && board[4] == player && board[6] == player) return true; //Anti-diag
            return false;
        }

        /**
         * Return the inner board data state so we can send it to a client.
         */
        public TicTacToeBoardData GetBoardData() {
            //it would be more academically correct if we would clone this object before returning it, but anyway.
            return board;
        }
    }
}