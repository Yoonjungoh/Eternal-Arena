using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager
{
    public bool IsEnterFirst { get; set; } = false;   // 클라이언트가 로비에 처음 접속했었는지 묻는 함수
    public PlayerSelectInfo MyPlayer { get; private set; }  // 선택된 플레이어 정보

    public void SetSelectedPlayerInfo(PlayerSelectInfo playerSelectInfo)
    {
        MyPlayer = playerSelectInfo;
    }
}