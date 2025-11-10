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
            //new Vector3(-150, -18, 112),  // 숲풀
            new Vector3(-676, 8, -471),
            new Vector3(-677, 8, -471),
            new Vector3(-678, 8, -471),
            new Vector3(-679, 8, -471),
        };

        public static DataManager Instance { get; } = new DataManager();
        public int MaxLobbyCount = 3;  // 최대 로비 개수
        public int MaxRoomPlayerCount = 2; // 방당 최대 플레이어 수
        
        public Vector3 GetStartPosition(int index)
        {
            if (index < 0 || index >= StartPositions.Count)
                return new Vector3(
                    StartPositions[StartPositions.Count - 1].X + index,
                    StartPositions[StartPositions.Count - 1].Y,
                    StartPositions[StartPositions.Count - 1].Z
                ); // 기본값

            return StartPositions[index];
        }
    }
}