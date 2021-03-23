using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/**
 * The main ChatLobbyClient where you will have to do most of your work.
 * 
 * @author J.C. Wichman
 */
public class ChatLobbyClient : MonoBehaviour {
    //reference to the helper class that hides all the avatar management behind a blackbox
    private AvatarAreaManager avatarAreaManager;
    //reference to the helper class that wraps the chat interface
    private PanelWrapper panelWrapper;

    [SerializeField] private string Server = "localhost";
    [SerializeField] private int Port = 55555;

    private TcpClient client;

    private void Start() {
        ConnectToServer();

        //register for the important events
        avatarAreaManager = FindObjectOfType<AvatarAreaManager>();
        avatarAreaManager.OnAvatarAreaClicked += OnAvatarAreaClicked;

        panelWrapper = FindObjectOfType<PanelWrapper>();
        panelWrapper.OnChatTextEntered += OnChatTextEntered;
    }

    private void ConnectToServer() {
        try {
            client = new TcpClient();
            client.Connect(Server, Port);
            Debug.Log("Connected to server.");
        } catch (Exception e) {
            Debug.Log($"Could not connect to server: {e.Message}");
        }
    }

    private void OnAvatarAreaClicked(Vector3 pClickPosition) {
        Debug.Log("ChatLobbyClient: you clicked on " + pClickPosition);
        //TODO pass data to the server so that the server can send a position update to all clients (if the position is valid!!)
    }

    private void OnChatTextEntered(string pText) {
        panelWrapper.ClearInput();
        SendString(pText);
    }

    private void SendString(string pOutString) {
        try {
            //we are still communicating with strings at this point, this has to be replaced with either packet or object communication
            Debug.Log("Sending:" + pOutString);
            byte[] outBytes = Encoding.UTF8.GetBytes(pOutString);
            StreamUtil.Write(client.GetStream(), outBytes);
        } catch (Exception e) {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            client.Close();
            ConnectToServer();
        }
    }

    // RECEIVING CODE
    private void Update() {
        try {
            if (client.Available > 0) {
                //we are still communicating with strings at this point, this has to be replaced with either packet or object communication
                byte[] inBytes = StreamUtil.Read(client.GetStream());
                string inString = Encoding.UTF8.GetString(inBytes);
                Debug.Log("Received:" + inString);
                ShowMessage(inString);
            }
        } catch (Exception e) {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            client.Close();
            ConnectToServer();
        }
    }

    private void ShowMessage(string pText) {
        //This is a stub for what should actually happen
        //What should actually happen is use an ID that you got from the server, to get the correct avatar
        //and show the text message through that
        List<int> allAvatarIds = avatarAreaManager.GetAllAvatarIds();

        if (allAvatarIds.Count == 0) {
            Debug.Log("No avatars available to show text through:" + pText);
            return;
        }

        int randomAvatarId = allAvatarIds[UnityEngine.Random.Range(0, allAvatarIds.Count)];
        AvatarView avatarView = avatarAreaManager.GetAvatarView(randomAvatarId);
        avatarView.Say(pText);
    }
}