using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;

namespace Server.Game
{
    // TODO - JSON 파싱
    public class DataManager
    {
        public static DataManager Instance { get; } = new DataManager();
        public int MaxLobbyCount = 3;  // 최대 로비 개수
    }
}