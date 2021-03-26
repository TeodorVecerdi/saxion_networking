using shared.serialization;

namespace shared.protocol {
    /**
	 * Send from SERVER to CLIENT to notify that the client has joined a specific room (i.e. that it should change state).
	 */
    [System.Serializable]
    public class RoomJoinedEvent : ASerializable  {
        public enum RoomType {
            LOGIN_ROOM,
            LOBBY_ROOM,
            GAME_ROOM
        };

        [Serialized] public RoomType Room;
    }
}