using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager
{
    public int RoomId = -1; // 현재 Room Id, -1이면 방에 없음

    public void EnterRoom(int roomId)
    {
        RoomId = roomId;
        Managers.Scene.LoadScene(Define.Scene.WaitingRoom);
        // 이후 상황은 WaitingRoomScene에서 Init 처리
    }

    public void ExitRoom()
    {
        RoomId = -1;
        Managers.Object.Clear();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}