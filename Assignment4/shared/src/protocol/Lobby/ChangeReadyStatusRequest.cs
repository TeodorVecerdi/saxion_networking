using System;
using shared.serialization;

namespace shared.protocol {
    /**
     * Send from CLIENT to SERVER to request enabling/disabling the ready status.
     */
    [Serializable]
    public class ChangeReadyStatusRequest : Printable {
        [Serialized] public bool Ready;
    }
}