using shared;
using System;
using System.Collections.Generic;
using shared.protocol;
using shared.serialization;

namespace server {
    public sealed class GameResultsRoom : Room {
        protected override RoomType RoomType => RoomType.GAME_RESULTS_ROOM;

        private readonly float timeout;
        private readonly Dictionary<TcpMessageChannel, DateTime> joinTime;

        public GameResultsRoom(TCPGameServer owner, float timeout) : base(owner) {
            this.timeout = timeout;
            joinTime = new Dictionary<TcpMessageChannel, DateTime>();
        }

        protected internal override void AddMember(TcpMessageChannel member) {
            base.AddMember(member);

            joinTime[member] = DateTime.Now;
            member.SendMessage(new RoomJoinedEvent {Room = RoomType});
        }

        protected override bool RemoveMember(TcpMessageChannel member) {
            joinTime.Remove(member);
            return base.RemoveMember(member);
        }

        public override void Update() {
            var now = DateTime.Now;
            Members.SafeForEach(member => {
                if ((now - joinTime[member]).TotalSeconds < timeout) return;
                MoveToLobby(member);
            });
        }

        private void MoveToLobby(TcpMessageChannel member) {
            RemoveMember(member);
            Server.LobbyRoom.AddMember(member);
        }

        protected override void HandleNetworkMessage(object message, TcpMessageChannel sender) {
            if (message is LeaveRoomRequest leaveRoomRequest) {
                if(leaveRoomRequest.Room == RoomType) MoveToLobby(sender);
                else RemoveAndCloseMember(sender, $"Unexpected request: Leave room {leaveRoomRequest.Room} when in room {RoomType}");
            }
        }
    }
}