using Google.Protobuf.Protocol;
using Newtonsoft;
using Newtonsoft.Json;
using Server.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Server.Game
{
    // TODO - JSON 파싱
    public class DataManager
    {
        public List<Vector3> StartPositions = new List<Vector3>()
        {
            new Vector3(0, 10, 0),
            new Vector3(3, 10, 0),
            new Vector3(-3, 10, 0),
            new Vector3(0, 10, 3)
        };

        public static DataManager Instance { get; } = new DataManager();
        public int MaxLobbyCount = 3;  // 최대 로비 개수
        public int MaxRoomPlayerCount = 2; // 방당 최대 플레이어 수
        
        public Vector3 GetStartPosition(int index)
        {
            if (index < 0 || index >= StartPositions.Count)
            {
                return new Vector3(0, 10, 0); // 기본값
            }
            return StartPositions[index];
        }
    }
}