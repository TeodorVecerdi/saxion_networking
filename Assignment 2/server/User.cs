using System;

internal class User {
    private string name;
    private string room;
    private DateTime lastHeartbeatTime;
    
    internal string Name => name;
    internal string Room => room;
    internal DateTime LastHeartbeatTime => lastHeartbeatTime;

    /// <summary>Initializes a new instance of the <see cref="T:User" /> class.</summary>
    internal User(string name, string defaultRoomName) {
        this.name = name;
        room = defaultRoomName;
        lastHeartbeatTime = DateTime.Now;
    }

    internal void OnHeartbeat() {
        lastHeartbeatTime = DateTime.Now;
    }

    internal void UpdateNickname(string newNickname) {
        name = newNickname.ToLowerInvariant();
    }

    internal void UpdateRoom(string newRoom) {
        room = newRoom.ToLowerInvariant();
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return $"@[{Name}]";
    }
}