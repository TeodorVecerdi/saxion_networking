using shared;
using System.Collections.Generic;
using shared.net;
using shared.protocol;

namespace server {
    /**
	 * The LobbyRoom is a little bit more extensive than the LoginRoom.
	 * In this room clients change their 'ready status'.
	 * If enough people are ready, they are automatically moved to the GameRoom to play a Game (assuming a game is not already in play).
	 */
    public class LobbyRoom : SimpleRoom {
        protected override RoomType RoomType => RoomType.LOBBY_ROOM;
        
        //this list keeps tracks of which players are ready to play a game, this is a subset of the people in this room
        private readonly List<TcpMessageChannel> readyMembers = new List<TcpMessageChannel>();

        public LobbyRoom(TCPGameServer owner) : base(owner) {
        }

        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);

            //tell the member it has joined the lobby
            var roomJoinedEvent = new RoomJoinedEvent {Room = RoomType};
            member.SendMessage(roomJoinedEvent);

            var playerInfo = Server.GetPlayerInfo(member);
            //print some info in the lobby (can be made more applicable to the current member that joined)
            var simpleMessage = new ChatMessage {Message = $"Client <b>{playerInfo.Name}</b> has joined the lobby!"};
            SendToAll(simpleMessage);

            //send information to all clients that the lobby count has changed
            SendLobbyUpdateCount();
        }

        /**
		 * Override removeMember so that our ready count and lobby count is updated (and sent to all clients)
		 * anytime we remove a member.
		 */
        protected internal override bool RemoveMember(TcpMessageChannel member) {
            if (!base.RemoveMember(member)) return false; 
            readyMembers.Remove(member);
            SendLobbyUpdateCount();
            return true;
        }

        protected override void HandleNetworkMessage(object message, TcpMessageChannel sender) {
            if (message is ChangeReadyStatusRequest readyStatusRequest) HandleReadyNotification(readyStatusRequest, sender);
            else if (message is ChatMessage chatMessage) HandleChatMessage(chatMessage, sender);
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
            if (readyMembers.Count >= 2) {
                var player1 = readyMembers[0];
                var player2 = readyMembers[1];
                
                RemoveMember(player1);
                RemoveMember(player2);

                Server.StartGame(player1, player2);
            }

            //(un)ready-ing / starting a game changes the lobby/ready count so send out an update
            //to all clients still in the lobby
            SendLobbyUpdateCount();
        }

        private void SendLobbyUpdateCount() {
            var lobbyInfoMessage = new LobbyInfoUpdate {
                MemberCount = MemberCount,
                ReadyCount = readyMembers.Count
            };
            SendToAll(lobbyInfoMessage);
        }

        private void HandleChatMessage(ChatMessage message, TcpMessageChannel sender) {
            var textMessage = message.Message.Trim();
            if (string.IsNullOrWhiteSpace(textMessage)) return;
            var playerInfo = Server.GetPlayerInfo(sender);
            message.Message = $"<b>{playerInfo.Name}</b>: {textMessage}";
            SendToAll(message);
        }
    }
}