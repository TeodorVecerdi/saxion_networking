using shared;
using System;
using System.Collections.Generic;
using shared.protocol;
using shared.serialization;

namespace server {
    /**
	 * Room is the abstract base class for all Rooms.
	 * 
	 * A room has a set of members and some base message processing functionality:
	 *	- addMember, removeMember, removeAndCloseMember, indexOfMember, memberCount
	 *	- safeForEach -> call a method on each member without crashing if a member leaves
	 *	- default administration: removing faulty member and processing incoming messages
	 *	
	 * Usage: subclass and override handleNetworkMessage
	 */
    public abstract class Room {
        //allows all rooms to access the server they are a part of so they can request access to other rooms, client info etc
        protected TCPGameServer Server { get; }
        //all members of this room (we identify them by their message channel)
        private readonly List<TcpMessageChannel> members;
        protected List<TcpMessageChannel> Members => members;
        
        protected abstract RoomType RoomType { get; }

        /**
		 * Create a room with an empty member list and reference to the server instance they are a part of.
		 */
        protected Room(TCPGameServer server) {
            Server = server;
            members = new List<TcpMessageChannel>();
        }

        protected internal virtual void AddMember(TcpMessageChannel member) {
            Logger.Info("Client joined.", this, "ROOM-INFO");
            members.Add(member);
            Server.UpdateRoomType(member, RoomType);
        }

        protected virtual bool RemoveMember(TcpMessageChannel member) {
            Logger.Info("Client left.", this, "ROOM-INFO");
            return members.Remove(member);
        }

        protected int MemberCount => members.Count;

        protected int IndexOfMember(TcpMessageChannel member) {
            return members.IndexOf(member);
        }

        /**
		 * Should be called each server loop so that this room can do it's work.
		 */
        public virtual void Update() {
            RemoveFaultyMembers();
            ReceiveAndProcessNetworkMessages();
        }

        /**
		 * Iterate over all members and remove the ones that have issues.
		 * Return true if any members were removed.
		 */
        protected void RemoveFaultyMembers() {
            members.SafeForEach(CheckFaultyMember);
        }

        /**
		 * Check if a member is no longer connected or has issues, if so remove it from the room, and close it's connection.
		 */
        private void CheckFaultyMember(TcpMessageChannel member) {
            if (!member.Connected) RemoveAndCloseMember(member, "No longer connected");
            else if (!Server.IsHeartbeatValid(member)) RemoveAndCloseMember(member, "Timeout");
        }

        /**
		 * Removes a member from this room and closes it's connection (basically it is being removed from the server).
		 */
        protected void RemoveAndCloseMember(TcpMessageChannel member, string reason) {
            RemoveMember(member);
            Server.RemovePlayerInfo(member);
            member.Close();

            Logger.Info($"Removed client at {member.GetRemoteEndPoint()}{(string.IsNullOrEmpty(reason) ? "" : $" [Reason: {reason}]")}", this, "ROOM-INFO");
        }

        /**
		 * Iterate over all members and get their network messages.
		 */
        protected void ReceiveAndProcessNetworkMessages() {
            members.SafeForEach(ReceiveAndProcessNetworkMessagesFromMember);
        }

        /**
		 * Get all the messages from a specific member and process them
		 */
        private void ReceiveAndProcessNetworkMessagesFromMember(TcpMessageChannel member) {
            while (member.HasMessage()) {
                Server.UpdateHeartbeat(member);
                HandleNetworkMessage(member.ReceiveMessage(), member);
            }
        }

        protected abstract void HandleNetworkMessage(object message, TcpMessageChannel sender);

        /**
		 * Sends a message to all members in the room.
		 */
        protected void SendToAll(object message, TcpMessageChannel except = null) {
            foreach (var member in members) {
                if(member == except) continue;
                member.SendMessage(message);
            }
        }
    }
}