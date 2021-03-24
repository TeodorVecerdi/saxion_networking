using shared.serialization;
using shared.serialization.attr;

namespace shared.protocol {
    /**
	 * BIDIRECTIONAL Chat message for the lobby
	 */
    public class ChatMessage : ASerializable {
        [Serialize] public string Message;
    }
}