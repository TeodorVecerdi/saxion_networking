using System.Linq;
using shared;
using shared.protocol;
using shared.serialization;

namespace server {
    /**
	 * The LoginRoom is the first room clients 'enter' until the client identifies himself with a PlayerJoinRequest. 
	 * If the client sends the wrong type of request, it will be kicked.
	 *
	 * A connected client that never sends anything will be stuck in here for life,
	 * unless the client disconnects (that will be detected in due time).
	 */
    public class LoginRoom : SimpleRoom {
        //arbitrary max amount just to demo the concept
        private const int MAX_MEMBERS = 50;

        public LoginRoom(TCPGameServer owner) : base(owner) {
        }

        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);

            //notify the client that (s)he is now in the login room, clients can wait for that before doing anything else
            RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
            roomJoinedEvent.Room = RoomJoinedEvent.RoomType.LOGIN_ROOM;
            member.SendMessage(roomJoinedEvent);
        }

        protected override void HandleNetworkMessage(object message, TcpMessageChannel sender) {
            if (message is PlayerJoinRequest playerJoinRequest) {
                HandlePlayerJoinRequest(playerJoinRequest, sender);
            } else //if member sends something else than a PlayerJoinRequest
            {
                Logger.Error("Declining client, auth request not understood", this);

                //don't provide info back to the member on what it is we expect, just close and remove
                RemoveAndCloseMember(sender);
            }
        }

        /**
		 * Tell the client he is accepted and move the client to the lobby room.
		 */
        private void HandlePlayerJoinRequest(PlayerJoinRequest message, TcpMessageChannel sender) {
            Logger.Info("Moving new client to accepted...", this, "ROOM-INFO");

            if (IsJoinRequestValid(message)) {
                var playerInfo = Server.GetPlayerInfo(sender);
                playerInfo.Name = message.Name;
                sender.SendMessage(new PlayerJoinResponse {Result = PlayerJoinResponse.RequestResult.ACCEPTED, PlayerInfo = playerInfo});
                RemoveMember(sender);
                Server.GetLobbyRoom().AddMember(sender);
                return;
            }

            sender.SendMessage(new PlayerJoinResponse {Result = PlayerJoinResponse.RequestResult.CONFLICT, PlayerInfo = null});
        }

        private bool IsJoinRequestValid(PlayerJoinRequest request) {
            return Server.GetPlayerInfo(info => info.Name == request.Name).Count == 0;
        }
    }
}