using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager
{
    public void EnterRoom(int roomId)
    {
        Managers.Object.RoomId = roomId;
        Managers.Scene.LoadScene(Define.Scene.WaitingRoom);
        // 이후 상황은 WaitingRoomScene에서 Init 처리
    }
}