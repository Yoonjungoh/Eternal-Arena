using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager
{
    public void EnterRoom(int roomId)
    {
        // 방 입장 하겠다고 패킷 전송
        C_EnterWaitingRoom enterRoomPacket = new C_EnterWaitingRoom();
        enterRoomPacket.UserId = Managers.Object.UserId;
        enterRoomPacket.RoomId = roomId;
        Managers.Network.Send(enterRoomPacket);
    }
}