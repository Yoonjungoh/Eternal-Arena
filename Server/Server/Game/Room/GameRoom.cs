using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Timers;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int RoomOwnerId { get; set; }
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public event Action<int> OnEmptyRoom; // 방이 비었을 때 알림 (roomId)
        public event Action<int> OnRoomInfoChanged;  // 방 정보 바뀌었을 때 알림 (roomId)
        public void Init()
        {
            //TestTimer();
        }

        // 어디선가 주기적으로 호출해줘야 함
        public void Update()
        {
            Flush();
            if (ObjectManager.Instance.Players.Count <= 100)
            {
                Player player = ObjectManager.Instance.Add<Player>();
                EnterGame(player);
            }
        }

        public void EnterGame(Player player)
        {
            if (player == null)
                return;

            player.GameRoom = this;
            player.ObjectState.Name = $"Player_{player.ObjectState.ObjectId}";

            S_EnterWaitingRoom enterWaitingRoomPacket = new S_EnterWaitingRoom();
            enterWaitingRoomPacket.ObjectState = new ObjectState();
            enterWaitingRoomPacket.ObjectState.Position = new ProtoVector3();
            enterWaitingRoomPacket.ObjectState.Velocity = new ProtoVector3();
            enterWaitingRoomPacket.ObjectState.Rotation = new ProtoQuaternion();

            // objectId 초기화
            enterWaitingRoomPacket.ObjectState.ObjectId = player.Id;

            // name 초기화
            enterWaitingRoomPacket.ObjectState.Name = player.ObjectState.Name;

            // position 초기화
            //int spawnIndex = _players.Count % DataManager.Instance.MaxRoomPlayerCount;
            int spawnIndex = _players.Count;
            Vector3 startPos = DataManager.Instance.GetStartPosition(spawnIndex);
            player.ObjectState.Position.X = startPos.X;
            player.ObjectState.Position.Y = startPos.Y;
            player.ObjectState.Position.Z = startPos.Z;
            enterWaitingRoomPacket.ObjectState.Position.X = player.ObjectState.Position.X;
            enterWaitingRoomPacket.ObjectState.Position.Y = player.ObjectState.Position.Y;
            enterWaitingRoomPacket.ObjectState.Position.Z = player.ObjectState.Position.Z;

            // TODO - stat

            // creatureState 초기화
            player.ObjectState.CreatureState = CreatureState.Idle;
            enterWaitingRoomPacket.ObjectState.CreatureState = CreatureState.Idle;

            if (player.Session != null)
            {
                player.Session.Send(enterWaitingRoomPacket);
            }

            _players.Add(player.Id, player);
            player.Init();

            // 본인한테 맵안의 플레이어 정보 전송
            S_Spawn spawnToMePacket = new S_Spawn();
            // 나를 제외하고 접속한 플레이어를 spawnPacket에 저장
            long serverReceivedTime = Util.GetTimestampMs();
            foreach (Player p in _players.Values)
            {
                if (p == null || player == p)
                    continue;

                p.ObjectState.ServerReceivedTime = serverReceivedTime;
                spawnToMePacket.ObjectStates.Add(p.ObjectState);
            }
            if (player.Session != null)
            {
                player.Session.Send(spawnToMePacket);
            }

            // 다른 플레이어에게도 내가 접속한 걸 알려주기
            S_Spawn spawnToOthersPacket = new S_Spawn();
            spawnToOthersPacket.ObjectStates.Add(player.ObjectState);
            foreach (Player p in _players.Values)
            {
                if (p == null || p.Session == null || player.Id == p.Id )
                    continue;

                p.ObjectState.ServerReceivedTime = serverReceivedTime;
                p.Session.Send(spawnToOthersPacket);
                ConsoleLogManager.Instance.Log($"[WaitingRoom Update] Player {p.Id} Pos({p.Position.X}, {p.Position.Y}, {p.Position.Z})");
            }
        }

        public void LeaveGame(int playerId)
        {

        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null || movePacket == null)
                return;

            // 서버에서 상태 업데이트
            player.ObjectState = movePacket.ObjectState;

            //TODO - 일정 거리 이상 순간이동 방지
            Vector3 serverPos = new Vector3(player.ObjectState.Position.X, player.ObjectState.Position.Y, player.ObjectState.Position.Z);
            Vector3 clientPos = new Vector3(movePacket.ObjectState.Position.X, movePacket.ObjectState.Position.Y, movePacket.ObjectState.Position.Z);

            float dist = Vector3.Distance(serverPos, clientPos);
            if (dist > 1.0f)
            {
                Console.WriteLine($"[Warning] Player {player.Id} position correction ({dist})");
                movePacket.ObjectState.Position = player.ObjectState.Position;
                movePacket.ObjectState.Velocity = new ProtoVector3 { X = 0, Y = 0, Z = 0 };
            }

            // 다른 유저들에게 브로드캐스트
            S_Move res = new S_Move { ObjectState = movePacket.ObjectState };
            res.ObjectState.ServerReceivedTime = Util.GetTimestampMs();
            //Console.WriteLine
            //    ($"Player {player.Id} -> Vel: ({player.Velocity.X}, {player.Velocity.Y}, {player.Velocity.Z})" +
            //    $"Rot: ({player.Rotation.X}, {player.Rotation.Y}, {player.Rotation.Z}, {player.Rotation.W})");
            Broadcast(res, player.Id);
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public void Broadcast(IMessage packet, int? exceptId = null)
        {
            foreach (Player p in _players.Values)
            {
                if (exceptId.HasValue && p.Id == exceptId.Value)
                    continue;

                if (p.Session == null)
                    continue;

                p.Session.Send(packet);
            }
        }
    }
}