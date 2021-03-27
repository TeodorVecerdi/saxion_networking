using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/**
 * Wraps all elements and functionality required for the GameView.
 */
public class GameView : View {
    [FormerlySerializedAs("_gameboard")] [SerializeField] private GameBoard gameboard = null;
    [FormerlySerializedAs("_player1Label")] [SerializeField] private TMP_Text player1Label = null;
    [FormerlySerializedAs("_player2Label")] [SerializeField] private TMP_Text player2Label = null;
    [SerializeField] private Button leaveButton = null;

    public GameBoard GameBoard => gameboard;
    public TMP_Text PlayerLabel1 => player1Label;
    public TMP_Text PlayerLabel2 => player2Label;
    public Button LeaveButton => leaveButton;
}