using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using shared.protocol;
using shared.serialization;
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

    private int selfUserId;
    private float serverTimeout;
    private bool receivedUserId = false;
    private float timeSinceLastHeartbeat;

    private TcpClient client;

    private void Start() {
        ConnectToServer();

        //register for the important events
        avatarAreaManager = FindObjectOfType<AvatarAreaManager>();
        avatarAreaManager.OnAvatarAreaClicked += OnAvatarAreaClicked;

        panelWrapper = FindObjectOfType<PanelWrapper>();
        panelWrapper.OnChatTextEntered += OnChatTextEntered;
        
        foreach (var keyValuePair in SerializationHelper.methods) {
            Debug.Log($"Method: {keyValuePair.Value.Serializer.GetType()} => Id: {keyValuePair.Key} , Name: {keyValuePair.Value.SerializedType.AssemblyQualifiedName}");
        }
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
        if (!receivedUserId) return;
        var positionChangeRequest = new PositionChangeRequest(selfUserId, pClickPosition.x, pClickPosition.y, pClickPosition.z);
        SendObject(positionChangeRequest);
        //TODO pass data to the server so that the server can send a position update to all clients (if the position is valid!!)
    }

    private void OnChatTextEntered(string pText) {
        panelWrapper.ClearInput();
        
        if (string.IsNullOrWhiteSpace(pText) || !receivedUserId) return;
        SendString(pText);
    }

    private void SendString(string pOutString) {
        try {
            // Command
            if (pOutString.StartsWith("/")) {
                var split = pOutString.Split(' ');
                var commandName = split[0].Substring(1);
                var parameters = new List<string>();
                for (var i = 1; i < split.Length; i++) {
                    if (string.IsNullOrWhiteSpace(split[i])) continue;
                    parameters.Add(split[i]);
                }

                var command = new Command(commandName, parameters);
                SendObject(command);
            } 
            // Normal message
            else {
                var message = new Message(selfUserId, pOutString);
                SendObject(message);
            } 
        } catch (Exception e) {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            client.Close();
            ConnectToServer();
        }
    }

    private void SendObject<T>(T obj) {
        timeSinceLastHeartbeat = 0.0f;
        var bytes = SerializationHelper.Serialize(obj);
        StreamUtil.Write(client.GetStream(), bytes);
    }

    // RECEIVING CODE
    private void Update() {
        try {
            if (client.Available > 0) {
                //we are still communicating with strings at this point, this has to be replaced with either packet or object communication
                byte[] inBytes = StreamUtil.Read(client.GetStream());
                var obj = SerializationHelper.Deserialize(inBytes);
                ProcessObject(obj);
            }
        } catch (Exception e) {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            client.Close();
            ConnectToServer();
        }

        if (!receivedUserId) return;
        timeSinceLastHeartbeat += Time.deltaTime;
        if (timeSinceLastHeartbeat >= serverTimeout / 2.0f) {
            SendObject(new Heartbeat());
        }
    }

    private void ProcessObject(object obj) {
        switch (obj) {
            case HelloWorld helloWorld:
                selfUserId = helloWorld.SelfUserId;
                serverTimeout = helloWorld.Timeout;
                receivedUserId = true;
                Debug.Log("Processed HelloWorld");
                break;
            case Message message: 
                var target = avatarAreaManager.GetAvatarView(message.UserId);
                target.Say(message.Text);
                Debug.Log("Processed Message");
                break;
            case ClientJoined clientJoined:
                var avatarView = avatarAreaManager.AddAvatarView(clientJoined.UserId);
                avatarView.SetSkin(clientJoined.SkinId);
                avatarView.transform.position = new Vector3(clientJoined.X, clientJoined.Y, clientJoined.Z);
                if(receivedUserId && clientJoined.UserId == selfUserId) avatarView.SetRingVisible(true);
                Debug.Log("Processed ClientJoined");
                break;
            case ClientLeft clientLeft:
                avatarAreaManager.RemoveAvatarView(clientLeft.UserId);
                Debug.Log("Processed ClientLeft");
                break;
            case PositionChanged positionChanged:
                avatarAreaManager.GetAvatarView(positionChanged.UserId).Move(new Vector3(positionChanged.X, positionChanged.Y, positionChanged.Z));
                Debug.Log("Processed PositionChanged");
                break;
            case SkinChanged skinChanged:
                avatarAreaManager.GetAvatarView(skinChanged.UserId).SetSkin(skinChanged.SkinId);
                Debug.Log("Processed SkinChanged");
                break;
            case ConnectedClients connectedClients:
                foreach (var client in connectedClients.Users) {
                    var avatarView2 = avatarAreaManager.AddAvatarView(client.UserId);
                    avatarView2.SetSkin(client.SkinId);
                    avatarView2.transform.position = new Vector3(client.X, client.Y, client.Z);
                }
                Debug.Log("Processed ConnectedClients");
                break;
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