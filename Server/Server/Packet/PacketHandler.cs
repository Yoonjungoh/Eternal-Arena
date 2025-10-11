using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
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
        Console.WriteLine($"(아이디: {enterGamePacket.ObjectInfo.ObjectId}, 이름: {enterGamePacket.ObjectInfo.Name})");
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

        Lobby lobby = user.Lobby;
        if (lobby == null)
            return;

        lobby.Push(lobby.EnterLobby, user);
    }
}
