using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * Wraps all elements and functionality required for the LobbyView.
 */
public class LobbyView : View
{
    //all components that need to be hooked up
    [SerializeField] private TMP_Text _textHeading = null;
    [SerializeField] private InputField _inputFieldChat = null;
    [SerializeField] private Text _textOutput = null;
    [SerializeField] private ScrollRect _scrollRect = null;
    [SerializeField] private Toggle _toggleReady = null;

    private bool _focusedRequested = false;     //weird unity stuff as usual ;)
    private bool _scrollRequested = false;      //weird unity stuff as usual ;)

    //the events you can register for
    public event Action<string> OnChatTextEntered = delegate { };
    public event Action<bool> OnReadyToggleClicked = delegate { };

    private void Start()
    {
        //setup chat input listener to trigger on enter
        _inputFieldChat.onEndEdit.AddListener(
            (value) => {
                    if (Input.GetKeyDown(KeyCode.Return))  OnChatTextEntered(value);
                }
         );

        //setup 
        _toggleReady.onValueChanged.AddListener((value) => OnReadyToggleClicked(value));

        //clear title by default
        _textHeading.text = "";
    }

    private void Update()
    {
        checkFocus();
    }

    private void checkFocus()
    {
        if (_focusedRequested)
        {
            _inputFieldChat.ActivateInputField();
            _inputFieldChat.Select();
            _focusedRequested = false;
        }

        if (_scrollRequested)
        {
            _scrollRect.verticalNormalizedPosition = 0;
            _scrollRequested = false;
        }
    }

    public void SetLobbyHeading (string pHeading)
    {
        _textHeading.text = pHeading;
    }

    public void AddOutput(string pOutput)
    {
        _textOutput.text += pOutput + "\n";
        _scrollRequested = true;
    }

    public void ClearOutput()
    {
        _textOutput.text = "";
        _scrollRequested = true;
    }

    public void ClearInput()
    {
        _inputFieldChat.text = "";
        _focusedRequested = true;
    }

    public void SetReadyToggle (bool pValue)
    {
        _toggleReady.SetIsOnWithoutNotify(pValue);
    }

}
