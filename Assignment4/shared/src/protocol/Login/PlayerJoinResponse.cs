using shared.model;
using shared.serialization;

namespace shared.protocol
{
    /**
     * Send from SERVER to CLIENT to let the client know whether it was allowed to join or not.
     * Currently the only possible result is accepted.
     */
    public class PlayerJoinResponse
    {
        public enum RequestResult { ACCEPTED, CONFLICT } //can add different result states if you want
        [Serialize] public RequestResult Result;
        [Serialize] public PlayerInfo PlayerInfo;
    }
}
