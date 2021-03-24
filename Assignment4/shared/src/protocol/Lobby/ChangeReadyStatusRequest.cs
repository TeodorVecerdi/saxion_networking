using shared.serialization;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to request enabling/disabling the ready status.
     */
    public class ChangeReadyStatusRequest : ASerializable {
        [Serialize] public bool Ready;
    }
}