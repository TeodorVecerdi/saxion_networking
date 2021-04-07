using System;
using SerializationSystem;

namespace shared.model {
    /**
	 * Super simple board model for TicTacToe that contains the minimal data to actually represent the board. 
	 * It doesn't say anything about whose turn it is, whether the game is finished etc.
	 * IF you want to actually implement a REAL Tic Tac Toe, that means you will have to add the data required for that (and serialize it!).
	 */
    [Serializable]
    public class TicTacToeBoardData : Printable {
        //board representation in 1d array, one element for each cell
        //0 is empty, 1 is player 1, 2 is player 2
        //might be that for your game, a 2d array is actually better
        [Serialized] public readonly int[] Board = {0, 0, 0, 0, 0, 0, 0, 0, 0};
        

        public override string ToString() {
            return GetType().Name + ":" + string.Join(",", Board);
        }

        public int this[int idx] {
	        get {
		        if (idx < 0 || idx >= 9) throw new IndexOutOfRangeException($"TicTacToe board index {idx} out of range [0,8]");
		        return Board[idx];
	        }
	        set {
		        if (idx < 0 || idx >= 9) throw new IndexOutOfRangeException($"TicTacToe board index {idx} out of range [0,8]");
		        Board[idx] = value;
	        }
        }
    }
}