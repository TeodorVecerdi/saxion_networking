﻿using SerializationSystem;
using shared.model;

namespace shared.protocol
{
    /**
     * Send from SERVER to CLIENT to let the client know whether it was allowed to join or not.
     * Currently the only possible result is accepted.
     */
    public class PlayerJoinResponse
    {
        public enum RequestResult { ACCEPTED, CONFLICT }
        [Serialized] public RequestResult Result;
        [Serialized] public PlayerInfo PlayerInfo;
    }
}
