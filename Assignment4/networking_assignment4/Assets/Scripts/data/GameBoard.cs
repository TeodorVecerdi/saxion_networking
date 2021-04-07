using System;
using System.Collections.Generic;
using SerializationSystem.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using shared.model;

/**
 * Simple checker-based game board that could be reused for other 1 piece based 2 player games.
 * (For multi piece based 2 player games, you'll probably have to make some edits, 
 * but to avoid even more code clutter I didn't go that far).
 */
public class GameBoard : MonoBehaviour, IPointerClickHandler {
    //sprites for each separate cell state, highly dependent on your type of game and board model!
    //in case of tictactoe we have empty, cross and nought.
    [SerializeField] private List<Sprite> CellStateSprites = new List<Sprite>();

    //All boards cells, stored as images so we can change their sprites
    //These cells are automatically gathered at board start
    private readonly List<Image> cells = new List<Image>();

    //called when we click on a cell
    public event Action<int> OnCellClicked = delegate { };

    private void Awake() {
        //GetComponentsInChildren<Image>() also includes images on the parent #$&#!@&*^!*&^@#
        //So we roll a custom loop to gather child cells.
        foreach (Transform child in transform) cells.Add(child.GetComponent<Image>());
        Debug.Log(cells.Count + " cells on the board found.");
    }

    /**
     * Updates the whole board view to reflect the given board data.
     */
    public void SetBoardData(TicTacToeBoardData board) {
        //pass the whole board to our view
        int[] boardData = board.Board;
        Log.Warn($"Is board data null? {boardData == null}", this);

        var cellsToSet = Mathf.Min(boardData.Length, cells.Count);

        for (var i = 0; i < cellsToSet; i++) {
            cells[i].sprite = CellStateSprites[boardData[i]];
        }
    }

    /**
     * Automatically called by the Unity UI system since we have implemented the IPointerClickHandler interface
     */
    public void OnPointerClick(PointerEventData eventData) {
        //check whether we clicked on a cell
        var clickedCellIndex = cells.IndexOf(eventData.pointerPressRaycast.gameObject.GetComponent<Image>());
        Debug.Log("Clicked cell index:" + clickedCellIndex);
        //and if we actually clicked on a cell, trigger our event
        if (clickedCellIndex > -1) OnCellClicked(clickedCellIndex);
    }
}