using Shared;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/**
 * Assignment 2 - Starting project.
 * 
 * @author J.C. Wichman
 */
public class TCPChatClient : MonoBehaviour {
    [SerializeField] private PanelWrapper panelWrapper = null;
    [SerializeField] private string hostname = "localhost";
    [SerializeField] private int port = 55555;
    [SerializeField] private bool verbose = false;

    private DateTime lastHeartbeatTime;
    private float? serverTimeout;
    private TcpClient client;

    private void Start() {
        panelWrapper.OnChatTextEntered += OnTextEntered;
        ConnectToServer();
    }

    private void Update() {
        try {
            if (client.Available > 0) {
                var stream = client.GetStream();
                var inBytes = StreamUtil.Read(stream);
                var received = Encoding.UTF8.GetString(inBytes);
                ProcessMessage(received);
            }
        } catch (Exception e) {
            panelWrapper.AddOutput(e.Message);
        }
        
        var currentTime = DateTime.Now;
        if (serverTimeout.HasValue && (currentTime - lastHeartbeatTime).Seconds > serverTimeout.Value / 2.0f) {
            EmitHeartbeat();
        }
    }

    private void EmitHeartbeat() {
        EmitMessage("HEARTBEAT");
    }

    private void EmitMessage(string message) {
        try {
            var outBytes = ServerUtility.EncodeMessageAsBytes(message);
            lastHeartbeatTime = DateTime.Now;
            if(verbose) Debug.Log($"Sent message: {ServerUtility.EncodeMessage(message)}");
            StreamUtil.Write(client.GetStream(), outBytes);
        } catch (Exception e) {
            panelWrapper.AddOutput(e.Message);
            //for quicker testing, we reconnect if something goes wrong.
            client.Close();
            ConnectToServer();
        }
    }

    private void ProcessMessage(string message) {
        // Normal message to output to console
        if (message.StartsWith("MSG:")) {
            if(verbose) Debug.Log($"Received message: {message}");
            panelWrapper.AddOutput(message.Substring(4));
            return;
        }

        // Server timeout setup for heartbeat
        if (message.StartsWith("TIMEOUT:")) {
            var timeoutStr = message.Split(':')[1];
            if (!float.TryParse(timeoutStr, out var timeout)) {
                serverTimeout = 0.5f;
                return;
            }
            
            serverTimeout = timeout;
            if(verbose) Debug.Log($"Received timeout from server {serverTimeout}");
        }
    }

    private void OnTextEntered(string input) {
        if (string.IsNullOrEmpty(input)) return;
        panelWrapper.ClearInput();
        EmitMessage(input);
    }

    private void ConnectToServer() {
        try {
            client = new TcpClient();
            client.Connect(hostname, port);
            panelWrapper.ClearOutput();
            panelWrapper.AddOutput("Connected to server.");
        } catch (Exception e) {
            panelWrapper.AddOutput("Could not connect to server:");
            panelWrapper.AddOutput(e.Message);
        }
    }
}