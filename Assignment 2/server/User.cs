using System;

internal class User {
    private string name;
    private DateTime lastHeartbeatTime;
    
    internal string Name => name;
    internal DateTime LastHeartbeatTime => lastHeartbeatTime;

    /// <summary>Initializes a new instance of the <see cref="T:User" /> class.</summary>
    internal User(string name) {
        this.name = name;
        lastHeartbeatTime = DateTime.Now;
    }

    internal void OnHeartbeat() {
        lastHeartbeatTime = DateTime.Now;
    }

    internal void UpdateNickname(string newNickname) {
        name = newNickname.ToLowerInvariant();
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return $"@[{Name}]";
    }
}