using shared;
using System;
using System.Collections.Generic;
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
        }

        protected virtual void RemoveMember(TcpMessageChannel member) {
            Logger.Info("Client left.", this, "ROOM-INFO");
            members.Remove(member);
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
            SafeForEach(CheckFaultyMember);
        }

        /**
		* Iterates backwards through all members and calls the given method on each of them.
		* This basically allows you to process all clients, and optionally remove them 
		* without weird crashes due to collections being modified.
		* 
		* This can happen while looking for faulty clients, or when deciding to move a bunch 
		* of members to a different room, while you are still processing them.
		*/
        protected void SafeForEach(Action<TcpMessageChannel> action) {
            for (int i = members.Count - 1; i >= 0; i--) {
                //skip any members that have been 'killed' in the mean time
                if (i >= members.Count) continue;
                //call the method on any still existing member
                action(members[i]);
            }
        }

        /**
		 * Check if a member is no longer connected or has issues, if so remove it from the room, and close it's connection.
		 */
        private void CheckFaultyMember(TcpMessageChannel member) {
            if (!member.Connected) RemoveAndCloseMember(member);
        }

        /**
		 * Removes a member from this room and closes it's connection (basically it is being removed from the server).
		 */
        protected void RemoveAndCloseMember(TcpMessageChannel member) {
            RemoveMember(member);
            Server.RemovePlayerInfo(member);
            member.Close();

            Logger.Info("Removed client at " + member.GetRemoteEndPoint(), this, "ROOM-INFO");
        }

        /**
		 * Iterate over all members and get their network messages.
		 */
        protected void ReceiveAndProcessNetworkMessages() {
            SafeForEach(ReceiveAndProcessNetworkMessagesFromMember);
        }

        /**
		 * Get all the messages from a specific member and process them
		 */
        private void ReceiveAndProcessNetworkMessagesFromMember(TcpMessageChannel pMember) {
            while (pMember.HasMessage()) {
                HandleNetworkMessage(pMember.ReceiveMessage(), pMember);
            }
        }

        protected abstract void HandleNetworkMessage(object message, TcpMessageChannel sender);

        /**
		 * Sends a message to all members in the room.
		 */
        protected void SendToAll(object message) {
            foreach (TcpMessageChannel member in members) {
                member.SendMessage(message);
            }
        }
    }
}