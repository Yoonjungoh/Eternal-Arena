using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CAssignUserId, MakePacket<C_AssignUserId>);
		_handler.Add((ushort)MsgId.CAssignUserId, PacketHandler.C_AssignUserIdHandler);		
		_onRecv.Add((ushort)MsgId.CLeaveLobby, MakePacket<C_LeaveLobby>);
		_handler.Add((ushort)MsgId.CLeaveLobby, PacketHandler.C_LeaveLobbyHandler);		
		_onRecv.Add((ushort)MsgId.CEnterWaitingRoom, MakePacket<C_EnterWaitingRoom>);
		_handler.Add((ushort)MsgId.CEnterWaitingRoom, PacketHandler.C_EnterWaitingRoomHandler);		
		_onRecv.Add((ushort)MsgId.CAddRoom, MakePacket<C_AddRoom>);
		_handler.Add((ushort)MsgId.CAddRoom, PacketHandler.C_AddRoomHandler);		
		_onRecv.Add((ushort)MsgId.CExitRoom, MakePacket<C_ExitRoom>);
		_handler.Add((ushort)MsgId.CExitRoom, PacketHandler.C_ExitRoomHandler);		
		_onRecv.Add((ushort)MsgId.CEnterLobby, MakePacket<C_EnterLobby>);
		_handler.Add((ushort)MsgId.CEnterLobby, PacketHandler.C_EnterLobbyHandler);		
		_onRecv.Add((ushort)MsgId.CStartGame, MakePacket<C_StartGame>);
		_handler.Add((ushort)MsgId.CStartGame, PacketHandler.C_StartGameHandler);		
		_onRecv.Add((ushort)MsgId.CEnterGame, MakePacket<C_EnterGame>);
		_handler.Add((ushort)MsgId.CEnterGame, PacketHandler.C_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.CAttack, MakePacket<C_Attack>);
		_handler.Add((ushort)MsgId.CAttack, PacketHandler.C_AttackHandler);		
		_onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CTimestamp, MakePacket<C_Timestamp>);
		_handler.Add((ushort)MsgId.CTimestamp, PacketHandler.C_TimestampHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
        {
			CustomHandler.Invoke(session, pkt, id);	
		}
        else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}