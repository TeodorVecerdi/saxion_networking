using System;

public class User {
    private string name;
    private DateTime lastHeartbeatTime;
    
    public string Name => name;
    public DateTime LastHeartbeatTime => lastHeartbeatTime;

    /// <summary>Initializes a new instance of the <see cref="T:User" /> class.</summary>
    public User(string name) {
        this.name = name;
        lastHeartbeatTime = DateTime.Now;
    }

    public void OnHeartbeat() {
        if (Server.Verbose) Logger.Info($"Received heartbeat from {this}", "INFO-VERBOSE");
        
        lastHeartbeatTime = DateTime.Now;
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return $"@[{Name}]";
    }
}