using shared.serialization;

namespace shared.protocol {
    public class LeaveRoomRequest : Printable {
        [Serialized] public RoomType Room;
    }
}