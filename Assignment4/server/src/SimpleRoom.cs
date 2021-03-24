using shared;

namespace server
{
    /**
     * Subclasses Room to create an SimpleRoom which allows adding members without any special considerations.
     */
    abstract class SimpleRoom : Room
    {
		protected SimpleRoom(TCPGameServer pServer) : base(pServer) { }

        public void AddMember (TcpMessageChannel pChannel)
        {
            addMember(pChannel);
        }

	}
}
