﻿using System;
using SerializationSystem;
using shared.protocol;

namespace shared.model {
    /**
     * Empty placeholder class for the PlayerInfo object which is being tracked for each client by the server.
     * Add any data you want to store for the player here and make it extend ASerializable.
     */
    [Serializable]
    public class PlayerInfo : Printable {
        [Serialized] public string Name;
        [NonSerialized] public DateTime LastHeartbeat;
        [NonSerialized] public RoomType CurrentRoom;
    }
}