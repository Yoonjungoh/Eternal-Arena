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
        WaitingRoom room = player.WaitingRoom;
		if (room == null)
			return;

		room.Push(room.HandleMove, player, movePacket);
	}

    public static void C_LeaveLobbyHandler(PacketSession session, IMessage packet)
    {
        C_LeaveLobby leaveLobbyPacket = packet as C_LeaveLobby;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null || user.Lobby == null)
            return;

        user.Lobby.Push(user.Lobby.LeaveLobby, user.Id);
    }


    public static void C_EnterWaitingRoomHandler(PacketSession session, IMessage packet)
    {
        C_EnterWaitingRoom enterWaitingRoomPacket = packet as C_EnterWaitingRoom;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null || user.Lobby == null)
            return;

        // 로비에 해당 유저를 먼저 떠나게 하고
        user.Lobby.Push(user.Lobby.LeaveLobby, user.Id);

        // 해당 유저를 방에 추가하기
        WaitingRoom watingRoom = user.Lobby.WaitingRoomManager.Find(enterWaitingRoomPacket.RoomId);
        if (watingRoom == null)
            return;
        
        watingRoom.EnterRoom(user);
    }

    public static void C_AddRoomHandler(PacketSession session, IMessage packet)
    {
        C_AddRoom addRoomPacket = packet as C_AddRoom;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null || user.Lobby == null)
            return;

        user.Lobby.Push(user.Lobby.HandleAddRoom, user, addRoomPacket.RoomName);
    }

    public static void C_ExitRoomHandler(PacketSession session, IMessage packet)
    {
        C_ExitRoom exitRoomPacket = packet as C_ExitRoom;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null || user.Lobby == null || user.WaitingRoom == null)
            return;

        user.Lobby.Push(user.WaitingRoom.LeaveRoom, user.Id);
    }

    public static void C_EnterLobbyHandler(PacketSession session, IMessage packet)
    {
        C_EnterLobby enterLobbyPacket = packet as C_EnterLobby;
        ClientSession clientSession = session as ClientSession;

        Player user = clientSession.MyPlayer;
        if (user == null || user.Lobby == null)
            return;

        LobbyManager.Instance.EnterLobby(1, user);	// TODO - 1번 로비로 강제 이동

    }
}
