using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoomManager
{
    // 현재 Room Id, -1이면 방에 없음
    public RoomInfo RoomInfo { get; set; } = new RoomInfo();
    public RepeatedField<int> PlayerIdList = new RepeatedField<int>();

    public void Init()
    {

    }

    public void EnterGame(RoomInfo roomInfo, RepeatedField<int> playerIdList)
    {
        RoomInfo = roomInfo;
        PlayerIdList = playerIdList;
        Managers.Scene.LoadScene(Define.Scene.GameRoom);
    }

    public void ExitGame()
    {
        RoomInfo = new RoomInfo();
        RoomInfo.RoomId = -1;
        Managers.GameRoomObject.Clear();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}