using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager
{
    // 현재 Room Id, -1이면 방에 없음
    public RoomInfo RoomInfo { get; set; } = new RoomInfo();
    public bool IsRoomOwner { get { return RoomInfo.RoomOwnerId == Managers.Object.UserId; } }
    public bool CanEnterGame { get { return RoomInfo.CurrentPlayerCount == RoomInfo.MaxPlayerCount; } }

    public void Init()
    {
        RoomInfo.RoomId = -1;
    }

    public void EnterRoom(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        Managers.Scene.LoadScene(Define.Scene.WaitingRoom);
        // 이후 상황은 WaitingRoomScene에서 Init 처리
    }

    public void ExitRoom()
    {
        RoomInfo = new RoomInfo();
        RoomInfo.RoomId = -1;
        Managers.Object.Clear();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}