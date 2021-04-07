using shared.net;

namespace server {
    /**
     * Subclasses Room to create an SimpleRoom which allows adding members without any special considerations.
     */
    public abstract class SimpleRoom : Room {
        protected SimpleRoom(TCPGameServer server) : base(server) {
        }

        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);
        }
    }
}