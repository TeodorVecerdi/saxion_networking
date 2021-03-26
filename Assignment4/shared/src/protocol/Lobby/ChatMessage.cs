using shared.serialization;

namespace shared.protocol {
    /**
	 * BIDIRECTIONAL Chat message for the lobby
	 */
    [System.Serializable]
    public class ChatMessage : ASerializable {
        [Serialized] public string Message;
    }
}