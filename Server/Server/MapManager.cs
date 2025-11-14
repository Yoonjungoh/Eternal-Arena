using System;
using System.IO;
using System.Numerics;

namespace Server
{

    public class MapData
    {
        public float CellSize;
        public Vector3 Origin;
        public int SizeX;
        public int SizeZ;

        public float[,] Height;
        public bool[,] CanGo;
    }

    public class MapManager
    {
        private MapData _mapData { get; set; }

        public static MapManager Instance { get; } = new MapManager();
        public const float NO_HEIGHT_VALUE = -9999f;

        public bool CanGo(float worldX, float worldZ)
        {
            if (_mapData == null) return false;
            
            int x = (int)((worldX - _mapData.Origin.X) / _mapData.CellSize);
            int z = (int)((worldZ - _mapData.Origin.Z) / _mapData.CellSize);

            if (x < 0 || z < 0 || x >= _mapData.SizeX || z >= _mapData.SizeZ)
                return false;

            return _mapData.CanGo[x, z];
        }

        public float GetHeight(float worldX, float worldZ)
        {
            if (_mapData == null) return NO_HEIGHT_VALUE;

            int x = (int)((worldX - _mapData.Origin.X) / _mapData.CellSize);
            int z = (int)((worldZ - _mapData.Origin.Z) / _mapData.CellSize);

            if (x < 0 || z < 0 || x >= _mapData.SizeX || z >= _mapData.SizeZ)
                return NO_HEIGHT_VALUE;

            return _mapData.Height[x, z];
        }

        public void Init()
        {
            string mapPath = GetMapPath();

            if (!File.Exists(mapPath))
            {
                ConsoleLogManager.Instance.Log($"[MapManager] Map file not found: {mapPath}");
                return;
            }
            
            ConsoleLogManager.Instance.Log($"[MapManager] Loading Map: {mapPath}");
            _mapData = Load(mapPath);

            ConsoleLogManager.Instance.Log($"[MapManager] Loaded Size = {_mapData.SizeX} x {_mapData.SizeZ}");
            ConsoleLogManager.Instance.Log($"[MapManager] Origin = {_mapData.Origin}");
            ConsoleLogManager.Instance.Log($"[MapManager] CellSize = {_mapData.CellSize}");
            ConsoleLogManager.Instance.Log($"[MapManager] Test");
            ConsoleLogManager.Instance.Log($"Height[0,0] = {_mapData.Height[0, 0]}");
            ConsoleLogManager.Instance.Log($"Height[100,100] = {_mapData.Height[100, 100]}");

            ConsoleLogManager.Instance.Log($"CanGo[0,0] = {_mapData.CanGo[0, 0]}");
            ConsoleLogManager.Instance.Log($"CanGo[100,100] = {_mapData.CanGo[100, 100]}");
            ConsoleLogManager.Instance.Log($"[MapManager] Load Complete");
        }

        private MapData Load(string filePath)
        {
            MapData map = new MapData();

            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
            {
                // Header
                map.CellSize = br.ReadSingle();
                float ox = br.ReadSingle();
                float oy = br.ReadSingle();
                float oz = br.ReadSingle();
                map.Origin = new Vector3(ox, oy, oz);

                map.SizeX = br.ReadInt32();
                map.SizeZ = br.ReadInt32();

                int sx = map.SizeX;
                int sz = map.SizeZ;
                int totalCells = sx * sz;

                map.Height = new float[sx, sz];
                map.CanGo = new bool[sx, sz];

                // Height (ushort)
                for (int x = 0; x < sx; x++)
                {
                    for (int z = 0; z < sz; z++)
                    {
                        ushort encoded = br.ReadUInt16();

                        if (encoded == 0)
                            map.Height[x, z] = -9999f;
                        else
                            map.Height[x, z] = (encoded / 100f) - 100f;
                    }
                }

                // CanGo (bit packing)
                int byteCount = (totalCells + 7) / 8;
                byte[] packed = br.ReadBytes(byteCount);

                int idx = 0;

                for (int x = 0; x < sx; x++)
                {
                    for (int z = 0; z < sz; z++)
                    {
                        int byteIndex = idx >> 3;  // idx / 8
                        int bitIndex = idx & 7;    // idx % 8

                        bool canGo = ((packed[byteIndex] >> bitIndex) & 1) == 1;
                        map.CanGo[x, z] = canGo;

                        idx++;
                    }
                }
            }

            return map;
        }

        private string GetMapPath()
        {
            // 서버 실행 파일 위치 
            string exeDir = AppContext.BaseDirectory;

            string root = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\.."));

            string mapPath = Path.Combine(root, "Common", "MapData", "MapData_001.bytes");

            return mapPath;
        }
    }
}
