using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using static System.Collections.Specialized.BitVector32;

class PacketHandler
{
    public static void C_AssignUserIdHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null)
            return;

        S_AssignUserId s_AssignUserId = new S_AssignUserId();
        s_AssignUserId.UserId = clientSession.MyPlayer.Id;
        clientSession.Send(s_AssignUserId);
    }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = packet as C_EnterGame;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        //GameRoom room = player.Room;
        //if (room == null)
        //    return;
        //room.Push(room.HandleMove, player, movePacket);
    }

    public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return; 
		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleMove, player, movePacket);
	}

    public static void C_LeaveLobbyHandler(PacketSession session, IMessage packet)
    {
        C_LeaveLobby leaveLobbyPacket = packet as C_LeaveLobby;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null)
            return;

        Lobby lobby = LobbyManager.Instance.Find(user.LobbyId);
        if (lobby == null)
            return;

        lobby.Push(lobby.LeaveLobby, user.Id);
    }

    public static void C_AddRoomHandler(PacketSession session, IMessage packet)
    {
        C_AddRoom addRoomPacket = packet as C_AddRoom;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null)
            return;

        Lobby lobby = LobbyManager.Instance.Find(user.LobbyId);
        if (lobby == null || lobby.RoomManager == null)
            return;
        
        lobby.Push(lobby.HandleAddRoom, user, addRoomPacket.RoomName);
    }
}
