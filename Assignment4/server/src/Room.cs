using shared;
using System;
using System.Collections.Generic;

namespace server
{
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
	abstract class Room
	{
		//allows all rooms to access the server they are a part of so they can request access to other rooms, client info etc
		protected TCPGameServer _server { private set; get; }
		//all members of this room (we identify them by their message channel)
		private List<TcpMessageChannel> _members;

		/**
		 * Create a room with an empty member list and reference to the server instance they are a part of.
		 */
		protected Room (TCPGameServer pServer)
		{
			_server = pServer;
			_members = new List<TcpMessageChannel>();
		}

		protected virtual void addMember(TcpMessageChannel pMember)
		{
			Log.LogInfo("Client joined.", this);
			_members.Add(pMember);
		}

		protected virtual void removeMember(TcpMessageChannel pMember)
		{
			Log.LogInfo("Client left.", this);
			_members.Remove(pMember);
		}

		protected int memberCount => _members.Count;
		
		protected int indexOfMember (TcpMessageChannel pMember)
		{
			return _members.IndexOf(pMember);
		}

		/**
		 * Should be called each server loop so that this room can do it's work.
		 */
		public virtual void Update()
		{
			removeFaultyMembers();
			receiveAndProcessNetworkMessages();
		}

		/**
		 * Iterate over all members and remove the ones that have issues.
		 * Return true if any members were removed.
		 */
		protected void removeFaultyMembers() 
		{
			safeForEach(checkFaultyMember);
		}

		/**
		* Iterates backwards through all members and calls the given method on each of them.
		* This basically allows you to process all clients, and optionally remove them 
		* without weird crashes due to collections being modified.
		* 
		* This can happen while looking for faulty clients, or when deciding to move a bunch 
		* of members to a different room, while you are still processing them.
		*/
		protected void safeForEach(Action<TcpMessageChannel> pMethod)
		{
			for (int i = _members.Count - 1; i >= 0; i--)
			{
				//skip any members that have been 'killed' in the mean time
				if (i >= _members.Count) continue;
				//call the method on any still existing member
				pMethod(_members[i]);
			}
		}

		/**
		 * Check if a member is no longer connected or has issues, if so remove it from the room, and close it's connection.
		 */
		private void checkFaultyMember(TcpMessageChannel pMember)
		{
			if (!pMember.Connected) removeAndCloseMember(pMember);
		}

		/**
		 * Removes a member from this room and closes it's connection (basically it is being removed from the server).
		 */
		protected void removeAndCloseMember(TcpMessageChannel pMember)
		{
			removeMember(pMember);
			_server.RemovePlayerInfo(pMember);
			pMember.Close();

			Log.LogInfo("Removed client at " + pMember.GetRemoteEndPoint(), this);
		}

		/**
		 * Iterate over all members and get their network messages.
		 */
		protected void receiveAndProcessNetworkMessages()
		{
			safeForEach(receiveAndProcessNetworkMessagesFromMember);
		}

		/**
		 * Get all the messages from a specific member and process them
		 */
		private void receiveAndProcessNetworkMessagesFromMember(TcpMessageChannel pMember)
		{
			while (pMember.HasMessage())
			{
				handleNetworkMessage(pMember.ReceiveMessage(), pMember);
			}
		}

		abstract protected void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender);

		/**
		 * Sends a message to all members in the room.
		 */
		protected void sendToAll(ASerializable pMessage)
		{
			foreach (TcpMessageChannel member in _members)
			{
				member.SendMessage(pMessage);
			}
		}

	}
}

