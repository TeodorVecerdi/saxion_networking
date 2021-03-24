using shared;
using System.Collections.Generic;
using shared.protocol;
using shared.serialization;

namespace server {
    /**
	 * The LobbyRoom is a little bit more extensive than the LoginRoom.
	 * In this room clients change their 'ready status'.
	 * If enough people are ready, they are automatically moved to the GameRoom to play a Game (assuming a game is not already in play).
	 */
    public class LobbyRoom : SimpleRoom {
        //this list keeps tracks of which players are ready to play a game, this is a subset of the people in this room
        private readonly List<TcpMessageChannel> readyMembers = new List<TcpMessageChannel>();

        public LobbyRoom(TCPGameServer owner) : base(owner) {
        }

        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);

            //tell the member it has joined the lobby
            RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
            roomJoinedEvent.Room = RoomJoinedEvent.RoomType.LOBBY_ROOM;
            member.SendMessage(roomJoinedEvent);

            //print some info in the lobby (can be made more applicable to the current member that joined)
            ChatMessage simpleMessage = new ChatMessage {Message = "Client 'John Doe' has joined the lobby!"};
            member.SendMessage(simpleMessage);

            //send information to all clients that the lobby count has changed
            SendLobbyUpdateCount();
        }

        /**
		 * Override removeMember so that our ready count and lobby count is updated (and sent to all clients)
		 * anytime we remove a member.
		 */
        protected override void RemoveMember(TcpMessageChannel member) {
            base.RemoveMember(member);
            readyMembers.Remove(member);

            SendLobbyUpdateCount();
        }

        protected override void HandleNetworkMessage(object message, TcpMessageChannel sender) {
            if (message is ChangeReadyStatusRequest readyStatusRequest) HandleReadyNotification(readyStatusRequest, sender);
        }

        private void HandleReadyNotification(ChangeReadyStatusRequest readyNotification, TcpMessageChannel sender) {
            //if the given client was not marked as ready yet, mark the client as ready
            if (readyNotification.Ready) {
                if (!readyMembers.Contains(sender)) readyMembers.Add(sender);
            } else //if the client is no longer ready, unmark it as ready
            {
                readyMembers.Remove(sender);
            }

            //do we have enough people for a game and is there no game running yet?
            if (readyMembers.Count >= 2 && !Server.GetGameRoom().IsGameInPlay) {
                TcpMessageChannel player1 = readyMembers[0];
                TcpMessageChannel player2 = readyMembers[1];
                RemoveMember(player1);
                RemoveMember(player2);
                Server.GetGameRoom().StartGame(player1, player2);
            }

            //(un)ready-ing / starting a game changes the lobby/ready count so send out an update
            //to all clients still in the lobby
            SendLobbyUpdateCount();
        }

        private void SendLobbyUpdateCount() {
            LobbyInfoUpdate lobbyInfoMessage = new LobbyInfoUpdate {
                MemberCount = MemberCount,
                ReadyCount = readyMembers.Count
            };
            SendToAll(lobbyInfoMessage);
        }
    }
}