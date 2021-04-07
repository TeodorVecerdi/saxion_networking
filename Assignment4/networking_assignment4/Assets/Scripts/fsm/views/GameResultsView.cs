using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultsView : View {
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button returnToLobbyButton;
    
    public Button ReturnToLobbyButton => returnToLobbyButton;
    
    public void Load(GameResult gameResult) {
        if (gameResult.IsTie) {
            winnerText.text = "Tie! No-one won this time";
        } else if (gameResult.SelfID == gameResult.WinnerID) {
            winnerText.text = "GG! You won!";
        } else {
            winnerText.text = $"<b>{gameResult.WinnerName}</b> won! Better luck next time!";
        } 
    }

}