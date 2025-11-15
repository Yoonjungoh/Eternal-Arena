using ServerCore;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server.Game.Room
{
    /* Cell = 맵 데이터 좌표 */
    public struct Cell
    {
        public int X;
        public int Y;
        public Cell(int x, int y) { X = x; Y = y; }
    }

    /* A* 내부 좌표 변환용 */
    public struct Pos
    {
        public int Y;
        public int X;
        public Pos(int y, int x) { Y = y; X = x; }
    }

    /* 우선순위 큐 노드 */
    public struct PQNode : IComparable<PQNode>
    {
        public int F;
        public int G;
        public int Y;
        public int X;

        public int CompareTo(PQNode other)
        {
            if (F == other.F) return 0;
            return F < other.F ? 1 : -1;
        }
    }

    public class Map
    {
        public MapData MapData { get; set; }
        public const float NO_HEIGHT_VALUE = -9999f;

        // U D L R
        int[] _deltaY = new int[] { 1, -1, 0, 0 };
        int[] _deltaX = new int[] { 0, 0, -1, 1 };
        int[] _cost = new int[] { 10, 10, 10, 10 };

        public int SizeX => MapData.SizeX;
        public int SizeZ => MapData.SizeZ;

        /* --------------------------
         * 좌표 변환 (World → Cell)
         * -------------------------- */
        public Cell WorldToCell(Vector3 w)
        {
            int x = (int)((w.X - MapData.Origin.X) / MapData.CellSize);
            int y = (int)((w.Z - MapData.Origin.Z) / MapData.CellSize);
            return new Cell(x, y);
        }

        /* --------------------------
         * 좌표 변환 (Cell → World)
         * -------------------------- */
        public Vector3 CellToWorld(Cell c)
        {
            float wx = MapData.Origin.X + c.X * MapData.CellSize + MapData.CellSize * 0.5f;
            float wz = MapData.Origin.Z + c.Y * MapData.CellSize + MapData.CellSize * 0.5f;
            float wy = GetCellHeight(c);
            return new Vector3(wx, wy, wz);
        }

        public bool IsValidCell(Cell c)
        {
            return (c.X >= 0 && c.Y >= 0 && c.X < MapData.SizeX && c.Y < MapData.SizeZ);
        }

        /* --------------------------
         * 높이 / 이동 가능
         * -------------------------- */
        public float GetCellHeight(Cell c)
        {
            if (!IsValidCell(c))
                return NO_HEIGHT_VALUE;
            return MapData.Height[c.X, c.Y];
        }

        public bool CanGo(Cell c)
        {
            if (!IsValidCell(c))
                return false;
            return MapData.CanGo[c.X, c.Y];
        }

        public bool CanGo(float worldX, float worldZ)
        {
            Cell c = WorldToCell(new Vector3(worldX, 0, worldZ));
            return CanGo(c);
        }

        /* =============================================================
         * 외부 API(몬스터/AI가 호출)
         * ============================================================= */
        public List<Vector3> FindPath(Vector3 startWorld, Vector3 destWorld)
        {
            Cell start = WorldToCell(startWorld);
            Cell dest = WorldToCell(destWorld);

            List<Pos> raw = InternalFindPath(start, dest);
            return ConvertToWorldPath(raw);
        }

        /* =============================================================
         * 내부 A* (자료구조 최적화 버전)
         * ============================================================= */
        private List<Pos> InternalFindPath(Cell start, Cell dest)
        {
            // (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
            HashSet<Pos> closeList = new HashSet<Pos>();  // == CloseList

            // (y, x) 가는 길을 발견한 적 있는지
            Dictionary<Pos, int> openList = new Dictionary<Pos, int>(); // == OpenList

            // 부모 정보 저장
            Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();

            PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

            Pos pos = new Pos(start.Y, start.X);
            Pos des = new Pos(dest.Y, dest.X);

            int hStart = 10 * (Math.Abs(des.Y - pos.Y) + Math.Abs(des.X - pos.X));
            openList[pos] = hStart;

            pq.Push(new PQNode()
            {
                F = hStart,
                G = 0,
                Y = pos.Y,
                X = pos.X
            });

            parent[pos] = pos;

            while (pq.Count > 0)
            {
                PQNode node = pq.Pop();
                Pos cur = new Pos(node.Y, node.X);

                if (closeList.Contains(cur))
                    continue;

                closeList.Add(cur);

                if (cur.Y == des.Y && cur.X == des.X)
                    break;

                for (int i = 0; i < 4; i++)
                {
                    int ny = cur.Y + _deltaY[i];
                    int nx = cur.X + _deltaX[i];

                    Pos next = new Pos(ny, nx);
                    Cell nextCell = new Cell(nx, ny);

                    // 유효?
                    if (!IsValidCell(nextCell))
                        continue;

                    // 갈 수 있나?
                    if (!CanGo(nextCell))
                        continue;

                    // 이미 방문?
                    if (closeList.Contains(next))
                        continue;

                    int g = node.G + _cost[i];
                    int h = 10 * (Math.Abs(des.Y - ny) + Math.Abs(des.X - nx));
                    int f = g + h;

                    if (openList.TryGetValue(next, out int oldF) && oldF <= f)
                        continue;

                    openList[next] = f;
                    parent[next] = cur;

                    pq.Push(new PQNode()
                    {
                        F = f,
                        G = g,
                        Y = ny,
                        X = nx
                    });
                }
            }

            return BuildPath(parent, pos, des);
        }

        /* --------------------------
         * 경로 역추적
         * -------------------------- */
        private List<Pos> BuildPath(Dictionary<Pos, Pos> parent, Pos start, Pos dest)
        {
            List<Pos> path = new List<Pos>();

            Pos cur = dest;
            while (!(cur.X == start.X && cur.Y == start.Y))
            {
                path.Add(cur);

                if (!parent.TryGetValue(cur, out Pos p))
                    break;

                if (p.X == cur.X && p.Y == cur.Y)
                    break;

                cur = p;
            }

            path.Add(start);
            path.Reverse();
            return path;
        }

        /* --------------------------
         * World 변환
         * -------------------------- */
        private List<Vector3> ConvertToWorldPath(List<Pos> raw)
        {
            List<Vector3> result = new List<Vector3>();
            foreach (Pos p in raw)
            {
                Cell c = new Cell(p.X, p.Y);
                result.Add(CellToWorld(c));
            }
            return result;
        }
    }
}
