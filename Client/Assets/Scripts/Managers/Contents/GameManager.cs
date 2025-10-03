using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    // 레이턴시 측정용
    public DateTime PrevLatency;
    public TimeSpan NowLatency;
}