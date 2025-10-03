using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;

namespace Server.Game.Room
{
    public class DataManager
    {
        public static DataManager Instance { get; } = new DataManager();
    }
}